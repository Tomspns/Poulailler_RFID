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
        public static void UpdatePoule(string idPoule, string nouveauNom, string nouvelleRace)
        {
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = @"UPDATE poule SET nom = @nouveauNom, race = @nouvelleRace WHERE idPoule = @idPoule";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@nouveauNom", nouveauNom);
                    cmd.Parameters.AddWithValue("@idPoule", idPoule);
                    cmd.Parameters.AddWithValue("@nouvelleRace", nouvelleRace);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
