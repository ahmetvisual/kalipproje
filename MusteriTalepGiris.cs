using FastReport;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kalipproje
{
    public partial class MusteriTalepGiris : Form
    {
        public bool HideButton6 { get; set; } = false;
        private List<DataGridViewRow> newRows = new List<DataGridViewRow>();
        private List<int> deletedRowIDs = new List<int>();

        public MusteriTalepGiris()
        {
            InitializeComponent();
            InitializeDataGridView();
            this.dataGridView1.CellValueChanged += this.dataGridView1_CellValueChanged;
            this.dataGridView1.RowsAdded += this.dataGridView1_RowsAdded;
            textBox2.KeyPress += textBox2_KeyPress;
            textBox5.KeyDown += textBox5_KeyDown;
            this.FormClosing += MusteriTalepGiris_FormClosing;
            CustomizeDataGridView(); // Yeni eklenen DataGridView özelleştirmesi
            // Form yüklendiğinde button6'nın görünürlüğünü ayarlayacağız
            this.Load += (s, e) =>
            {
                button6.Visible = !HideButton6;
                button5.Visible = HideButton6;
                textBox2.Focus();
            };
        }

        private int currentTalepNo; // Talep numarasını tutacak alan
        private Dictionary<DataGridViewRow, bool> newRowStates = new Dictionary<DataGridViewRow, bool>();

        private void DataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            for (int i = 0; i < e.RowCount; i++)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex + i];
                if (!row.IsNewRow)
                {
                    newRowStates[row] = true;  // Yeni eklenen satır olarak işaretle
                }
            }
        }

        private void DataGridView1_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            // Silinen satırın ID'sini listeye ekle
            if (e.Row.Cells["DetailID"].Value != null)
            {
                int detailID = Convert.ToInt32(e.Row.Cells["DetailID"].Value);
                deletedRowIDs.Add(detailID);
            }
        }

        public void LoadExistingData(DataRow data)
        {
            textBox5.Text = data["MusteriUnvani"].ToString();
            richTextBox1.Text = data["Aciklama"].ToString();
            dateTimePicker1.Value = Convert.ToDateTime(data["TalepTarihi"]);
            textBox3.Text = data["TalepNo"].ToString();  // TalepNo değerini textBox3'e yükle

            currentTalepNo = Convert.ToInt32(data["TalepNo"]); // currentTalepNo'yu set et
            LoadTalepDetails(currentTalepNo); // Detayları yükle

            button5.Visible = false; // Ekleme butonunu gizle
            button6.Visible = true;  // Güncelleme butonunu göster
        }

        private void InitializeDataGridView()
        {
            dataGridView1.ColumnCount = 5;
            dataGridView1.Columns[0].Name = "Barkod";
            dataGridView1.Columns[0].Visible = false;
            dataGridView1.Columns[1].Name = "ModelKodu";
            dataGridView1.Columns[2].Name = "TabanKodu";
            dataGridView1.Columns[3].Name = "Adet";
            dataGridView1.Columns[4].Name = "Aciklama";

            DataGridViewTextBoxColumn detailIDColumn = new DataGridViewTextBoxColumn
            {
                Name = "DetailID",
                HeaderText = "DetailID",
                Visible = false
            };
            dataGridView1.Columns.Add(detailIDColumn);

            DataGridViewCheckBoxColumn chk = new DataGridViewCheckBoxColumn
            {
                HeaderText = "Verildi",
                Name = "chkVerildi"
            };
            dataGridView1.Columns.Add(chk);

            // Varsayılan GridView Tasarımı
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AllowUserToAddRows = false;
        }

        private void CustomizeDataGridView()
        {
            // Mevcut başlık ve satır stil kodlarınız aynen kalsın...
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(200, 200, 200);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8, FontStyle.Regular);
            dataGridView1.ColumnHeadersDefaultCellStyle.SelectionBackColor = dataGridView1.ColumnHeadersDefaultCellStyle.BackColor;
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.ColumnHeadersHeight = 30;

            dataGridView1.RowsDefaultCellStyle.BackColor = Color.White;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            dataGridView1.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 130, 180);
            dataGridView1.RowsDefaultCellStyle.SelectionForeColor = Color.White;

            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            dataGridView1.GridColor = Color.FromArgb(220, 220, 220);

            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.BackgroundColor = Color.White;

            // 1) Önce sütunları Fill moduna al:
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // 2) Diğer sütunları sabit genişlikte bırak:
            if (dataGridView1.Columns["ModelKodu"] != null)
            {
                dataGridView1.Columns["ModelKodu"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridView1.Columns["ModelKodu"].Width = 70;
            }
            if (dataGridView1.Columns["TabanKodu"] != null)
            {
                dataGridView1.Columns["TabanKodu"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridView1.Columns["TabanKodu"].Width = 75;
            }
            if (dataGridView1.Columns["Adet"] != null)
            {
                dataGridView1.Columns["Adet"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridView1.Columns["Adet"].Width = 35;
            }
            if (dataGridView1.Columns["Aciklama"] != null)
            {
                dataGridView1.Columns["Aciklama"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dataGridView1.Columns["Aciklama"].Width = 241;
            }
            // DetailID sütunu görünmez kaldığı için atlıyoruz.

            // 3) “Verildi” sütununu kalan alana yay:
            if (dataGridView1.Columns["chkVerildi"] != null)
            {
                dataGridView1.Columns["chkVerildi"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "chk")
            {
                DataGridViewCheckBoxCell chk = (DataGridViewCheckBoxCell)dataGridView1.Rows[e.RowIndex].Cells["chk"];
                bool isChecked = (bool)chk.Value;
                int modelId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["ModelId"].Value);  // ModelId değeri varsayılan olarak satırlarınıza eklenmelidir.

                UpdateVerildiStatus(modelId, isChecked);
            }
        }
        private void UpdateVerildiStatus(int modelId, bool verildi)
        {
            string query = "UPDATE TalepHeader SET Verildi = @Verildi WHERE ModelId = @ModelId";
            using (SqlConnection con = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Verildi", verildi);
                cmd.Parameters.AddWithValue("@ModelId", modelId);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
        }
        private void ClearForm()
        {
            // Tekst kutularını temizle
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is TextBox)
                {
                    ((TextBox)ctrl).Clear();
                }
            }
            richTextBox1.Clear();
            dateTimePicker1.Value = DateTime.Now;
            dataGridView1.Rows.Clear();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            // Zorunlu alan kontrolleri
            if (string.IsNullOrWhiteSpace(textBox5.Text))
            {
                MessageBox.Show("Müşteri Ünvanı zorunludur.", "Eksik Bilgi",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("En az bir satır eklenmelidir.", "Eksik Bilgi",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Header ekle ve detayları yaz
            InsertTalepHeader();      // İçinde InsertTalepDetails de çağrılıyor
                                      // Yeniden yükle
            LoadTalepDetails(currentTalepNo);
            // Değişiklikler kaydedildi
            IsDirty = false;
            // Form açık kalsın, kullanıcı detay görsün
        }

        private void InsertTalepHeader()
        {
            string query = "INSERT INTO TalepHeader (TalepTarihi, Aciklama, MusteriUnvani, Durum) OUTPUT INSERTED.TalepNo VALUES (@TalepTarihi, @Aciklama, @MusteriUnvani, 0)";
            var parameters = new Dictionary<string, object>
            {
                {"@TalepTarihi", dateTimePicker1.Value},
                {"@Aciklama", richTextBox1.Text},
                {"@MusteriUnvani", textBox5.Text}
            };

            currentTalepNo = Convert.ToInt32(ExecuteScalar(query, parameters));
            InsertTalepDetails();
        }

        private void InsertTalepDetails()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow)
                {
                    var parameters = new Dictionary<string, object>
            {
                {"@TalepNo", currentTalepNo},
                {"@ModelKodu", row.Cells["ModelKodu"].Value},
                {"@TabanKodu", row.Cells["TabanKodu"].Value},
                {"@Adet", row.Cells["Adet"].Value},
                {"@Verildi", row.Cells["chkVerildi"].Value},
                {"@Aciklama", row.Cells["Aciklama"].Value}  // Aciklama alanını ekle
            };
                    ExecuteDatabaseAction("INSERT INTO TalepDetails (TalepNo, ModelKodu, TabanKodu, Adet, Verildi, Aciklama) VALUES (@TalepNo, @ModelKodu, @TabanKodu, @Adet, @Verildi, @Aciklama)", parameters);
                }
            }
            MessageBox.Show("Kayıt başarılı.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearForm();
        }

        public void LoadTalepDetails(int talepNo)
        {
            string query = "SELECT DetailID, ModelKodu, TabanKodu, Adet, Verildi, Aciklama FROM TalepDetails WHERE TalepNo = @TalepNo";
            dataGridView1.Rows.Clear(); // Mevcut satırları temizle

            using (SqlConnection con = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@TalepNo", talepNo);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int index = dataGridView1.Rows.Add();
                        dataGridView1.Rows[index].Cells["DetailID"].Value = reader["DetailID"];
                        dataGridView1.Rows[index].Cells["ModelKodu"].Value = reader["ModelKodu"];
                        dataGridView1.Rows[index].Cells["TabanKodu"].Value = reader["TabanKodu"];
                        dataGridView1.Rows[index].Cells["Adet"].Value = reader["Adet"];
                        dataGridView1.Rows[index].Cells["chkVerildi"].Value = reader["Verildi"];
                        dataGridView1.Rows[index].Cells["Aciklama"].Value = reader["Aciklama"];  // Aciklama alanını doldur
                    }
                }
            }
        }

        private bool IsDirty = false;
        private bool isCellValueChanging = false; // Hücre değeri değiştiriliyor mu?
        private int totalRowsAdded;

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (!isCellValueChanging)
            {
                isCellValueChanging = true; // Değişiklik işlemi başladı
                // Değişiklikle ilgili işlemleriniz burada yapılır
                isCellValueChanging = false; // Değişiklik işlemi tamamlandı
            }
        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            // Sadece gerçekten yeni eklenen satırlar için satır sayısını arttır
            if (!dataGridView1.Rows[e.RowIndex].IsNewRow)
            {
                // İşlemlerinizi burada yapın, örneğin:
                totalRowsAdded += e.RowCount; // Eklenecek her satır için toplam eklenen satır sayısını güncelle
            }
        }

        private void ExecuteDatabaseAction(string query, Dictionary<string, object> parameters)
        {
            using (SqlConnection con = DatabaseHelper.GetConnection())
            using (var command = new SqlCommand(query, con))
            {
                try
                {
                    // Parametreleri komuta ekle
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }

                    // Bağlantıyı aç
                    con.Open();

                    // SQL komutunu çalıştır
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // Hata mesajını göster
                    MessageBox.Show("Veritabanı işlemi sırasında bir hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private object ExecuteScalar(string query, Dictionary<string, object> parameters)
        {
            using (SqlConnection con = DatabaseHelper.GetConnection())
            using (var command = new SqlCommand(query, con))
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
                con.Open();
                return command.ExecuteScalar();
            }
        }

        private void MusteriTalepGiris_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsDirty)
            {
                var result = MessageBox.Show("Değişiklikler kaydedilmedi. Kaydetmek istiyor musunuz?", "Kaydet", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    // Kaydetme işlemini gerçekleştir
                    SaveChanges();
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true; // Form kapanmasını engelle
                }
            }
        }
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13) // Enter tuşuna basıldığında
            {
                string inputText = textBox2.Text.Trim();
                if (string.IsNullOrEmpty(inputText))
                {
                    MessageBox.Show("Giriş boş olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string modelKodu = null, tabanKodu = null;

                // Girişin sayısal olup olmadığını kontrol et
                bool isNumeric = inputText.All(char.IsDigit);

                // Eğer giriş sayısal, 8 haneli ve 2 ile başlıyorsa
                if (isNumeric && inputText.Length == 8 && inputText.StartsWith("2"))
                {
                    // Barkod olarak kontrol ediyoruz
                    modelKodu = GetModelKoduFromSiparisHeaderByBarkod(inputText);
                    if (!string.IsNullOrEmpty(modelKodu))
                    {
                        tabanKodu = GetKalipKoduFromSiparisHeaderByBarkod(inputText);
                    }
                    else
                    {
                        MessageBox.Show("Girilen barkod SiparisHeader tablosunda bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        textBox2.Clear();
                        return;
                    }
                }
                else if (isNumeric && inputText.Length == 7 && inputText.StartsWith("1"))
                {
                    // 1 ile başlayan 7 haneli sayılar için ModelHeader tablosunu kontrol et
                    modelKodu = GetModelKoduFromModelHeaderByBarkod(inputText); // Barkoda göre ModelHeader'dan sorguluyoruz
                    if (!string.IsNullOrEmpty(modelKodu))
                    {
                        // ModelKodu'nu SiparisHeader tablosunda kontrol ediyoruz
                        tabanKodu = GetKalipKoduFromSiparisHeaderByModelKodu(modelKodu);
                        if (string.IsNullOrEmpty(tabanKodu))
                        {
                            // KalipKodu yoksa sadece ModelKodu ekle
                            tabanKodu = null; // TabanKodu boş bırakılacak
                        }
                    }
                    else
                    {
                        MessageBox.Show("Girilen barkod ModelHeader tablosunda bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        textBox2.Clear();
                        return;
                    }
                }
                else
                {
                    // Diğer mantık aynı kalıyor
                    modelKodu = GetModelKoduFromSiparisHeaderByKalipKodu(inputText);
                    if (!string.IsNullOrEmpty(modelKodu))
                    {
                        tabanKodu = inputText; // KalipKodu, TabanKodu'nu temsil eder
                    }
                    else
                    {
                        // KalipKodu bulunamadı, hata ver ve işlemi sonlandır
                        MessageBox.Show("Kod bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        textBox2.Clear();
                        return;
                    }
                }

                // Şimdi mevcut satırları kontrol ediyoruz
                bool isNewEntry = true;
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow)
                        continue;

                    string existingModelKodu = row.Cells["ModelKodu"].Value?.ToString();
                    string existingTabanKodu = row.Cells["TabanKodu"].Value?.ToString();

                    // Eğer ModelKodu ve TabanKodu eşleşiyorsa
                    if (string.Equals(existingModelKodu, modelKodu, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(existingTabanKodu ?? "", tabanKodu ?? "", StringComparison.OrdinalIgnoreCase))
                    {
                        int currentCount = Convert.ToInt32(row.Cells["Adet"].Value ?? "0");
                        row.Cells["Adet"].Value = currentCount + 1;
                        isNewEntry = false;
                        dataGridView1.Refresh();
                        break;
                    }
                }

                if (isNewEntry)
                {
                    AddRowToDataGridView(inputText, modelKodu, tabanKodu);
                }

                textBox2.Clear(); // Metin kutusunu temizle
            }
        }

        private void AddRowToDataGridView(string barkod, string modelKodu, string tabanKodu)
        {
            int index = dataGridView1.Rows.Add();
            dataGridView1.Rows[index].Cells["Barkod"].Value = barkod;
            dataGridView1.Rows[index].Cells["ModelKodu"].Value = modelKodu;
            dataGridView1.Rows[index].Cells["TabanKodu"].Value = tabanKodu; // KalipKodu, TabanKodu olarak alınıyor
            dataGridView1.Rows[index].Cells["Adet"].Value = 1;
            dataGridView1.Rows[index].Cells["Aciklama"].Value = "";
            dataGridView1.Rows[index].Cells["chkVerildi"].Value = false;
            dataGridView1.Rows[index].Cells["DetailID"].Value = DBNull.Value;
            dataGridView1.Refresh();
        }
        private string GetModelKoduFromModelHeaderByBarkod(string barkod)
        {
            string modelKodu = null;
            string query = "SELECT ModelKodu FROM ModelHeader WHERE Barkod = @Barkod";
            using (SqlConnection con = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Barkod", barkod);
                con.Open();
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    modelKodu = result.ToString();
                }
            }
            return modelKodu;
        }

        private string GetKalipKoduFromSiparisHeaderByModelKodu(string modelKodu)
        {
            string kalipKodu = null;
            string query = "SELECT KalipKodu FROM SiparisHeader WHERE ModelKodu = @ModelKodu";
            using (SqlConnection con = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@ModelKodu", modelKodu);
                con.Open();
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    kalipKodu = result.ToString();
                }
            }
            return kalipKodu;
        }

        private string GetModelKoduFromSiparisHeaderByBarkod(string barkod)
        {
            string modelKodu = null;

            // Barkod doğrudan NVARCHAR olarak sorgulanıyor
            string query = "SELECT ModelKodu FROM SiparisHeader WHERE Barkod = @Barkod";
            using (SqlConnection con = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                // Barkodu doğrudan string olarak ele alıyoruz
                cmd.Parameters.AddWithValue("@Barkod", barkod);

                con.Open();
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    modelKodu = result.ToString();
                }
            }
            return modelKodu;
        }

        private string GetKalipKoduFromSiparisHeaderByBarkod(string barkod)
        {
            string kalipKodu = null;
            string query = "SELECT KalipKodu FROM SiparisHeader WHERE Barkod = @Barkod";
            using (SqlConnection con = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Barkod", barkod);
                con.Open();
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    kalipKodu = result.ToString();
                }
                else
                {
                    MessageBox.Show("KalipKodu bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return kalipKodu;
        }

        private string GetModelKoduFromSiparisHeaderByKalipKodu(string kalipKodu)
        {
            string modelKodu = null;
            string query = "SELECT TOP 1 ModelKodu FROM SiparisHeader WHERE KalipKodu = @KalipKodu ORDER BY SiparisTarihi DESC";
            using (SqlConnection con = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@KalipKodu", kalipKodu);
                con.Open();
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    modelKodu = result.ToString();
                }
            }
            return modelKodu;
        }

        private void textBox5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string initialFilter = textBox5.Text.Trim();
                using (var cariListForm = new CariList(initialFilter))
                {
                    var dialogResult = cariListForm.ShowDialog();

                    if (dialogResult == DialogResult.OK)
                    {
                        textBox5.Text = cariListForm.SelectedCariUnvani; // Seçilen cari ünvanını güncelle
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SaveChanges()
        {
            try
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open(); // Veritabanı bağlantısını aç
                    using (SqlTransaction transaction = con.BeginTransaction())
                    using (SqlCommand command = con.CreateCommand())
                    {
                        command.Transaction = transaction;

                        // TalepHeader güncellemesi
                        command.CommandText = "UPDATE TalepHeader SET TalepTarihi=@TalepTarihi, Aciklama=@Aciklama, MusteriUnvani=@MusteriUnvani WHERE TalepNo=@TalepNo";
                        command.Parameters.AddWithValue("@TalepTarihi", dateTimePicker1.Value);
                        command.Parameters.AddWithValue("@Aciklama", richTextBox1.Text);
                        command.Parameters.AddWithValue("@MusteriUnvani", textBox5.Text);
                        command.Parameters.AddWithValue("@TalepNo", currentTalepNo);
                        command.ExecuteNonQuery();

                        // Detay işleme
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                if (row.Cells["DetailID"].Value == DBNull.Value)
                                {
                                    // Yeni detay satırı ekle
                                    command.CommandText = "INSERT INTO TalepDetails (TalepNo, ModelKodu, TabanKodu, Adet, Verildi) VALUES (@TalepNo, @ModelKodu, @TabanKodu, @Adet, @Verildi)";
                                    command.Parameters.Clear();
                                    command.Parameters.AddWithValue("@TalepNo", currentTalepNo);
                                    command.Parameters.AddWithValue("@ModelKodu", row.Cells["ModelKodu"].Value ?? DBNull.Value);
                                    command.Parameters.AddWithValue("@TabanKodu", row.Cells["TabanKodu"].Value ?? DBNull.Value);
                                    command.Parameters.AddWithValue("@Adet", row.Cells["Adet"].Value);
                                    command.Parameters.AddWithValue("@Verildi", Convert.ToBoolean(row.Cells["chkVerildi"].Value));
                                    command.ExecuteNonQuery();
                                }
                                else
                                {
                                    // Mevcut detay satırı güncelle
                                    command.CommandText = "UPDATE TalepDetails SET ModelKodu=@ModelKodu, TabanKodu=@TabanKodu, Adet=@Adet, Verildi=@Verildi, Aciklama=@Aciklama WHERE DetailID=@DetailID";
                                    command.Parameters.Clear();
                                    command.Parameters.AddWithValue("@DetailID", row.Cells["DetailID"].Value);
                                    command.Parameters.AddWithValue("@ModelKodu", row.Cells["ModelKodu"].Value ?? DBNull.Value);
                                    command.Parameters.AddWithValue("@TabanKodu", row.Cells["TabanKodu"].Value ?? DBNull.Value);
                                    command.Parameters.AddWithValue("@Adet", row.Cells["Adet"].Value);
                                    command.Parameters.AddWithValue("@Verildi", Convert.ToBoolean(row.Cells["chkVerildi"].Value));
                                    command.Parameters.AddWithValue("@Aciklama", row.Cells["Aciklama"].Value ?? DBNull.Value);  // Aciklama güncelle
                                    command.ExecuteNonQuery();
                                }
                            }
                        }

                        // Silinen detay satırları kaldır
                        foreach (int id in deletedRowIDs)
                        {
                            command.CommandText = "DELETE FROM TalepDetails WHERE DetailID=@DetailID";
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@DetailID", id);
                            command.ExecuteNonQuery();
                        }
                        deletedRowIDs.Clear();

                        transaction.Commit();
                    }
                }

                MessageBox.Show("Değişiklikler başarıyla kaydedildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                IsDirty = false; // Değişiklikler kaydedildiği için IsDirty bayrağını sıfırla
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı işlemi sırasında bir hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public delegate void DataUpdatedEventHandler(object sender, EventArgs e);
        public event DataUpdatedEventHandler DataUpdated;

        protected virtual void OnDataUpdated()
        {
            DataUpdated?.Invoke(this, EventArgs.Empty);  // Tüm bağlı olay dinleyicilerini tetikle
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // Değişiklikleri DB'ye yaz
            SaveChanges();            // Commit, güncellemeler vs.
                                      // Listeyi yenilemesi için parent form’a bildir
            OnDataUpdated();
            // Detayları tekrar ekranda göster
            LoadTalepDetails(currentTalepNo);
            // Değişiklik bayrağını sıfırla
            IsDirty = false;
            // Form açık kalsın, kullanıcı güncellenmiş veriyi görsün
        }

        private void gidenToolStripMenuItem_Click(object sender, EventArgs e) // Sağ tık satır silme işlemi
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                {
                    if (!row.IsNewRow)
                    {
                        // Silinen satırın ID'sini listeye ekle
                        if (row.Cells["DetailID"].Value != null && row.Cells["DetailID"].Value != DBNull.Value)
                        {
                            int detailID = Convert.ToInt32(row.Cells["DetailID"].Value);
                            deletedRowIDs.Add(detailID);
                        }
                        dataGridView1.Rows.RemoveAt(row.Index);  // Satırı DataGridView'den kaldır
                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir satır seçin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private DataTable GetTalepDataForReport(int talepNo)
        {
            DataTable dt = new DataTable();
            string query = @"SELECT 
                        TH.TalepNo, 
                        TH.TalepTarihi, 
                        TH.Aciklama AS MasterAciklama, 
                        TH.MusteriUnvani, 
                        TD.ModelKodu, 
                        TD.TabanKodu, 
                        TD.Adet, 
                        TD.Verildi, 
                        TD.Aciklama AS DetailAciklama 
                    FROM TalepHeader TH
                    JOIN TalepDetails TD ON TH.TalepNo = TD.TalepNo
                    WHERE TH.TalepNo = @TalepNo";

            using (SqlConnection con = DatabaseHelper.GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@TalepNo", talepNo);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
            }

            return dt;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // TalepHeader ve TalepDetails tablosundan TalepNo'ya ait tüm satırları alalım
            DataTable dtForReport = GetTalepDataForReport(currentTalepNo);

            if (dtForReport.Rows.Count > 0)
            {
                // Report nesnesini kullanarak rapor işlemlerini yapıyoruz
                using (var report = new Report())
                {
                    try
                    {
                        // Raporu yüklüyoruz
                        report.Load(@"Rapor\TalepRapor2.frx");

                        // Rapor için veriyi bağlıyoruz
                        report.RegisterData(dtForReport, "taleprapor");

                        // Raporu hazırlıyoruz
                        report.Prepare();

                        // Hazırlanan raporu gösteriyoruz
                        report.ShowPrepared();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Rapor yükleme hatası: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Bu talep numarasına ait herhangi bir detay bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

    }
}
