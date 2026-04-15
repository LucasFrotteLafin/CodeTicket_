using Dapper;
using CodeTicket.API.DTOs;
using CodeTicket.API.Models;

namespace CodeTicket.API.Repositories;

public class EventoRepository(DbConnectionFactory factory) : IEventoRepository
{
    public async Task<IEnumerable<EventoListarDto>> ListarEventos()
    {
        using var db = factory.CreateConnection();
        return await db.QueryAsync<EventoListarDto>(
            "SELECT e.id, e.nome, e.dataevento, e.precopadrao, e.capacidadetotal, e.descricao, e.local, e.imagemurl, u.nome AS NomeUsuario FROM eventos e JOIN usuarios u ON u.cpf = e.usuariocpf ORDER BY e.dataevento ASC"
        );
    }

    public async Task Criar(Evento evento)
    {
        using var db = factory.CreateConnection();
        await db.ExecuteAsync(
            "INSERT INTO Eventos (Nome, CapacidadeTotal, DataEvento, PrecoPadrao, UsuarioCpf, Descricao, Local, ImagemUrl) VALUES (@Nome, @CapacidadeTotal, @DataEvento, @PrecoPadrao, @UsuarioCpf, @Descricao, @Local, @ImagemUrl)",
            evento
        );
    }
}
