using System;
using System.Data.SqlClient;
using System.IO;

namespace kalipproje
{
    internal class DatabaseHelper
    {
        public static string GetConnectionString()
        {
            // .txt dosyasındaki IP adresi ve portu oku
            var ipAddressAndPort = GetIpAddressAndPortFromConfigFile();

            // Geri kalan bilgileri sabit olarak tanımla
            string dataSource = $"{ipAddressAndPort}";
            string initialCatalog = "projeDB";
            string userId = "artech";
            string password = "Ay12348008_!19";

            // Bağlantı dizesini oluştur
            return $"Data Source={dataSource};Initial Catalog={initialCatalog};User ID={userId};Password={password};Encrypt=False";
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(GetConnectionString());
        }

        private static string GetIpAddressAndPortFromConfigFile()
        {
            string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configuration.txt");
            if (File.Exists(configFilePath))
            {
                return File.ReadAllText(configFilePath).Trim();
            }
            else
            {
                throw new FileNotFoundException("The configuration file was not found.");
            }
        }
    }
}
