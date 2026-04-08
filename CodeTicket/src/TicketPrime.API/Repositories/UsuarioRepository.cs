using Dapper;
using TicketPrime.API.Models;

namespace TicketPrime.API.Repositories;

public class UsuarioRepository(DbConnectionFactory factory)
{
    public async Task<Usuario?> BuscarPorCpf(string cpf)
    {
        using var db = factory.CreateConnection();
        return await db.QueryFirstOrDefaultAsync<Usuario>(
            "SELECT * FROM Usuarios WHERE Cpf = @Cpf",
            new { Cpf = cpf }
        );
    }
        public async Task Criar(Usuario usuario)
    {
        using var db = factory.CreateConnection();
        await db.ExecuteAsync(
            "INSERT INTO Usuarios (Cpf, Nome, Email) VALUES (@Cpf, @Nome, @Email)",
            usuario
        );
    }
}
