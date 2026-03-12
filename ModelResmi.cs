using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace kalipproje
{
    public partial class ModelResmi : Form
    {
        private string modelKodu;
        private List<byte[]> imageBytesList = new List<byte[]>();

        public ModelResmi(string modelKodu)
        {
            InitializeComponent();
            this.modelKodu = modelKodu;

            LoadImages();
        }

        private void LoadImages()
        {
            imageBytesList.Clear();
            comboBox7.Items.Clear();
            using (SqlConnection connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT ImageData FROM ModelImages WHERE ModelKodu = @ModelKodu ORDER BY ImageID", connection))
                {
                    command.Parameters.AddWithValue("@ModelKodu", modelKodu);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int index = 1;
                        while (reader.Read())
                        {
                            imageBytesList.Add((byte[])reader["ImageData"]);
                            comboBox7.Items.Add("Resim " + index);
                            index++;
                        }
                    }
                }
            }

            if (imageBytesList.Count > 0)
            {
                comboBox7.SelectedIndex = 0; // İlk resmi seç
                ShowImage(0); // İlk resmi göster
            }
        }

        private void ShowImage(int index)
        {
            if (index >= 0 && index < imageBytesList.Count)
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

        private void comboBox7_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            ShowImage(comboBox7.SelectedIndex);
        }
    }
}
