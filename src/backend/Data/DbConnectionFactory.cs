using System.Data;
using Npgsql;

public class DbConnectionFactory(IConfiguration config)
{
    private readonly string _connectionString = BuildConnectionString(config);

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    private static string BuildConnectionString(IConfiguration config)
    {
        // Render e Neon fornecem DATABASE_URL no formato:
        // postgresql://user:pass@host:port/db?sslmode=require
        var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

        if (!string.IsNullOrEmpty(dbUrl))
            return ConvertUrlToNpgsql(dbUrl);

        // Fallback para desenvolvimento local (appsettings.Development.json)
        return config.GetConnectionString("DefaultConnection")!;
    }

    private static string ConvertUrlToNpgsql(string url)
    {
        var uri = new Uri(url);
        var userInfo = uri.UserInfo.Split(':');
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');
        var username = userInfo[0];
        var password = userInfo[1];

        return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }
}
