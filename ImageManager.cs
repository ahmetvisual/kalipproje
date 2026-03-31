using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace kalipproje
{
    public static class ImageManager
    {
        // Sabit olarak model başına maksimum resim limiti
        private const int MaxImagesPerModel = 3;

        /// <summary>
        /// Seçilen resmi ModelImages tablosuna kopyalarak ekler.
        /// Eklerken Max sınırını kontrol eder.
        /// </summary>
        /// <param name="modelID">Resmin ekleneceği Model ID</param>
        /// <param name="modelKodu">Resmin ekleneceği Model Kodu</param>
        /// <param name="filePath">Eklenecek fiziksel resim dosyasının yolu</param>
        /// <param name="showPrompts">True ise limit dolu olduğunda kullanıcıya uyarı gösterir, False ise sessizce atlar.</param>
        /// <returns>Eğer başarıyla eklendiyse True, limit aşımı veya hata oluştuysa False döner.</returns>
        public static bool AddImageToModel(int modelID, string modelKodu, string filePath, bool showPrompts = true)
        {
            try
            {
                byte[] imageBytes = File.ReadAllBytes(filePath);

                using (SqlConnection connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    // ─── Kopya kontrolü: bu model için kaç resim var? ───
                    // NOT: DATALENGTH(ImageData) tüm binary satırları tarayarak timeout'a neden oluyordu.
                    // Bunun yerine sadece adet kontrolü yapıp kullanıcıya bilgi veriyoruz.
                    using (SqlCommand checkCmd = new SqlCommand(
                        "SELECT COUNT(*) FROM ModelImages WHERE ModelID = @ModelID", connection))
                    {
                        checkCmd.Parameters.AddWithValue("@ModelID", modelID);
                        int existingCount = (int)checkCmd.ExecuteScalar();
                        
                        if (existingCount >= MaxImagesPerModel)
                        {
                            if (showPrompts)
                            {
                                var answer = MessageBox.Show(
                                    $"Bu model için zaten {existingCount} resim mevcut (Maksimum {MaxImagesPerModel}).\n" +
                                    $"'{Path.GetFileName(filePath)}' yine de eklensin mi?",
                                    "Resim Limiti Uyarı",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Warning);

                                if (answer != DialogResult.Yes) return false;
                            }
                            else
                            {
                                // Eğer showPrompts false ise (sessiz mod - toplu ekleme gibi) limit aşıldıysa direkt atla
                                return false;
                            }
                        }
                    }

                    // ─── Resmi veritabanına kayıt ekle ───
                    using (SqlCommand command = new SqlCommand(
                        "INSERT INTO ModelImages (ModelID, ImageData, ModelKodu) VALUES (@ModelID, @ImageData, @ModelKodu)",
                        connection))
                    {
                        command.Parameters.AddWithValue("@ModelID", modelID);
                        command.Parameters.Add("@ImageData", SqlDbType.VarBinary, imageBytes.Length).Value = imageBytes;
                        command.Parameters.AddWithValue("@ModelKodu", modelKodu);
                        command.ExecuteNonQuery();
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                if (showPrompts)
                {
                    MessageBox.Show($"Resim eklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
        }
    }
}
