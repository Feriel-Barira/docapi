using System.Data;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace DocApi.Infrastructure
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new System.ArgumentNullException("DefaultConnection string is not configured.");
            
            // Debug: Log the connection string (remove password for security)
            var debugConnectionString = _connectionString.Replace("Password=Stage55HHd", "Password=***");
            Console.WriteLine($"[DEBUG] Connection String: {debugConnectionString}");
        }

        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
