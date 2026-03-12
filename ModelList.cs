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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace kalipproje
{
    public partial class ModelList : Form
    {
        public ModelList()
        {
            InitializeComponent();
            CustomizeDataGridView(dataGridView1);
            LoadDataFromDatabase(); // Veritabanından verileri yükle          

        }
        private string initialFilter;

        public string CurrentFilterText
        {
            get { return textBox1.Text; } // textBox1'nin mevcut değerini döndür
        }

        public ModelList(string modelKodu = "")
        {
            InitializeComponent();     
            CustomizeDataGridView(dataGridView1);
            this.initialFilter = modelKodu; // Başlangıç filtresi olarak ModelKodu'nu sakla
            this.dataGridView1.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellDoubleClick);
            textBox1.TextChanged += TextBox1_TextChanged; // Bu satırı ekleyin
        }
        public string SelectedBarkod { get; private set; }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                string selectedBarkod = dataGridView1.Rows[e.RowIndex].Cells["Barkod"].Value?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(selectedBarkod))
                {
                    SelectedBarkod = dataGridView1.Rows[e.RowIndex].Cells["Barkod"].Value.ToString();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);


            LoadDataFromDatabase(); // Veritabanından verileri yükle
            if (!string.IsNullOrEmpty(this.initialFilter))
            {
                textBox1.Text = this.initialFilter; // Başlangıç filtresini uygula
                FilterData(this.initialFilter); // Filtreyi uygula
            }
        }

        private void FilterData(string filter)
        {
            if (dataGridView1.DataSource is DataTable dataTable)
            {
                dataTable.DefaultView.RowFilter = $"ModelKodu LIKE '%{filter}%'";
            }
        }

        

        private void CustomizeDataGridView(DataGridView dgv)
        {
            // Sütun başlıklarının ve satırların görsel ayarlarını yapılandır
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 223, 0);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold);
            dgv.RowsDefaultCellStyle.BackColor = Color.LightYellow;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.White;
            dgv.RowsDefaultCellStyle.SelectionBackColor = Color.DarkGoldenrod;
            dgv.RowsDefaultCellStyle.SelectionForeColor = Color.White;
            dgv.RowsDefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Regular);

            // Sütunları, mevcut görüntü alanını dolduracak şekilde genişlet
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Kullanıcıların satır ve sütun boyutlarını değiştirmesine izin ver
            dgv.AllowUserToResizeRows = true;
            dgv.AllowUserToResizeColumns = true;

            // DataGridView'de ek satır eklenmesini engelle
            dgv.AllowUserToAddRows = false;
        }

        private void LoadDataFromDatabase()
        {
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                try
                {
                    string query = "SELECT ModelKodu, Barkod FROM ModelHeader";
                    SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection);

                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    dataGridView1.DataSource = dataTable;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Veritabanı hatası: " + ex.Message);
                }
            }
        }
        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = string.Format("ModelKodu LIKE '%{0}%'", textBox1.Text);

        }
    }
}
