using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace RFIDwpf.DAO
{
    public static class DeletePouleDAO
    {
        public static void DeletePoule(string idPoule)
        {
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = @"DELETE FROM poule WHERE idPoule = @idPoule";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idPoule", idPoule);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
