namespace kalipproje
{
    partial class MusteriTalepList
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MusteriTalepList));
            label2 = new Label();
            textBox2 = new TextBox();
            radioButton3 = new RadioButton();
            radioButton2 = new RadioButton();
            radioButton1 = new RadioButton();
            dataGridView1 = new DataGridView();
            contextMenuStrip1 = new ContextMenuStrip(components);
            gidenToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            iptalToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            kopyalaToolStripMenuItem = new ToolStripMenuItem();
            kalıpRaporuResimliToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            TalepRaportoolStripMenuItem1 = new ToolStripMenuItem();
            dateTimePicker1 = new DateTimePicker();
            dateTimePicker2 = new DateTimePicker();
            button1 = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 162);
            label2.Location = new Point(418, 23);
            label2.Name = "label2";
            label2.Size = new Size(59, 15);
            label2.TabIndex = 72;
            label2.Text = "Cari Filtre";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(481, 18);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(124, 23);
            textBox2.TabIndex = 71;
            // 
            // radioButton3
            // 
            radioButton3.AutoSize = true;
            radioButton3.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold);
            radioButton3.Location = new Point(278, 21);
            radioButton3.Name = "radioButton3";
            radioButton3.Size = new Size(100, 17);
            radioButton3.TabIndex = 70;
            radioButton3.TabStop = true;
            radioButton3.Text = "İptal Talepler";
            radioButton3.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            radioButton2.AutoSize = true;
            radioButton2.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold);
            radioButton2.Location = new Point(155, 21);
            radioButton2.Name = "radioButton2";
            radioButton2.Size = new Size(108, 17);
            radioButton2.TabIndex = 69;
            radioButton2.TabStop = true;
            radioButton2.Text = "Giden Talepler";
            radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            radioButton1.AutoSize = true;
            radioButton1.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold);
            radioButton1.Location = new Point(12, 21);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new Size(127, 17);
            radioButton1.TabIndex = 68;
            radioButton1.TabStop = true;
            radioButton1.Text = "Bekleyen Talepler";
            radioButton1.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToOrderColumns = true;
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.BorderStyle = BorderStyle.Fixed3D;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.Raised;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.ContextMenuStrip = contextMenuStrip1;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.Location = new Point(12, 58);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(997, 572);
            dataGridView1.TabIndex = 73;
            dataGridView1.VirtualMode = true;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.BackgroundImageLayout = ImageLayout.None;
            contextMenuStrip1.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 162);
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { gidenToolStripMenuItem, toolStripSeparator1, iptalToolStripMenuItem, toolStripSeparator2, kopyalaToolStripMenuItem, kalıpRaporuResimliToolStripMenuItem, toolStripSeparator3, TalepRaportoolStripMenuItem1 });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.RenderMode = ToolStripRenderMode.Professional;
            contextMenuStrip1.Size = new Size(212, 132);
            // 
            // gidenToolStripMenuItem
            // 
            gidenToolStripMenuItem.AccessibleDescription = "s";
            gidenToolStripMenuItem.Font = new Font("Microsoft Sans Serif", 10.125F);
            gidenToolStripMenuItem.Image = Properties.Resources.Fatcow_Farm_Fresh_Check_boxes_series_16;
            gidenToolStripMenuItem.Name = "gidenToolStripMenuItem";
            gidenToolStripMenuItem.Size = new Size(211, 22);
            gidenToolStripMenuItem.Text = "Giden Olarak İşaretle";
            gidenToolStripMenuItem.Click += gidenToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(208, 6);
            // 
            // iptalToolStripMenuItem
            // 
            iptalToolStripMenuItem.Font = new Font("Microsoft Sans Serif", 10.125F, FontStyle.Regular, GraphicsUnit.Point, 162);
            iptalToolStripMenuItem.Image = Properties.Resources.Fatcow_Farm_Fresh_Check_boxes_series_16;
            iptalToolStripMenuItem.Name = "iptalToolStripMenuItem";
            iptalToolStripMenuItem.Size = new Size(211, 22);
            iptalToolStripMenuItem.Text = "İptal Olarak İşaretle";
            iptalToolStripMenuItem.Click += iptalToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(208, 6);
            // 
            // kopyalaToolStripMenuItem
            // 
            kopyalaToolStripMenuItem.Font = new Font("Microsoft Sans Serif", 10.125F);
            kopyalaToolStripMenuItem.Image = Properties.Resources.Fatcow_Farm_Fresh_Compare_16;
            kopyalaToolStripMenuItem.Name = "kopyalaToolStripMenuItem";
            kopyalaToolStripMenuItem.Size = new Size(211, 22);
            kopyalaToolStripMenuItem.Text = "Kopyala";
            kopyalaToolStripMenuItem.Click += kopyalaToolStripMenuItem_Click;
            // 
            // kalıpRaporuResimliToolStripMenuItem
            // 
            kalıpRaporuResimliToolStripMenuItem.Font = new Font("Microsoft Sans Serif", 10.125F);
            kalıpRaporuResimliToolStripMenuItem.Image = Properties.Resources.Fatcow_Farm_Fresh_Report_images_16;
            kalıpRaporuResimliToolStripMenuItem.Name = "kalıpRaporuResimliToolStripMenuItem";
            kalıpRaporuResimliToolStripMenuItem.Size = new Size(211, 22);
            kalıpRaporuResimliToolStripMenuItem.Text = "Kalıp Raporu Resimli";
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(208, 6);
            // 
            // TalepRaportoolStripMenuItem1
            // 
            TalepRaportoolStripMenuItem1.Font = new Font("Microsoft Sans Serif", 10.125F);
            TalepRaportoolStripMenuItem1.Image = Properties.Resources.icon1;
            TalepRaportoolStripMenuItem1.Name = "TalepRaportoolStripMenuItem1";
            TalepRaportoolStripMenuItem1.Size = new Size(211, 22);
            TalepRaportoolStripMenuItem1.Text = "Talep Raporu Resimli";
            TalepRaportoolStripMenuItem1.Click += TalepRaportoolStripMenuItem1_Click;
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.Location = new Point(752, 19);
            dateTimePicker1.MinDate = new DateTime(1999, 12, 24, 0, 0, 0, 0);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(94, 23);
            dateTimePicker1.TabIndex = 74;
            // 
            // dateTimePicker2
            // 
            dateTimePicker2.Format = DateTimePickerFormat.Custom;
            dateTimePicker2.Location = new Point(852, 19);
            dateTimePicker2.MinDate = new DateTime(1999, 12, 24, 0, 0, 0, 0);
            dateTimePicker2.Name = "dateTimePicker2";
            dateTimePicker2.Size = new Size(94, 23);
            dateTimePicker2.TabIndex = 74;
            // 
            // button1
            // 
            button1.BackColor = Color.Teal;
            button1.Location = new Point(952, 17);
            button1.Name = "button1";
            button1.Size = new Size(38, 25);
            button1.TabIndex = 75;
            button1.Text = "<>";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // MusteriTalepList
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlLight;
            ClientSize = new Size(1021, 653);
            Controls.Add(button1);
            Controls.Add(dateTimePicker2);
            Controls.Add(dateTimePicker1);
            Controls.Add(dataGridView1);
            Controls.Add(label2);
            Controls.Add(textBox2);
            Controls.Add(radioButton3);
            Controls.Add(radioButton2);
            Controls.Add(radioButton1);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MusteriTalepList";
            Text = "MusteriTalepList";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label2;
        private TextBox textBox2;
        private RadioButton radioButton3;
        private RadioButton radioButton2;
        private RadioButton radioButton1;
        private DataGridView dataGridView1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem gidenToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem iptalToolStripMenuItem;
        private ToolStripMenuItem kopyalaToolStripMenuItem;
        private ToolStripMenuItem kalıpRaporuResimliToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private DateTimePicker dateTimePicker1;
        private DateTimePicker dateTimePicker2;
        private Button button1;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem TalepRaportoolStripMenuItem1;
    }
}