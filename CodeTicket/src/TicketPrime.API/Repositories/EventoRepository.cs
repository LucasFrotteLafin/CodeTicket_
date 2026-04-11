using Dapper;
using TicketPrime.API.DTOs;
using TicketPrime.API.Models;

namespace TicketPrime.API.Repositories;

public class EventoRepository(DbConnectionFactory factory)
{
    public async Task<IEnumerable<EventoListarDto>> ListarEventos()
    {
        using var db = factory.CreateConnection();

        var sql = "SELECT id, nome FROM eventos";

        return await db.QueryAsync<EventoListarDto>(sql);
    }
    //Listar Eventos

    public async Task Criar(Evento evento)
    {
        using var db = factory.CreateConnection();
        await db.ExecuteAsync(
            "INSERT INTO Eventos (Nome, CapacidadeTotal, DataEvento, PrecoPadrao) VALUES (@Nome, @CapacidadeTotal, @DataEvento, @PrecoPadrao)",
            evento
        );
    }
}