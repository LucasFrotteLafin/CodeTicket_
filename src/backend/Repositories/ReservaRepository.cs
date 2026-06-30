using Dapper;
using CodeTicket.API.DTOs;
using CodeTicket.API.Models;

namespace CodeTicket.API.Repositories;

public class ReservaRepository : IReservaRepository
{
    private readonly DbConnectionFactory _factory;

    public ReservaRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<int> ContarReservasPorEvento(int eventoId)
    {
        using var conn = _factory.CreateConnection();
        var sql = "SELECT COUNT(*) FROM reservas WHERE eventoid = @EventoId";
        return await conn.ExecuteScalarAsync<int>(sql, new { EventoId = eventoId });
    }

    public async Task<bool> UsuarioJaComprouIngresso(string cpf, int eventoId)
    {
        using var conn = _factory.CreateConnection();
        var sql = "SELECT COUNT(*) FROM reservas WHERE usuariocpf = @Cpf AND eventoid = @EventoId";
        var count = await conn.ExecuteScalarAsync<int>(sql, new { Cpf = cpf, EventoId = eventoId });
        return count > 0;
    }

    public async Task<int> InserirReserva(string usuarioCpf, int eventoId, string? cupomCodigo, decimal valorFinal)
    {
        using var conn = _factory.CreateConnection();
        var sql = @"INSERT INTO reservas (usuariocpf, eventoid, cupomutilizado, valorfinalpago) 
                    VALUES (@UsuarioCpf, @EventoId, @CupomCodigo, @ValorFinal)
                    RETURNING id";
        
        return await conn.ExecuteScalarAsync<int>(sql, new 
        { 
            UsuarioCpf = usuarioCpf, 
            EventoId = eventoId, 
            CupomCodigo = cupomCodigo, 
            ValorFinal = valorFinal 
        });
    }

    public async Task<List<IngressoDetalheDto>> ListarIngressosUsuario(string cpf)
    {
        using var conn = _factory.CreateConnection();
        
        // Query com INNER JOIN entre reservas, usuarios e eventos
        var sql = @"
            SELECT 
                r.id AS ReservaId,
                u.nome AS UsuarioNome,
                u.cpf AS UsuarioCpf,
                e.nome AS EventoNome,
                e.dataevento AS DataEvento,
                e.local AS LocalEvento,
                e.precopadrao AS PrecoPadrao,
                r.cupomutilizado AS CupomUtilizado,
                r.valorfinalpago AS ValorFinalPago
            FROM reservas r
            INNER JOIN usuarios u ON r.usuariocpf = u.cpf
            INNER JOIN eventos e ON r.eventoid = e.id
            WHERE r.usuariocpf = @Cpf
            ORDER BY e.dataevento DESC";
        
        var ingressos = await conn.QueryAsync<IngressoDetalheDto>(sql, new { Cpf = cpf });
        
        // Calcular desconto aplicado
        foreach (var ingresso in ingressos)
        {
            if (!string.IsNullOrEmpty(ingresso.CupomUtilizado))
            {
                ingresso.DescontoAplicado = ingresso.PrecoPadrao - ingresso.ValorFinalPago;
            }
        }
        
        return ingressos.ToList();
    }
}
