using System.Data;
using Npgsql;

public class DbConnectionFactory(IConfiguration config)
{
    private readonly string _connectionString = config.GetConnectionString("DefaultConnection")!;

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}
