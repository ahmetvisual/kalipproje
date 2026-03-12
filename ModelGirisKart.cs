using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace kalipproje
{
    public partial class ModelGirisKart : Form
    {
        private List<int> imageIds = new List<int>(); // Veritabanından alınacak resim ID'leri
        private int currentImageIndex = 0; // Şu anda gösterilen resmin indeksi
        private SiparisGiris _siparisGirisForm; // SiparisGiris formuna referans


        public ModelGirisKart(SiparisGiris siparisGirisForm)
        {
            InitializeComponent();
            _siparisGirisForm = siparisGirisForm; // Referansı burada doğru şekilde alıyoruz
            this.StartPosition = FormStartPosition.CenterScreen;
            textBox3.Text = "1";
            // Yıl seçeneklerini doldur
            int currentYear = DateTime.Now.Year;
            for (int i = currentYear - 8; i <= currentYear + 3; i++)
            {
                comboBox2.Items.Add(i);
            }
            comboBox2.SelectedItem = currentYear;

            // ComboBox SelectedIndexChanged olaylarını bağla
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
            comboBox2.SelectedIndexChanged += ComboBox2_SelectedIndexChanged;

            // İlk değerler atanırken textBox1'i güncelle
            if (comboBox1.SelectedIndex != -1 && comboBox2.SelectedIndex != -1)
            {
                UpdateModelKodu();
            }

            // Diğer bileşenlerin yapılandırması
            LoadImagesFromDatabase();
            comboBox3.SelectedIndexChanged += ComboBox3_SelectedIndexChanged;
            button3.Click += Button3_Click;
        }

        private int lastCodeIndex = 0;
        private int selectedMDLID = -1; // Başlangıçta geçersiz bir değer

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateModelKodu();
        }

        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateModelKodu();
        }
        private void UpdateModelKodu()
        {
            if (comboBox1.SelectedIndex != -1 && comboBox2.SelectedIndex != -1)
            {
                // comboBox1'den seçilen prefix değeri ve comboBox2'den yılın son iki hanesini alalım
                string prefix = comboBox1.SelectedItem.ToString();  // ZY24 gibi
                string yearSuffix = comboBox2.SelectedItem.ToString().Substring(2); // Yılın son iki hanesi

                // Veritabanında mevcut en yüksek model kodunu kontrol et ve artır
                lastCodeIndex = GetLastModelKoduIndex(prefix + yearSuffix);

                // Model kodunu ZY240514 gibi oluştur
                string newModelKodu = prefix + yearSuffix + lastCodeIndex.ToString("D4");

                // textBox1'e yaz
                textBox1.Text = newModelKodu;
            }
        }

        private int GetLastModelKoduIndex(string modelPrefix)
        {
            int lastIndex = 0;

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT MAX(ModelKodu) FROM ModelHeader WHERE ModelKodu LIKE @ModelPrefix + '%'";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ModelPrefix", modelPrefix);

                    var result = command.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        string maxModelKodu = (string)result;

                        // modelPrefix'ten sonraki kısmı al ve int'e çevir
                        // Örneğin, ZY240501 -> 0501 alınacak ve bir artırılacak
                        lastIndex = int.Parse(maxModelKodu.Substring(modelPrefix.Length)) + 1;
                    }
                    else
                    {
                        lastIndex = 1; // Eğer veri yoksa başlangıç değeri olarak 1 döner
                    }
                }
            }
            return lastIndex;
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

        private void LoadImagesFromDatabase()
        {
            using (SqlConnection connection = DatabaseHelper.GetConnection()) // Sizin DatabaseHelper'dan aldığınız bağlantı
            {
                connection.Open();
                string query = "SELECT ImageID, ModelKodu FROM ModelImages WHERE ModelID = @ModelID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // ModelID parametresini uygun şekilde değiştirin
                    command.Parameters.AddWithValue("@ModelID", 1); // Model ID'yi dinamik olarak belirleyin

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int imageId = reader.GetInt32(0);
                            string modelKodu = reader.GetString(1);

                            // Resim ID'lerini ve ModelKodu'nu listeye ekle
                            imageIds.Add(imageId);
                            comboBox3.Items.Add(modelKodu);
                        }
                    }
                }

                if (imageIds.Count > 0 && comboBox3.Items.Count > 0)
                {
                    comboBox3.SelectedIndex = 0; // İlk resim otomatik olarak seçilir
                    ShowImageFromDatabase(imageIds[0]); // İlk resmi göster
                }
            }
        }

        private void ComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Seçilen resim comboBox3'teki resim listesine göre gösterilecek
            int selectedIndex = comboBox3.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < imageIds.Count)
            {
                ShowImageFromDatabase(imageIds[selectedIndex]);
            }
        }

        private void ShowImageFromDatabase(int imageId)
        {
            using (SqlConnection connection = DatabaseHelper.GetConnection()) // Bağlantı tekrar alınıyor
            {
                connection.Open();
                string query = "SELECT ImageData FROM ModelImages WHERE ImageID = @ImageID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ImageID", imageId);
                    var result = command.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        byte[] imageBytes = (byte[])result;
                        using (var ms = new MemoryStream(imageBytes))
                        {
                            pictureBox1.Image = Image.FromStream(ms); // PictureBox'ta göster
                        }
                    }
                    else
                    {
                        pictureBox1.Image = null; // Resim yoksa temizle
                    }
                }
            }
        }
        private void Button3_Click(object sender, EventArgs e)
        {
            string modelKodu = textBox1.Text;
            string modeliGetiren = textBox2.Text;
            DateTime modelTarihi = dateTimePicker1.Value;
            string notes = richTextBox1.Text;

            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

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
                    // Yeni kayıt ekleme işlemi
                    if (string.IsNullOrWhiteSpace(modelKodu))
                    {
                        MessageBox.Show("Lütfen model ön ekini oluşturunuz.");
                        return;
                    }

                    int count;
                    if (!int.TryParse(textBox3.Text, out count) || count <= 0)
                    {
                        MessageBox.Show("Geçersiz sayı girdiniz.");
                        return;
                    }

                    // Yeni MDLID ve Barkod değeri al
                    int lastMDLID = GetLastMDLID(connection);
                    int lastBarkodNum = GetLastBarkodNum(connection) + 1;

                    for (int i = 0; i < count; i++)
                    {
                        string newModelKodu = modelKodu;  // Model kodunu burada tekrardan eklemiyoruz
                        string newBarkod = (lastBarkodNum + i).ToString("D8");  // Barkod için 8 haneli sayı

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
                        lastMDLID++; // MDLID her iterasyonda bir artırılıyor
                    }

                    MessageBox.Show("Kayıt Başarılı.");
                    // SiparisGiris formundaki textBox1'e yeni oluşturulan model kodunu yazalım
                    _siparisGirisForm.SetModelKodu(modelKodu);
                    // ModelGirisKart formunu kapatalım
                    this.Close();
                }
            }

            // Ekleme veya güncelleme sonrası selectedMDLID'yi sıfırla
            selectedMDLID = -1;
        }

    }
}
