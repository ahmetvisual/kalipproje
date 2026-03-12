using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace kalipproje
{
    public partial class Form1 : Form
    {
        private Dictionary<string, Form> openForms = new Dictionary<string, Form>();

        public Form1()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized; // Formu tam ekran yapar
            // TabControl özelleştirmelerini ekleyin
            CustomizeTabControl(tabControl1);
            this.FormClosing += Form1_FormClosing; // Form kapanırken çağrılacak olay
            CustomizeTabControl(tabControl1); // TabControl özelleştirmeleri
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Uygulamanın tamamen kapanmasını sağlar
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            List<string> buttonNames = GetButtonNames();
            AddModulesToFlowLayoutPanel(buttonNames); // Form yüklenirken butonları FlowLayoutPanel'e ekle

            // TabControl özelleştirmelerini ekleyin
            CustomizeTabControl(tabControl1);
        }

        private void CustomizeTabControl(TabControl tabControl)
        {
            tabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl.ItemSize = new Size(tabControl.ItemSize.Width, 28); // Sekme başlıklarının yüksekliğini ayarlar
            tabControl.DrawItem += new DrawItemEventHandler(tabControl_DrawItem);
            tabControl.Padding = new Point(20, 4); // Tab başlıkları arasındaki boşluğu ayarlar
        }

        private void tabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            Graphics g = e.Graphics;
            Rectangle tabRect = tabControl.GetTabRect(e.Index);
            tabRect.Inflate(-2, -2);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Color mainColor = ColorTranslator.FromHtml("#3376a7");
            Color backgroundColor = ControlPaint.Light(mainColor, 0.8f);

            // Sekme başlıklarının arka planını çizin
            Color tabColor = e.Index == tabControl.SelectedIndex ? mainColor : backgroundColor;
            using (SolidBrush brush = new SolidBrush(tabColor))
            {
                g.FillRectangle(brush, tabRect);
            }

            using (Pen pen = new Pen(ControlPaint.Dark(mainColor, 0.1f)))
            {
                g.DrawRectangle(pen, tabRect);
            }

            string tabName = tabControl.TabPages[e.Index].Text;
            Font tabFont = new Font("Segoe UI", 10f, FontStyle.Regular);

            int iconSize = 16;
            int padding = 5;
            Rectangle iconRect = new Rectangle(tabRect.X + padding, tabRect.Y + (tabRect.Height - iconSize) / 2, iconSize, iconSize);

            // İkonu çizmek için Resources klasöründen resmi yükleyin
            Image icon = kalipproje.Properties.Resources.icon2; // icon.png dosyasını Resources'a ekleyin
            g.DrawImage(icon, iconRect);

            // Metin için yeni dikdörtgen hesaplama
            Size textSize = TextRenderer.MeasureText(tabName, tabFont);
            int textY = tabRect.Y + (tabRect.Height - textSize.Height) / 2;
            Rectangle textRect = new Rectangle(iconRect.Right + padding, textY, tabRect.Width - iconRect.Width - padding * 3, textSize.Height);

            Color textColor = e.Index == tabControl.SelectedIndex ? Color.White : ControlPaint.Dark(mainColor, 0.2f);

            // Metin çizimi
            using (SolidBrush textBrush = new SolidBrush(textColor))
            {
                g.DrawString(tabName, tabFont, textBrush, textRect);
            }

            if (e.Index == tabControl.SelectedIndex)
            {
                using (Pen underlinePen = new Pen(Color.White, 2))
                {
                    g.DrawLine(underlinePen, tabRect.Left, tabRect.Bottom - 1, tabRect.Right, tabRect.Bottom - 1);
                }
            }
        }

        private List<string> GetButtonNames()
        {
            List<string> buttonNames = new List<string>();
            foreach (Control ctrl in flowLayoutPanel1.Controls)
            {
                if (ctrl is Button)
                {
                    buttonNames.Add(ctrl.Text);
                }
            }
            return buttonNames;
        }

        private void AddModulesToFlowLayoutPanel(List<string> modulAdlari)
        {
            foreach (var modulAdi in modulAdlari)
            {
                Button modulButton = new Button
                {
                    Text = modulAdi,
                    Name = modulAdi.Replace(" ", "_"), // Unique name for control
                    Visible = false // Başlangıçta görünmez olarak ayarla
                };
                flowLayoutPanel1.Controls.Add(modulButton);
            }
        }

        public void SetModulePermissions(Dictionary<string, bool> moduleAccess)
        {
            foreach (Control control in flowLayoutPanel1.Controls)
            {
                if (control is Button button)
                {
                    // Eğer butonun metni moduleAccess'de varsa ve yetki false ise gizle
                    bool hasPermission = moduleAccess.ContainsKey(button.Text) ? moduleAccess[button.Text] : true;  // Default true olmalı
                    button.Visible = hasPermission;
                }
            }
        }

        private void OpenOrActivateForm(string formName, Form formInstance)
        {
            if (openForms.ContainsKey(formName))
            {
                // Form zaten açıksa, onu ön plana getirin
                openForms[formName].Activate();
            }
            else
            {
                // Form açık değilse, açın ve dictionary'e ekleyin
                formInstance.FormClosed += (s, args) => openForms.Remove(formName); // Form kapandığında dictionary'den çıkar
                openForms[formName] = formInstance;
                formInstance.Owner = this; // Form1'i owner olarak ayarla
                formInstance.Show();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ModelGiris modelGirisForm = new ModelGiris();
            OpenOrActivateForm("ModelGiris", modelGirisForm);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SiparisGiris siparisGirisForm = new SiparisGiris();
            OpenOrActivateForm("SiparisGiris", siparisGirisForm);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            UretimTipleri UretimTipleriForm = new UretimTipleri();
            OpenOrActivateForm("UretimTipleri", UretimTipleriForm);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SiparisList SiparisListForm = new SiparisList(LoginForm.LoggedInUser);
            OpenOrActivateForm("SiparisList", SiparisListForm);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            CariKartListesi CariKartListesiForm = new CariKartListesi();
            OpenOrActivateForm("CariKartListesi", CariKartListesiForm);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            GenelTanimlar GenelTanimlarForm = new GenelTanimlar();
            OpenOrActivateForm("GenelTanimlar", GenelTanimlarForm);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            List<string> buttonNames = GetButtonNames();
            YonetimPaneli YonetimPaneliForm = new YonetimPaneli(buttonNames);
            OpenOrActivateForm("YonetimPaneli", YonetimPaneliForm);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            MusteriTalepGiris musteriTalepGirisForm = new MusteriTalepGiris
            {
                HideButton6 = true
            };
            OpenOrActivateForm("MusteriTalepGiris", musteriTalepGirisForm);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            MusteriTalepList MusteriTalepListForm = new MusteriTalepList();
            OpenOrActivateForm("MusteriTalepList", MusteriTalepListForm);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }
    }
}