using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Globalization;
using System.Security;               // ProtectedData için

namespace kalipproje
{
    public partial class LoginForm : Form
    {
        // --------------------------------------------------------------------
        //                              G L O B A L
        // --------------------------------------------------------------------
        public static int LoggedInUserID;
        public static string LoggedInUser;
        public static int UserYetki;

        private bool hasLoggedIn = false;               // Çift açılmayı önler
        private const int CheckIntervalDays = 7;        // Lisans kontrol aralığı (gün)

        // AES
        private static readonly byte[] key = Convert.FromBase64String(
            "KkqVXvBPCzUQFvYzX8M+QeFDnm7LUQYaxRBRD7hK4nI=");
        private static readonly byte[] iv = Convert.FromBase64String(
            "aVxO7TQLgX2FpCJYxKTxwQ==");

        // Sürüm / güncelleme konumları (DEĞİŞTİRİLMEDİ)
        private const string LocalVersionPath = @"\\192.168.0.2\Kalıp\version.txt";
        private const string RemoteVersionUrl = "https://ztlicense.com/myapplication/version.txt";
        private const string LocalInstallerPath = @"\\192.168.0.2\Kalıp\kalıpprogram.exe";
        private const string RemoteInstallerUrl = "https://ztlicense.com/myapplication/kalipprogram.exe";

        // Lisans doğrulama (DEĞİŞTİRİLMEDİ)
        private const string RemoteLicenseCheckUrl = "https://ztlicense.com/checklicense.php";

        // Registry
        private const string RegistryKeyPath = @"Software\KalipProje";
        private const string CredentialsValueName = "SavedCredentials";
        private const string LicenseValueName = "LicenseKey";
        private const string LicenseLastCheckName = "LastCheck";

        // --------------------------------------------------------------------
        //                                C T O R
        // --------------------------------------------------------------------
        public LoginForm()
        {
            InitializeComponent();

            // Şifre kutusu gizlensin + Enter kısayolu
            textBox2.UseSystemPasswordChar = true;
            textBox2.KeyPress += textBox2_KeyPress;

            // Versiyon etiketi
            labelVersion.Text = "Versiyon: " + GetCurrentVersion();

            // “Beni Hatırla” ⇒ kayıtlı bilgiler
            var saved = ReadCredentialsFromRegistry();
            if (saved.HasValue)
            {
                textBox1.Text = saved.Value.user;
                textBox2.Text = saved.Value.pass;
                checkBox1.Checked = true;
            }

            // Güncelleme kontrolü
            CheckForUpdatesAsync();

            // ---------------- Lisans kontrolü ----------------
            if (IsTimeForLicenseCheck())
            {
                if (CheckHttpAccess())        // İnternet varsa yeni lisans üret & kaydet
                    CreateLicenseFile();
                else
                {
                    MessageBox.Show("Lisans doğrulaması yapılamadı. İnternet bağlantınızı kontrol edin.",
                                    "Lisans Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                    return;
                }
            }
            else if (!IsRegistryLicenseValid())   // 7 gün içinde ama kayıt bozuksa
            {
                MessageBox.Show("Lisans geçersiz. İnternet bağlantısı gerekiyor.",
                                "Lisans Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
        }
        // --------------------------------------------------------------------
        //                         G Ü N C E L L E M E  L O J İ Ğ İ
        // --------------------------------------------------------------------
        private async void CheckForUpdatesAsync()
        {
            if (await IsNewVersionAvailableAsync())
            {
                var res = MessageBox.Show("Yeni bir versiyon mevcut. Güncelleme yapmak ister misiniz?",
                                          "Güncelleme", MessageBoxButtons.YesNo);
                if (res == DialogResult.Yes)
                    await DownloadAndUpdateAsync();
            }
        }

        private async Task<bool> IsNewVersionAvailableAsync()
        {
            string current = GetCurrentVersion();
            string latest = await GetLatestVersionAsync();
            if (latest == null) return false;

            try { return new Version(latest) > new Version(current); }
            catch { return false; }
        }

        private async Task<string> GetLatestVersionAsync()
        {
            // 1) Yerel dosya
            try
            {
                if (await FileExistsWithTimeout(LocalVersionPath, 1500))
                {
                    var txt = await ReadAllTextWithTimeout(LocalVersionPath, 1500);
                    if (!string.IsNullOrWhiteSpace(txt))
                        return txt.Trim();
                }
            }
            catch { /* zaman aşımı vs. */ }

            // 2) Uzak dosya
            using var client = new HttpClient();
            try
            {
                var resp = await client.GetAsync(RemoteVersionUrl);
                resp.EnsureSuccessStatusCode();
                return (await resp.Content.ReadAsStringAsync()).Trim();
            }
            catch { }

            return null;
        }

        private async Task DownloadAndUpdateAsync()
        {
            string installer = null;

            // Önce paylaşımdaki EXE’yi deneyelim
            if (File.Exists(LocalInstallerPath))
            {
                installer = LocalInstallerPath;
            }
            else
            {
                // Yoksa internetten indir → %TEMP%
                string tmp = Path.Combine(Path.GetTempPath(), "kalipprogram.exe");
                using var wc = new WebClient();
                try
                {
                    await wc.DownloadFileTaskAsync(RemoteInstallerUrl, tmp);
                    installer = tmp;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Güncelleme indirme hatası:\n" + ex.Message,
                                    "Güncelleme", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // Yöneticili yeniden başlat
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = installer,
                    UseShellExecute = true,
                    Verb = "runas"
                });
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Güncelleme başlatılamadı:\n" + ex.Message,
                                "Güncelleme", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --------------------------------------------------------------------
        //                           E N T E R   K I S A Y O L U
        // --------------------------------------------------------------------
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                button1_Click(this, EventArgs.Empty);
            }
        }

        // --------------------------------------------------------------------
        //                      L İ S A N S   İ Ş L E M L E R İ
        // --------------------------------------------------------------------
        private bool IsTimeForLicenseCheck()
        {
            var (_, last) = ReadLicenseFromRegistry();
            if (last == null) return true;
            return (DateTime.UtcNow - last.Value).TotalDays > CheckIntervalDays;
        }

        private (string encryptedLicense, DateTime? lastCheck) ReadLicenseFromRegistry()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            if (key == null) return (null, null);

            var enc = key.GetValue(LicenseValueName) as string;
            var dateStr = key.GetValue(LicenseLastCheckName) as string;

            DateTime? dt = null;
            if (!string.IsNullOrEmpty(dateStr) &&
                DateTime.TryParseExact(dateStr, "o", CultureInfo.InvariantCulture,
                                        DateTimeStyles.RoundtripKind, out var parsed))
                dt = parsed;

            return (enc, dt);
        }

        private void SaveLicenseToRegistry(string encryptedLicense)
        {
            using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
            key.SetValue(LicenseValueName, encryptedLicense, RegistryValueKind.String);
            key.SetValue(LicenseLastCheckName, DateTime.UtcNow.ToString("o"), RegistryValueKind.String);
        }

        private bool IsRegistryLicenseValid()
        {
            var (enc, _) = ReadLicenseFromRegistry();
            if (enc == null) return false;
            return Decrypt(enc) == GenerateLicenseKey();
        }

        private void CreateLicenseFile()
        {
            string lic = GenerateLicenseKey();
            SaveLicenseToRegistry(Encrypt(lic));
        }

        // AES
        private string Encrypt(string plain)
        {
            using var aes = Aes.Create();
            aes.Key = key; aes.IV = iv;
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using var sw = new StreamWriter(cs);
            sw.Write(plain);
            sw.Flush();
            cs.FlushFinalBlock();
            return Convert.ToBase64String(ms.ToArray());
        }

        private string Decrypt(string cipher)
        {
            using var aes = Aes.Create();
            aes.Key = key; aes.IV = iv;
            using var ms = new MemoryStream(Convert.FromBase64String(cipher));
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }

        private string GenerateLicenseKey()
        {
            string sysInfo = GetSystemInfo();
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(sysInfo)));
        }

        // CPU + tüm MAC adresleri + makine & kullanıcı
        private string GetSystemInfo()
{
    var sb = new StringBuilder()
        .Append(Environment.MachineName)
        .Append(Environment.UserName);

    // CPU (ilk işlemci yeterli)
    foreach (ManagementObject o in new ManagementObjectSearcher(
             "SELECT ProcessorId FROM Win32_Processor").Get())
    {
        sb.Append(o["ProcessorId"]);
        break;
    }

    // 1) Fiziksel ağ kartlarının MAC'leri
    var macList = new List<string>();

    foreach (ManagementObject o in new ManagementObjectSearcher(
             "SELECT MACAddress, PhysicalAdapter FROM Win32_NetworkAdapter " +
             "WHERE MACAddress IS NOT NULL").Get())
    {
        // Yalnızca gerçek (PhysicalAdapter = TRUE) kartları al
        if (o["PhysicalAdapter"] is bool isPhys && isPhys)
        {
            string mac = o["MACAddress"]?.ToString();
            if (!string.IsNullOrWhiteSpace(mac))
                macList.Add(mac);
        }
    }

    // MAC yoksa (nadiren sanal makinelerde) tüm adaptörleri yedek olarak al
    if (macList.Count == 0)
    {
        foreach (ManagementObject o in new ManagementObjectSearcher(
                 "SELECT MACAddress FROM Win32_NetworkAdapter WHERE MACAddress IS NOT NULL").Get())
        {
            string mac = o["MACAddress"]?.ToString();
            if (!string.IsNullOrWhiteSpace(mac))
                macList.Add(mac);
        }
    }

    macList.Sort(StringComparer.OrdinalIgnoreCase);  // SIRAYI SABİTLE
    foreach (string mac in macList)
        sb.Append(mac);

    // 2) BIOS seri numarasını yedek bilgi olarak ekle
    foreach (ManagementObject o in new ManagementObjectSearcher(
             "SELECT SerialNumber FROM Win32_BIOS").Get())
    {
        sb.Append(o["SerialNumber"]);
        break;
    }

    return sb.ToString();
}

        // Uzak lisans servisinde “status”:”ok” bekleniyor
        private bool CheckHttpAccess()
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(RemoteLicenseCheckUrl);
                req.Timeout = 5000;
                req.ReadWriteTimeout = 5000;
                req.Proxy = null;
                req.KeepAlive = false;

                using var resp = (HttpWebResponse)req.GetResponse();
                using var rdr = new StreamReader(resp.GetResponseStream());
                dynamic json = JsonConvert.DeserializeObject(rdr.ReadToEnd());
                return json.status == "ok";
            }
            catch
            {
                return false;
            }
        }

        // --------------------------------------------------------------------
        //                 “ B e n i  H a t ı r l a ”   F O N K S .
        // --------------------------------------------------------------------
        private void SaveCredentialsToRegistry(string user, string pass)
        {
            string plain = $"{user}:{pass}";
            byte[] enc = ProtectedData.Protect(Encoding.UTF8.GetBytes(plain),
                                                 null,
                                                 DataProtectionScope.CurrentUser);
            using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
            key.SetValue(CredentialsValueName,
                         Convert.ToBase64String(enc),
                         RegistryValueKind.String);
        }

        private (string user, string pass)? ReadCredentialsFromRegistry()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            var blob = key?.GetValue(CredentialsValueName) as string;
            if (string.IsNullOrEmpty(blob)) return null;

            try
            {
                byte[] enc = Convert.FromBase64String(blob);
                byte[] clr = ProtectedData.Unprotect(enc, null, DataProtectionScope.CurrentUser);
                var parts = Encoding.UTF8.GetString(clr).Split(':');
                if (parts.Length == 2) return (parts[0], parts[1]);
            }
            catch { /* bozuksa yoksay */ }
            return null;
        }

        private void DeleteSavedCredentials()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            key?.DeleteValue(CredentialsValueName, throwOnMissingValue: false);
        }

        // --------------------------------------------------------------------
        //                          L O G I N   B U T O N U
        // --------------------------------------------------------------------
        private void button1_Click(object sender, EventArgs e)
        {
            if (hasLoggedIn) return;      // Çift tıklamayı engelle
            string kullanici = textBox1.Text;
            string sifre = textBox2.Text;

            using var con = DatabaseHelper.GetConnection();
            con.Open();

            // Kullanıcı kontrolü
            using (var cmd = new SqlCommand(
                   "SELECT YonetID, Yetki FROM YonetHeader WHERE KullaniciAdi=@u AND Sifre=@p", con))
            {
                cmd.Parameters.AddWithValue("@u", kullanici);
                cmd.Parameters.AddWithValue("@p", sifre);

                using var r = cmd.ExecuteReader();
                if (!r.Read())
                {
                    MessageBox.Show("Kullanıcı adı veya şifre hatalı!");
                    return;
                }

                LoggedInUserID = (int)r["YonetID"];
                UserYetki = (r["Yetki"] is bool b && b) ? 1 : 0;
                LoggedInUser = kullanici;
            }

            // Modül yetkileri
            var moduleAccess = new Dictionary<string, bool>();
            using (var cmd2 = new SqlCommand(
                   "SELECT ModulAdi, Yetki FROM YetkiHeader WHERE Kullanici=@u", con))
            {
                cmd2.Parameters.AddWithValue("@u", kullanici);
                using var r2 = cmd2.ExecuteReader();
                while (r2.Read())
                    moduleAccess[r2["ModulAdi"].ToString()] =
                        Convert.ToBoolean(r2["Yetki"]);
            }

            // “Beni Hatırla”
            if (checkBox1.Checked) 
                SaveCredentialsToRegistry(kullanici, sifre);
            else
                DeleteSavedCredentials();

            // Ana forma geç
            hasLoggedIn = true;
            Hide();
            var mainForm = new Form1();
            mainForm.SetModulePermissions(moduleAccess);
            mainForm.Show();
            mainForm.Activate();
        }
        // --------------------------------------------------------------------
        //                       Y A R D I M C I  M E T O T L A R
        // --------------------------------------------------------------------
        private async Task<bool> FileExistsWithTimeout(string path, int timeoutMs)
        {
            using var cts = new CancellationTokenSource();
            var task = Task.Run(() => File.Exists(path), cts.Token);
            if (await Task.WhenAny(task, Task.Delay(timeoutMs)) == task)
                return task.Result;
            throw new TimeoutException("Dosya erişimi zaman aşımına uğradı: " + path);
        }

        private async Task<string> ReadAllTextWithTimeout(string path, int timeoutMs)
        {
            using var cts = new CancellationTokenSource();
            var task = Task.Run(() => File.ReadAllText(path), cts.Token);
            if (await Task.WhenAny(task, Task.Delay(timeoutMs)) == task)
                return task.Result;
            throw new TimeoutException("Dosya okuma zaman aşımına uğradı: " + path);
        }

        private string GetCurrentVersion() => Application.ProductVersion;
    }
}
