using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;

namespace Pennywise.Backend
{
    internal class Database
    {
        private readonly string connectionString =
        @"Server=GhofGhof;Database=PennywiseDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
