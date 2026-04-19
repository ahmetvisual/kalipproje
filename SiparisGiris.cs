using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FastReport;
using FastReport.Data;
using FastReport.Utils;
using DevExpress.XtraReports.UI;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.Parameters;

namespace kalipproje
{
    public partial class SiparisGiris : Form
    {
        private bool isEditMode = false; // Varsayılan olarak false
        private bool initialLoad = true; // Form ilk kez yüklendiğinde true olacak
        private List<byte[]> imageBytesList = new List<byte[]>(); // Resimlerin byte dizisi olarak saklandığı liste
        private List<int> imageIds = new List<int>(); // Resimlerin ID'lerinin saklandığı liste
        private int currentImageIndex = 0; // Gezinti için mevcut resim indeksi
        private bool isModelKoduReady = false;
        private bool isUretimTanimiReady = false;
        private int currentSiparisId = -1; // Sınıf seviyesinde bir değişken olarak sipariş ID'sini tutun
        private string originalKalipKodu = null;
        private string kullaniciDepartman;
        private string currentAsortisi;

        private bool isCopyMode = false;
        public bool IsManualChange { get; set; } = false;

        public SiparisGiris()
        {
            InitializeComponent();
            // Kullanıcının yetkisini kontrol et
            if (LoginForm.UserYetki == 1)
            {
                button1.Enabled = false; // Yetki 1 ise button1 pasif
                button5.Enabled = false;
            }
            else
            {
                button1.Enabled = true; // Yetki 0 ise button1 aktif
                button5.Enabled = true;
            }
            InitializeForm();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Load += new EventHandler(SiparisGiris_Load); // Load olayı için işleyici ekle
            textBox3.Text = "15";
            this.textBox2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox2_KeyDown);
            this.comboBox7.SelectedIndexChanged += new System.EventHandler(this.comboBox7_SelectedIndexChanged);
            comboBox3.SelectedIndexChanged += comboBox3_SelectedIndexChanged;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            this.textBox5.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox5_KeyDown);
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
         
        }
        public SiparisGiris(int siparisId) : this()
        {
            if (siparisId > 0)
            {
                isEditMode = true; // Düzenleme modunu etkinleştir
                currentSiparisId = siparisId; // Load event'inde kullanılacak
            }
        }

        public void LoadSiparisForCopy(int siparisId)
        {
            currentSiparisId = siparisId; // Load event'inde LoadSiparisData çağrılacak
            isEditMode = false; // Düzenleme modunu devre dışı bırak — Load event'inde butonlar buna göre ayarlanacak
            isCopyMode = true; // Kopyalama modu — Load event'inde yükleme sonrası edit modunu kapat
        }

        private async Task<bool> IsKalipKoduChangedAndExistsAsync(string kalipKodu)
        {
            // KalipKodu değişmemişse veya mevcut siparişin KalipKodu ise kontrol yapmaya gerek yok
            if (kalipKodu.Equals(originalKalipKodu)) return false;

            // KalipKodu değişmişse, yeni değerin veritabanında olup olmadığını kontrol et
            using (var connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                var query = "SELECT COUNT(*) FROM SiparisHeader WHERE KalipKodu = @KalipKodu AND SipID <> @SipID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@KalipKodu", kalipKodu);
                    command.Parameters.AddWithValue("@SipID", currentSiparisId); // Mevcut sipariş ID'si dışında kontrol
                    var count = (int)await command.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }

        /// <summary>
        /// ComboBox'ta verilen değeri items arasında bulup seçer.
        /// DB'den gelen CHAR kolonlarındaki trailing whitespace ve case farklılıklarını tolere eder.
        /// </summary>
        private static void SelectComboBoxItem(System.Windows.Forms.ComboBox comboBox, object dbValue)
        {
            if (dbValue == null || dbValue == DBNull.Value)
            {
                comboBox.SelectedIndex = -1;
                return;
            }

            string target = dbValue.ToString().Trim();
            if (string.IsNullOrEmpty(target))
            {
                comboBox.SelectedIndex = -1;
                return;
            }

            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                string item = comboBox.Items[i]?.ToString()?.Trim() ?? "";
                if (string.Equals(item, target, StringComparison.OrdinalIgnoreCase))
                {
                    comboBox.SelectedIndex = i;
                    return;
                }
            }

            comboBox.SelectedIndex = -1;
        }

        private void LoadSiparisData(int siparisId)
        {
            currentSiparisId = siparisId;
            string modelKodu = "";

            // Event handler'ları geçici olarak kaldır — yükleme sırasında
            // tetiklenen olaylar (uyumluluk kontrolü, comboBox3 items.Clear, comboBox4 sıfırlama vb.)
            // veritabanından gelen değerleri bozmasın.
            comboBox3.SelectedIndexChanged -= comboBox3_SelectedIndexChanged;
            comboBox1.SelectedIndexChanged -= comboBox1_SelectedIndexChanged;
            textBox1.TextChanged -= textBox1_TextChanged;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                // SiparisHeader'dan bilgileri çekme
                string siparisQuery = @"
        SELECT
            SH.*,
            YH1.KullaniciAdi AS OlusturanKullanici,
            YH2.KullaniciAdi AS GuncelleyenKullanici,
            SH.OlusturmaTarihi,
            SH.GuncellemeTarihi
        FROM
            SiparisHeader SH
        LEFT JOIN
            YonetHeader YH1 ON SH.OlusturanKullaniciID = YH1.YonetID
        LEFT JOIN
            YonetHeader YH2 ON SH.GuncelleyenKullaniciID = YH2.YonetID
        WHERE
            SH.SipID = @SipID";

                SqlCommand siparisCommand = new SqlCommand(siparisQuery, connection);
                siparisCommand.Parameters.AddWithValue("@SipID", siparisId);

                using (SqlDataReader reader = siparisCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        modelKodu = reader["ModelKodu"].ToString();

                        // richTextBox1 dolduruluyor
                        richTextBox1.Text = reader["Aciklama"] != DBNull.Value ? reader["Aciklama"].ToString() : "";

                        // ÖNEMLİ: Önce ModelKodu'nu set et ki comboBox3 items'ı
                        // prefix'e göre filtrelenmiş hale gelsin (LoadValidUretimTipleri).
                        // Aksi halde TextChanged event'i sonradan tetiklenip
                        // comboBox3.Items.Clear() ile seçimimizi silerdi.
                        string trimmedModel = modelKodu.Trim();
                        textBox1.Text = trimmedModel;
                        if (trimmedModel.Length >= 4)
                        {
                            LoadValidUretimTipleri(trimmedModel.Substring(0, 4));
                        }

                        // Şimdi comboBox3'ü doğru items üzerinden seç
                        SelectComboBoxItem(comboBox3, reader["UretimTanimi"]);

                        // Diğer kontroller
                        textBox6.Text = reader["KalipKodu"].ToString().Trim();
                        originalKalipKodu = textBox6.Text; // Orijinal değeri saklayın
                        dateTimePicker1.Value = reader["SiparisTarihi"] != DBNull.Value ? Convert.ToDateTime(reader["SiparisTarihi"]) : DateTime.Now;
                        SelectComboBoxItem(comboBox2, reader["SiparisVeren"]);
                        textBox5.Text = reader["MusteriUnvani"].ToString().Trim();
                        textBox3.Text = reader["Termin"].ToString().Trim();
                        textBox4.Text = reader["AyakkabiNo"].ToString().Trim();

                        // Önce Çeşit'i set et (trim + case-insensitive karşılaştırma)
                        SelectComboBoxItem(comboBox1, reader["Cesit"]);

                        // Çeşit'e göre comboBox4 durumunu ayarla (event devre dışı olduğu için manuel)
                        string cesit = comboBox1.SelectedItem?.ToString();
                        if (cesit == "Seri" || cesit == "Özel Seri")
                        {
                            comboBox4.Enabled = true;
                            comboBox4.BackColor = Color.LightYellow;
                        }
                        else
                        {
                            comboBox4.Enabled = false;
                            comboBox4.BackColor = SystemColors.Control;
                        }

                        SelectComboBoxItem(comboBox4, reader["Asortisi"]);
                        SelectComboBoxItem(comboBox5, reader["KalipTuru"]);
                        SelectComboBoxItem(comboBox6, reader["Tedarikci"]);
                        string olusturanKullanici = reader["OlusturanKullanici"]?.ToString() ?? "Bilinmiyor";
                        string guncelleyenKullanici = reader["GuncelleyenKullanici"]?.ToString() ?? "Bilinmiyor";
                        string olusturmaTarihi = reader["OlusturmaTarihi"] != DBNull.Value ? Convert.ToDateTime(reader["OlusturmaTarihi"]).ToString("g") : "Bilinmiyor";
                        string guncellemeTarihi = reader["GuncellemeTarihi"] != DBNull.Value ? Convert.ToDateTime(reader["GuncellemeTarihi"]).ToString("g") : "Bilinmiyor";

                        label15.Text = $"Oluşturan: {olusturanKullanici} ({olusturmaTarihi}), Güncelleyen: {guncelleyenKullanici} ({guncellemeTarihi})";
                    }
                } // İlk reader burada otomatik olarak kapanır

                // ModelHeader'dan Barkod bilgisini çekme
                if (!string.IsNullOrEmpty(modelKodu))
                {
                    string barkodQuery = "SELECT Barkod FROM ModelHeader WHERE ModelKodu = @ModelKodu";
                    SqlCommand barkodCommand = new SqlCommand(barkodQuery, connection);
                    barkodCommand.Parameters.AddWithValue("@ModelKodu", modelKodu);

                    using (SqlDataReader barkodReader = barkodCommand.ExecuteReader())
                    {
                        if (barkodReader.Read())
                        {
                            textBox2.Text = barkodReader["Barkod"].ToString().Trim();
                        }
                    } // İkinci reader burada otomatik olarak kapanır
                }
            }

            // Event handler'lar devre dışıyken set edilmediği için
            // ready flag'lerini manuel güncelle (sonraki validasyonlar için)
            isModelKoduReady = !string.IsNullOrEmpty(textBox1.Text.Trim());
            isUretimTanimiReady = comboBox3.SelectedItem != null;

            // Event handler'ları geri bağla
            comboBox3.SelectedIndexChanged += comboBox3_SelectedIndexChanged;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            textBox1.TextChanged += textBox1_TextChanged;

            // comboBox3 değerine göre textBox6 readonly durumunu ayarla
            string cb3Val = comboBox3.SelectedItem?.ToString()?.Trim();
            if (!string.IsNullOrEmpty(cb3Val) &&
                (cb3Val.Equals("ÖZEL KALIP", StringComparison.OrdinalIgnoreCase) ||
                 cb3Val.Equals("Z-BİZİM KALIP", StringComparison.OrdinalIgnoreCase)))
            {
                textBox6.ReadOnly = false;
            }
            else
            {
                textBox6.ReadOnly = true;
            }

            textBox2_KeyDown(textBox2, new KeyEventArgs(Keys.Enter));
        }

        private async Task<bool> IsKalipKoduExistsAsync(string kalipKodu)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                var query = "SELECT COUNT(*) FROM SiparisHeader WHERE KalipKodu = @KalipKodu";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@KalipKodu", kalipKodu);
                    var count = (int)await command.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }
        // ===== TAM METOT – eskisinin yerine kopyala =====
        private async void UpdateSiparis()
        {
            // Kalıp kodu çakışma kontrolü
            if (await IsKalipKoduChangedAndExistsAsync(textBox6.Text))
            {
                MessageBox.Show("Bu Kalıp Kodu zaten kullanımda. Lütfen farklı bir kod giriniz.",
                                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // ───────────────── Mevcut değerler ─────────────────
            DateTime? dbSeriTarihi = null;
            string dbAsortisi = null;

            using (var con = DatabaseHelper.GetConnection())
            {
                await con.OpenAsync();
                using (var cmd = new SqlCommand(
                       "SELECT SeriTarihi , Asortisi FROM SiparisHeader WHERE SipID = @id", con))
                {
                    cmd.Parameters.AddWithValue("@id", currentSiparisId);
                    using (var rdr = await cmd.ExecuteReaderAsync())
                    {
                        if (await rdr.ReadAsync())
                        {
                            dbSeriTarihi = rdr["SeriTarihi"] as DateTime?;
                            dbAsortisi = rdr["Asortisi"] as string;
                        }
                    }
                }
            }

            // ───────────────── Yeni Asorti değeri (UI) ─────────────────
            string uiAsortisi = comboBox4.SelectedItem?.ToString();   // null = boş
            bool asortiGirildi = !string.IsNullOrEmpty(uiAsortisi);
            bool asortiDegisti = !string.Equals(dbAsortisi ?? "", uiAsortisi ?? "", StringComparison.Ordinal);
            bool asortiSilindi = string.IsNullOrEmpty(uiAsortisi) && !string.IsNullOrEmpty(dbAsortisi);

            // ───────────────── SeriTarihi ne yapılacak? ─────────────────
            string seriClause = "";   // SQL’e eklenecek parça
            bool paramEkle = false; // @SeriTarihi parametresi gerekir mi?

            if (asortiSilindi)
            {
                // Asorti silinmeden önce onay iste: bu işlem SeriTarihi'ni sıfırlar
                var confirmResult = MessageBox.Show(
                    "Asorti değeri silinecek ve Seri Tarihi sıfırlanacak.\nBu işlem kalıbın durumunu etkileyebilir.\n\nDevam etmek istiyor musunuz?",
                    "Asortisi Sil - Onay",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (confirmResult != DialogResult.Yes) return;
                seriClause = ", SeriTarihi = NULL";
            }
            else if (asortiGirildi && (dbSeriTarihi == null || asortiDegisti))
            {
                seriClause = ", SeriTarihi = @SeriTarihi";
                paramEkle = true;
            }

            // ───────────────── UPDATE sorgusu ─────────────────
            string sql = $@"
UPDATE SiparisHeader SET
      SiparisTarihi          = @SiparisTarihi ,
      SiparisVeren           = @SiparisVeren  ,
      Cesit                  = @Cesit ,
      KalipTuru              = @KalipTuru ,
      Tedarikci              = @Tedarikci ,
      MusteriUnvani          = @MusteriUnvani ,
      Termin                 = @Termin ,
      AyakkabiNo             = @AyakkabiNo ,
      KalipKodu              = @KalipKodu ,
      ModelKodu              = @ModelKodu ,
      Barkod                 = @Barkod ,
      UretimTanimi           = @UretimTanimi ,
      Asortisi               = @Asortisi ,
      Aciklama               = @Aciklama ,
      GuncelleyenKullaniciID = @GuncelleyenKullaniciID ,
      GuncellemeTarihi       = @GuncellemeTarihi
      {seriClause}
WHERE SipID = @SipID;";

            using (var con = DatabaseHelper.GetConnection())
            using (var cmd = new SqlCommand(sql, con))
            {
                // Ana parametreler
                cmd.Parameters.AddWithValue("@SipID", currentSiparisId);
                cmd.Parameters.AddWithValue("@SiparisTarihi", dateTimePicker1.Value);
                cmd.Parameters.AddWithValue("@SiparisVeren", comboBox2.SelectedItem?.ToString() ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Cesit", comboBox1.SelectedItem?.ToString() ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@KalipTuru", comboBox5.SelectedItem?.ToString() ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Tedarikci", comboBox6.SelectedItem?.ToString() ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@MusteriUnvani", textBox5.Text.Trim() == "" ? (object)DBNull.Value : textBox5.Text.Trim());
                cmd.Parameters.AddWithValue("@Termin", textBox3.Text.Trim() == "" ? (object)DBNull.Value : Convert.ToInt32(textBox3.Text));
                cmd.Parameters.AddWithValue("@AyakkabiNo", textBox4.Text.Trim() == "" ? (object)DBNull.Value : Convert.ToInt32(textBox4.Text));
                cmd.Parameters.AddWithValue("@KalipKodu", textBox6.Text.Trim() == "" ? (object)DBNull.Value : textBox6.Text.Trim());
                cmd.Parameters.AddWithValue("@ModelKodu", textBox1.Text.Trim() == "" ? (object)DBNull.Value : textBox1.Text.Trim());
                cmd.Parameters.AddWithValue("@Barkod", textBox2.Text.Trim() == "" ? (object)DBNull.Value : textBox2.Text.Trim());
                cmd.Parameters.AddWithValue("@UretimTanimi", comboBox3.SelectedItem?.ToString() ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Asortisi", asortiGirildi ? uiAsortisi : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Aciklama", richTextBox1.Text.Trim() == "" ? (object)DBNull.Value : richTextBox1.Text.Trim());
                cmd.Parameters.AddWithValue("@GuncelleyenKullaniciID", LoginForm.LoggedInUserID);
                cmd.Parameters.AddWithValue("@GuncellemeTarihi", DateTime.Now);

                if (paramEkle)
                    cmd.Parameters.AddWithValue("@SeriTarihi", DateTime.Now);

                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!ValidateComboBox4())
            {
                return; // Doğrulama başarısızsa, işlemi durdur
            }
            UpdateSiparis();
            MessageBox.Show("Sipariş güncellendi.");
            // Form açık kalacak - kullanıcı barkod yazdırabilir
        }

        private void InitializeForm()
        {
            // Ortak form başlangıç ayarları (varsa)
        }

        private async void textBox1_TextChanged(object sender, EventArgs e)
        {
            isModelKoduReady = !string.IsNullOrEmpty(textBox1.Text.Trim());
            await CheckAndValidateModelKodu();

            string selectedModelKodu = textBox1.Text;

            // Eğer metin en az 4 karakter değilse substring işlemi yapma
            if (!string.IsNullOrEmpty(selectedModelKodu) && selectedModelKodu.Length >= 4)
            {
                // Model Koduna göre geçerli üretim tiplerini yükle
                LoadValidUretimTipleri(selectedModelKodu.Substring(0, 4)); // İlk 4 karakter prefix olabilir
            }
            // Girilen metni büyük harfe çevir
            textBox1.Text = textBox1.Text.ToUpper();

            // İmlecin son karakterde kalması için:
            textBox1.SelectionStart = textBox1.Text.Length;
        }
        private async void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            isUretimTanimiReady = comboBox3.SelectedItem != null;
            await CheckAndValidateModelKodu();
            CheckCesitAndUretimTipiCompatibility();

            // comboBox3'teki seçilen değere göre textBox6'nın yazma korumasını kaldır
            if (comboBox3.SelectedItem != null &&
                (comboBox3.SelectedItem.ToString() == "ÖZEL KALIP" || comboBox3.SelectedItem.ToString() == "Z-BİZİM KALIP"))
            {
                textBox6.ReadOnly = false; // Yazma korumasını kaldır
            }
            else
            {
                textBox6.ReadOnly = true; // Yazma korumasını etkinleştir
            }

            if (!isEditMode || !initialLoad)
            {
                var uretimTanimi = comboBox3.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(uretimTanimi))
                {
                    var mevcutKodlar = await GetMevcutKodlarAsync();

                    using (var connection = DatabaseHelper.GetConnection())
                    {
                        await connection.OpenAsync();
                        var query = "SELECT OnKod, KodAraligi FROM UretimTipHeader WHERE UretimTanimi = @UretimTanimi";
                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@UretimTanimi", uretimTanimi);
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    var onKod = reader["OnKod"].ToString();
                                    var kodAraligi = reader["KodAraligi"].ToString();
                                    var parts = kodAraligi.Split('-');
                                    if (parts.Length == 2 && int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end))
                                    {
                                        for (int i = start; i <= end; i++)
                                        {
                                            var potentialKod = onKod + i.ToString();
                                            if (!mevcutKodlar.Contains(potentialKod))
                                            {
                                                textBox6.Text = potentialKod;
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Veritabanında bu üretim tanımı için bir kayıt bulunamadı.");
                                }
                            }
                        }
                    }
                }
            }

            // İlk yükleme tamamlandıktan sonra, initialLoad'ı false olarak ayarla
            if (initialLoad)
            {
                initialLoad = false;
            }
        }
        private async Task<List<string>> GetMevcutKodlarAsync()
        {
            List<string> existingKodlari = new List<string>();
            using (var connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                var query = "SELECT KalipKodu FROM SiparisHeader";
                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        existingKodlari.Add(reader["KalipKodu"].ToString());
                    }
                }
            }
            return existingKodlari;
        }
        private async Task CheckAndValidateModelKodu()
        {
            if (isModelKoduReady && isUretimTanimiReady)
            {
                var modelKodu = textBox1.Text.Trim();
                var uretimTanimi = comboBox3.SelectedItem?.ToString();

                if (!string.IsNullOrEmpty(modelKodu) && !string.IsNullOrEmpty(uretimTanimi))
                {
                    using (var connection = DatabaseHelper.GetConnection())
                    {
                        await connection.OpenAsync();
                        var query = "SELECT ModelKontrol FROM UretimTipHeader WHERE UretimTanimi = @UretimTanimi";
                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@UretimTanimi", uretimTanimi);
                            var modelKontrol = await command.ExecuteScalarAsync() as string;

                            if (!string.IsNullOrEmpty(modelKontrol))
                            {
                                if (!ValidateModelKodu(modelKontrol, modelKodu))
                                {
                                    MessageBox.Show("Model Kodu, Üretim Tipi ile uyumlu değil. Doğru Üretim Tipini seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    comboBox3.SelectedIndex = -1;
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool ValidateModelKodu(string modelKontrol, string modelKodu)
        {
            return modelKodu.StartsWith(modelKontrol);
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ModelList modelListForm = new ModelList(textBox1.Text);
                modelListForm.Owner = this;
                var dialogResult = modelListForm.ShowDialog();
                if (dialogResult == DialogResult.OK)
                {
                    // ModelList formundan gelen SelectedBarkod değerini textBox2'ye atıyoruz.
                    textBox2.Text = modelListForm.SelectedBarkod;
                    // textBox1'i ModelList formundaki mevcut filtre metni ile güncelle
                    textBox1.Text = modelListForm.CurrentFilterText;

                    textBox2_KeyDown(textBox2, new KeyEventArgs(Keys.Enter));
                }
            }
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
        private bool ValidateComboBox4()
        {
            string selectedCesit = comboBox1.SelectedItem?.ToString();
            string selectedAsorti = comboBox4.SelectedItem?.ToString();

            if ((selectedCesit == "Seri" || selectedCesit == "Özel Seri") && string.IsNullOrEmpty(selectedAsorti))
            {
                MessageBox.Show("Lütfen Asorti (comboBox4) seçiniz.", "Eksik Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboBox4.Focus();
                return false;
            }
            else if ((selectedCesit == "Numune" || selectedCesit == "Özel Numune") && !string.IsNullOrEmpty(selectedAsorti))
            {
                MessageBox.Show("Numune veya Özel Numune seçildiğinde Asorti girilemez.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboBox4.SelectedIndex = -1; // Seçimi temizle
                return false;
            }

            return true;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckCesitAndUretimTipiCompatibility();

            string selectedCesit = comboBox1.SelectedItem?.ToString();

            if (selectedCesit == "Seri" || selectedCesit == "Özel Seri")
            {
                comboBox4.Enabled = true;
                comboBox4.BackColor = Color.LightYellow; // Zorunlu alan olduğunu belirtmek için
            }
            else
            {
                comboBox4.Enabled = false;
                comboBox4.SelectedIndex = -1; // Seçimi temizle
                comboBox4.BackColor = SystemColors.Control; // Varsayılan arka plan rengi
            }
        }


        private void CheckCesitAndUretimTipiCompatibility()
        {
            if (comboBox3.SelectedItem != null && comboBox1.SelectedItem != null)
            {
                bool isOzelInUretimTipi = comboBox3.SelectedItem.ToString().ToLower().Contains("özel");
                bool isOzelInCesit = comboBox1.SelectedItem.ToString().ToLower().Contains("özel");

                if (isOzelInUretimTipi != isOzelInCesit)
                {
                    MessageBox.Show("Çeşit ile Üretim Tipi uyuşmamaktadır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    comboBox1.SelectedIndex = -1; // Çeşit seçimini temizle
                    comboBox3.SelectedIndex = -1; // Üretim Tipi seçimini temizle
                }
            }
        }
        private async void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string barcode = textBox2.Text.Trim();
                int mdlId = -1;

                using (var connection = DatabaseHelper.GetConnection())
                {
                    await connection.OpenAsync();
                    string query = "SELECT ModelKodu, MDLID FROM ModelHeader WHERE Barkod = @Barkod";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Barkod", barcode);
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                textBox1.Text = reader["ModelKodu"].ToString();
                                mdlId = (int)reader["MDLID"];
                                LoadImages(mdlId);
                                // İlk resmi yükle
                                if (imageIds.Count > 0)
                                    LoadImage(imageIds[0]);
                            }
                            else
                            {
                                MessageBox.Show("Hatalı barkod numarası.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }
        private void LoadImages(int mdlId)
        {
            imageIds.Clear();
            imageBytesList.Clear();
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT ImageID, ImageData FROM ModelImages WHERE ModelID = @ModelID ORDER BY ImageID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ModelID", mdlId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            imageIds.Add(reader.GetInt32(0));
                            imageBytesList.Add((byte[])reader["ImageData"]);
                        }
                    }
                }
            }
            currentImageIndex = 0;

            // Combobox'ı güncelle
            UpdateComboBox();
        }
        private void LoadImage(int imageId)
        {
            int index = imageIds.IndexOf(imageId);
            if (index != -1 && index < imageBytesList.Count)
            {
                byte[] imageBytes = imageBytesList[index];
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    using (var ms = new MemoryStream(imageBytes))
                    {
                        var image = Image.FromStream(ms);
                        pictureBox1.Image = image;
                    }
                }
                else
                {
                    pictureBox1.Image = null;
                }
            }
            else
            {
                pictureBox1.Image = null;
            }
        }
        private void UpdateComboBox()
        {
            comboBox7.Items.Clear();
            for (int i = 1; i <= imageIds.Count; i++)
            {
                comboBox7.Items.Add($"Resim {i}");
            }
        }
        private void comboBox7_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedImageIndex = comboBox7.SelectedIndex;
            if (selectedImageIndex >= 0 && selectedImageIndex < imageIds.Count)
            {
                LoadImage(imageIds[selectedImageIndex]);
            }
        }
        private void Temizle()
        {
            dateTimePicker1.Value = DateTime.Now;
            comboBox2.SelectedItem = null;
            comboBox1.SelectedItem = null;
            comboBox5.SelectedItem = null;
            comboBox4.SelectedItem = null;
            comboBox6.SelectedItem = null;
            textBox5.Text = "";
            textBox2.Text = "";
            textBox4.Text = "";
            textBox6.Text = "";
            textBox1.Text = "";
            richTextBox1.Text = "";
        }
        private async void button5_Click(object sender, EventArgs e)
        {
            if (!ValidateComboBox4()) return;

            if (await IsKalipKoduExistsAsync(textBox6.Text))
            {
                MessageBox.Show("Bu Kalıp Kodu zaten kullanımda. Lütfen farklı bir kod giriniz.",
                                "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string missing = CheckRequiredFields();
            if (missing != "")
            {
                MessageBox.Show("Eksik alanlar:\n" + missing, "Uyarı",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Barkodun başına '2' ekleme
            string updatedBarkod = "2" + textBox2.Text.Substring(1);

            bool asortiGirildi = comboBox4.SelectedItem != null;

            string sql = @"
INSERT INTO SiparisHeader
(
  SiparisTarihi, SiparisVeren, Cesit, KalipTuru, Tedarikci,
  MusteriUnvani, Termin, AyakkabiNo, KalipKodu, ModelKodu,
  Barkod, UretimTanimi, Asortisi, Aciklama,
  OlusturanKullaniciID, OlusturmaTarihi, GuncellemeTarihi"
          + (asortiGirildi ? ", SeriTarihi" : "") + @"
)
OUTPUT INSERTED.SipID
VALUES
(
  @SiparisTarihi, @SiparisVeren, @Cesit, @KalipTuru, @Tedarikci,
  @MusteriUnvani, @Termin, @AyakkabiNo, @KalipKodu, @ModelKodu,
  @Barkod, @UretimTanimi, @Asortisi, @Aciklama,
  @OlusturanKullaniciID, @OlusturmaTarihi, @GuncellemeTarihi"
          + (asortiGirildi ? ", @SeriTarihi" : "") + @"
);";

            int newSipId = -1;
            using (var con = DatabaseHelper.GetConnection())
            using (var cmd = new SqlCommand(sql, con))
            {
                // ---- Parametreler ----
                cmd.Parameters.AddWithValue("@SiparisTarihi", dateTimePicker1.Value);
                cmd.Parameters.AddWithValue("@SiparisVeren", comboBox2.SelectedItem?.ToString() ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Cesit", comboBox1.SelectedItem?.ToString() ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@KalipTuru", comboBox5.SelectedItem?.ToString() ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Tedarikci", comboBox6.SelectedItem?.ToString() ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@MusteriUnvani", textBox5.Text);
                cmd.Parameters.AddWithValue("@Termin", Convert.ToInt32(textBox3.Text));
                cmd.Parameters.AddWithValue("@AyakkabiNo", Convert.ToInt32(textBox4.Text));
                cmd.Parameters.AddWithValue("@KalipKodu", textBox6.Text);
                cmd.Parameters.AddWithValue("@ModelKodu", textBox1.Text);
                cmd.Parameters.AddWithValue("@Barkod", updatedBarkod);
                cmd.Parameters.AddWithValue("@UretimTanimi", comboBox3.SelectedItem?.ToString() ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Asortisi", asortiGirildi ? comboBox4.SelectedItem.ToString() : (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Aciklama", richTextBox1.Text);
                cmd.Parameters.AddWithValue("@OlusturanKullaniciID", LoginForm.LoggedInUserID);
                cmd.Parameters.AddWithValue("@OlusturmaTarihi", DateTime.Now);
                cmd.Parameters.AddWithValue("@GuncellemeTarihi", DateTime.Now);

                if (asortiGirildi)
                    cmd.Parameters.AddWithValue("@SeriTarihi", DateTime.Now);

                await con.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                if (result != null)
                    newSipId = Convert.ToInt32(result);
            }

            // Yeni SipID'yi currentSiparisId'ye ata - barkod yazdırmak için gerekli
            currentSiparisId = newSipId;
            originalKalipKodu = textBox6.Text; // Kalıp kodunu kaydet
            
            // Düzenleme moduna geç
            isEditMode = true;
            button5.Visible = false; // Kaydet butonunu gizle
            button1.Visible = true;  // Güncelle butonunu göster

            MessageBox.Show("Yeni sipariş kaydedildi.", "Bilgi",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string CheckRequiredFields()
        {
            StringBuilder missingFields = new StringBuilder();

            if (dateTimePicker1.Value == null)
                missingFields.AppendLine("Sipariş Tarihi");

            if (comboBox2.SelectedItem == null)
                missingFields.AppendLine("Sipariş Veren");

            if (comboBox1.SelectedItem == null)
                missingFields.AppendLine("Çeşit");

            if (comboBox5.SelectedItem == null)
                missingFields.AppendLine("Kalıp Türü");

            if (comboBox6.SelectedItem == null)
                missingFields.AppendLine("Tedarikçi");

            if (string.IsNullOrWhiteSpace(textBox5.Text))
                missingFields.AppendLine("Müşteri Ünvanı");

            if (string.IsNullOrWhiteSpace(textBox3.Text))
                missingFields.AppendLine("Termin");

            if (string.IsNullOrWhiteSpace(textBox4.Text))
                missingFields.AppendLine("Ayakkabı No");

            if (string.IsNullOrWhiteSpace(textBox6.Text))
                missingFields.AppendLine("Kalıp Kodu");

            if (string.IsNullOrWhiteSpace(textBox1.Text))
                missingFields.AppendLine("Model Kodu");

            string selectedCesit = comboBox1.SelectedItem?.ToString();
            string selectedAsorti = comboBox4.SelectedItem?.ToString();

            if ((selectedCesit == "Seri" || selectedCesit == "Özel Seri") && string.IsNullOrWhiteSpace(selectedAsorti))
            {
                missingFields.AppendLine("Asorti (comboBox4)");
            }
            else if ((selectedCesit == "Numune" || selectedCesit == "Özel Numune") && !string.IsNullOrWhiteSpace(selectedAsorti))
            {
                missingFields.AppendLine("Asorti (comboBox4) boş olmalıdır.");
            }

            return missingFields.ToString();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private async void SiparisGiris_Load(object sender, EventArgs e)
        {
            button5.Visible = !isEditMode;
            button1.Visible = isEditMode;

            // ComboBox'ları önce doldur (varsayılan değerler dahil tüm seçenekler hazır olsun)
            await FillComboBoxes();
            initialLoad = true;
            await FillUretimTanimiToComboBox();

            // Varsayılan değerler SADECE yeni sipariş girişinde atanmalı.
            // Mevcut bir kaydı açarken (currentSiparisId > 0) veritabanından yüklenen
            // gerçek değerler kullanılır.
            if (currentSiparisId <= 0)
            {
                comboBox2.SelectedItem = "Ercan Koçum";
                comboBox1.SelectedItem = "Numune";
                textBox5.Text = "ZİYLAN";
            }
            else
            {
                // ComboBox'lar artık dolu — veritabanından sipariş verilerini yükle
                LoadSiparisData(currentSiparisId);

                if (isCopyMode)
                {
                    // Kopyalama: verileri yükledik ama yeni kayıt olarak kaydedilecek
                    isEditMode = false;
                    currentSiparisId = -1;
                    button1.Visible = false;
                    button5.Visible = true;
                }
            }

            // "Manuel Değişiklik" (NumuneSeri dönüşüm) akışı
            if (currentSiparisId > 0 && IsManualChange)
            {
                if (comboBox1.SelectedItem != null)
                {
                    if (comboBox1.SelectedItem.ToString() == "Numune")
                        comboBox1.SelectedItem = "Seri";
                    else if (comboBox1.SelectedItem.ToString() == "Özel Numune")
                        comboBox1.SelectedItem = "Özel Seri";
                }
                comboBox4.Enabled = true;
                comboBox4.Focus();
                comboBox4.DroppedDown = true;
            }

            textBox2.Focus();
        }
        private async Task FillUretimTanimiToComboBox()
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                string query = "SELECT DISTINCT UretimTanimi FROM UretimTipHeader ORDER BY UretimTanimi";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        comboBox3.Items.Clear();
                        while (await reader.ReadAsync())
                        {
                            string item = reader["UretimTanimi"].ToString();
                            comboBox3.Items.Add(item);
                        }
                    }
                }
            }
        }
        private async Task FillComboBoxes()
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                await connection.OpenAsync();

                // comboBox2 - Yönetici Ad Soyad
                await FillComboBoxWithQuery(comboBox2, "SELECT Ad + ' ' + Soyad AS AdSoyad FROM YonetHeader WHERE Ad IS NOT NULL AND Soyad IS NOT NULL ORDER BY Ad, Soyad", "AdSoyad");

                // comboBox4 - AsortiTanimlari
                await FillComboBoxWithQuery(comboBox4, "SELECT DISTINCT AsortiTanimlari FROM GenelTanimlar WHERE AsortiTanimlari IS NOT NULL ORDER BY AsortiTanimlari", "AsortiTanimlari");

                // comboBox5 - KalipTuru
                await FillComboBoxWithQuery(comboBox5, "SELECT DISTINCT KalipTuru FROM GenelTanimlar WHERE KalipTuru IS NOT NULL ORDER BY KalipTuru", "KalipTuru");

                // comboBox6 - CariUnvani (TEDARİKÇİ)
                await FillComboBoxWithQuery(comboBox6, "SELECT CariUnvani FROM CariKartHeader WHERE CariTipi = N'TEDARİKÇİ' ORDER BY CariUnvani", "CariUnvani");

            }
        }
        private async Task FillComboBoxWithQuery(System.Windows.Forms.ComboBox comboBox, string query, string columnName)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        comboBox.Items.Clear();
                        while (await reader.ReadAsync())
                        {
                            string item = reader[columnName].ToString();
                            comboBox.Items.Add(item);
                        }
                    }
                }
            }
        }    
        private void button2_Click(object sender, EventArgs e)
        {
            if (currentSiparisId > 0)
            {
                // Veriyi almak için DataTable oluştur
                DataTable dt = new DataTable();

                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = @"
                WITH RankedImages AS (
                    SELECT
                        MI.ModelKodu,
                        MI.ImageData,
                        ROW_NUMBER() OVER (PARTITION BY MI.ModelKodu ORDER BY (SELECT NULL)) AS RowNum
                    FROM ModelImages MI
                ),
                PivotedImages AS (
                    SELECT
                        RI.ModelKodu,
                        MAX(CASE WHEN RI.RowNum = 1 THEN RI.ImageData ELSE NULL END) AS Image1,
                        MAX(CASE WHEN RI.RowNum = 2 THEN RI.ImageData ELSE NULL END) AS Image2,
                        MAX(CASE WHEN RI.RowNum = 3 THEN RI.ImageData ELSE NULL END) AS Image3
                    FROM RankedImages RI
                    GROUP BY RI.ModelKodu
                )
                SELECT
                    SH.SipID,
                    SH.SiparisTarihi,
                    SH.ModelKodu,
                    SH.AyakkabiNo,
                    SH.Tedarikci,
                    SH.KalipKodu,
                    SH.Asortisi,
                    SH.UretimTanimi,
                    SH.Barkod,
                    PI.Image1,
                    PI.Image2,
                    PI.Image3
                FROM SiparisHeader SH
                LEFT JOIN PivotedImages PI ON SH.ModelKodu = PI.ModelKodu
                WHERE SH.SipID = @SipID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Parametreyi burada tanımlayıp set ediyoruz
                        command.Parameters.AddWithValue("@SipID", currentSiparisId);
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(dt);
                    }
                }

                // Resim sayısını kontrol et
                int imageCount = 0;
                if (dt.Rows.Count > 0)
                {
                    if (dt.Rows[0]["Image1"] != DBNull.Value) imageCount++;
                    if (dt.Rows[0]["Image2"] != DBNull.Value) imageCount++;
                    if (dt.Rows[0]["Image3"] != DBNull.Value) imageCount++;
                }

                // Uygun raporu seç
                string reportPath = @"Rapor\SiparisFormu.frx"; // Varsayılan rapor
                if (imageCount == 2) reportPath = @"Rapor\SiparisFormu2.frx";
                else if (imageCount == 3) reportPath = @"Rapor\SiparisFormu3.frx";

                // FastReport'u yükleyip veriyi rapora aktar
                Report report = new Report();
                try
                {
                    report.Load(reportPath);
                    // Veriyi rapora ekle
                    report.RegisterData(dt, "SiparisHeader");
                    report.GetDataSource("SiparisHeader").Enabled = true;
                    // Raporu hazırla ve göster
                    report.Prepare();
                    report.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Rapor oluşturulurken bir hata oluştu: " + ex.Message);
                }
                finally
                {
                    report.Dispose();
                }
            }
            else
            {
                MessageBox.Show("Lütfen önce bir sipariş seçin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private DataTable GetSiparisBarkod(int siparisId)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ModelKodu", typeof(string));
            dt.Columns.Add("KalipKodu", typeof(string));
            dt.Columns.Add("Asortisi", typeof(string));
            dt.Columns.Add("KalipTuru", typeof(string));
            dt.Columns.Add("Barkod", typeof(long));

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT ModelKodu, KalipKodu, Asortisi, KalipTuru, Barkod FROM SiparisHeader WHERE SipID = @SipID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SipID", siparisId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            DataRow newRow = dt.NewRow();
                            newRow["ModelKodu"] = reader["ModelKodu"].ToString();
                            newRow["KalipKodu"] = reader["KalipKodu"].ToString();
                            newRow["Asortisi"] = reader["Asortisi"].ToString();
                            newRow["KalipTuru"] = reader["KalipTuru"].ToString();
                            newRow["Barkod"] = Convert.ToInt64(reader["Barkod"]);
                            dt.Rows.Add(newRow);
                        }
                    }
                }
            }

            return dt;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (currentSiparisId > 0)
            {
                // Sipariş barkodunu al
                DataTable dt = GetSiparisBarkod(currentSiparisId);

                if (dt.Rows.Count > 0)
                {
                    // Tek barkod için raporu yazdır
                    PrintSingleBarkod(dt);
                }
                else
                {
                    MessageBox.Show("Barkod verisi bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Lütfen bir sipariş seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void PrintSingleBarkod(DataTable dt)
        {
            string reportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Rapor", "TabanBarkod.repx");
            if (!File.Exists(reportPath))
            {
                MessageBox.Show("Rapor dosyası bulunamadı: " + reportPath);
                return;
            }

            XtraReport report = new XtraReport();
            report.LoadLayout(reportPath);
            report.DataSource = dt;

            try
            {
                report.CreateDocument();

                ReportPrintTool printTool = new ReportPrintTool(report);
                printTool.PrintingSystem.ShowMarginsWarning = false;
                printTool.PrintingSystem.ShowPrintStatusDialog = true;

                printTool.ShowPreviewDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rapor yazdırma hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // ModelGirisKart formunu SiparisGiris referansıyla açıyoruz
            ModelGirisKart modelGirisKartForm = new ModelGirisKart(this);
            modelGirisKartForm.ShowDialog(); // Formu modally açıyoruz, yani bu form kapanmadan diğer form açılmıyor
        }

        public void SetModelKodu(string modelKodu)
        {
            textBox1.Text = modelKodu;
        }
        private void LoadValidUretimTipleri(string modelKoduPrefix)
        {
            // Önce combobox'ı temizleyin
            comboBox3.Items.Clear();

            // Eğer ModelKodu "SS" ile başlıyorsa sadece "ÖZEL KALIP" göster
            if (modelKoduPrefix.StartsWith("SS"))
            {
                comboBox3.Items.Add("ÖZEL KALIP");
            }
            else
            {
                // Geçerli Üretim Tiplerini veritabanından getirin
                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT UretimTanimi FROM UretimTipHeader WHERE ModelKontrol = @ModelKoduPrefix";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ModelKoduPrefix", modelKoduPrefix);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Üretim Tiplerini ComboBox'a ekleyin
                                comboBox3.Items.Add(reader["UretimTanimi"].ToString());
                            }
                        }
                    }
                }
            }

            // Eğer sadece bir üretim tipi varsa, otomatik olarak seçin
            if (comboBox3.Items.Count == 1)
            {
                comboBox3.SelectedIndex = 0;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Lütfen önce bir ModelKodu girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Resim Dosyaları|*.jpg;*.jpeg;*.png;*.bmp";
                ofd.Multiselect = true;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    int mdlId = GetModelIDByModelKodu(textBox1.Text);
                    if (mdlId < 0)
                    {
                        MessageBox.Show("Model bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    int eklenenSayisi = 0;
                    foreach (string fileName in ofd.FileNames)
                    {
                        if (ImageManager.AddImageToModel(mdlId, textBox1.Text, fileName, true))
                        {
                            eklenenSayisi++;
                        }
                    }

                    if (eklenenSayisi > 0)
                    {
                        LoadImages(mdlId);
                        if (imageIds.Count > 0) LoadImage(imageIds[imageIds.Count - 1]); // En son eklenen resmi göster
                        MessageBox.Show($"{eklenenSayisi} resim başarıyla eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Hiçbir resim eklenmedi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        // ModelKodu'na göre MDLID'yi döndüren yardımcı metot
        private int GetModelIDByModelKodu(string modelKodu)
        {
            int mdlId = -1;
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT MDLID FROM ModelHeader WHERE ModelKodu = @ModelKodu";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ModelKodu", modelKodu);
                    mdlId = (int)command.ExecuteScalar();
                }
            }
            return mdlId;
        }
    }
}
