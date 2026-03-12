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
    public partial class CariKartListesi : Form
    {
        public CariKartListesi()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.CariKartListesi_Load);
            textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged); // Bu satırı ekleyin
            this.dataGridView1.CellDoubleClick += new DataGridViewCellEventHandler(this.dataGridView_CellDoubleClick);
            this.dataGridView2.CellDoubleClick += new DataGridViewCellEventHandler(this.dataGridView_CellDoubleClick);
        }
        // Form seviyesinde değişkenler
        private bool isUpdate = false;
        private int selectedMusteriID = 0;

        private void dataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // sender üzerinden hangi DataGridView'dan geldiğini anlıyoruz.
                DataGridView dgv = sender as DataGridView;
                if (dgv != null)
                {
                    DataGridViewRow row = dgv.Rows[e.RowIndex];
                    UpdateFormFields(row); // Sadece ilgili satırı parametre olarak gönderiyoruz.
                }
            }
        }

        private void UpdateFormFields(DataGridViewRow row)
        {
            // Bu metotta, ilgili satırın bilgilerini kullanarak güncelleme yapıyoruz.
            // Hangi DataGridView'dan geldiğine bağlı bir işlem yapmıyorsak, bu yeterli olacaktır.
            selectedMusteriID = Convert.ToInt32(row.Cells["MusteriID"].Value);

            foreach (var item in comboBox2.Items)
            {
                if (item.ToString() == row.Cells["CariTipi"].Value.ToString())
                {
                    comboBox2.SelectedItem = item;
                    break;
                }
            }

            textBox3.Text = row.Cells["CariUnvani"].Value.ToString();
            isUpdate = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                string query;
                if (isUpdate)
                {
                    // Güncelleme işlemi
                    query = "UPDATE CariKartHeader SET CariTipi = @CariTipi, CariUnvani = @CariUnvani WHERE MusteriID = @MusteriID";
                }
                else
                {
                    // Yeni kayıt ekleme
                    query = "INSERT INTO CariKartHeader (CariTipi, CariUnvani) VALUES (@CariTipi, @CariUnvani)";
                }

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CariTipi", comboBox2.SelectedItem.ToString());
                    command.Parameters.AddWithValue("@CariUnvani", textBox3.Text);
                    if (isUpdate)
                    {
                        command.Parameters.AddWithValue("@MusteriID", selectedMusteriID);
                    }

                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show(isUpdate ? "Kayıt güncellendi." : "Yeni kayıt eklendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Her iki DataGridView'yi de güncelleyin.
                        UpdateDataGridView(dataGridView1, comboBox2.Items[0].ToString());
                        UpdateDataGridView(dataGridView2, comboBox2.Items[1].ToString());

                        // Kontrolleri temizle
                        comboBox2.SelectedIndex = -1;
                        textBox3.Clear();

                        // Güncelleme modunu ve seçilen kaydın ID'sini sıfırlayın
                        isUpdate = false;
                        selectedMusteriID = 0;
                    }
                    else
                    {
                        MessageBox.Show("İşlem başarısız.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

        }

        private void comboBox2_SelectionChangeCommitted(object sender, EventArgs e)
        {
            // Her seçim değişikliğinde, ilgili DataGridView'i güncelleyin.
            UpdateDataGridView(dataGridView1, comboBox2.Items[0].ToString());
            UpdateDataGridView(dataGridView2, comboBox2.Items[1].ToString());
        }

        private void UpdateDataGridView(DataGridView dataGridView, string cariTipi)
        {
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                string query = "SELECT MusteriID, CariTipi, CariUnvani FROM CariKartHeader WHERE CariTipi = @CariTipi";
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                {
                    adapter.SelectCommand.Parameters.AddWithValue("@CariTipi", cariTipi);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dataGridView.DataSource = dataTable;
                }
            }
        }

        private void CariKartListesi_Load(object sender, EventArgs e)
        {
            CustomizeDataGridView(dataGridView1);
            CustomizeDataGridView(dataGridView2);

            // Form açıldığında her iki DataGridView kontrolü için verileri yükle
            UpdateDataGridView(dataGridView1, comboBox2.Items[0].ToString());
            UpdateDataGridView(dataGridView2, comboBox2.Items[1].ToString());
        }

        private void CustomizeDataGridView(DataGridView dgv)
        {
            // Sütun başlıklarının arkaplan rengini altın sarısı olarak ayarla
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 223, 0);
            // Sütun başlıklarının yazı rengini siyah olarak ayarla
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            // Sütun başlıklarının fontunu ayarla
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold);

            // Satırların varsayılan arkaplan rengini açık sarı olarak ayarla
            dgv.RowsDefaultCellStyle.BackColor = Color.LightYellow;
            // Alternatif satırların arkaplan rengini beyaz olarak ayarla
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.White;
            // Seçili satırların arkaplan rengini koyu altın çubuk olarak ayarla
            dgv.RowsDefaultCellStyle.SelectionBackColor = Color.DarkGoldenrod;
            // Seçili satırların yazı rengini beyaz olarak ayarla
            dgv.RowsDefaultCellStyle.SelectionForeColor = Color.White;
            // Satırların fontunu ayarla
            dgv.RowsDefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Regular);

            // Otomatik sütun boyutlandırmayı etkinleştir
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Kullanıcı tarafından satır ve sütun boyutlarının değiştirilmesini engelle
            dgv.AllowUserToResizeRows = false;
            dgv.AllowUserToResizeColumns = false;

            // Kullanıcı tarafından yeni satır eklenmesini engelle
            dgv.AllowUserToAddRows = false;

            // Sütunları içeriklerine göre otomatik olarak sığdır
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

        }
        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            FilterDataGridView(dataGridView1, textBox2.Text);
            FilterDataGridView(dataGridView2, textBox2.Text);
        }

        private void FilterDataGridView(DataGridView dgv, string filterValue)
        {
            // DataTable olarak varsayıyorum. Eğer veri kaynağınız farklı bir türdeyse, uygun şekilde dönüştürün.
            if (dgv.DataSource is DataTable)
            {
                DataTable dt = (DataTable)dgv.DataSource;
                dt.DefaultView.RowFilter = string.Format("CariUnvani LIKE '%{0}%'", filterValue);
            }
        }


    }
}
