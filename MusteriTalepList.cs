using FastReport.DevComponents.DotNetBar.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using FastReport;


namespace kalipproje
{
    public partial class MusteriTalepList : Form
    {
        private SqlConnection con;
        private Form activeForm = null;

        public MusteriTalepList()
        {
            InitializeComponent();
            con = DatabaseHelper.GetConnection();
            LoadData("0"); // Form yüklendiğinde varsayılan olarak durumu "0" olan talepleri yükle
            CustomizeDataGridView();
            SetDefaultDateRange();

            // RadioButton olaylarını bağla
            radioButton1.CheckedChanged += radioButton1_CheckedChanged;
            radioButton2.CheckedChanged += radioButton2_CheckedChanged;
            radioButton3.CheckedChanged += radioButton3_CheckedChanged;
            textBox2.TextChanged += textBox2_TextChanged;
            dataGridView1.DoubleClick += dataGridView1_CellDoubleClick;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            string filter = textBox2.Text.Trim();
            LoadFilteredData(filter); // Filtreleme fonksiyonunu çağır
        }

        private void LoadFilteredData(string filter)
        {
            // 1) Önce seçili TalepNo’yu al
            int? selectedTalepNo = null;
            if (dataGridView1.CurrentRow != null)
                selectedTalepNo = Convert.ToInt32(dataGridView1.CurrentRow.Cells["TalepNo"].Value);

            // 2) Veri yükleme
            string query = @"SELECT TH.TalepNo, TH.TalepTarihi, TH.MusteriUnvani, 
                            ISNULL(SUM(TD.Adet), 0) AS Tabancifti, 
                            TH.Aciklama, TH.Kullanici 
                     FROM TalepHeader TH 
                     LEFT JOIN TalepDetails TD ON TH.TalepNo = TD.TalepNo
                     WHERE TH.MusteriUnvani LIKE @Filter AND TH.Durum = @Durum 
                     GROUP BY TH.TalepNo, TH.TalepTarihi, TH.MusteriUnvani, TH.Aciklama, TH.Kullanici";

            DataTable dataTable = new DataTable();
            using (SqlCommand command = new SqlCommand(query, con))
            {
                command.Parameters.AddWithValue("@Filter", "%" + filter + "%");
                command.Parameters.AddWithValue("@Durum", GetCurrentStatus());
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    adapter.Fill(dataTable);
            }
            dataGridView1.DataSource = dataTable;
            CustomizeDataGridView();

            // 3) Yeniden seçim
            if (selectedTalepNo.HasValue)
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (Convert.ToInt32(row.Cells["TalepNo"].Value) == selectedTalepNo.Value)
                    {
                        dataGridView1.ClearSelection();
                        row.Selected = true;
                        dataGridView1.CurrentCell = row.Cells["TalepNo"];
                        dataGridView1.FirstDisplayedScrollingRowIndex = row.Index;
                        break;
                    }
                }
            }
        }

        private void CustomizeDataGridView()
        {
            // Sütun başlıkları stilini ayarla
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(200, 200, 200);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersHeight = 30;

            // Seçili sütunun arka plan rengini aynı yapalım
            dataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 200, 200); // Aynı renk yapıyoruz
            dataGridView1.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.Black;

            // Satır renkleri
            dataGridView1.RowsDefaultCellStyle.BackColor = Color.White;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);

            // Seçili satır stili
            dataGridView1.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 130, 180);
            dataGridView1.RowsDefaultCellStyle.SelectionForeColor = Color.White;

            // Satır fontu ve hizalaması
            dataGridView1.RowsDefaultCellStyle.Font = new Font("Segoe UI", 8, FontStyle.Regular);
            dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // Hücre çizgilerini ince ve yatay-dikey olacak şekilde ayarla
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            dataGridView1.GridColor = Color.FromArgb(220, 220, 220);

            // DataGridView arka plan rengini beyaz yap
            dataGridView1.BackgroundColor = Color.White;

            // Sütun genişliklerini formun genişliğine göre otomatik ayarlamak için
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Sütun genişlik oranlarını ayarlayın
            if (dataGridView1.Columns.Count > 0)
            {       
                dataGridView1.Columns["TalepNo"].FillWeight = 7;  // Oranları belirleyebilirsiniz
                dataGridView1.Columns["TalepTarihi"].FillWeight = 15;
                dataGridView1.Columns["MusteriUnvani"].FillWeight = 25;
                dataGridView1.Columns["Tabancifti"].FillWeight = 10;
                dataGridView1.Columns["Kullanici"].FillWeight = 10;
                dataGridView1.Columns["Aciklama"].FillWeight = 33;
            }
        }

        private void SetDefaultDateRange()
        {
            dateTimePicker2.Value = DateTime.Today.AddDays(1); // Bugün tarihine bir gün ekle
            dateTimePicker1.Value = DateTime.Today.AddMonths(-6); // Son 6 ayı hesapla
        }

        private void RefreshData()
        {
            // 1) Güncelleme öncesi seçili TalepNo'yu yakala
            int? selectedTalepNo = null;
            if (dataGridView1.CurrentRow != null)
                selectedTalepNo = Convert.ToInt32(dataGridView1.CurrentRow.Cells["TalepNo"].Value);

            // 2) Duruma göre veriyi yükle
            if (radioButton1.Checked)
                LoadData("0");
            else if (radioButton2.Checked)
                LoadData("1");
            else if (radioButton3.Checked)
                LoadData("2");

            // 3) Seçili kalması gereken satırı bulup tekrar seç ve scroll’u ayarla
            if (selectedTalepNo.HasValue)
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (Convert.ToInt32(row.Cells["TalepNo"].Value) == selectedTalepNo.Value)
                    {
                        dataGridView1.ClearSelection();
                        row.Selected = true;
                        dataGridView1.CurrentCell = row.Cells["TalepNo"];
                        dataGridView1.FirstDisplayedScrollingRowIndex = row.Index;
                        break;
                    }
                }
            }
        }

        private void dataGridView1_CellDoubleClick(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Seçili satır yok.");
                return;
            }

            if (activeForm != null)
            {
                activeForm.Close();
            }

            DataRow dataRow = ((DataRowView)dataGridView1.CurrentRow.DataBoundItem).Row;
            MusteriTalepGiris talepGirisForm = new MusteriTalepGiris();
            talepGirisForm.LoadExistingData(dataRow);

            // DataUpdated olayına bir handler ekle
            talepGirisForm.DataUpdated += (s, args) => RefreshData();

            talepGirisForm.FormClosed += (s, args) => activeForm = null;
            activeForm = talepGirisForm;
            talepGirisForm.Show();
        }

        private void LoadData(string status)
        {
            DateTime startDate = dateTimePicker1.Value.Date;
            DateTime endDate = dateTimePicker2.Value.Date.AddDays(1); // Bitiş tarihine bir gün ekleyerek bugünü de dahil et

            string query = @"SELECT TH.TalepNo, TH.TalepTarihi, TH.MusteriUnvani, 
                        ISNULL(SUM(TD.Adet), 0) AS Tabancifti, 
                        TH.Aciklama, TH.Kullanici 
                 FROM TalepHeader TH 
                 LEFT JOIN TalepDetails TD ON TH.TalepNo = TD.TalepNo
                 WHERE TH.Durum = @Durum 
                 AND TalepTarihi >= @StartDate AND TalepTarihi < @EndDate
                 GROUP BY TH.TalepNo, TH.TalepTarihi, TH.MusteriUnvani, TH.Aciklama, TH.Kullanici";

            try
            {
                con.Open(); // Bağlantıyı aç
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    command.Parameters.AddWithValue("@Durum", status);
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate); // Eksik olan parametreler eklendi
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dataGridView1.DataSource = dataTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı hatası: " + ex.Message);
            }
            finally
            {
                con.Close(); // Bağlantıyı kapat
            }

            // CustomizeDataGridView'i çağırarak sütun genişliklerini ayarla
            CustomizeDataGridView();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                LoadData("0");
                button1_Click(sender, e);
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                LoadData("1");
                button1_Click(sender, e);
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                LoadData("2");
                button1_Click(sender, e);
            }
        }

        private void gidenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Eğer DataGridView'de seçili bir satır yoksa işlem yapma
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen bir satır seçiniz.");
                return;
            }

            // Kullanıcıya onay sorusu sor
            DialogResult result = MessageBox.Show("Giden taleplere taşımak istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Seçili satırdan TalepNo değerini al
                int talepNo = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["TalepNo"].Value);

                // SQL bağlantısını aç
                try
                {
                    con.Open();
                    string query = "UPDATE TalepHeader SET Durum = 1 WHERE TalepNo = @TalepNo";
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@TalepNo", talepNo);
                        int affectedRows = command.ExecuteNonQuery(); // Güncelleme sorgusunu çalıştır

                        if (affectedRows > 0)
                        {
                            MessageBox.Show("Giden taleplere taşındı.");
                        }
                        else
                        {
                            MessageBox.Show("Güncelleme yapılamadı. Lütfen tekrar deneyiniz.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Bir hata oluştu: " + ex.Message);
                }
                finally
                {
                    con.Close(); // SQL bağlantısını kapat
                }
            }
        }
        private void iptalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Eğer DataGridView'de seçili bir satır yoksa işlem yapma
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen bir satır seçiniz.");
                return;
            }

            // Kullanıcıya onay sorusu sor
            DialogResult result = MessageBox.Show("Bu talebi iptal etmek istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Seçili satırdan TalepNo değerini al
                int talepNo = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["TalepNo"].Value);

                // SQL bağlantısını aç
                try
                {
                    con.Open();
                    string query = "UPDATE TalepHeader SET Durum = 2 WHERE TalepNo = @TalepNo";
                    using (SqlCommand command = new SqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@TalepNo", talepNo);
                        int affectedRows = command.ExecuteNonQuery(); // Güncelleme sorgusunu çalıştır

                        if (affectedRows > 0)
                        {
                            MessageBox.Show("Talep iptal edildi.");
                        }
                        else
                        {
                            MessageBox.Show("Güncelleme yapılamadı. Lütfen tekrar deneyiniz.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Bir hata oluştu: " + ex.Message);
                }
                finally
                {
                    con.Close(); // SQL bağlantısını kapat
                }
            }
        }
        private void kopyalaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen bir satır seçiniz.");
                return;
            }

            if (activeForm != null)
            {
                activeForm.Close();
            }

            DataRowView currentRow = dataGridView1.SelectedRows[0].DataBoundItem as DataRowView;
            if (currentRow != null)
            {
                MusteriTalepGiris talepGirisForm = new MusteriTalepGiris
                {
                    HideButton6 = true // button6'yı gizleyecek şekilde ayarla
                };
                talepGirisForm.LoadExistingData(currentRow.Row);
                talepGirisForm.FormClosed += (s, args) => activeForm = null;
                activeForm = talepGirisForm;
                talepGirisForm.Show();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DateTime startDate = dateTimePicker1.Value.Date;
            DateTime endDate = dateTimePicker2.Value.Date.AddDays(1); // Bitiş tarihine bir gün ekleyerek bugünü de dahil et

            string query = @"SELECT TH.TalepNo, TH.TalepTarihi, TH.MusteriUnvani, 
                            ISNULL(SUM(TD.Adet), 0) AS Tabancifti, 
                            TH.Aciklama, TH.Kullanici 
                     FROM TalepHeader TH 
                     LEFT JOIN TalepDetails TD ON TH.TalepNo = TD.TalepNo
                     WHERE TH.Durum = @Durum 
                     AND TalepTarihi >= @StartDate AND TalepTarihi < @EndDate
                     GROUP BY TH.TalepNo, TH.TalepTarihi, TH.MusteriUnvani, TH.Aciklama, TH.Kullanici";

            try
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(query, con))
                {
                    command.Parameters.AddWithValue("@Durum", GetCurrentStatus());
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dataGridView1.DataSource = dataTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı hatası: " + ex.Message);
            }
            finally
            {
                con.Close();
            }
        }
        private string GetCurrentStatus()
        {
            if (radioButton1.Checked)
                return "0"; // Bekleyen talepler
            else if (radioButton2.Checked)
                return "1"; // Onaylanan talepler
            else if (radioButton3.Checked)
                return "2"; // İptal edilen talepler
            return "0"; // Varsayılan durum
        }

        private void TalepRaportoolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // Rapor dosyasının yolunu belirle
            string reportPath = "Rapor\\TalepRapor.frx";

            // FastReport rapor nesnesi oluştur
            Report report = new Report();

            try
            {
                // Rapor dosyasını yükle
                report.Load(reportPath);

                // Raporu göster
                report.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rapor görüntülenirken bir hata oluştu: " + ex.Message);
            }
            finally
            {
                // Rapor nesnesini temizle
                report.Dispose();
            }
        }
    }
}
