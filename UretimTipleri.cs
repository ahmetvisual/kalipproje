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
    public partial class UretimTipleri : Form
    {
        private DataTable dt = new DataTable();
        private DataView dv; // DataView, filtreleme için kullanılacak.
        public UretimTipleri()
        {
            InitializeComponent();
            CustomizeDataGridView();
            FillDataGridView();
            this.StartPosition = FormStartPosition.CenterScreen;
            dataGridView1.CellDoubleClick += new DataGridViewCellEventHandler(dataGridView1_CellDoubleClick);
            // textBox5 için TextChanged event'ini bağla
            textBox5.TextChanged += new EventHandler(textBox5_TextChanged);
        }

        private void CustomizeDataGridView()
        {
            // Checkbox sütunu ekleme
            DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn();
            checkBoxColumn.HeaderText = "Seç";
            checkBoxColumn.Name = "checkBoxColumn";
            checkBoxColumn.Width = 70; // "Seç" sütununun genişliğini 70 olarak ayarlama
            checkBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None; // Otomatik boyutlandırmayı devre dışı bırakma
            dataGridView1.Columns.Insert(0, checkBoxColumn);

            // Diğer tasarım özellikleri
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.Navy;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Regular);
            dataGridView1.RowsDefaultCellStyle.BackColor = Color.LightYellow;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.White;
            dataGridView1.RowsDefaultCellStyle.SelectionBackColor = Color.DarkGoldenrod;
            dataGridView1.RowsDefaultCellStyle.SelectionForeColor = Color.White;
            dataGridView1.RowsDefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Regular);

            // Satır seçici sütununu ve boş satırları kaldırma
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AllowUserToAddRows = false;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Eğer geçerli bir satırın üzerine tıklanırsa, tüm satırı seç
            if (e.RowIndex >= 0)
            {
                dataGridView1.ClearSelection();
                dataGridView1.Rows[e.RowIndex].Selected = true;
            }
        }

        private int selectedID = -1; // Form seviyesinde bir değişken olarak
        private int selectedRecordID = -1;

        private void FillDataGridView()
        {
            // Mevcut filtre metnini al
            string filterText = textBox5.Text.Trim();

            // DataTable'ı temizle ve veriyi yeniden çek
            dt.Clear();
            string query = "SELECT ID, UretimTanimi, OnKod, ModelKontrol, KodAraligi FROM UretimTipHeader";
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                SqlDataAdapter adapter = new SqlDataAdapter(query, con);
                adapter.Fill(dt);
            }

            // DataView oluştur ve filtre uygula (varsa)
            dv = new DataView(dt);
            if (!string.IsNullOrEmpty(filterText))
                dv.RowFilter = $"UretimTanimi LIKE '%{filterText}%'";

            // Grid’e ata
            dataGridView1.DataSource = dv;
            dataGridView1.Columns["ID"].Visible = false;

            // Sütun genişlikleri
            foreach (DataGridViewColumn col in dataGridView1.Columns)
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            // Son görünür sütunu Fill moduna al
            for (int i = dataGridView1.Columns.Count - 1; i >= 0; i--)
            {
                if (dataGridView1.Columns[i].Visible)
                {
                    dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    break;
                }
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            string filterText = textBox5.Text.Trim(); // TextBox'taki değeri al ve boşlukları temizle

            if (!string.IsNullOrEmpty(filterText))
            {
                // Filtreleme işlemi, UretimTanimi sütununda arama yapar
                dv.RowFilter = $"UretimTanimi LIKE '%{filterText}%'";
            }
            else
            {
                // Eğer TextBox boşsa filtreyi temizle
                dv.RowFilter = string.Empty;
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            bool isUpdate = selectedRecordID != -1;
            int idToReSelect = selectedRecordID;  // güncellediğimiz kaydın ID'si

            if (!isUpdate)
            {
                // Yeni kayıt ekleme
                string insertQuery = @"
            INSERT INTO UretimTipHeader
            (UretimTanimi, OnKod, ModelKontrol, KodAraligi)
            VALUES (@UretimTanimi, @OnKod, @ModelKontrol, @KodAraligi)";
                using (SqlConnection con = DatabaseHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                {
                    cmd.Parameters.AddWithValue("@UretimTanimi", textBox1.Text);
                    cmd.Parameters.AddWithValue("@OnKod", textBox2.Text);
                    cmd.Parameters.AddWithValue("@ModelKontrol", textBox3.Text);
                    cmd.Parameters.AddWithValue("@KodAraligi", textBox4.Text);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Kayıt başarıyla eklendi.", "Bilgi",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Mevcut kaydı güncelleme
                string updateQuery = @"
            UPDATE UretimTipHeader
            SET UretimTanimi = @UretimTanimi,
                OnKod        = @OnKod,
                ModelKontrol = @ModelKontrol,
                KodAraligi   = @KodAraligi
            WHERE ID = @ID";
                using (SqlConnection con = DatabaseHelper.GetConnection())
                using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                {
                    cmd.Parameters.AddWithValue("@UretimTanimi", textBox1.Text);
                    cmd.Parameters.AddWithValue("@OnKod", textBox2.Text);
                    cmd.Parameters.AddWithValue("@ModelKontrol", textBox3.Text);
                    cmd.Parameters.AddWithValue("@KodAraligi", textBox4.Text);
                    cmd.Parameters.AddWithValue("@ID", selectedRecordID);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Kayıt başarıyla güncellendi.", "Bilgi",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // 1) Grid'i yeniden çek (filtre FillDataGridView içinde tekrar uygulanıyor)
            FillDataGridView();

            // 2) Eğer güncelleme yaptıysak, aynı kaydı yeniden seç ve scroll’u ayarla
            if (isUpdate)
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if ((int)dataGridView1.Rows[i].Cells["ID"].Value == idToReSelect)
                    {
                        dataGridView1.ClearSelection();
                        dataGridView1.Rows[i].Selected = true;
                        // Aktif hücreyi de o satıra taşı ki imleç orada kalsın
                        dataGridView1.CurrentCell = dataGridView1.Rows[i].Cells["UretimTanimi"];
                        // Görünür pencereyi de o satıra kaydır
                        dataGridView1.FirstDisplayedScrollingRowIndex = i;
                        break;
                    }
                }
            }

            // 3) Formu sıfırla, mod’u (yeni kayıt/güncelleme) resetle
            ClearForm();
            selectedRecordID = -1;
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                textBox1.Text = row.Cells["UretimTanimi"].Value.ToString();
                textBox2.Text = row.Cells["OnKod"].Value.ToString();
                textBox3.Text = row.Cells["ModelKontrol"].Value.ToString();
                textBox4.Text = row.Cells["KodAraligi"].Value.ToString();
                selectedRecordID = Convert.ToInt32(row.Cells["ID"].Value); // Seçilen kaydın ID'sini sakla
            }
        }

        private void ClearForm()
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Kullanıcıdan onay alma
            var onay = MessageBox.Show("Seçili satırları silmek istediğinizden emin misiniz?", "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (onay == DialogResult.Yes)
            {
                // Kullanıcı evet dediyse, seçili satırları sil
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (Convert.ToBoolean(row.Cells["checkBoxColumn"].Value))
                    {
                        using (SqlConnection con = DatabaseHelper.GetConnection())
                        {
                            using (SqlCommand cmd = new SqlCommand("DELETE FROM UretimTipHeader WHERE UretimTanimi = @UretimTanimi", con))
                            {
                                cmd.Parameters.AddWithValue("@UretimTanimi", row.Cells["UretimTanimi"].Value.ToString());

                                con.Open();
                                cmd.ExecuteNonQuery();
                                con.Close();
                            }
                        }
                    }
                }

                FillDataGridView();
            }
        }
    }
}
