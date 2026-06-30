using CodeTicket.API.DTOs;
using CodeTicket.API.Models;

namespace CodeTicket.API.Repositories;

public interface IReservaRepository
{
    Task<int> ContarReservasPorEvento(int eventoId);
    Task<bool> UsuarioJaComprouIngresso(string cpf, int eventoId);
    Task<int> InserirReserva(string usuarioCpf, int eventoId, string? cupomCodigo, decimal valorFinal);
    Task<List<IngressoDetalheDto>> ListarIngressosUsuario(string cpf);
}
