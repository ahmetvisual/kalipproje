using System.Data;
using System.Data.SqlClient;
using FastReport;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using FastReport.Barcode;
using DevExpress.XtraReports.UI;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.Parameters;

namespace kalipproje
{
    public partial class ModelGiris : Form
    {
        private int lastCodeIndex = 0; // Son oluşturulan kodun indis değeri
        private string lastModelPrefix = ""; // Son oluşturulan model kodunun ön ek değeri
        // Bağlantı dizesini ve diğer değişkenleri burada tanımlayın

        public ModelGiris()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            comboBox1.SelectedIndex = 0;
            textBox3.KeyPress += textBox3_KeyPress;
            textBox1.ReadOnly = true;
            textBox4.KeyPress += TextBox4_KeyPress;
            dataGridView1.CellDoubleClick += DataGridView1_CellDoubleClick;
            AddCheckBoxColumn();
            CustomizeDataGridView(); // DataGridView özelleştirmelerini uygula

            _ = LoadLast500RecordsAsync(); // Form açıldığında son 500 kaydı yükler

            label7.Visible = false;

            int currentYear = DateTime.Now.Year;
            for (int i = currentYear - 8; i <= currentYear + 3; i++) // 2 yıl daha ekleniyor
            {
                comboBox2.Items.Add(i);
            }

            comboBox2.SelectedItem = currentYear;
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
            comboBox2.SelectedIndexChanged += ComboBox2_SelectedIndexChanged;
            button3.Click += Button3_Click;
        }

        private async Task LoadLast500RecordsAsync()
        {
            if (isLoading) return;
            isLoading = true;

            string query = @"
    SELECT TOP 500 MDLID, ModelKodu, ModelTarihi, ModeliGetiren, Notes, Asortisi, Durumu, Barkod 
    FROM ModelHeader
    ORDER BY MDLID DESC";  // Son 500 kaydı al

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                await connection.OpenAsync();

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);

                    barcodeDataCache = dataTable;  // Verileri önbelleğe alıyoruz
                    dataGridView1.DataSource = barcodeDataCache;
                    dataGridView1.Columns["MDLID"].Visible = false;
                }
            }

            UpdateRowCount();  // Satır sayısını günceller
            isLoading = false;
        }

        private async void TextBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13) // Enter tuşu
            {
                string filterText = textBox4.Text;

                if (!string.IsNullOrWhiteSpace(filterText))
                {
                    await LoadFilteredRecordsAsync(filterText); // Filtreye göre tüm verileri getir
                }
                else
                {
                    await LoadLast500RecordsAsync(); // Eğer filtre boşsa, sadece son 500 kaydı yükle
                }
            }
        }
        private async Task LoadFilteredRecordsAsync(string filterText)
        {
            if (isLoading) return;
            isLoading = true;

            string query = @"
    SELECT MDLID, ModelKodu, ModelTarihi, ModeliGetiren, Notes, Asortisi, Durumu, Barkod 
    FROM ModelHeader
    WHERE ModelKodu LIKE @FilterText
    ORDER BY MDLID DESC"; // Filtreye göre tüm kayıtları al

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@FilterText", "%" + filterText + "%");

                await connection.OpenAsync();

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);

                    barcodeDataCache = dataTable; // Verileri önbelleğe al
                    dataGridView1.DataSource = barcodeDataCache;
                    dataGridView1.Columns["MDLID"].Visible = false;
                }
            }

            UpdateRowCount(); // Satır sayısını günceller
            isLoading = false;
        }
        private void AddCheckBoxColumn()
        {
            // DataGridView'e checkbox sütununu ekleyin
            DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn();
            checkBoxColumn.HeaderText = "Seç";
            checkBoxColumn.Name = "checkBoxColumn";
            dataGridView1.Columns.Insert(0, checkBoxColumn); // Checkbox sütununu ilk sütun olarak ekler

            // Checkbox sütununun görünür olmasını sağlayın
            dataGridView1.Columns["checkBoxColumn"].Visible = true;
        }

        private int selectedMDLID = -1; // Başlangıçta geçersiz bir değer

        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Seçili satırın verilerini al
                var row = dataGridView1.Rows[e.RowIndex];
                selectedMDLID = Convert.ToInt32(row.Cells["MDLID"].Value);
                textBox2.Text = row.Cells["ModeliGetiren"].Value.ToString();
                richTextBox1.Text = row.Cells["Notes"].Value.ToString();
            }
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTextBox();
        }

        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTextBox();
        }

        private void UpdateTextBox()
        {
            string combinedText = comboBox1.SelectedItem.ToString() + comboBox2.SelectedItem.ToString().Substring(2);
            textBox1.Text = combinedText;
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Sadece rakam girişine ve sadece 1 ile 99 arasındaki değere izin ver
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
            else
            {
                // Giriş bir rakam ise, değeri kontrol et
                int value;
                if (int.TryParse(textBox3.Text + e.KeyChar, out value))
                {
                    // Değer 1 ile 99 arasında değilse, girişi engelle
                    if (value < 1 || value > 99)
                    {
                        e.Handled = true;
                    }
                }
            }
        }
        private void Button3_Click(object sender, EventArgs e)
        {
            string combinedText = textBox1.Text;
            string modeliGetiren = textBox2.Text;
            DateTime modelTarihi = dateTimePicker1.Value;
            string notes = richTextBox1.Text;

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                // Güncelleme işlemi için kontrol
                if (selectedMDLID >= 0)
                {
                    // Güncelleme sorgusu
                    string updateQuery = @"UPDATE ModelHeader 
                    SET ModeliGetiren = @ModeliGetiren, 
                        ModelTarihi = @ModelTarihi, 
                        Notes = @Notes
                    WHERE MDLID = @MDLID";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ModeliGetiren", modeliGetiren);
                        command.Parameters.AddWithValue("@ModelTarihi", modelTarihi);
                        command.Parameters.AddWithValue("@Notes", notes);
                        command.Parameters.AddWithValue("@MDLID", selectedMDLID);

                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Kayıt güncellendi.");
                }
                else
                {
                    // Yeni kayıt için textBox1'in boş olup olmadığını kontrol edin
                    if (string.IsNullOrWhiteSpace(textBox1.Text))
                    {
                        MessageBox.Show("Lütfen model ön ekini oluşturunuz.");
                        return; // Eğer textBox1 boşsa, işlemi durdur
                    }

                    int count;
                    // textBox3 girişinin sadece yeni kayıt ekleme işlemi için kontrolü
                    if (!int.TryParse(textBox3.Text, out count) || count <= 0)
                    {
                        MessageBox.Show("Geçersiz sayı girdiniz.");
                        return;
                    }

                    int lastMDLID = GetLastMDLID(connection);
                    int lastBarkodNum = GetLastBarkodNum(connection) + 1;

                    if (lastModelPrefix != combinedText)
                    {
                        string sql = "SELECT MAX(ModelKodu) FROM ModelHeader WHERE ModelKodu LIKE @ModelPrefix + '%'";
                        using (SqlCommand getMaxIndexCommand = new SqlCommand(sql, connection))
                        {
                            getMaxIndexCommand.Parameters.AddWithValue("@ModelPrefix", combinedText);
                            var result = getMaxIndexCommand.ExecuteScalar();
                            if (result != DBNull.Value && result != null)
                            {
                                string maxModelKodu = (string)result;
                                lastCodeIndex = int.Parse(maxModelKodu.Substring(combinedText.Length)) + 1;
                            }
                            else
                            {
                                lastCodeIndex = 1;
                            }
                        }
                        lastModelPrefix = combinedText;
                    }

                    for (int i = 0; i < count; i++)
                    {
                        string newModelKodu = combinedText + lastCodeIndex.ToString("D4");
                        string newBarkod = (lastBarkodNum + i).ToString("D8");

                        using (SqlCommand command = new SqlCommand("INSERT INTO ModelHeader (MDLID, ModeliGetiren, ModelKodu, ModelTarihi, Notes, Barkod) VALUES (@MDLID, @ModeliGetiren, @ModelKodu, @ModelTarihi, @Notes, @Barkod)", connection))
                        {
                            command.Parameters.AddWithValue("@MDLID", lastMDLID + 1);
                            command.Parameters.AddWithValue("@ModeliGetiren", modeliGetiren);
                            command.Parameters.AddWithValue("@ModelKodu", newModelKodu);
                            command.Parameters.AddWithValue("@ModelTarihi", modelTarihi);
                            command.Parameters.AddWithValue("@Notes", notes);
                            command.Parameters.AddWithValue("@Barkod", newBarkod);
                            command.ExecuteNonQuery();
                        }
                        lastCodeIndex++;
                        lastMDLID++;
                    }
                    lastBarkodNum += count;
                    MessageBox.Show("Kayıt Başarılı.");
                }
            }

            // Ekleme veya güncelleme sonrası DataGridView'i yenile
            _ = LoadLast500RecordsAsync();
            // Ekleme veya güncelleme sonrası selectedMDLID'yi sıfırla
            selectedMDLID = -1;
        }
        private int GetLastBarkodNum(SqlConnection connection)
        {
            int lastBarkodNum = 10000000; // Varsayılan başlangıç değeri
            string query = "SELECT MAX(CONVERT(INT, Barkod)) FROM ModelHeader";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                var result = command.ExecuteScalar();
                if (result != DBNull.Value && result != null)
                {
                    lastBarkodNum = Convert.ToInt32(result);
                }
            }
            return lastBarkodNum;
        }
        private int GetLastMDLID(SqlConnection connection)
        {
            int lastMDLID = 0;
            using (SqlCommand command = new SqlCommand("SELECT MAX(MDLID) FROM ModelHeader", connection))
            {
                object result = command.ExecuteScalar();
                if (result != DBNull.Value)
                {
                    lastMDLID = Convert.ToInt32(result);
                }
            }
            return lastMDLID;
        }
        private const int PageSize = 500;
        private int currentPage = 0;
        private bool isLoading = false;

        private void tumunusec_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                DataGridViewCheckBoxCell checkBox = row.Cells["checkBoxColumn"] as DataGridViewCheckBoxCell;
                checkBox.Value = true;
            }
        }
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            for (int i = dataGridView1.Rows.Count - 1; i >= 0; i--)
            {
                DataGridViewRow row = dataGridView1.Rows[i];
                DataGridViewCheckBoxCell checkBox = row.Cells["checkBoxColumn"] as DataGridViewCheckBoxCell;
                if (Convert.ToBoolean(checkBox.Value) == true)
                {
                    int mdlid = Convert.ToInt32(row.Cells["MDLID"].Value);

                    using (SqlConnection connection = DatabaseHelper.GetConnection())
                    {
                        connection.Open();
                        // Önce resimleri sil
                        using (SqlCommand command = new SqlCommand("DELETE FROM ModelImages WHERE ModelID = @ModelID", connection))
                        {
                            command.Parameters.AddWithValue("@ModelID", mdlid);
                            command.ExecuteNonQuery();
                        }
                        // Sonra modeli sil
                        using (SqlCommand command = new SqlCommand("DELETE FROM ModelHeader WHERE MDLID = @MDLID", connection))
                        {
                            command.Parameters.AddWithValue("@MDLID", mdlid);
                            command.ExecuteNonQuery();
                        }
                    }

                    dataGridView1.Rows.RemoveAt(i);
                }
            }

            MessageBox.Show("Seçili satırlar başarıyla silindi.", "Silme İşlemi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void UpdateRowCount()
        {
            int rowCount = dataGridView1.Rows.Count;
            label7.Text = $"{rowCount} adet MoldeKartı listelenmiştir.";
            label7.Font = new Font(label7.Font, FontStyle.Bold);
            label7.Visible = true;
        }

        private void ekleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.gif"; // Desteklenen dosya formatları
            openFileDialog.Title = "Resim Seç";
            openFileDialog.Multiselect = true; // Kullanıcının birden fazla dosya seçmesine izin ver

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                int selectedMdlId;
                string modelKodu = "";
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    selectedMdlId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["MDLID"].Value);

                    // Veritabanından ModelKodu'nu çek
                    using (SqlConnection connection = DatabaseHelper.GetConnection())
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand("SELECT ModelKodu FROM ModelHeader WHERE MDLID = @MDLID", connection))
                        {
                            command.Parameters.AddWithValue("@MDLID", selectedMdlId);
                            var result = command.ExecuteScalar();
                            if (result != null)
                            {
                                modelKodu = result.ToString();
                            }
                            else
                            {
                                MessageBox.Show("Model kodu bulunamadı.");
                                return;
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Lütfen önce bir model seçin.");
                    return;
                }

                int eklenenSayisi = 0;
                foreach (string filePath in openFileDialog.FileNames)
                {
                    if (ImageManager.AddImageToModel(selectedMdlId, modelKodu, filePath, true))
                        eklenenSayisi++;
                }

                if (eklenenSayisi > 0)
                    MessageBox.Show($"{eklenenSayisi} resim başarıyla kaydedildi.");
                else
                    MessageBox.Show("Hiçbir resim eklenmedi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        private void ShowImageFromDatabase(int imageId)
        {
            currentImageId = imageId;
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT ImageData FROM ModelImages WHERE ImageID = @ImageID", connection))
                {
                    command.Parameters.AddWithValue("@ImageID", imageId);
                    var result = command.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        byte[] imageBytes = (byte[])result;
                        // MemoryStream using bloğu içine ALMIYORUZ:
                        // Image.FromStream, stream kapatılıncaya kadar stream'e bağlı kalır.
                        // using kullansaydık GDI+ erişim hatası oluşurdu.
                        var ms = new MemoryStream(imageBytes);
                        pictureBox1.Image?.Dispose(); // Eski resmi temizle
                        pictureBox1.Image = System.Drawing.Image.FromStream(ms);
                    }
                    else
                    {
                        pictureBox1.Image = null;
                    }
                }
            }
        }
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int mdlid = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["MDLID"].Value);

                // Önce imageIds listesini temizle
                imageIds.Clear();
                currentImageIndex = 0; // Gezinmeye baştan başla

                // Seçili modelin tüm resim ID'lerini doldur
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("SELECT ImageID FROM ModelImages WHERE ModelID = @ModelID", connection))
                    {
                        command.Parameters.AddWithValue("@ModelID", mdlid);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                imageIds.Add(reader.GetInt32(0)); // ImageID'leri listeye ekle
                            }
                        }
                    }
                }

                if (imageIds.Count > 0)
                {
                    ShowImageFromDatabase(imageIds[currentImageIndex]); // İlk resmi göster
                }
                else
                {
                    pictureBox1.Image = null; // Resim listesi boşsa PictureBox'ı temizle
                }
            }
        }

        // Form seviyesinde resimler listesini ve mevcut resmin indeksini tutacak değişkenler
        private List<int> imageIds = new List<int>();
        private int currentImageIndex = 0;

        private void sonra_Click(object sender, EventArgs e)
        {
            if (currentImageIndex < imageIds.Count - 1)
            {
                currentImageIndex++;
                ShowImageFromDatabase(imageIds[currentImageIndex]);
            }
        }

        private void once_Click(object sender, EventArgs e)
        {
            if (currentImageIndex > 0)
            {
                currentImageIndex--;
                ShowImageFromDatabase(imageIds[currentImageIndex]);
            }
        }

        private int currentImageId = -1; // Başlangıçta geçersiz bir değer atayın

        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (currentImageId != -1) // Geçerli bir ImageID kontrolü
            {
                // Kullanıcıdan silme işlemi için onay isteyin
                var confirmResult = MessageBox.Show("Bu resmi silmek istediğinize emin misiniz?", "Resim Sil", MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.Yes)
                {
                    // Veritabanından resmi sil
                    DeleteImageFromDatabase(currentImageId);

                    // PictureBox'tan resmi kaldır
                    pictureBox1.Image = null;

                    // İşlem başarılı mesajı
                    MessageBox.Show("Resim başarıyla silindi.");

                    // Sonraki adımlar için currentImageId'yi sıfırlayın veya bir sonraki resmi yükleyin
                    currentImageId = -1;
                }
            }
            else
            {
                MessageBox.Show("Silinecek resim bulunamadı.");
            }
        }

        private void DeleteImageFromDatabase(int imageId)
        {
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                // Veritabanı bağlantısını aç
                connection.Open();

                // Resmi silme SQL sorgusu
                string query = "DELETE FROM ModelImages WHERE ImageID = @ImageID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // ImageID parametresini sorguya ekle
                    command.Parameters.AddWithValue("@ImageID", imageId);

                    // Sorguyu çalıştır
                    command.ExecuteNonQuery();
                }
            }
            MessageBox.Show("Resim veritabanından başarıyla silindi.");
        }


        private DataTable barcodeDataCache = null; // Verileri saklamak için global bir DataTable

        private void LoadDataIntoCache()
        {
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT MDLID, ModelKodu, Barkod FROM ModelHeader"; // MDLID sütunu da dahil ediliyor
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);
                    barcodeDataCache = dataTable; // Verileri önbelleğe al
                }
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (barcodeDataCache == null)
            {
                LoadDataIntoCache(); // Eğer önbellek boşsa, verileri yükle
            }

            DataTable selectedData = new DataTable();
            selectedData.Columns.Add("ModelKodu", typeof(string));
            selectedData.Columns.Add("Barkod", typeof(float)); // Barkod sütununun tipi float olarak ayarlandı

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (Convert.ToBoolean(row.Cells["checkBoxColumn"].Value))
                {
                    int mdlid = Convert.ToInt32(row.Cells["MDLID"].Value);
                    DataRow[] filteredRows = barcodeDataCache.Select($"MDLID = {mdlid}");

                    // Her satırı ekleyerek veri tipini uygun şekilde dönüştür
                    foreach (DataRow filteredRow in filteredRows)
                    {
                        DataRow newRow = selectedData.NewRow();
                        newRow["ModelKodu"] = filteredRow["ModelKodu"];
                        newRow["Barkod"] = Convert.ToSingle(filteredRow["Barkod"]); // Barkod değerini float'a dönüştürüyoruz
                        selectedData.Rows.Add(newRow);
                    }
                }
            }

            if (selectedData.Rows.Count > 0)
            {
                await Task.Run(() => PrintDevExpressReport(selectedData)); // Yazdırma işlemini arka planda çalıştır
            }
            else
            {
                MessageBox.Show("Lütfen en az bir satır seçin.");
            }
        }

        private void PrintDevExpressReport(DataTable selectedData)
        {
            XtraReport report = new XtraReport();
            report.LoadLayout(@"Rapor\ModelBarkod.repx");
            report.DataSource = selectedData;

            // Verilerin doğru şekilde bağlandığından emin olmak için
            report.DataMember = "ModelHeader"; // DataMember adını doğru şekilde ayarlayın

            try
            {
                // Raporu önceden hazırla
                report.CreateDocument();

                // Rapor yazdırma aracını oluştur
                ReportPrintTool printTool = new ReportPrintTool(report);
                printTool.PrintingSystem.ShowMarginsWarning = false; // Kenar boşluğu uyarısını devre dışı bırak
                printTool.PrintingSystem.ShowPrintStatusDialog = true; // Yazdırma durum diyalogunu göster

                // Raporu ön izleme diyalogunda göster
                printTool.ShowPreviewDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rapor yazdırma hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // Sadece sütun başlıkları için özel çizim yap
            if (e.RowIndex == -1 && e.ColumnIndex >= 0)
            {
                // Arka planı çiz
                e.PaintBackground(e.ClipBounds, true);

                // Üst kısım (daha açık renk)
                using (Brush topBrush = new SolidBrush(Color.FromArgb(250, 223, 0))) // Altın sarısı
                {
                    e.Graphics.FillRectangle(topBrush, e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Width, e.CellBounds.Height / 2);
                }

                // Alt kısım (bir tık daha koyu)
                using (Brush bottomBrush = new SolidBrush(Color.FromArgb(240, 210, 0))) // Bir tık koyu altın sarısı
                {
                    e.Graphics.FillRectangle(bottomBrush, e.CellBounds.Left, e.CellBounds.Top + e.CellBounds.Height / 2, e.CellBounds.Width, e.CellBounds.Height / 2);
                }

                // Asıl başlık hücresinin içeriğini çiz
                e.PaintContent(e.CellBounds);

                e.Handled = true; // Olayı işledik, varsayılan çizimi devre dışı bırak
            }
            else if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // Normal hücre çizimi: sadece gerekli kısımlar boyanır
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.Border);

                using (Pen pen = new Pen(Color.FromArgb(242, 242, 242), 0.5f)) // Beyazın açık tonunu kullanarak çizgileri ayarla
                {
                    // Üst çizgi
                    e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Right, e.CellBounds.Top);
                    // Sol çizgi
                    e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Left, e.CellBounds.Bottom);
                    // Alt çizgi
                    e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
                    // Sağ çizgi
                    e.Graphics.DrawLine(pen, e.CellBounds.Right - 1, e.CellBounds.Top, e.CellBounds.Right - 1, e.CellBounds.Bottom);
                }

                e.Handled = true; // Olayı işledik, varsayılan çizimi devre dışı bırak
            }
        }
        private void CustomizeDataGridView()
        {
            if (dataGridView1.Columns.Count > 0)
            {
                // Performans için DoubleBuffered özelliğini etkinleştir
                typeof(DataGridView).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                    null, dataGridView1, new object[] { true });

                // "Seç" sütununu sabit bırak
                if (dataGridView1.Columns["checkBoxColumn"] != null)
                {
                    dataGridView1.Columns["checkBoxColumn"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dataGridView1.Columns["checkBoxColumn"].Width = 35; // Sabit genişlik
                }

                // Diğer sütunların genişliğini Fill olarak ayarla
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    if (column.Name != "checkBoxColumn")
                    {
                        column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
                }
                dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);

                // Hücrelerin arka plan rengini ve seçili hücrelerin rengini ayarla
                dataGridView1.RowsDefaultCellStyle.BackColor = Color.LightYellow;
                dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.White;
                dataGridView1.RowsDefaultCellStyle.SelectionBackColor = Color.DarkGoldenrod;
                dataGridView1.RowsDefaultCellStyle.SelectionForeColor = Color.White;
                dataGridView1.RowsDefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Regular);

                // Hücrelerde kenar boşluğu (padding) ekle
                dataGridView1.DefaultCellStyle.Padding = new Padding(2); // Hücrelere 2 piksel padding ekle

                // Hücre çizgilerini beyaz yap
                dataGridView1.GridColor = Color.WhiteSmoke;

                // CellPainting olayına abone ol
                dataGridView1.CellPainting += dataGridView1_CellPainting;
            }
            else
            {
                MessageBox.Show("DataGridView sütunları henüz yüklenmedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void KaydetStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null) // Eğer PictureBox'ta bir resim varsa
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    string barkod = dataGridView1.SelectedRows[0].Cells["Barkod"].Value.ToString();

                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "JPEG Image|*.jpg|PNG Image|*.png|GIF Image|*.gif";
                    saveFileDialog.Title = "Resmi Kaydet";
                    saveFileDialog.FileName = barkod;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Jpeg;
                        string extension = System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower();

                        switch (extension)
                        {
                            case ".png":
                                format = System.Drawing.Imaging.ImageFormat.Png;
                                break;
                            case ".gif":
                                format = System.Drawing.Imaging.ImageFormat.Gif;
                                break;
                        }

                        try
                        {
                            using (Bitmap bitmap = new Bitmap(pictureBox1.Image)) // Resmi kopyala
                            {
                                bitmap.Save(saveFileDialog.FileName, format);
                            }
                            MessageBox.Show("Resim başarıyla kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Resmi kaydederken bir hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Lütfen bir satır seçin ve tekrar deneyin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Kaydedilecek bir resim bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

    }
}
