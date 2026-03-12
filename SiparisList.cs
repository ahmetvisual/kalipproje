using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Windows.Forms.VisualStyles;
using FastReport;
using System.Text;
using DevExpress.XtraReports.UI;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.Parameters;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using FastReport.Export.Image;

namespace kalipproje
{
    public partial class SiparisList : Form
    {
        private bool isCheckBox1Checked = false;
        private bool isCheckBox2Checked = false;
        private bool isCheckBox3Checked = false;
        private string kullaniciDepartman;
        private string currentUserName;
        private Form activeForm = null;
        private DataTable barcodeDataCache;
        private System.Windows.Forms.Timer debounceTimer;

        public SiparisList(string username)
        {
            InitializeComponent();
            // Kullanıcının yetkisini kontrol et
            if (LoginForm.UserYetki == 1)
            {
                BarkodlStripMenuItem1.Enabled = false; // Yetki 1 ise menü öğesi pasif
                GirisStripMenuItem1.Enabled = false;   // Yetki 1 ise menü öğesi pasif
                GeriToolStripMenuItem.Enabled = false;
                iptalStripMenuItem1.Enabled = false;
                resimcekStripMenuItem1.Enabled = false;
                resimliraporStripMenuItem1.Enabled = false;
                resimliraporlogoStripMenuItem1.Enabled = false;
            }
            else
            {
                BarkodlStripMenuItem1.Enabled = true; // Yetki 0 ise menü öğesi aktif
                GirisStripMenuItem1.Enabled = true;   // Yetki 0 ise menü öğesi aktif
                GeriToolStripMenuItem.Enabled = true;
                iptalStripMenuItem1.Enabled = true;
            }
            this.currentUserName = username;  // Kullanıcı adını form değişkenine atayın.

            // DataGridView'i özelleştirme (sadece bir kez)
            CustomizeDataGridViewInitialSetup();

            // Event handler'ları ve diğer ayarları yapıyoruz
            this.StartPosition = FormStartPosition.CenterScreen;
            textBox1.TextChanged += textBox1_TextChanged;
            textBox2.TextChanged += textBox2_TextChanged;
            debounceTimer = new System.Windows.Forms.Timer();
            debounceTimer.Interval = 100; // 100ms bekleme süresi
            debounceTimer.Tick += DebounceTimer_Tick;
            textBox3.KeyDown += textBox3_KeyDown; // textBox3 için KeyDown olayını ekledik.
            textBox4.KeyDown += textBox4_KeyDown;
            this.dataGridView1.CellDoubleClick += new DataGridViewCellEventHandler(this.dataGridView1_CellDoubleClick);
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            checkBox2.CheckedChanged += checkBox2_CheckedChanged;
            checkBox3.CheckedChanged += checkBox3_CheckedChanged;
            radioButton4.CheckedChanged += radioButton4_CheckedChanged;
            radioButton1.CheckedChanged += radioButton1_CheckedChanged;
            radioButton2.CheckedChanged += radioButton2_CheckedChanged;
            radioButton3.CheckedChanged += radioButton3_CheckedChanged;

            // DateTimePicker ayarları
            DateTime today = DateTime.Today;
            dateTimePicker1.Value = today.AddMonths(-6); // 6 ay önceki tarih
            dateTimePicker2.Value = today;

            // Tarih değişikliklerinde veri yükleme
            dateTimePicker1.ValueChanged += dateTimePicker_ValueChanged;
            dateTimePicker2.ValueChanged += dateTimePicker_ValueChanged;

            // Kullanıcı departmanını al
            GetUserDepartment();
            // Sağ tıklama olayını ekle
            dataGridView1.MouseDown += new MouseEventHandler(dataGridView1_MouseDown);

            // Form gösterildiğinde textBox2'ye odaklan
            this.Shown += new EventHandler(SiparisList_Shown);

            // radioButton1'in varsayılan olarak seçili olmasını sağlayın
            radioButton1.Checked = true;

            // Seç sütununu başlangıçta gizli yap
            if (dataGridView1.Columns.Contains("checkBoxColumn"))
            {
                dataGridView1.Columns["checkBoxColumn"].Visible = false;
            }

            // Veri yükleme
            LoadData(0);
        }

        private void SiparisList_Shown(object sender, EventArgs e)
        {
            // textBox2'ye odaklan
            textBox2.Focus();
        }
        private void textBox4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AramaYapVeListele();
            }
        }
        private void AramaYapVeListele()
        {
            string talepNoText = textBox4.Text.Trim();
            if (int.TryParse(talepNoText, out int talepNo))
            {
                try
                {
                    using (SqlConnection con = DatabaseHelper.GetConnection())
                    {
                        con.Open();
                        // Önce TalepDetails tablosundan TabanKodu değerlerini alalım
                        string queryTabanKodu = "SELECT TabanKodu FROM TalepDetails WHERE TalepNo = @TalepNo";
                        List<string> tabanKoduList = new List<string>();
                        using (SqlCommand cmd = new SqlCommand(queryTabanKodu, con))
                        {
                            cmd.Parameters.AddWithValue("@TalepNo", talepNo);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string tabanKodu = reader["TabanKodu"].ToString();
                                    if (!string.IsNullOrEmpty(tabanKodu))
                                    {
                                        tabanKoduList.Add(tabanKodu);
                                    }
                                }
                            }
                        }

                        if (tabanKoduList.Count > 0)
                        {
                            // Şimdi SiparisHeader tablosundan KalipKodu değerleri TabanKodu listesinde olan siparişleri alalım
                            // Diğer filtreleri dikkate almıyoruz
                            string kalipKoduInClause = string.Join(",", tabanKoduList.Select((k, index) => $"@TabanKodu{index}"));
                            string querySiparis = $@"
                            SELECT 
                                0 AS Sec, SipID, ModelKodu, KalipKodu, AyakkabiNo, Tedarikci, SiparisTarihi, 
                                SeriTarihi, GelenTarih, IptalTarih, SiparisVeren, MusteriUnvani, Cesit, 
                                KalipTuru, Termin, Barkod, UretimTanimi, Asortisi, Aciklama 
                            FROM 
                                SiparisHeader 
                            WHERE 
                                KalipKodu IN ({kalipKoduInClause})
                            ORDER BY 
                                KalipKodu ASC";

                            using (SqlCommand cmd = new SqlCommand(querySiparis, con))
                            {
                                // Parametreleri ekleyelim
                                for (int i = 0; i < tabanKoduList.Count; i++)
                                {
                                    cmd.Parameters.AddWithValue($"@TabanKodu{i}", tabanKoduList[i]);
                                }

                                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                                {
                                    DataTable dt = new DataTable();
                                    sda.Fill(dt);
                                    dataGridView1.DataSource = dt;

                                    // Sütun ayarlarını güncelleyelim
                                    CustomizeDataGridViewColumns();
                                    AdjustColumnVisibility();

                                    int rowCount = dataGridView1.Rows.Count;
                                    label3.Text = $"{rowCount} adet Sipariş listelenmiştir.";
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Girilen TalepNo için TabanKodu bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            dataGridView1.DataSource = null;
                            label3.Text = "0 adet Sipariş listelenmiştir.";
                        }

                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Veri çekme hatası: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Lütfen geçerli bir Talep No giriniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            // Sağ tıklama kontrolü
            if (e.Button == MouseButtons.Right)
            {
                // Tıklanan noktanın satır bilgilerini alma
                var hit = dataGridView1.HitTest(e.X, e.Y);
                if (hit.RowIndex >= 0)
                {
                    // Satırı seçme
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[hit.RowIndex].Selected = true;
                }
            }
        }

        private void GetUserDepartment()
        {
            try
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT Departman FROM YonetHeader WHERE KullaniciAdi = @KullaniciAdi", con);
                    cmd.Parameters.AddWithValue("@KullaniciAdi", currentUserName);

                    kullaniciDepartman = cmd.ExecuteScalar()?.ToString();
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Departman bilgisi alınamadı: " + ex.Message);
            }
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BarkodSorgula();
            }
        }

        private void BarkodSorgula()
        {
            if (long.TryParse(textBox3.Text.Trim(), out long barkod))
            {
                long alternativeBarkod = GetAlternativeBarkod(barkod);

                string query = @"
                    SELECT 0 AS Sec, SipID, ModelKodu, KalipKodu, AyakkabiNo, Tedarikci, SiparisTarihi, SiparisVeren, MusteriUnvani, Cesit, KalipTuru, Termin, Barkod, UretimTanimi, Asortisi, Aciklama 
                    FROM SiparisHeader 
                    WHERE Barkod = @Barkod OR Barkod = @AlternativeBarkod";

                try
                {
                    using (SqlConnection con = DatabaseHelper.GetConnection())
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.Add("@Barkod", SqlDbType.BigInt).Value = barkod;
                            cmd.Parameters.Add("@AlternativeBarkod", SqlDbType.BigInt).Value = alternativeBarkod;

                            using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                            {
                                DataTable dt = new DataTable();
                                sda.Fill(dt);
                                dataGridView1.DataSource = dt;
                                CustomizeDataGridViewColumns(); // Sütun ayarlarını yap
                                AdjustColumnVisibility(); // Sütun görünürlüğünü ayarla
                            }
                        }
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Barkod sorgulama hatası: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Geçersiz barkod formatı. Lütfen yalnızca sayısal değerler girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private long GetAlternativeBarkod(long barkod)
        {
            string barkodStr = barkod.ToString();
            if (barkodStr.StartsWith("100000"))
            {
                return long.Parse("200000" + barkodStr.Substring(6));
            }
            else if (barkodStr.StartsWith("200000"))
            {
                return long.Parse("100000" + barkodStr.Substring(6));
            }
            return barkod;
        }

        private void dateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            int siparisDurumu = GetCurrentSiparisDurumu();
            LoadData(siparisDurumu);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            isCheckBox1Checked = checkBox1.Checked;
            LoadData(GetCurrentSiparisDurumu());
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            isCheckBox2Checked = checkBox2.Checked;
            LoadData(GetCurrentSiparisDurumu());
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            isCheckBox3Checked = checkBox3.Checked;
            LoadData(GetCurrentSiparisDurumu());
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            LoadData(GetCurrentSiparisDurumu());
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            debounceTimer.Stop(); // Her yeni karakter girişinde timer sıfırlanır
            debounceTimer.Start(); // Bekleme süresi yeniden başlar
        }

        private void DebounceTimer_Tick(object sender, EventArgs e)
        {
            debounceTimer.Stop(); // Timer durdurulur
            LoadData(GetCurrentSiparisDurumu()); // Filtreleme yapılır
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                LoadData(GetCurrentSiparisDurumu());  // Enter tuşuna basıldığında filtreleme yapılır
            }
        }

        private int GetCurrentSiparisDurumu()
        {
            if (radioButton1.Checked) return 0;
            if (radioButton2.Checked) return 1;
            if (radioButton3.Checked) return 2;
            if (radioButton4.Checked) return 3;
            return -1;
        }

        private void CustomizeDataGridViewInitialSetup()
        {
            // Checkbox sütunu ekleme
            if (!dataGridView1.Columns.Contains("checkBoxColumn"))
            {
                DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn();
                checkBoxColumn.HeaderText = "Seç";
                checkBoxColumn.Name = "checkBoxColumn";
                checkBoxColumn.Width = 38;
                dataGridView1.Columns.Insert(0, checkBoxColumn);
            }

            // Performans için DoubleBuffered özelliğini etkinleştir
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                null, dataGridView1, new object[] { true });

            // Genel stil ayarları
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);
            dataGridView1.RowsDefaultCellStyle.BackColor = Color.LightYellow;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.White;
            dataGridView1.RowsDefaultCellStyle.SelectionBackColor = Color.DarkGoldenrod;
            dataGridView1.RowsDefaultCellStyle.SelectionForeColor = Color.White;
            dataGridView1.RowsDefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Regular);
            dataGridView1.DefaultCellStyle.Padding = new Padding(2);
            dataGridView1.GridColor = Color.WhiteSmoke;

            // CellPainting olayına abone ol
            dataGridView1.CellPainting += dataGridView1_CellPainting;

            // Seç sütununu başlangıçta gizli yap
            if (dataGridView1.Columns.Contains("checkBoxColumn"))
            {
                dataGridView1.Columns["checkBoxColumn"].Visible = false;
            }
        }

        private void CustomizeDataGridViewColumns()
        {
            // Sütun dizilimlerini ayarla
            int displayIndex = 0;

            // Sütun genişliklerini ayarla ve görünürlüklerini belirle
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                if (column.Name != "checkBoxColumn")
                {
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }

                if (column.Name == "checkBoxColumn")
                {
                    column.DisplayIndex = displayIndex++;
                    column.Visible = false; // Başlangıçta gizli
                }
                else if (column.Name == "Sec")
                {
                    column.DisplayIndex = displayIndex++;
                    column.Visible = false; // Sütunu gizli yap
                }
                else if (column.Name == "SipID")
                {
                    column.DisplayIndex = displayIndex++;
                    column.Visible = false; // Sütunu gizli yap
                }
                else if (column.Name == "ModelKodu")
                {
                    column.DisplayIndex = displayIndex++;
                    column.Visible = true;
                }
                else if (column.Name == "KalipKodu")
                {
                    column.DisplayIndex = displayIndex++;
                    column.Visible = true;
                }
                else if (column.Name == "AyakkabiNo")
                {
                    column.DisplayIndex = displayIndex++;
                    column.Visible = true;
                }
                else if (column.Name == "Tedarikci")
                {
                    column.DisplayIndex = displayIndex++;
                    column.Visible = true;
                }
                else if (column.Name == "SiparisTarihi")
                {
                    column.DisplayIndex = displayIndex++;
                    column.Visible = true;
                }
                else if (column.Name == "SeriTarihi")
                {
                    column.DisplayIndex = displayIndex++;
                    // Görünürlük AdjustColumnVisibility'de ayarlanacak
                }
                else if (column.Name == "GelenTarih")
                {
                    column.DisplayIndex = displayIndex++;
                    column.Visible = true;
                }
                else if (column.Name == "IptalTarih")
                {
                    column.DisplayIndex = displayIndex++;
                    // Görünürlük AdjustColumnVisibility'de ayarlanacak
                }
                else if (column.Name == "SiparisVeren")
                {
                    column.DisplayIndex = displayIndex++;
                    column.Visible = true;
                }
                else if (column.Name == "MusteriUnvani")
                {
                    column.DisplayIndex = displayIndex++;
                    column.Visible = true;
                }
                else if (column.Name == "Cesit")
                {
                    column.DisplayIndex = displayIndex++;
                    column.Visible = false;
                }
                else if (column.Name == "KalipTuru")
                {
                    column.DisplayIndex = displayIndex++;
                    column.Visible = false;
                }
                else if (column.Name == "Termin")
                {
                    column.DisplayIndex = displayIndex++;
                    column.Visible = false;
                }
                else if (column.Name == "Barkod")
                {
                    column.DisplayIndex = displayIndex++;
                    column.Visible = false;
                }
                else if (column.Name == "UretimTanimi")
                {
                    column.DisplayIndex = displayIndex++;
                    column.Visible = true;
                }
                else if (column.Name == "Asortisi")
                {
                    column.DisplayIndex = displayIndex++;
                    column.Visible = true;
                }
                else if (column.Name == "Aciklama")
                {
                    column.DisplayIndex = displayIndex++;
                    column.Visible = true;
                }
            }

            // Son sütunu doldurma
            int lastColumnIndex = dataGridView1.Columns.Count - 1;
            if (lastColumnIndex >= 0)
            {
                dataGridView1.Columns[lastColumnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        private void AdjustColumnVisibility()
        {
            if (dataGridView1.Columns.Contains("SeriTarihi"))
            {
                dataGridView1.Columns["SeriTarihi"].Visible = !radioButton1.Checked;
            }
            if (dataGridView1.Columns.Contains("IptalTarih"))
            {
                dataGridView1.Columns["IptalTarih"].Visible = radioButton3.Checked;
            }
        }

        // Hücrelerin ve sütun başlıklarının özel çizimi için CellPainting olayı
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

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Eğer kullanıcının departmanı 'Bayi' ise işlem yapılmasın
                if (kullaniciDepartman == "Bayi")
                {
                    MessageBox.Show("Bu işlem Bayi departmanına ait kullanıcılar için devre dışı bırakılmıştır.", "Yetkisiz İşlem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (activeForm != null)
                {
                    activeForm.Close();
                }

                int siparisId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["SipID"].Value);
                SiparisGiris siparisGirisForm = new SiparisGiris(siparisId);
                siparisGirisForm.FormClosed += (s, args) => activeForm = null;
                activeForm = siparisGirisForm;
                siparisGirisForm.Show();
                // LoadData(GetCurrentSiparisDurumu());
            }
        }

        private async void LoadData(int siparisDurumu = -1)
        {
            var dt = await Task.Run(() =>
            {
                if (siparisDurumu == -1)
                {
                    siparisDurumu = GetCurrentSiparisDurumu();
                }

                string aciklamaFilter = textBox1.Text.Trim();
                string searchValue = textBox2.Text.Trim();
                StringBuilder filterCondition = new StringBuilder();

                if (siparisDurumu == 3)
                {
                    filterCondition.Append(" WHERE SiparisDurumu IN (0, 1)");
                }
                else
                {
                    filterCondition.AppendFormat(" WHERE SiparisDurumu = {0}", siparisDurumu);
                }

                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    filterCondition.AppendFormat(" AND (KalipKodu LIKE '{0}%' OR ModelKodu LIKE '{0}%')", searchValue);
                }

                if (!string.IsNullOrWhiteSpace(aciklamaFilter))
                {
                    filterCondition.AppendFormat(" AND Aciklama LIKE '%{0}%'", aciklamaFilter);
                }

                if (isCheckBox1Checked && !isCheckBox2Checked)
                {
                    filterCondition.Append(" AND Cesit LIKE '%Özel%'");
                }
                else if (!isCheckBox1Checked && isCheckBox2Checked)
                {
                    filterCondition.Append(" AND Cesit NOT LIKE '%Özel%'");
                }

                if (isCheckBox3Checked)
                {
                    filterCondition.Append(" AND Asortisi IS NOT NULL AND Asortisi <> ''");
                }

                if (!string.IsNullOrWhiteSpace(aciklamaFilter) || !string.IsNullOrWhiteSpace(searchValue))
                {
                    DateTime startDate = new DateTime(2000, 1, 1);
                    DateTime endDate = DateTime.Today;
                    filterCondition.AppendFormat(" AND SiparisTarihi BETWEEN '{0}' AND '{1}'", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
                }
                else if (!radioButton1.Checked)
                {
                    DateTime startDate = dateTimePicker1.Value.Date;
                    DateTime endDate = dateTimePicker2.Value.Date;
                    filterCondition.AppendFormat(" AND SiparisTarihi BETWEEN '{0}' AND '{1}'", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
                }

                // Sıralamayı sadece KalipKodu'na göre yapıyoruz
                string query = $"SELECT 0 AS Sec, SipID, ModelKodu, KalipKodu, AyakkabiNo, Tedarikci, SiparisTarihi, SeriTarihi, GelenTarih, IptalTarih, SiparisVeren, MusteriUnvani, Cesit, KalipTuru, Termin, Barkod, UretimTanimi, Asortisi, Aciklama FROM SiparisHeader{filterCondition} ORDER BY KalipKodu ASC";

                DataTable dtResult = new DataTable();

                try
                {
                    using (SqlConnection con = DatabaseHelper.GetConnection())
                    {
                        con.Open();
                        SqlDataAdapter sda = new SqlDataAdapter(query, con);
                        sda.Fill(dtResult);
                        con.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Veri yükleme hatası: " + ex.Message);
                }

                return dtResult;
            });

            dataGridView1.DataSource = dt;

            // Sütun ayarlarını DataSource atandıktan sonra yapıyoruz
            CustomizeDataGridViewColumns();
            AdjustColumnVisibility();

            int rowCount = dataGridView1.Rows.Count;
            label3.Text = $"{rowCount} adet Sipariş listelenmiştir.";
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                dateTimePicker1.Enabled = false;
                dateTimePicker2.Enabled = false;
                LoadData(0);
            }
            else
            {
                dateTimePicker1.Enabled = true;
                dateTimePicker2.Enabled = true;
            }
            AdjustColumnVisibility();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                LoadData(1);
            }
            AdjustColumnVisibility();
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                LoadData(2);
            }
            AdjustColumnVisibility();
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
            {
                LoadData(3);
            }
            AdjustColumnVisibility();
        }

        private void tümünüSecToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                DataGridViewCheckBoxCell checkBox = row.Cells["checkBoxColumn"] as DataGridViewCheckBoxCell;
                checkBox.Value = true;
            }
        }
        private async void BarkodlStripMenuItem1_Click(object sender, EventArgs e)
        {
            // DataGridView'da yapılan son düzenlemelerin işlenmesini sağlar
            dataGridView1.EndEdit();

            // Seç sütunu görünür değilse, görünür yap ve kullanıcıya seçim yapmasını söyle
            if (!dataGridView1.Columns["checkBoxColumn"].Visible)
            {
                dataGridView1.Columns["checkBoxColumn"].Visible = true;
                MessageBox.Show("Lütfen yazdırmak istediğiniz barkodları seçin ve tekrar tıklayın.");
                return;
            }

            // Seç sütunu görünürse, seçili satırları işle
            if (barcodeDataCache == null)
            {
                LoadDataIntoCache(); // Eğer önbellek boşsa, verileri yükle
            }

            DataTable selectedData = new DataTable();
            selectedData.Columns.Add("ModelKodu", typeof(string));
            selectedData.Columns.Add("KalipKodu", typeof(string)); // KalipKodu sütununu ekleyin
            selectedData.Columns.Add("Asortisi", typeof(string)); // Asortisi sütununu ekleyin
            selectedData.Columns.Add("KalipTuru", typeof(string)); // KalipTuru sütununu ekleyin
            selectedData.Columns.Add("Barkod", typeof(long)); // Barkod sütununu long olarak ayarlayın

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (Convert.ToBoolean(row.Cells["checkBoxColumn"].Value))
                {
                    int sipId = Convert.ToInt32(row.Cells["SipID"].Value);
                    DataRow[] filteredRows = barcodeDataCache.Select($"SipID = {sipId}");
                    if (filteredRows.Length > 0)
                    {
                        foreach (DataRow filteredRow in filteredRows)
                        {
                            DataRow newRow = selectedData.NewRow();
                            newRow["ModelKodu"] = filteredRow["ModelKodu"];
                            newRow["KalipKodu"] = filteredRow["KalipKodu"];
                            newRow["Asortisi"] = filteredRow["Asortisi"];
                            newRow["KalipTuru"] = filteredRow["KalipTuru"];
                            newRow["Barkod"] = Convert.ToInt64(filteredRow["Barkod"]); // Barkod'u long olarak dönüştür
                            selectedData.Rows.Add(newRow);
                        }
                    }
                }
            }

            if (selectedData.Rows.Count > 0)
            {
                await Task.Run(() => PrintDevExpressReport(selectedData)); // Yazdırma işlemini arka planda çalıştır

                // İşlem tamamlandıktan sonra seç sütununu gizle ve seçimleri temizle
                dataGridView1.Columns["checkBoxColumn"].Visible = false;
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    row.Cells["checkBoxColumn"].Value = false;
                }
            }
            else
            {
                MessageBox.Show("Lütfen en az bir satır seçin.");
            }
        }
        private void LoadDataIntoCache()
        {
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string query = "SELECT SipID, ModelKodu, KalipKodu, Asortisi, KalipTuru, CAST(Barkod AS BIGINT) AS Barkod FROM SiparisHeader"; // Barkod'u long olarak dönüştürün
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);
                    barcodeDataCache = dataTable; // Verileri önbelleğe al
                }
            }
        }
        private void PrintDevExpressReport(DataTable selectedData)
        {
            XtraReport report = new XtraReport();
            string reportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Rapor", "TabanBarkod.repx");
            if (!File.Exists(reportPath))
            {
                MessageBox.Show("Rapor dosyası bulunamadı: " + reportPath);
                return;
            }
            report.LoadLayout(reportPath);
            report.DataSource = selectedData;

            // Verilerin doğru şekilde bağlandığından emin olmak için
            report.DataMember = "SiparisHeader"; // DataMember adını doğru şekilde ayarlayın

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
        private void KopyalaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                if (activeForm != null)
                {
                    activeForm.Close();
                }

                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                if (selectedRow != null)
                {
                    int siparisId = Convert.ToInt32(selectedRow.Cells["SipID"].Value);
                    SiparisGiris siparisGirisForm = new SiparisGiris();
                    siparisGirisForm.LoadSiparisForCopy(siparisId);
                    siparisGirisForm.FormClosed += (s, args) => activeForm = null;
                    activeForm = siparisGirisForm;
                    siparisGirisForm.Show();
                }
            }
            else
            {
                MessageBox.Show("Lütfen bir satır seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Mevcut radioButton seçimini dikkate alarak LoadData metodunu çağır
            int siparisDurumu = GetCurrentSiparisDurumu();
            LoadData(siparisDurumu);
        }
        private void kalıpRaporuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RaporuGoster();
        }

        private void kalıpRaporuResimliToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResimliRaporuGoster(@"Rapor\resimli_rapor_3lu.frx");
        }

        private void kalıpRaporuResimli2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResimliRaporuGoster(@"Rapor\resimli_rapor_3lu_2.frx");
        }

        private DataTable CopyDataTableFromDataGridView()
        {
            DataTable dt = new DataTable();

            // Sütunları ekle
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                if (column.Visible) // Sadece görünen sütunları ekleyin
                {
                    dt.Columns.Add(column.Name, column.ValueType ?? typeof(string));
                }
            }

            // Görüntülenen satırları ekle
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Visible)
                {
                    DataRow newRow = dt.NewRow();
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (dataGridView1.Columns[cell.ColumnIndex].Visible) // Sadece görünen hücreleri ekleyin
                        {
                            newRow[cell.OwningColumn.Name] = cell.Value ?? DBNull.Value;
                        }
                    }
                    dt.Rows.Add(newRow);
                }
            }

            return dt;
        }
        private void RaporuGoster()
        {
            DataTable dtForReport = CopyDataTableFromDataGridView();

            if (dtForReport == null || dtForReport.Rows.Count == 0)
            {
                MessageBox.Show("Rapor için yeterli veri yok.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var report = new Report())
            {
                try
                {
                    report.Load(@"Rapor\kaliprapor10.frx");
                    report.RegisterData(dtForReport, "SiparisHeader");
                    report.Prepare();
                    report.ShowPrepared();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Rapor yükleme hatası: " + ex.Message);
                }
            }
        }
        private void ResimliRaporuGoster(string raporYolu)
        {
            DataTable dtForReport = CopyDataTableFromDataGridView();

            if (dtForReport == null || dtForReport.Rows.Count == 0)
            {
                MessageBox.Show("Resimli rapor için yeterli veri yok.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // DataGridView'deki tüm görünen satırların ModelKodu değerlerini alın
            List<string> listedModelKodus = new List<string>();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Visible)
                {
                    listedModelKodus.Add($"'{row.Cells["ModelKodu"].Value.ToString()}'");
                }
            }

            if (listedModelKodus.Count == 0)
            {
                MessageBox.Show("Resimli rapor için listelenmiş satır bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string modelKoduFilter = string.Join(", ", listedModelKodus);

            using (var report = new Report())
            {
                try
                {
                    using (SqlConnection con = DatabaseHelper.GetConnection())
                    {
                        con.Open();
                        string query = $@"
                    WITH RankedImages AS (
                        SELECT
                            MI.ModelKodu,
                            MI.ImageData,
                            ROW_NUMBER() OVER (PARTITION BY MI.ModelKodu ORDER BY (SELECT NULL)) AS RowNum
                        FROM ModelImages MI
                        WHERE MI.ModelKodu IN ({modelKoduFilter})
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
                        SH.ModelKodu,
                        SH.AyakkabiNo,
                        SH.Tedarikci,
                        SH.KalipKodu,
                        SH.Asortisi,
                        SH.Barkod,
                        PI.Image1,
                        PI.Image2,
                        PI.Image3
                    FROM SiparisHeader SH
                    LEFT JOIN PivotedImages PI ON SH.ModelKodu = PI.ModelKodu
                    WHERE SH.ModelKodu IN ({modelKoduFilter})";

                        SqlDataAdapter sda = new SqlDataAdapter(query, con);
                        DataTable imageDataTable = new DataTable();
                        sda.Fill(imageDataTable);
                        con.Close();

                        // Merge the two DataTables
                        DataTable mergedDataTable = new DataTable();

                        foreach (DataColumn col in dtForReport.Columns)
                        {
                            mergedDataTable.Columns.Add(col.ColumnName, col.DataType);
                        }
                        mergedDataTable.Columns.Add("Image1", typeof(byte[]));
                        mergedDataTable.Columns.Add("Image2", typeof(byte[]));
                        mergedDataTable.Columns.Add("Image3", typeof(byte[]));

                        foreach (DataRow row in dtForReport.Rows)
                        {
                            DataRow newRow = mergedDataTable.NewRow();
                            newRow.ItemArray = row.ItemArray;
                            DataRow[] imageRows = imageDataTable.Select($"ModelKodu = '{row["ModelKodu"]}'");

                            if (imageRows.Length > 0)
                            {
                                newRow["Image1"] = imageRows[0]["Image1"];
                                newRow["Image2"] = imageRows[0]["Image2"];
                                newRow["Image3"] = imageRows[0]["Image3"];
                            }

                            mergedDataTable.Rows.Add(newRow);
                        }

                        // Parametreyle gelen rapor yolunu yükle
                        report.Load(raporYolu);
                        report.RegisterData(mergedDataTable, "ressam1");  // Aynı 'ressam1' ismini kullanalım
                        report.Prepare();
                        report.ShowPrepared();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Resimli rapor yükleme hatası: " + ex.Message + "\nStackTrace: " + ex.StackTrace);
                }
            }
        }
        private void ModelStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                string modelKodu = dataGridView1.SelectedRows[0].Cells["ModelKodu"].Value.ToString();
                ModelResmi modelResmiForm = new ModelResmi(modelKodu);
                modelResmiForm.Show();
            }
            else
            {
                MessageBox.Show("Lütfen bir model seçin.");
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            LoadData(GetCurrentSiparisDurumu());
        }

        private void GeriToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bekleyen Listesine geri almak istediğinize emin misiniz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection con = DatabaseHelper.GetConnection())
                    {
                        con.Open();
                        foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                        {
                            int siparisId = Convert.ToInt32(row.Cells["SipID"].Value);
                            SqlCommand cmd = new SqlCommand("UPDATE SiparisHeader SET SiparisDurumu = 0 WHERE SipID = @SipID", con);
                            cmd.Parameters.AddWithValue("@SipID", siparisId);
                            cmd.ExecuteNonQuery();
                        }
                        con.Close();
                        LoadData(GetCurrentSiparisDurumu());
                        MessageBox.Show("Seçili siparişler başarıyla geri alındı.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Sipariş geri alma hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            // "Hayır" seçeneği seçildiğinde herhangi bir işlem yapılmaz.
        }
        private void SeriStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int siparisId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["SipID"].Value);

                SiparisGiris siparisGirisForm = new SiparisGiris(siparisId)
                {
                    IsManualChange = true
                };

                siparisGirisForm.Show();
            }
            else
            {
                MessageBox.Show("Lütfen bir satır seçiniz.");
            }
        }
        private void GirisStripMenuItem1_Click(object sender, EventArgs e)
        {
            // DataGridView'da yapılan son düzenlemelerin işlenmesini sağlar
            dataGridView1.EndEdit();

            // Seç sütunu görünür değilse, görünür yap ve kullanıcıya seçim yapmasını söyle
            if (!dataGridView1.Columns["checkBoxColumn"].Visible)
            {
                dataGridView1.Columns["checkBoxColumn"].Visible = true;
                MessageBox.Show("Lütfen durumunu güncellemek istediğiniz siparişleri seçin ve tekrar tıklayın.");
                return;
            }

            // Seç sütunu görünürse, seçili satırların durumunu güncelle
            bool isUpdated = false;
            try
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        DataGridViewCheckBoxCell checkBoxCell = row.Cells["checkBoxColumn"] as DataGridViewCheckBoxCell;
                        if (checkBoxCell != null && Convert.ToBoolean(checkBoxCell.Value))
                        {
                            int sipId = Convert.ToInt32(row.Cells["SipID"].Value);
                            SqlCommand cmd = new SqlCommand("UPDATE SiparisHeader SET SiparisDurumu = 1, GelenTarih = @GelenTarih WHERE SipID = @SipID", con);
                            cmd.Parameters.AddWithValue("@SipID", sipId);
                            cmd.Parameters.AddWithValue("@GelenTarih", DateTime.Now);
                            cmd.ExecuteNonQuery();
                            isUpdated = true;
                        }
                    }
                    con.Close();

                    if (isUpdated)
                    {
                        MessageBox.Show("Seçili siparişlerin durumu güncellendi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadData(GetCurrentSiparisDurumu());

                        // İşlem tamamlandıktan sonra seç sütununu gizle ve seçimleri temizle
                        dataGridView1.Columns["checkBoxColumn"].Visible = false;
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            row.Cells["checkBoxColumn"].Value = false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Lütfen en az bir sipariş seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Durum güncelleme hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void iptalStripMenuItem1_Click(object sender, EventArgs e)
        {
            // DataGridView'da yapılan son düzenlemelerin işlenmesini sağlar
            dataGridView1.EndEdit();

            // Seç sütunu görünür değilse, görünür yap ve kullanıcıya seçim yapmasını söyle
            if (!dataGridView1.Columns["checkBoxColumn"].Visible)
            {
                dataGridView1.Columns["checkBoxColumn"].Visible = true;
                MessageBox.Show("Lütfen iptal etmek istediğiniz siparişleri seçin ve tekrar tıklayın.");
                return;
            }

            // Seç sütunu görünürse, seçili satırların durumunu güncelle
            bool isUpdated = false;
            try
            {
                using (SqlConnection con = DatabaseHelper.GetConnection())
                {
                    con.Open();
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        DataGridViewCheckBoxCell checkBoxCell = row.Cells["checkBoxColumn"] as DataGridViewCheckBoxCell;
                        if (checkBoxCell != null && Convert.ToBoolean(checkBoxCell.Value))
                        {
                            int sipId = Convert.ToInt32(row.Cells["SipID"].Value);
                            SqlCommand cmd = new SqlCommand("UPDATE SiparisHeader SET SiparisDurumu = 2, IptalTarih = @IptalTarih WHERE SipID = @SipID", con);
                            cmd.Parameters.AddWithValue("@SipID", sipId);
                            cmd.Parameters.AddWithValue("@IptalTarih", DateTime.Now);
                            cmd.ExecuteNonQuery();
                            isUpdated = true;
                        }
                    }
                    con.Close();

                    if (isUpdated)
                    {
                        MessageBox.Show("Seçili siparişlerin durumu iptal edildi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadData(GetCurrentSiparisDurumu());

                        // İşlem tamamlandıktan sonra seç sütununu gizle ve seçimleri temizle
                        dataGridView1.Columns["checkBoxColumn"].Visible = false;
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            row.Cells["checkBoxColumn"].Value = false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Lütfen en az bir sipariş seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Durum güncelleme hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void resimcekStripMenuItem1_Click(object sender, EventArgs e)
        {
            // DataGridView'deki checkbox durumlarını kaydetmek için düzenlemeleri tamamla
            dataGridView1.EndEdit();

            // Eğer "Seç" sütunu görünmüyorsa kullanıcıya seçim yapmasını söyle
            if (!dataGridView1.Columns["checkBoxColumn"].Visible)
            {
                dataGridView1.Columns["checkBoxColumn"].Visible = true;
                MessageBox.Show("Lütfen indirilecek resimleri seçin ve tekrar tıklayın.", "Seçim Yapın", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Seçilen satırların ModelKodu ve KalipKodu değerlerini toplamak için bir liste oluşturun
            var selectedItems = new List<(string ModelKodu, string KalipKodu)>();

            // Seçilen satırları checkbox durumuna göre listele
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit); // Her bir checkbox seçim durumunu kaydet
                if (Convert.ToBoolean(row.Cells["checkBoxColumn"].Value))
                {
                    string modelKodu = row.Cells["ModelKodu"].Value.ToString();
                    string kalipKodu = row.Cells["KalipKodu"].Value.ToString();
                    selectedItems.Add((ModelKodu: modelKodu, KalipKodu: kalipKodu));
                }
            }

            // Eğer hiçbir satır seçilmediyse uyarı ver
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir satır seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Klasör seçimi yapılması için FolderBrowserDialog aç
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = fbd.SelectedPath;

                    // İşlem başladığında kullanıcıyı bilgilendirmek için label4'ü ayarla
                    label4.Text = "İşlem devam ediyor, lütfen bekleyin...";
                    label4.Visible = true;

                    // ProgressBar'ı indeterminate moda al (Marquee)
                    progressBar1.Style = ProgressBarStyle.Marquee;
                    progressBar1.MarqueeAnimationSpeed = 30;

                    int totalImages = 0;

                    // Toplam indirilmesi gereken resim sayısını hesapla
                    await Task.Run(() =>
                    {
                        using (SqlConnection connection = DatabaseHelper.GetConnection())
                        {
                            connection.Open();
                            foreach (var item in selectedItems)
                            {
                                string countQuery = "SELECT COUNT(*) FROM ModelImages WHERE ModelKodu = @ModelKodu";
                                using (SqlCommand countCommand = new SqlCommand(countQuery, connection))
                                {
                                    countCommand.Parameters.AddWithValue("@ModelKodu", item.ModelKodu);
                                    int imageCount = (int)countCommand.ExecuteScalar();
                                    totalImages += imageCount;
                                }
                            }
                            connection.Close();
                        }
                    });

                    if (totalImages == 0)
                    {
                        // İşlem tamamlandığında label'ı temizle veya gizle
                        label4.Text = "";
                        label4.Visible = false;

                        progressBar1.Style = ProgressBarStyle.Blocks;
                        progressBar1.Value = 0;

                        MessageBox.Show("Seçili siparişler için indirilecek resim bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // "Seç" sütununu gizle ve seçimleri temizle
                        dataGridView1.Columns["checkBoxColumn"].Visible = false;
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            row.Cells["checkBoxColumn"].Value = false;
                        }

                        return;
                    }

                    // ProgressBar'ı determinate moda geri al
                    progressBar1.Style = ProgressBarStyle.Blocks;
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = totalImages;
                    progressBar1.Value = 0;

                    // İlerlemeyi raporlamak için IProgress<int> kullan
                    var progress = new Progress<int>(value =>
                    {
                        progressBar1.Value = value;
                        int percentage = (int)((value * 100) / totalImages);
                        label4.Text = $"İşlem devam ediyor... %{percentage}";
                    });

                    int progressValue = 0;

                    await Task.Run(() =>
                    {
                        // Veritabanından resimleri indir ve dosyaları kaydet
                        using (SqlConnection connection = DatabaseHelper.GetConnection())
                        {
                            connection.Open();

                            foreach (var item in selectedItems)
                            {
                                string modelKodu = item.ModelKodu;
                                string kalipKodu = item.KalipKodu;
                                int imageIndex = 0;

                                string query = "SELECT ImageData FROM ModelImages WHERE ModelKodu = @ModelKodu";
                                using (SqlCommand command = new SqlCommand(query, connection))
                                {
                                    command.Parameters.AddWithValue("@ModelKodu", modelKodu);

                                    using (SqlDataReader reader = command.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            byte[] imageData = (byte[])reader["ImageData"];
                                            string fileName = Path.Combine(selectedPath, $"{kalipKodu}{(imageIndex > 0 ? $" ({imageIndex})" : "")}.jpg");
                                            File.WriteAllBytes(fileName, imageData);
                                            imageIndex++;

                                            // İlerlemeyi güncelle
                                            progressValue++;
                                            ((IProgress<int>)progress).Report(progressValue);
                                        }
                                    }
                                }
                            }
                            connection.Close();
                        }
                    });

                    // İşlem tamamlandığında label'ı temizle veya gizle
                    label4.Text = "";
                    label4.Visible = false;

                    MessageBox.Show("Seçili resimler başarıyla indirildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // İşlem tamamlandıktan sonra "Seç" sütununu gizle ve seçimleri temizle
                    dataGridView1.Columns["checkBoxColumn"].Visible = false;
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        row.Cells["checkBoxColumn"].Value = false;
                    }

                    // ProgressBar'ı sıfırla
                    progressBar1.Value = 0;
                }
            }
        }
        private async void resimliraporlogoStripMenuItem1_Click(object sender, EventArgs e)
        {
            // DataGridView'deki checkbox durumlarını kaydetmek için düzenlemeleri tamamla
            dataGridView1.EndEdit();

            // Eğer "Seç" sütunu görünmüyorsa kullanıcıya seçim yapmasını söyle
            if (!dataGridView1.Columns["checkBoxColumn"].Visible)
            {
                dataGridView1.Columns["checkBoxColumn"].Visible = true;
                MessageBox.Show("Lütfen raporlarını almak istediğiniz siparişleri seçin ve tekrar tıklayın.", "Seçim Yapın", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Seçilen satırların SipID, ModelKodu ve KalipKodu değerlerini toplamak için bir liste oluşturun
            var selectedItems = new List<(int SipID, string ModelKodu, string KalipKodu)>();

            // Seçilen satırları checkbox durumuna göre listele
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit); // Her bir checkbox seçim durumunu kaydet
                if (Convert.ToBoolean(row.Cells["checkBoxColumn"].Value))
                {
                    int sipID = Convert.ToInt32(row.Cells["SipID"].Value);
                    string modelKodu = row.Cells["ModelKodu"].Value.ToString();
                    string kalipKodu = row.Cells["KalipKodu"].Value.ToString();
                    selectedItems.Add((SipID: sipID, ModelKodu: modelKodu, KalipKodu: kalipKodu));
                }
            }

            // Eğer hiçbir satır seçilmediyse uyarı ver
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir satır seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Klasör seçimi yapılması için FolderBrowserDialog aç
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = fbd.SelectedPath;

                    // İlerlemeyi göstermek için ProgressBar ve Label kullanmak isterseniz burada ayarlayabilirsiniz
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = selectedItems.Count;
                    progressBar1.Value = 0;
                    label4.Text = "İşlem devam ediyor, lütfen bekleyin...";
                    label4.Visible = true;

                    // İlerlemeyi raporlamak için IProgress<int> kullanabilirsiniz
                    var progress = new Progress<int>(value =>
                    {
                        progressBar1.Value = value;
                        int percentage = (int)((value * 100) / selectedItems.Count);
                        label4.Text = $"İşlem devam ediyor... %{percentage}";
                    });

                    int progressValue = 0;

                    await Task.Run(() =>
                    {
                        foreach (var item in selectedItems)
                        {
                            int sipID = item.SipID;
                            string modelKodu = item.ModelKodu;
                            string kalipKodu = item.KalipKodu;

                            // Veriyi almak için DataTable oluştur
                            DataTable dt = new DataTable();

                            using (var connection = DatabaseHelper.GetConnection())
                            {
                                connection.Open();

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
                                    command.Parameters.AddWithValue("@SipID", sipID);
                                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                                    adapter.Fill(dt);
                                }

                                connection.Close();
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
                                // Raporu hazırla
                                report.Prepare();

                                // Raporu .jpeg olarak kaydet
                                string fileName = Path.Combine(selectedPath, $"{kalipKodu}.jpeg");
                                using (FastReport.Export.Image.ImageExport imageExport = new FastReport.Export.Image.ImageExport())
                                {
                                    imageExport.ImageFormat = ImageExportFormat.Jpeg; // Düzeltme burada yapıldı
                                    imageExport.Resolution = 300;
                                    imageExport.JpegQuality = 100;
                                    imageExport.Export(report, fileName);
                                }
                            }
                            catch (Exception ex)
                            {
                                // Hata mesajını kaydedebilir veya kullanıcıya bildirebilirsiniz
                                MessageBox.Show($"SipID {sipID} için rapor oluşturulurken bir hata oluştu: {ex.Message}");
                            }
                            finally
                            {
                                report.Dispose();
                            }

                            // İlerlemeyi güncelle
                            progressValue++;
                            ((IProgress<int>)progress).Report(progressValue);
                        }
                    });

                    // İşlem tamamlandığında kullanıcıya bilgi ver
                    MessageBox.Show("Seçili siparişlerin raporları başarıyla kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // İşlem tamamlandıktan sonra "Seç" sütununu gizle ve seçimleri temizle
                    dataGridView1.Columns["checkBoxColumn"].Visible = false;
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        row.Cells["checkBoxColumn"].Value = false;
                    }

                    // ProgressBar ve Label'ı sıfırla
                    progressBar1.Value = 0;
                    label4.Text = "";
                    label4.Visible = false;
                }
            }
        }
        private async void resimliraporStripMenuItem1_Click(object sender, EventArgs e)
        {
            // DataGridView'deki checkbox durumlarını kaydetmek için düzenlemeleri tamamla
            dataGridView1.EndEdit();

            // Eğer "Seç" sütunu görünmüyorsa kullanıcıya seçim yapmasını söyle
            if (!dataGridView1.Columns["checkBoxColumn"].Visible)
            {
                dataGridView1.Columns["checkBoxColumn"].Visible = true;
                MessageBox.Show("Lütfen raporlarını almak istediğiniz siparişleri seçin ve tekrar tıklayın.", "Seçim Yapın", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Seçilen satırların SipID, ModelKodu ve KalipKodu değerlerini toplamak için bir liste oluşturun
            var selectedItems = new List<(int SipID, string ModelKodu, string KalipKodu)>();

            // Seçilen satırları checkbox durumuna göre listele
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit); // Her bir checkbox seçim durumunu kaydet
                if (Convert.ToBoolean(row.Cells["checkBoxColumn"].Value))
                {
                    int sipID = Convert.ToInt32(row.Cells["SipID"].Value);
                    string modelKodu = row.Cells["ModelKodu"].Value.ToString();
                    string kalipKodu = row.Cells["KalipKodu"].Value.ToString();
                    selectedItems.Add((SipID: sipID, ModelKodu: modelKodu, KalipKodu: kalipKodu));
                }
            }

            // Eğer hiçbir satır seçilmediyse uyarı ver
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir satır seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Klasör seçimi yapılması için FolderBrowserDialog aç
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = fbd.SelectedPath;

                    // İlerlemeyi göstermek için ProgressBar ve Label kullanmak isterseniz burada ayarlayabilirsiniz
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = selectedItems.Count;
                    progressBar1.Value = 0;
                    label4.Text = "İşlem devam ediyor, lütfen bekleyin...";
                    label4.Visible = true;

                    // İlerlemeyi raporlamak için IProgress<int> kullanabilirsiniz
                    var progress = new Progress<int>(value =>
                    {
                        progressBar1.Value = value;
                        int percentage = (int)((value * 100) / selectedItems.Count);
                        label4.Text = $"İşlem devam ediyor... %{percentage}";
                    });

                    int progressValue = 0;

                    await Task.Run(() =>
                    {
                        foreach (var item in selectedItems)
                        {
                            int sipID = item.SipID;
                            string modelKodu = item.ModelKodu;
                            string kalipKodu = item.KalipKodu;

                            // Veriyi almak için DataTable oluştur
                            DataTable dt = new DataTable();

                            using (var connection = DatabaseHelper.GetConnection())
                            {
                                connection.Open();

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
                                    command.Parameters.AddWithValue("@SipID", sipID);
                                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                                    adapter.Fill(dt);
                                }

                                connection.Close();
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
                            string reportPath = @"Rapor\SiparisFormuLsuz.frx"; // Varsayılan rapor
                            if (imageCount == 2) reportPath = @"Rapor\SiparisFormu2Lsuz.frx";
                            else if (imageCount == 3) reportPath = @"Rapor\SiparisFormu3Lsuz.frx";

                            // FastReport'u yükleyip veriyi rapora aktar
                            Report report = new Report();
                            try
                            {
                                report.Load(reportPath);
                                // Veriyi rapora ekle
                                report.RegisterData(dt, "SiparisHeader");
                                report.GetDataSource("SiparisHeader").Enabled = true;
                                // Raporu hazırla
                                report.Prepare();

                                // Raporu .jpeg olarak kaydet
                                string fileName = Path.Combine(selectedPath, $"{kalipKodu}.jpeg");
                                using (FastReport.Export.Image.ImageExport imageExport = new FastReport.Export.Image.ImageExport())
                                {
                                    imageExport.ImageFormat = ImageExportFormat.Jpeg; // Düzeltme burada yapıldı
                                    imageExport.Resolution = 300;
                                    imageExport.JpegQuality = 100;
                                    imageExport.Export(report, fileName);
                                }
                            }
                            catch (Exception ex)
                            {
                                // Hata mesajını kaydedebilir veya kullanıcıya bildirebilirsiniz
                                MessageBox.Show($"SipID {sipID} için rapor oluşturulurken bir hata oluştu: {ex.Message}");
                            }
                            finally
                            {
                                report.Dispose();
                            }

                            // İlerlemeyi güncelle
                            progressValue++;
                            ((IProgress<int>)progress).Report(progressValue);
                        }
                    });

                    // İşlem tamamlandığında kullanıcıya bilgi ver
                    MessageBox.Show("Seçili siparişlerin raporları başarıyla kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // İşlem tamamlandıktan sonra "Seç" sütununu gizle ve seçimleri temizle
                    dataGridView1.Columns["checkBoxColumn"].Visible = false;
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        row.Cells["checkBoxColumn"].Value = false;
                    }
                    // ProgressBar ve Label'ı sıfırla
                    progressBar1.Value = 0;
                    label4.Text = "";
                    label4.Visible = false;
                }
            }
        }

        private async void resimliraportekresimtoolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // 1) DataGridView'deki düzenlemeleri bitir
            dataGridView1.EndEdit();

            // 2) Eğer "Seç" sütunu görünmüyorsa, görünür yap ve kullanıcıyı bilgilendir
            if (!dataGridView1.Columns["checkBoxColumn"].Visible)
            {
                dataGridView1.Columns["checkBoxColumn"].Visible = true;
                MessageBox.Show("Lütfen raporunu almak istediğiniz siparişleri seçin ve tekrar tıklayın.",
                                "Seçim Yapın", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 3) Seçili satırlardan SipID, ModelKodu ve KalipKodu bilgilerini topla
            var selectedItems = new List<(int SipID, string ModelKodu, string KalipKodu)>();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
                if (Convert.ToBoolean(row.Cells["checkBoxColumn"].Value))
                {
                    selectedItems.Add((
                        SipID: Convert.ToInt32(row.Cells["SipID"].Value),
                        ModelKodu: row.Cells["ModelKodu"].Value.ToString(),
                        KalipKodu: row.Cells["KalipKodu"].Value.ToString()
                    ));
                }
            }
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Lütfen en az bir satır seçin.", "Uyarı",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 4) Klasör seçtir
            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() != DialogResult.OK) return;
                string selectedPath = fbd.SelectedPath;

                // 5) ProgressBar ve Label ayarları
                progressBar1.Minimum = 0;
                progressBar1.Maximum = selectedItems.Count;
                progressBar1.Value = 0;
                label4.Text = "İşlem devam ediyor, lütfen bekleyin...";
                label4.Visible = true;

                var progress = new Progress<int>(value =>
                {
                    progressBar1.Value = value;
                    label4.Text = $"İşlem devam ediyor... %{value * 100 / selectedItems.Count}";
                });

                int progressValue = 0;

                // 6) Arka planda her bir sipariş için tek resmi çek ve Tekli raporu export et
                await Task.Run(() =>
                {
                    using (var connection = DatabaseHelper.GetConnection())
                    {
                        connection.Open();

                        foreach (var item in selectedItems)
                        {
                            // SQL: yalnızca Image1'i getiren pivot sorgusu
                            string query = @"
                        WITH RankedImages AS (
                            SELECT
                                MI.ModelKodu,
                                MI.ImageData,
                                ROW_NUMBER() OVER (PARTITION BY MI.ModelKodu ORDER BY (SELECT NULL)) AS RowNum
                            FROM ModelImages MI
                        ),
                        OnlyFirstImage AS (
                            SELECT
                                RI.ModelKodu,
                                MAX(CASE WHEN RI.RowNum = 1 THEN RI.ImageData ELSE NULL END) AS Image1
                            FROM RankedImages RI
                            GROUP BY RI.ModelKodu
                        )
                        SELECT
                            SH.SipID,
                            SH.SiparisTarihi,
                            SH.ModelKodu,
                            SH.AyakkabiNo,
                            SH.KalipKodu,
                            SH.Asortisi,
                            SH.UretimTanimi,
                            SH.Barkod,
                            OFI.Image1
                        FROM SiparisHeader SH
                        LEFT JOIN OnlyFirstImage OFI ON SH.ModelKodu = OFI.ModelKodu
                        WHERE SH.SipID = @SipID";

                            DataTable dt = new DataTable();
                            using (var cmd = new SqlCommand(query, connection))
                            {
                                cmd.Parameters.AddWithValue("@SipID", item.SipID);
                                using (var adapter = new SqlDataAdapter(cmd))
                                    adapter.Fill(dt);
                            }

                            // FastReport ile SiparisFormuTekli.frx'i yükle + export et
                            using (var report = new Report())
                            {
                                try
                                {
                                    report.Load(@"Rapor\SiparisFormuTekli.frx");
                                    report.RegisterData(dt, "SiparisHeader");
                                    report.GetDataSource("SiparisHeader").Enabled = true;
                                    report.Prepare();

                                    var fileName = Path.Combine(selectedPath, $"{item.KalipKodu}.jpeg");
                                    using (var imageExport = new FastReport.Export.Image.ImageExport())
                                    {
                                        imageExport.ImageFormat = ImageExportFormat.Jpeg;
                                        imageExport.Resolution = 300;
                                        imageExport.JpegQuality = 100;
                                        imageExport.Export(report, fileName);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Hata olsa bile işlem devam etsin
                                    MessageBox.Show($"SipID {item.SipID} için hata: {ex.Message}");
                                }
                            }

                            // ilerlemeyi bildir
                            progressValue++;
                            ((IProgress<int>)progress).Report(progressValue);
                        }

                        connection.Close();
                    }
                });

                // 7) İşlem sonu temizlik ve kullanıcıya bilgi
                MessageBox.Show("Seçili siparişlerin raporları başarıyla kaydedildi.",
                                "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                dataGridView1.Columns["checkBoxColumn"].Visible = false;
                foreach (DataGridViewRow row in dataGridView1.Rows)
                    row.Cells["checkBoxColumn"].Value = false;

                progressBar1.Value = 0;
                label4.Visible = false;
            }
        }


    }
}
