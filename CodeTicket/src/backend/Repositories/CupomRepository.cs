using Dapper;
using CodeTicket.API.Models;

namespace CodeTicket.API.Repositories;

public class CupomRepository(DbConnectionFactory factory) : ICupomRepository
{
    public async Task<bool> ExisteCupom(string codigo)
    {
        using var db = factory.CreateConnection();
        var count = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Cupons WHERE Codigo = @Codigo",
            new { Codigo = codigo }
        );
        return count > 0;
    }

    public async Task InserirCupom(Cupom cupom)
    {
        using var db = factory.CreateConnection();
        await db.ExecuteAsync(
            "INSERT INTO Cupons (Codigo, PorcentagemDesconto, ValorMinimoRegra) VALUES (@Codigo, @PorcentagemDesconto, @ValorMinimoRegra)",
            cupom
        );
    }
}
