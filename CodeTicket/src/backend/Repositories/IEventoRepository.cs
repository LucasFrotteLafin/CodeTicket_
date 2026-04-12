using CodeTicket.API.DTOs;
using CodeTicket.API.Models;

namespace CodeTicket.API.Repositories;

public interface IEventoRepository
{
    Task<IEnumerable<EventoListarDto>> ListarEventos();
    Task Criar(Evento evento);
}
