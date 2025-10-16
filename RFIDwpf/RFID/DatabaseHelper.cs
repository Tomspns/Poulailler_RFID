using MySql.Data.MySqlClient;
using RFIDwpf.DAO;
using System;

namespace RFIDwpf.RFID
{
    public static class DatabaseHelper
    {
        // ⚙️ Chaîne de connexion AlwaysData
        // Remplace les valeurs ci-dessous par celles de ton collègue
        private static string connectionString = "Server=mysql-poulailler.alwaysdata.net;Database=poulailler_bdd_poulailler;Uid=433737;Pwd=poulaillersaintmichel;SslMode=Required;";

        public static void InsertPoule(string idPOULE, string nom)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    INSERT INTO poule (idPOULE, nom, date_d_enregistrement, rfid)
                    VALUES (@idPOULE, @nom, @date, @rfid)";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idPOULE", idPOULE);
                    cmd.Parameters.AddWithValue("@nom", nom);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@rfid", idPOULE);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteLastPoule()
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    DELETE FROM poule 
                    ORDER BY date_d_enregistrement DESC 
                    LIMIT 1;";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void AddPoule(string idPoule, string nom, string race)
        {
            AddPouleDAO.AddPoule(idPoule, nom, race);
        }

        public static void DeletePoule(string idPoule)
        {
            DeletePouleDAO.DeletePoule(idPoule);
        }

        public static void UpdatePoule(string idPoule, string nouveauNom, string nouvelleRace)
        {
            UpdatePouleDAO.UpdatePoule(idPoule, nouveauNom, nouvelleRace);
        }
    }
}
