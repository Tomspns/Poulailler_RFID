using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace RFIDwpf.DAO
{
    public static class UpdatePouleDAO
    {
        public static void UpdatePoule(string idPoule, string nouveauNom)
        {
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = @"UPDATE poule SET nom = @nouveauNom WHERE idPoule = @idPoule";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@nouveauNom", nouveauNom);
                    cmd.Parameters.AddWithValue("@idPoule", idPoule);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
