namespace kalipproje
{
    partial class CariKartListesi
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CariKartListesi));
            label2 = new Label();
            textBox2 = new TextBox();
            contextMenuStrip1 = new ContextMenuStrip(components);
            tümünüSecToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            detaylarıAcToolStripMenuItem = new ToolStripMenuItem();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            dataGridView1 = new DataGridView();
            tabPage2 = new TabPage();
            dataGridView2 = new DataGridView();
            comboBox2 = new ComboBox();
            label1 = new Label();
            textBox1 = new TextBox();
            label3 = new Label();
            label4 = new Label();
            textBox3 = new TextBox();
            button5 = new Button();
            button4 = new Button();
            contextMenuStrip1.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            label2.Location = new Point(12, 116);
            label2.Name = "label2";
            label2.Size = new Size(64, 15);
            label2.TabIndex = 69;
            label2.Text = "Cari Filtresi";
            // 
            // textBox2
            // 
            textBox2.BackColor = Color.AntiqueWhite;
            textBox2.Location = new Point(12, 134);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(577, 23);
            textBox2.TabIndex = 68;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.BackgroundImageLayout = ImageLayout.None;
            contextMenuStrip1.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 162);
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { tümünüSecToolStripMenuItem, toolStripSeparator1, detaylarıAcToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.RenderMode = ToolStripRenderMode.Professional;
            contextMenuStrip1.ShowCheckMargin = true;
            contextMenuStrip1.ShowImageMargin = false;
            contextMenuStrip1.Size = new Size(152, 54);
            // 
            // tümünüSecToolStripMenuItem
            // 
            tümünüSecToolStripMenuItem.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 162);
            tümünüSecToolStripMenuItem.Name = "tümünüSecToolStripMenuItem";
            tümünüSecToolStripMenuItem.Size = new Size(151, 22);
            tümünüSecToolStripMenuItem.Text = "Yeni Cari Kart ";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(148, 6);
            // 
            // detaylarıAcToolStripMenuItem
            // 
            detaylarıAcToolStripMenuItem.Name = "detaylarıAcToolStripMenuItem";
            detaylarıAcToolStripMenuItem.Size = new Size(151, 22);
            detaylarıAcToolStripMenuItem.Text = "Detayları Ac";
            // 
            // tabControl1
            // 
            tabControl1.ContextMenuStrip = contextMenuStrip1;
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(12, 163);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(776, 498);
            tabControl1.TabIndex = 71;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(dataGridView1);
            tabPage1.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 162);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(768, 470);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "MÜŞTERİLER";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.Location = new Point(0, 0);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(768, 474);
            dataGridView1.TabIndex = 11;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(dataGridView2);
            tabPage2.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 162);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(768, 470);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "TEDARİKÇİLER";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // dataGridView2
            // 
            dataGridView2.AllowUserToAddRows = false;
            dataGridView2.AllowUserToDeleteRows = false;
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.EnableHeadersVisualStyles = false;
            dataGridView2.Location = new Point(0, 1);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.RowHeadersVisible = false;
            dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView2.Size = new Size(768, 473);
            dataGridView2.TabIndex = 12;
            // 
            // comboBox2
            // 
            comboBox2.BackColor = SystemColors.Control;
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 162);
            comboBox2.Items.AddRange(new object[] { "MÜŞTERİ", "TEDARİKÇİ" });
            comboBox2.Location = new Point(87, 10);
            comboBox2.Name = "comboBox2";
            comboBox2.RightToLeft = RightToLeft.No;
            comboBox2.Size = new Size(88, 23);
            comboBox2.TabIndex = 72;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            label1.Location = new Point(13, 13);
            label1.Name = "label1";
            label1.Size = new Size(50, 15);
            label1.TabIndex = 73;
            label1.Text = "Cari Tipi";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(87, 39);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(88, 23);
            textBox1.TabIndex = 74;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            label3.Location = new Point(13, 44);
            label3.Name = "label3";
            label3.Size = new Size(58, 15);
            label3.TabIndex = 75;
            label3.Text = "Cari Kodu";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            label4.Location = new Point(13, 75);
            label4.Name = "label4";
            label4.Size = new Size(67, 15);
            label4.TabIndex = 77;
            label4.Text = "Cari Ünvanı";
            // 
            // textBox3
            // 
            textBox3.Location = new Point(87, 72);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(502, 23);
            textBox3.TabIndex = 76;
            // 
            // button5
            // 
            button5.BackColor = Color.Green;
            button5.Font = new Font("Segoe UI", 10.125F, FontStyle.Bold, GraphicsUnit.Point, 162);
            button5.Location = new Point(648, 10);
            button5.Name = "button5";
            button5.Size = new Size(136, 49);
            button5.TabIndex = 94;
            button5.Text = "Kaydet";
            button5.UseVisualStyleBackColor = false;
            button5.Click += button5_Click;
            // 
            // button4
            // 
            button4.BackColor = Color.IndianRed;
            button4.Font = new Font("Segoe UI", 10.125F, FontStyle.Bold, GraphicsUnit.Point, 162);
            button4.Location = new Point(648, 65);
            button4.Name = "button4";
            button4.Size = new Size(136, 50);
            button4.TabIndex = 95;
            button4.Text = "Kapat";
            button4.UseVisualStyleBackColor = false;
            button4.Click += button4_Click;
            // 
            // CariKartListesi
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlLight;
            ClientSize = new Size(800, 683);
            Controls.Add(button4);
            Controls.Add(button5);
            Controls.Add(label4);
            Controls.Add(textBox3);
            Controls.Add(label3);
            Controls.Add(textBox1);
            Controls.Add(label1);
            Controls.Add(comboBox2);
            Controls.Add(tabControl1);
            Controls.Add(label2);
            Controls.Add(textBox2);
            DoubleBuffered = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "CariKartListesi";
            Text = "CariKartListesi";
            contextMenuStrip1.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label2;
        private TextBox textBox2;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem tümünüSecToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem detaylarıAcToolStripMenuItem;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private DataGridView dataGridView1;
        private TabPage tabPage2;
        private DataGridView dataGridView2;
        private ComboBox comboBox2;
        private Label label1;
        private TextBox textBox1;
        private Label label3;
        private Label label4;
        private TextBox textBox3;
        private Button button5;
        private Button button4;
    }
}