using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace RFIDwpf.DAO
{
    public static class AddPouleDAO
    {
        public static void AddPoule(string idPoule, string nom)
        {
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = @"INSERT INTO poule (idPoule, nom, date_d_enregistrement, rfid)
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
