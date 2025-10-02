using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFIDwpf.RFID
{
    public static class DatabaseHelper
    {
        private static string connectionString = "Server=localhost;Database=poulaillier;Uid=root;Pwd=;";

        public static void InsertPoule(string idPoule, string nom)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    INSERT INTO poule (idPoule, nom, date_d_enregistrement, rfid)
                    VALUES (@idPoule, @nom, @date, @rfid)";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idPoule", idPoule);
                    cmd.Parameters.AddWithValue("@nom", nom);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@rfid", idPoule);

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
