using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace RFIDwpf.DAO
{
    public static class DatabaseConnection
    {
        private static readonly string connectionString =
            "Server=localhost;Database=poulaillier;Uid=root;Pwd=;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
}
