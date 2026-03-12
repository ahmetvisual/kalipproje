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
using FastReport;
using System.Net.Mail;
using System.Net.Mime;

namespace kalipproje
{
    public partial class YonetimPaneli : Form
    {

        private List<string> modulAdlari = new List<string>();

        public YonetimPaneli(List<string> modulAdlari = null)
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            if (modulAdlari != null)
            {
                this.modulAdlari = modulAdlari;
            }
            dataGridView1.MouseUp += new MouseEventHandler(dataGridView1_MouseUp);
            this.Load += new EventHandler(YonetimPaneli_Load);

            // listBox1 için KeyDown eventini bağla
            listBox1.KeyDown += new KeyEventHandler(listBox1_KeyDown);
            listBox2.KeyDown += new KeyEventHandler(listBox2_KeyDown);

            // Form yüklenirken e-posta adreslerini yükle
            this.Load += new EventHandler(Form_Load);
        }

        private void YonetimPaneli_Load(object sender, EventArgs e)
        {
            LoadData();
            FillComboBoxWithUsernames();
            InitializeDataGridView2Columns(); // Önce sütunları tanımla
            FillDataGridView2WithModuleNames(); // Ardından verileri doldur
                                                // DataGridView için CellDoubleClick eventini bağla
            dataGridView1.CellDoubleClick += new DataGridViewCellEventHandler(dataGridView1_CellDoubleClick);
        }

        private void FillDataGridView2WithModuleNames()
        {
            foreach (var modulAdi in modulAdlari)
            {
                int rowIndex = dataGridView2.Rows.Add();
                dataGridView2.Rows[rowIndex].Cells["ModulAdi"].Value = modulAdi;
                dataGridView2.Rows[rowIndex].Cells["Yetki"].Value = false; // Varsayılan olarak yetkisiz olarak ayarla
            }
        }

        private void InitializeDataGridView2Columns()
        {
            // ModulAdi sütunu
            DataGridViewTextBoxColumn modulAdiColumn = new DataGridViewTextBoxColumn();
            modulAdiColumn.Name = "ModulAdi";
            modulAdiColumn.HeaderText = "Modül Adı";
            modulAdiColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; // Sütunu otomatik boyutlandır

            // Yetki sütunu (CheckBox tipinde)
            DataGridViewCheckBoxColumn yetkiColumn = new DataGridViewCheckBoxColumn();
            yetkiColumn.Name = "Yetki";
            yetkiColumn.HeaderText = "Yetki";
            yetkiColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            yetkiColumn.FalseValue = "0";
            yetkiColumn.TrueValue = "1";

            // Sütunları DataGridView'e ekle
            dataGridView2.Columns.Add(modulAdiColumn);
            dataGridView2.Columns.Add(yetkiColumn);
        }

        private void InitializeForm()
        {
            LoadData();
            CustomizeDataGridView();
            dataGridView1.MouseUp += new MouseEventHandler(dataGridView1_MouseUp);

        }

        private void CustomizeDataGridView()
        {
            // Sütun başlıkları için stil ayarları
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkGray; // Koyu gri
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White; // Beyaz yazı rengi
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold); // Segoe UI, 10pt, Kalın
            dataGridView1.EnableHeadersVisualStyles = false; // Özel stilin uygulanabilmesi için

            // Sıra (satır) için stil ayarları
            dataGridView1.RowsDefaultCellStyle.BackColor = Color.LightYellow; // Açık sarı
            dataGridView1.RowsDefaultCellStyle.ForeColor = Color.Black; // Siyah yazı rengi
            dataGridView1.RowsDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Regular); // Segoe UI, 9pt, Normal

            // Çizgilerin rengi
            dataGridView1.GridColor = Color.Gainsboro; // Gainsboro rengi

            // Alternatif sıralar için stil ayarları
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.White; // Beyaz
            dataGridView1.AlternatingRowsDefaultCellStyle.ForeColor = Color.Black; // Siyah yazı rengi
            dataGridView1.AlternatingRowsDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Regular); // Segoe UI, 9pt, Normal

            // Seçili hücreler için stil ayarları
            dataGridView1.RowsDefaultCellStyle.SelectionBackColor = Color.DarkKhaki; // Koyu Haki
            dataGridView1.RowsDefaultCellStyle.SelectionForeColor = Color.White; // Beyaz

            // Sütunları içeriğe göre otomatik olarak sığdırma
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            // Sütun genişliklerini ayarla
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Son sütunun kalan alanı doldurmasını sağla
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }
            if (dataGridView1.Columns.Count > 0)
            {
                dataGridView1.Columns[dataGridView1.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    SqlCommand cmd;

                    if (selectedYonetID.HasValue)
                    {
                        // Güncelleme işlemi
                        cmd = new SqlCommand("UPDATE YonetHeader SET KullaniciAdi = @KullaniciAdi, Sifre = @Sifre, Ad = @Ad, Soyad = @Soyad, email = @Email, Departman = @Departman, Yetki = @Yetki WHERE YonetID = @YonetID", con);
                        cmd.Parameters.AddWithValue("@YonetID", selectedYonetID.Value);
                    }
                    else
                    {
                        // Yeni kayıt ekleme işlemi
                        cmd = new SqlCommand("INSERT INTO YonetHeader (KullaniciAdi, Sifre, Ad, Soyad, email, Departman, Yetki) VALUES (@KullaniciAdi, @Sifre, @Ad, @Soyad, @Email, @Departman, @Yetki)", con);
                    }

                    cmd.Parameters.AddWithValue("@KullaniciAdi", textBox1.Text);
                    cmd.Parameters.AddWithValue("@Sifre", textBox2.Text);
                    cmd.Parameters.AddWithValue("@Ad", textBox3.Text);
                    cmd.Parameters.AddWithValue("@Soyad", textBox4.Text);
                    cmd.Parameters.AddWithValue("@Email", textBox5.Text);
                    cmd.Parameters.AddWithValue("@Departman", comboBox2.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@Yetki", checkBox1.Checked ? 1 : 0); // CheckBox değerini ekleyin

                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                // Verileri güncelle ve başarı mesajı göster
                LoadData();
                MessageBox.Show(selectedYonetID.HasValue ? "Kayıt başarıyla güncellendi!" : "Kayıt başarıyla eklendi!");

                // Formu temizle
                textBox1.Clear();
                textBox2.Clear();
                textBox3.Clear();
                textBox4.Clear();
                textBox5.Clear();
                checkBox1.Checked = false; // CheckBox'ı sıfırla
                selectedYonetID = null; // Güncelleme bitti, yeni kayıt için hazır
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }
        private void LoadData()
        {
            try
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    SqlDataAdapter da = new SqlDataAdapter("SELECT YonetID, KullaniciAdi, Sifre, Ad, Soyad, email, Departman, Yetki FROM YonetHeader", con); // Yetki sütununu ekleyin
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dataGridView1.DataSource = dt;
                    dataGridView1.Columns["YonetID"].Visible = false; // YonetID sütununu gizle
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (selectedYonetID.HasValue)
            {
                try
                {
                    using (SqlConnection con = DatabaseHelper.GetConnection())
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand("UPDATE YonetHeader SET Yetki = @Yetki WHERE YonetID = @YonetID", con);
                        cmd.Parameters.AddWithValue("@Yetki", checkBox1.Checked ? 1 : 0);
                        cmd.Parameters.AddWithValue("@YonetID", selectedYonetID.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                }
            }
        }

        private void dataGridView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hti = dataGridView1.HitTest(e.X, e.Y);
                if (hti.RowIndex != -1)
                {
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[hti.RowIndex].Selected = true;
                }
            }
        }

        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedIndex = dataGridView1.SelectedRows[0].Index;
                // YonetID değerini doğru sütundan alıyoruz
                int yonetID = Convert.ToInt32(dataGridView1["YonetID", selectedIndex].Value);

                try
                {
                    using (SqlConnection con = DatabaseHelper.GetConnection())
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand("DELETE FROM YonetHeader WHERE YonetID = @YonetID", con);
                        cmd.Parameters.AddWithValue("@YonetID", yonetID);
                        cmd.ExecuteNonQuery();
                        con.Close();

                        // DataGridView'den satırı kaldır
                        dataGridView1.Rows.RemoveAt(selectedIndex);
                        MessageBox.Show("Kayıt başarıyla silindi!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message);
                    using (SqlConnection con = DatabaseHelper.GetConnection())
                    {
                        con.Close();
                    }
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Lütfen bir kullanıcı seçiniz.");
                return;
            }

            string selectedUser = comboBox1.SelectedItem.ToString();
            try
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT ModulAdi, Yetki FROM YetkiHeader WHERE Kullanici = @Kullanici", con);
                    cmd.Parameters.AddWithValue("@Kullanici", selectedUser);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Önce tüm yetkileri false olarak ayarla
                    foreach (DataGridViewRow row in dataGridView2.Rows)
                    {
                        row.Cells["Yetki"].Value = false;
                    }

                    Form1 mainForm = (Form1)this.Owner; // Form1'e erişim
                    Dictionary<string, bool> moduleAccess = new Dictionary<string, bool>();

                    // Veritabanından alınan yetki bilgilerine göre DataGridView2'yi ve FlowLayoutPanel'i güncelle
                    foreach (DataRow dataRow in dt.Rows)
                    {
                        foreach (DataGridViewRow gridRow in dataGridView2.Rows)
                        {
                            if (gridRow.Cells["ModulAdi"].Value.ToString() == dataRow["ModulAdi"].ToString())
                            {
                                bool yetki = Convert.ToBoolean(dataRow["Yetki"]);
                                gridRow.Cells["Yetki"].Value = yetki;
                                moduleAccess[gridRow.Cells["ModulAdi"].Value.ToString()] = yetki;
                                break;
                            }
                        }
                    }

                    // Yetkileri güncelle
                    mainForm.SetModulePermissions(moduleAccess);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
            finally
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Close();
                }
            }
        }

        private void FillComboBoxWithUsernames()
        {
            try
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT KullaniciAdi FROM YonetHeader", con);
                    SqlDataReader reader = cmd.ExecuteReader();

                    comboBox1.Items.Clear(); // Mevcut öğeleri temizle

                    while (reader.Read())
                    {
                        comboBox1.Items.Add(reader["KullaniciAdi"].ToString());
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
            finally
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Close();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // comboBox1'den seçilen kullanıcı adını kontrol et
            if (comboBox1.SelectedIndex == -1 || comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir kullanıcı seçiniz.");
                return; // Daha fazla işlem yapmadan metodtan çık
            }

            try
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    string selectedUser = comboBox1.SelectedItem.ToString();

                    // Kullanıcı adına ait bir kayıt olup olmadığını kontrol et
                    SqlCommand checkUserCmd = new SqlCommand("SELECT COUNT(*) FROM YetkiHeader WHERE Kullanici = @Kullanici", con);
                    checkUserCmd.Parameters.AddWithValue("@Kullanici", selectedUser);
                    int userExists = (int)checkUserCmd.ExecuteScalar();

                    if (userExists > 0)
                    {
                        // Kullanıcı varsa, önce mevcut kayıtları sil (veya güncelle)
                        SqlCommand deleteCmd = new SqlCommand("DELETE FROM YetkiHeader WHERE Kullanici = @Kullanici", con);
                        deleteCmd.Parameters.AddWithValue("@Kullanici", selectedUser);
                        deleteCmd.ExecuteNonQuery();
                    }

                    // YetkiHeader tablosuna yeni bir kayıt ekle
                    foreach (DataGridViewRow row in dataGridView2.Rows)
                    {
                        int yetkiValue = row.Cells["Yetki"].Value.ToString() == "1" ? 1 : 0;
                        string modulAdi = row.Cells["ModulAdi"].Value.ToString();
                        SqlCommand insertCmd = new SqlCommand("INSERT INTO YetkiHeader (Kullanici, Yetki, ModulAdi) VALUES (@Kullanici, @Yetki, @ModulAdi)", con);
                        insertCmd.Parameters.AddWithValue("@Kullanici", selectedUser);
                        insertCmd.Parameters.AddWithValue("@Yetki", yetkiValue);
                        insertCmd.Parameters.AddWithValue("@ModulAdi", modulAdi);
                        insertCmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Yetki başarıyla eklendi/güncellendi!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
            finally
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Close();
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string email = textBox6.Text;

            if (!string.IsNullOrEmpty(email) && IsValidEmail(email))
            {
                listBox1.Items.Add(email);
                SaveEmailToDatabase(email); // Veritabanına kaydet
                textBox6.Clear(); // Email adresini girdikten sonra textBox'u temizle
            }
            else
            {
                MessageBox.Show("Geçerli bir e-posta adresi giriniz.");
            }
        }

        private void SaveEmailToDatabase(string email)
        {
            try
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO MailHeader (Email) VALUES (@Email)", con);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        private void LoadEmailsFromDatabase()
        {
            try
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT Email FROM MailHeader", con);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        listBox1.Items.Add(reader["Email"].ToString());
                    }
                    reader.Close();
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void DeleteEmailFromDatabase(string email)
        {
            try
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("DELETE FROM MailHeader WHERE Email = @Email", con);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (listBox1.SelectedItem != null)
                {
                    string email = listBox1.SelectedItem.ToString();
                    listBox1.Items.Remove(email);
                    DeleteEmailFromDatabase(email); // Veritabanından sil
                }
            }
        }

        private void Form_Load(object sender, EventArgs e)
        {
            LoadEmailsFromDatabase(); // Form yüklenirken e-posta adreslerini veritabanından yükle
            LoadSmtpSettingsFromDatabase(); // Form yüklenirken SMTP ayarlarını veritabanından yükle
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DataTable dt = GetReportData();
            GenerateReport(dt);
        }

        private DataTable GetReportData()
        {
            DataTable dt = new DataTable();
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
                SH.SeriTarihi,
                SH.ModelKodu,
                SH.AyakkabiNo,
                SH.Tedarikci,
                SH.KalipKodu,
                SH.Asortisi,
                SH.UretimTanimi,
                PI.Image1,
                PI.Image2,
                PI.Image3
            FROM SiparisHeader SH
            LEFT JOIN PivotedImages PI ON SH.ModelKodu = PI.ModelKodu
            WHERE SH.SeriTarihi >= DATEADD(day, -1, GETDATE());
        ";

            try
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    da.Fill(dt);
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }

            return dt;
        }

        private void GenerateReport(DataTable dt)
        {
            // FastReport raporu oluştur
            Report report = new Report();
            report.Load(@"Rapor\Seribildirim.frx");

            // Veriyi rapora bağla
            report.RegisterData(dt, "SiparisHeader");
            report.GetDataSource("SiparisHeader").Enabled = true;

            // Raporu göster
            report.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            DataTable dt = GetReportData();
            string pdfPath = GenerateReportAndSaveAsPdf(dt);

            foreach (string email in listBox1.Items)
            {
                SendReportByEmail(email, pdfPath);
            }
        }

        private string GenerateReportAndSaveAsPdf(DataTable dt)
        {
            // FastReport raporu oluştur
            Report report = new Report();
            report.Load(@"Rapor\Seribildirim.frx");

            // Veriyi rapora bağla
            report.RegisterData(dt, "SiparisHeader");
            report.GetDataSource("SiparisHeader").Enabled = true;

            // PDF olarak kaydet
            string pdfPath = @"C:\path\to\save\Seribildirim.pdf";
            report.Prepare();
            FastReport.Export.Pdf.PDFExport pdfExport = new FastReport.Export.Pdf.PDFExport();
            report.Export(pdfExport, pdfPath);

            return pdfPath;
        }

        private void SendReportByEmail(string email, string pdfPath)
        {
            try
            {
                SmtpSettings smtpSettings = GetSmtpSettings();

                if (smtpSettings == null)
                {
                    MessageBox.Show("SMTP ayarları bulunamadı. Lütfen ayarları girip tekrar deneyin.");
                    return;
                }

                MailMessage mail = new MailMessage();
                SmtpClient smtpServer = new SmtpClient(smtpSettings.ServerAdres);
                mail.From = new MailAddress(smtpSettings.Gonderen);
                mail.To.Add(email);
                mail.Subject = "Seri Bildirim Raporu";
                mail.Body = "Lütfen ekteki Seri Bildirim Raporunu inceleyiniz.";

                Attachment attachment = new Attachment(pdfPath, MediaTypeNames.Application.Pdf);
                mail.Attachments.Add(attachment);

                smtpServer.Port = smtpSettings.GidenPort;
                smtpServer.Credentials = new System.Net.NetworkCredential(smtpSettings.Gonderen, smtpSettings.SmtpPassword);
                smtpServer.EnableSsl = true;

                smtpServer.Send(mail);
                MessageBox.Show("Rapor başarıyla gönderildi!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            SaveSmtpSettings();
            LoadSmtpSettingsFromDatabase(); // Yeni eklenen SMTP ayarlarını yükle
        }

        private void listBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (listBox2.SelectedItem != null)
                {
                    string selectedItem = listBox2.SelectedItem.ToString();
                    DeleteSmtpSetting(selectedItem);
                    listBox2.Items.Remove(selectedItem);
                }
            }
        }

        private void SaveSmtpSettings()
        {
            string serverAdres = textBox7.Text;
            string gonderen = textBox8.Text;
            string smtpPassword = textBox10.Text;
            int gidenPort;

            if (string.IsNullOrEmpty(serverAdres) || string.IsNullOrEmpty(gonderen) || string.IsNullOrEmpty(smtpPassword) || !int.TryParse(textBox9.Text, out gidenPort))
            {
                MessageBox.Show("Lütfen tüm SMTP ayarlarını doğru bir şekilde giriniz.");
                return;
            }

            try
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO SmtpHeader (ServerAdres, Gonderen, SmtpPassword, GidenPort) VALUES (@ServerAdres, @Gonderen, @SmtpPassword, @GidenPort)", con);
                    cmd.Parameters.AddWithValue("@ServerAdres", serverAdres);
                    cmd.Parameters.AddWithValue("@Gonderen", gonderen);
                    cmd.Parameters.AddWithValue("@SmtpPassword", smtpPassword);
                    cmd.Parameters.AddWithValue("@GidenPort", gidenPort);
                    cmd.ExecuteNonQuery();
                    con.Close();
                    MessageBox.Show("SMTP ayarları başarıyla kaydedildi!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        private SmtpSettings GetSmtpSettings()
        {
            try
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT TOP 1 ServerAdres, Gonderen, SmtpPassword, GidenPort FROM SmtpHeader", con);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        SmtpSettings settings = new SmtpSettings
                        {
                            ServerAdres = reader["ServerAdres"].ToString(),
                            Gonderen = reader["Gonderen"].ToString(),
                            SmtpPassword = reader["SmtpPassword"].ToString(),
                            GidenPort = Convert.ToInt32(reader["GidenPort"])
                        };
                        reader.Close();
                        return settings;
                    }
                    else
                    {
                        reader.Close();
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
                return null;
            }
        }

        private void LoadSmtpSettingsFromDatabase()
        {
            try
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT ServerAdres, Gonderen, SmtpPassword, GidenPort FROM SmtpHeader", con);
                    SqlDataReader reader = cmd.ExecuteReader();
                    listBox2.Items.Clear();
                    while (reader.Read())
                    {
                        string smtpInfo = $"{reader["ServerAdres"]}, {reader["Gonderen"]}, {reader["SmtpPassword"]}, {reader["GidenPort"]}";
                        listBox2.Items.Add(smtpInfo);
                    }
                    reader.Close();
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        private void DeleteSmtpSetting(string smtpInfo)
        {
            string[] smtpParts = smtpInfo.Split(new string[] { ", " }, StringSplitOptions.None);
            if (smtpParts.Length != 4)
                return;

            string serverAdres = smtpParts[0];
            string gonderen = smtpParts[1];
            string smtpPassword = smtpParts[2];
            int gidenPort;
            if (!int.TryParse(smtpParts[3], out gidenPort))
                return;

            try
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("DELETE FROM SmtpHeader WHERE ServerAdres = @ServerAdres AND Gonderen = @Gonderen AND SmtpPassword = @SmtpPassword AND GidenPort = @GidenPort", con);
                    cmd.Parameters.AddWithValue("@ServerAdres", serverAdres);
                    cmd.Parameters.AddWithValue("@Gonderen", gonderen);
                    cmd.Parameters.AddWithValue("@SmtpPassword", smtpPassword);
                    cmd.Parameters.AddWithValue("@GidenPort", gidenPort);
                    cmd.ExecuteNonQuery();
                    con.Close();
                    MessageBox.Show("SMTP ayarı başarıyla silindi!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        private void TestEmailSettings()
        {
            try
            {
                SmtpSettings smtpSettings = GetSmtpSettings();

                if (smtpSettings == null)
                {
                    MessageBox.Show("SMTP ayarları bulunamadı. Lütfen ayarları girip tekrar deneyin.");
                    return;
                }

                MailMessage mail = new MailMessage();
                SmtpClient smtpServer = new SmtpClient(smtpSettings.ServerAdres);
                mail.From = new MailAddress(smtpSettings.Gonderen);
                mail.To.Add(smtpSettings.Gonderen); // Test için kendi adresinize gönderin
                mail.Subject = "Test Email";
                mail.Body = "Bu bir test e-postasıdır.";

                smtpServer.Port = smtpSettings.GidenPort;
                smtpServer.Credentials = new System.Net.NetworkCredential(smtpSettings.Gonderen, smtpSettings.SmtpPassword);
                smtpServer.EnableSsl = true;

                smtpServer.Send(mail);
                MessageBox.Show("Test e-postası başarıyla gönderildi!");
            }
            catch (SmtpException smtpEx)
            {
                MessageBox.Show("SMTP Hatası: " + smtpEx.Message + "\nDetaylar: " + smtpEx.InnerException?.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message + "\nDetaylar: " + ex.InnerException?.Message);
            }
        }

        private class SmtpSettings
        {
            public string ServerAdres { get; set; }
            public string Gonderen { get; set; }
            public string SmtpPassword { get; set; }
            public int GidenPort { get; set; }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            TestEmailSettings();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Geçerli bir satır seçildiğinde
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                // TextBox ve diğer bileşenlere DataGridView'deki değerleri doldur
                textBox1.Text = row.Cells["KullaniciAdi"].Value.ToString();
                textBox2.Text = row.Cells["Sifre"].Value.ToString();
                textBox3.Text = row.Cells["Ad"].Value.ToString();
                textBox4.Text = row.Cells["Soyad"].Value.ToString();
                textBox5.Text = row.Cells["email"].Value.ToString();
                comboBox2.SelectedItem = row.Cells["Departman"].Value.ToString();

                // Yetki değerini CheckBox'a ayarla
                // row.Cells["Yetki"].Value değerini int olarak kontrol edin
                bool yetkiDurumu = row.Cells["Yetki"].Value != DBNull.Value && Convert.ToInt32(row.Cells["Yetki"].Value) == 1;
                checkBox1.Checked = yetkiDurumu;

                // Kaydın güncelleneceği ID değerini sakla
                selectedYonetID = Convert.ToInt32(row.Cells["YonetID"].Value);
            }
        }


        private int? selectedYonetID = null;

    }
}
