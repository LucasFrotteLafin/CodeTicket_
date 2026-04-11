using Dapper;
using System.Data;
using TicketPrime.API.Models;

namespace TicketPrime.API.Repository;

public class CupomRepository
{
    private readonly IDbConnection _connection;

    public CupomRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<bool> ExisteCupom(string codigo)
    {
        var sql = "SELECT COUNT(1) FROM Cupons WHERE Codigo = @Codigo";

        var count = await _connection.ExecuteScalarAsync<int>(sql, new { Codigo = codigo });

        return count > 0;
    }

    public async Task InserirCupom(Cupom cupom)
    {
        var sql = @"INSERT INTO Cupons 
                    (Codigo, PorcentagemDesconto, ValorMinimoRegra)
                    VALUES (@Codigo, @PorcentagemDesconto, @ValorMinimoRegra)";

        await _connection.ExecuteAsync(sql, cupom);
    }
}
