using Dapper;
using TicketPrime.API.Models;

namespace TicketPrime.API.Repositories;

public class EventoRepository(DbConnectionFactory factory)
{
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