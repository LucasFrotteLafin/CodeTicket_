using Dapper;
using CodeTicket.API.Models;

namespace CodeTicket.API.Repositories;

public class UsuarioRepository(DbConnectionFactory factory) : IUsuarioRepository
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

    public async Task<Usuario?> ObterPorCpf(string cpf)
    {
        using var db = factory.CreateConnection();
        return await db.QueryFirstOrDefaultAsync<Usuario>(
            "SELECT * FROM usuarios WHERE cpf = @Cpf",
            new { Cpf = cpf }
        );
    }
}
