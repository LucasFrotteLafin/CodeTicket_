using Dapper;
using CodeTicket.API.DTOs;
using CodeTicket.API.Models;

namespace CodeTicket.API.Repositories;

public class EventoRepository(DbConnectionFactory factory) : IEventoRepository
{
    public async Task<IEnumerable<EventoListarDto>> ListarEventos()
    {
        using var db = factory.CreateConnection();
        return await db.QueryAsync<EventoListarDto>("SELECT id, nome FROM eventos");
    }

    public async Task Criar(Evento evento)
    {
        using var db = factory.CreateConnection();
        await db.ExecuteAsync(
            "INSERT INTO Eventos (Nome, CapacidadeTotal, DataEvento, PrecoPadrao) VALUES (@Nome, @CapacidadeTotal, @DataEvento, @PrecoPadrao)",
            evento
        );
    }
}
