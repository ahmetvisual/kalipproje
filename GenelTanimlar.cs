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
    public partial class GenelTanimlar : Form
    {
        public GenelTanimlar()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            // dataGridView1 için CellDoubleClick olay işleyicisini ekleyin
            this.dataGridView1.CellDoubleClick += new DataGridViewCellEventHandler(this.dataGridView1_CellDoubleClick);

            // dataGridView2 için CellDoubleClick olay işleyicisini ekleyin
            this.dataGridView2.CellDoubleClick += new DataGridViewCellEventHandler(this.dataGridView2_CellDoubleClick);

            this.Load += new System.EventHandler(this.GenelTanimlar_Load);
            // Olay işleyicilerini ekle
            this.textBox1.TextChanged += new EventHandler(textBox1_TextChanged);
            this.textBox2.TextChanged += new EventHandler(textBox2_TextChanged);
            this.dataGridView1.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_CellMouseDown);
            this.dataGridView2.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView2_CellMouseDown);

        }
        private void GenelTanimlar_Load(object sender, EventArgs e)
        {
            // Form yüklendiğinde veritabanından veri çekip DataGridView'lere yükle
            RefreshDataGridView(dataGridView1, "AsortiTanimlari");
            RefreshDataGridView(dataGridView2, "KalipTuru");
            CustomizeDataGridView(); // Tasarımı uygula
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // textBox1'deki metni büyük harfe çevir
            textBox1.Text = textBox1.Text.ToUpper();
            // Metni değiştirirken imlecin pozisyonunu korumak için
            textBox1.SelectionStart = textBox1.Text.Length;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            // textBox2'deki metni büyük harfe çevir
            textBox2.Text = textBox2.Text.ToUpper();
            // Metni değiştirirken imlecin pozisyonunu korumak için
            textBox2.SelectionStart = textBox2.Text.Length;
        }


        private void CustomizeDataGridView()
        {
            // dataGridView1 ve dataGridView2 için tasarım ayarlarını burada yap.
            var dataGrids = new[] { dataGridView1, dataGridView2 };
            foreach (var dataGridView in dataGrids)
            {
                dataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 223, 0); // Altın sarısı
                dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
                dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold);
                dataGridView.RowsDefaultCellStyle.BackColor = Color.LightYellow;
                dataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.White;
                dataGridView.RowsDefaultCellStyle.SelectionBackColor = Color.DarkGoldenrod;
                dataGridView.RowsDefaultCellStyle.SelectionForeColor = Color.White;
                dataGridView.RowsDefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Regular);
                dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // Sütunları sığdır

            }
        }
        private void RefreshDataGridView(DataGridView dataGridView, string columnName)
        {
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                // NULL olmayan kayıtları çekmek için SQL sorgusunda filtreleme yapın
                string query = $"SELECT GenelID, {columnName} FROM GenelTanimlar WHERE {columnName} IS NOT NULL";
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                {
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    // Datatable'ı doğrudan DataGridView'e bağlayın
                    dataGridView.DataSource = dataTable;

                    // GenelID sütununu gizlemek
                    if (dataGridView.Columns["GenelID"] != null)
                    {
                        dataGridView.Columns["GenelID"].Visible = false;
                    }
                }
            }
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dataGridView1.Rows[e.RowIndex];
                // AsortiTanimlari sütunundan değeri al ve textBox1'e ata
                textBox1.Text = row.Cells["AsortiTanimlari"].Value.ToString();
                textBox1.Tag = row.Cells["GenelID"].Value.ToString(); // GenelID'yi sakla
            }
        }

        private void dataGridView2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dataGridView2.Rows[e.RowIndex];
                // KalipTuru sütunundan değeri al ve textBox2'e ata
                textBox2.Text = row.Cells["KalipTuru"].Value.ToString();
                textBox2.Tag = row.Cells["GenelID"].Value.ToString(); // GenelID'yi sakla
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                if (textBox1.Tag != null)
                {
                    // Güncelleme işlemi
                    UpdateRecord(textBox1.Tag.ToString(), textBox1.Text, null); // KalipTuru null geçilir
                }
                else
                {
                    // Ekleme işlemi
                    InsertData(textBox1.Text, null); // KalipTuru için null geçilir
                }
                RefreshDataGridView(dataGridView1, "AsortiTanimlari");
                textBox1.Clear();
                textBox1.Tag = null;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox2.Text))
            {
                if (textBox2.Tag != null)
                {
                    // Güncelleme işlemi
                    UpdateRecord(textBox2.Tag.ToString(), null, textBox2.Text); // AsortiTanimlari için null geçilir
                }
                else
                {
                    // Ekleme işlemi
                    InsertData(null, textBox2.Text); // AsortiTanimlari için null geçilir
                }
                RefreshDataGridView(dataGridView2, "KalipTuru");
                textBox2.Clear();
                textBox2.Tag = null;
            }
        }

        private void InsertData(string asortiTanimlari, string kalipTuru)
        {
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                string query = "INSERT INTO GenelTanimlar (AsortiTanimlari, KalipTuru) VALUES (@AsortiTanimlari, @KalipTuru)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AsortiTanimlari", string.IsNullOrWhiteSpace(asortiTanimlari) ? DBNull.Value : (object)asortiTanimlari);
                    command.Parameters.AddWithValue("@KalipTuru", string.IsNullOrWhiteSpace(kalipTuru) ? DBNull.Value : (object)kalipTuru);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }


        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                int selectedIndex = dataGridView2.SelectedRows[0].Index;
                // GenelID sütununun index'i varsayılan olarak 0 olarak kabul edilmiştir.
                // Gerçek index numarasına göre burayı güncelleyin.
                int genelId = int.Parse(dataGridView2["GenelID", selectedIndex].Value.ToString());
                DeleteRecord(genelId, "GenelTanimlar"); // Tablo adını ve GenelID'yi silme fonksiyonuna geçir
                RefreshDataGridView(dataGridView2, "KalipTuru"); // dataGridView2'yi güncelle
            }

        }

        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedIndex = dataGridView1.SelectedRows[0].Index;
                // GenelID sütununun index'i varsayılan olarak 0 olarak kabul edilmiştir.
                // Gerçek index numarasına göre burayı güncelleyin.
                int genelId = int.Parse(dataGridView1["GenelID", selectedIndex].Value.ToString());
                DeleteRecord(genelId, "GenelTanimlar"); // Tablo adını ve GenelID'yi silme fonksiyonuna geçir
                RefreshDataGridView(dataGridView1, "AsortiTanimlari"); // dataGridView1'i güncelle
            }
        }

        private void DeleteRecord(int genelId, string tableName)
        {
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                // DELETE sorgusunda GenelID parametresi kullanılıyor
                string query = $"DELETE FROM {tableName} WHERE GenelID = @GenelID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@GenelID", genelId);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        private void UpdateRecord(string genelID, string asortiTanimlari, string kalipTuru)
        {
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                var query = new StringBuilder("UPDATE GenelTanimlar SET ");
                var parameters = new List<SqlParameter>();

                if (asortiTanimlari != null)
                {
                    query.Append("AsortiTanimlari = @AsortiTanimlari, ");
                    parameters.Add(new SqlParameter("@AsortiTanimlari", asortiTanimlari));
                }

                if (kalipTuru != null)
                {
                    query.Append("KalipTuru = @KalipTuru, ");
                    parameters.Add(new SqlParameter("@KalipTuru", kalipTuru));
                }

                // Son virgülü kaldır
                query.Length -= 2;

                query.Append(" WHERE GenelID = @GenelID");
                parameters.Add(new SqlParameter("@GenelID", genelID));

                using (SqlCommand command = new SqlCommand(query.ToString(), connection))
                {
                    command.Parameters.AddRange(parameters.ToArray());
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Sağ tıklama için kontrol
            if (e.Button == MouseButtons.Right)
            {
                // Geçerli bir satır üzerinde sağ tıklama yapıldığını kontrol et
                if (e.RowIndex != -1)
                {
                    // Satırın seçili olmadığını kontrol et ve seç
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[e.RowIndex].Selected = true;
                }
            }
        }

        private void dataGridView2_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            // Sağ tıklama için kontrol
            if (e.Button == MouseButtons.Right)
            {
                // Geçerli bir satır üzerinde sağ tıklama yapıldığını kontrol et
                if (e.RowIndex != -1)
                {
                    // Satırın seçili olmadığını kontrol et ve seç
                    dataGridView2.ClearSelection();
                    dataGridView2.Rows[e.RowIndex].Selected = true;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFolder = folderDialog.SelectedPath;
                    string[] imageFiles = Directory.GetFiles(selectedFolder, "*.jpg");

                    if (imageFiles.Length == 0)
                    {
                        MessageBox.Show("Seçilen klasörde resim bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    int imageCount = 0;

                    foreach (string imageFile in imageFiles)
                    {
                        string barcode = Path.GetFileNameWithoutExtension(imageFile);
                        var modelInfo = GetModelInfoFromBarcode(barcode);
                        if (modelInfo != null)
                        {
                            byte[] imageData = File.ReadAllBytes(imageFile);
                            if (InsertImageData(modelInfo.Item1, modelInfo.Item2, imageData))
                                imageCount++;
                        }
                    }

                    MessageBox.Show($"{imageCount} adet resim eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private Tuple<int, string> GetModelInfoFromBarcode(string barcode)
        {
            Tuple<int, string> modelInfo = null;

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                string query = "SELECT MDLID, ModelKodu FROM ModelHeader WHERE Barkod = @Barkod";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Barkod", barcode);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        int modelID = Convert.ToInt32(reader["MDLID"]);
                        string modelCode = reader["ModelKodu"].ToString();
                        modelInfo = new Tuple<int, string>(modelID, modelCode);
                    }
                }
            }

            return modelInfo;
        }

        private bool InsertImageData(int modelID, string modelCode, byte[] imageData)
        {
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                // Resim limit kontrolü (DATALENGTH kullanılmıyor — binary full scan timeout yapar)
                using (SqlCommand checkCmd = new SqlCommand(
                    "SELECT COUNT(*) FROM ModelImages WHERE ModelID = @ModelID", connection))
                {
                    checkCmd.Parameters.AddWithValue("@ModelID", modelID);
                    int existingCount = (int)checkCmd.ExecuteScalar();
                    if (existingCount >= 3)
                    {
                        // Bu model için 3 resim limiti doldu, atla
                        return false;
                    }
                }

                string query = "INSERT INTO ModelImages (ModelID, ModelKodu, ImageData) VALUES (@ModelID, @ModelKodu, @ImageData)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ModelID", modelID);
                    command.Parameters.AddWithValue("@ModelKodu", modelCode);
                    command.Parameters.Add("@ImageData", SqlDbType.VarBinary, imageData.Length).Value = imageData;
                    command.ExecuteNonQuery();
                }
            }
            return true;
        }


        private void button4_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFolder = folderDialog.SelectedPath;
                    string[] imageFiles = Directory.GetFiles(selectedFolder, "*.jpg");

                    if (imageFiles.Length == 0)
                    {
                        MessageBox.Show("Seçilen klasörde resim bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Kullanıcıya hangi listeye alınacağını sor
                    DialogResult listSecimi = MessageBox.Show(
                        "Resimler eklenecek siparişler Gelen Kalıplar listesine alınsın mı?\n\n" +
                        "EVET = Gelen Kalıplar listesine al\n" +
                        "HAYIR = Bekleyen Kalıplar listesinde kalsın",
                        "Liste Seçimi",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    bool gelenListesineAl = (listSecimi == DialogResult.Yes);

                    int imageCount = 0;
                    int notFoundCount = 0;
                    int durumGuncellenenCount = 0;

                    foreach (string imageFile in imageFiles)
                    {
                        string kalipKodu = Path.GetFileNameWithoutExtension(imageFile);
                        var siparisInfo = GetSiparisInfoFromKalipKodu(kalipKodu);

                        if (siparisInfo != null)
                        {
                            // Resmi ekle — false dönerse limit aşıldı ya da hata oluştu
                            byte[] imageData = File.ReadAllBytes(imageFile);
                            bool eklendi = InsertImageData(siparisInfo.ModelID, siparisInfo.ModelKodu, imageData);

                            if (eklendi)
                            {
                                imageCount++;

                                // Durum güncellemesi SADECE resim başarıyla eklenince yapılır
                                if (gelenListesineAl && siparisInfo.SiparisDurumu == 0)
                                {
                                    UpdateSiparisDurumuToGelen(siparisInfo.SipID);
                                    durumGuncellenenCount++;
                                }
                            }
                        }
                        else
                        {
                            notFoundCount++;
                        }
                    }

                    string message = $"{imageCount} adet resim eklendi.";
                    if (durumGuncellenenCount > 0)
                    {
                        message += $"\n{durumGuncellenenCount} adet sipariş Gelen Kalıplar listesine alındı.";
                    }
                    if (notFoundCount > 0)
                    {
                        message += $"\n{notFoundCount} adet kalıp kodu siparişlerde bulunamadı.";
                    }
                    MessageBox.Show(message, "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // Sipariş bilgilerini tutan sınıf
        private class SiparisInfo
        {
            public int SipID { get; set; }
            public string ModelKodu { get; set; }
            public int ModelID { get; set; }
            public int SiparisDurumu { get; set; }
        }

        private SiparisInfo GetSiparisInfoFromKalipKodu(string kalipKodu)
        {
            SiparisInfo siparisInfo = null;

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                // SiparisHeader'dan KalipKodu'na göre bilgileri çek
                string siparisQuery = "SELECT SipID, ModelKodu, SiparisDurumu FROM SiparisHeader WHERE KalipKodu = @KalipKodu";

                using (SqlCommand command = new SqlCommand(siparisQuery, connection))
                {
                    command.Parameters.AddWithValue("@KalipKodu", kalipKodu);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            siparisInfo = new SiparisInfo
                            {
                                SipID = Convert.ToInt32(reader["SipID"]),
                                ModelKodu = reader["ModelKodu"].ToString(),
                                SiparisDurumu = Convert.ToInt32(reader["SiparisDurumu"])
                            };
                        }
                    }
                }

                // ModelKodu bulunduysa, ModelHeader'dan MDLID çek
                if (siparisInfo != null && !string.IsNullOrEmpty(siparisInfo.ModelKodu))
                {
                    string modelQuery = "SELECT MDLID FROM ModelHeader WHERE ModelKodu = @ModelKodu";
                    using (SqlCommand command = new SqlCommand(modelQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ModelKodu", siparisInfo.ModelKodu);
                        if (connection.State != System.Data.ConnectionState.Open)
                            connection.Open();
                        var mdlIdResult = command.ExecuteScalar();
                        if (mdlIdResult != null)
                        {
                            siparisInfo.ModelID = Convert.ToInt32(mdlIdResult);
                        }
                        else
                        {
                            // ModelID bulunamazsa null döndür
                            siparisInfo = null;
                        }
                    }
                }
            }

            return siparisInfo;
        }

        private void UpdateSiparisDurumuToGelen(int sipID)
        {
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                string updateQuery = "UPDATE SiparisHeader SET SiparisDurumu = 1, GelenTarih = @GelenTarih WHERE SipID = @SipID";
                using (SqlCommand command = new SqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@SipID", sipID);
                    command.Parameters.AddWithValue("@GelenTarih", DateTime.Now);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
