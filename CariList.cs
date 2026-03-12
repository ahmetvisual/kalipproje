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
    public partial class CariList : Form
    {
       
        private string initialFilter; // Sınıf seviyesinde initialFilter tanımla

        public CariList() : this("") // Parametresiz yapıcı, parametreli yapıcıyı default değer ile çağırır
        {
            InitializeComponent();
            Load += CariList_Load;
        }
        public string CurrentFilterText
        {
            get { return textBox2.Text; } // textBox2'nin mevcut değerini döndür
        }
        public CariList(string initialFilter)
        {
            InitializeComponent();
            this.initialFilter = initialFilter; // initialFilter alanını ayarla
            Load += CariList_Load;
        }
        private void CariList_Load(object sender, EventArgs e)
        {
            dataGridView1.AutoGenerateColumns = false; // Otomatik sütun oluşturmayı kapat
            dataGridView1.ColumnCount = 2; // Sütun sayısı
            dataGridView1.CellDoubleClick += new DataGridViewCellEventHandler(dataGridView1_CellDoubleClick);
            textBox2.Text = initialFilter; // Başlangıç filtresini textBox2'ye ata
            LoadData(initialFilter); // İlk yüklemede filtreleme yap

            dataGridView1.Columns[0].Visible = false; // MusteriID sütununu gizle
                                                      // Sütun başlıklarının ve satırların görsel ayarlarını yapılandır
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 223, 0);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 8, FontStyle.Bold);
            dataGridView1.RowsDefaultCellStyle.BackColor = Color.LightYellow;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.White;
            dataGridView1.RowsDefaultCellStyle.SelectionBackColor = Color.DarkGoldenrod;
            dataGridView1.RowsDefaultCellStyle.SelectionForeColor = Color.White;
            dataGridView1.RowsDefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Regular);

            // Sütunları, mevcut görüntü alanını dolduracak şekilde genişlet
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Kullanıcıların satır ve sütun boyutlarını değiştirmesine izin ver
            dataGridView1.AllowUserToResizeRows = true;
            dataGridView1.AllowUserToResizeColumns = true;

            // MusteriID Sütunu
            dataGridView1.Columns[0].Name = "MusteriID";
            dataGridView1.Columns[0].HeaderText = "Müşteri ID";
            dataGridView1.Columns[0].DataPropertyName = "MusteriID";
            dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; // İçeriğe göre genişlet

            // CariUnvani Sütunu
            dataGridView1.Columns[1].Name = "CariUnvani";
            dataGridView1.Columns[1].HeaderText = "Cari Ünvanı";
            dataGridView1.Columns[1].DataPropertyName = "CariUnvani";
            dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; // İçeriğe göre genişlet

            

            // Zebra deseni için DataGridView'in RowPrePaint event'ini kullan
            dataGridView1.RowPrePaint += new DataGridViewRowPrePaintEventHandler(dataGridView1_RowPrePaint);

            textBox2.TextChanged += textBox2_TextChanged;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            LoadData(textBox2.Text);
        }

        private void LoadData(string filter)
        {
            using (SqlConnection con = DatabaseHelper.GetConnection())
            {
                var query = string.IsNullOrWhiteSpace(filter) ?
                    "SELECT MusteriID, CariUnvani FROM CariKartHeader WHERE CariTipi COLLATE Latin1_General_CI_AI = 'MÜŞTERİ'" :
                    "SELECT MusteriID, CariUnvani FROM CariKartHeader WHERE CariTipi COLLATE Latin1_General_CI_AI = 'MÜŞTERİ' AND CariUnvani LIKE @filter + '%' COLLATE Latin1_General_CI_AI";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    if (!string.IsNullOrWhiteSpace(filter))
                    {
                        cmd.Parameters.AddWithValue("@filter", filter);
                    }

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                }
            }
        }

        private void dataGridView1_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            // Zebra deseni için her çift satırı açık sarı yap
            if (e.RowIndex % 2 == 0)
            {
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
            }
            else
            {
                dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
            }
        }

        public string SelectedCariUnvani { get; private set; } // Seçilen Cari Ünvanını saklamak için

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                SelectedCariUnvani = dataGridView1.Rows[e.RowIndex].Cells["CariUnvani"].Value.ToString();
                this.DialogResult = DialogResult.OK; // Dialog sonucunu OK olarak ayarla
                this.Close(); // Formu kapat
            }
        }
    }
}
