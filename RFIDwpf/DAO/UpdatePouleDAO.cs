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
        /// <summary>
        /// Met à jour les informations d'une poule existante dans la base.
        /// </summary>
        /// <param name="idPoule">Identifiant unique de la poule</param>
        /// <param name="nouveauNom">Nouveau nom à enregistrer</param>
        /// <param name="nouvelleRace">Nouvelle race à enregistrer</param>
        public static void UpdatePoule(string idPoule, string nouveauNom, string nouvelleRace)
        {
            if (string.IsNullOrWhiteSpace(idPoule))
                throw new ArgumentException("L'ID de la poule est requis.");

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();

                string query = @"
                    UPDATE poule 
                    SET nom = @nouveauNom, race = @nouvelleRace 
                    WHERE idPoule = @idPoule";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idPoule", idPoule);
                    cmd.Parameters.AddWithValue("@nouveauNom", nouveauNom);
                    cmd.Parameters.AddWithValue("@nouvelleRace", nouvelleRace ?? string.Empty);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        throw new Exception($"Aucune poule trouvée avec l'ID {idPoule}.");
                    }
                }
            }
        }
    }
}