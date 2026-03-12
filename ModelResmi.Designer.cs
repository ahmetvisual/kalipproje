namespace kalipproje
{
    partial class ModelResmi
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModelResmi));
            comboBox7 = new ComboBox();
            pictureBox1 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // comboBox7
            // 
            comboBox7.BackColor = SystemColors.HighlightText;
            comboBox7.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox7.Font = new Font("Segoe UI", 10.125F, FontStyle.Bold, GraphicsUnit.Point, 162);
            comboBox7.FormattingEnabled = true;
            comboBox7.Location = new Point(147, 400);
            comboBox7.Name = "comboBox7";
            comboBox7.RightToLeft = RightToLeft.No;
            comboBox7.Size = new Size(133, 25);
            comboBox7.TabIndex = 63;
            comboBox7.SelectedIndexChanged += comboBox7_SelectedIndexChanged_1;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Gainsboro;
            pictureBox1.Location = new Point(11, 11);
            pictureBox1.Margin = new Padding(2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(441, 377);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 62;
            pictureBox1.TabStop = false;
            // 
            // ModelResmi
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(463, 437);
            Controls.Add(comboBox7);
            Controls.Add(pictureBox1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "ModelResmi";
            Text = "ModelResmi";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private ComboBox comboBox7;
        private PictureBox pictureBox1;
    }
}