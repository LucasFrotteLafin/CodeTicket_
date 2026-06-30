using CodeTicket.API.Models;

namespace CodeTicket.API.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> BuscarPorCpf(string cpf);
    Task Criar(Usuario usuario);
    Task<Usuario?> ObterPorCpf(string cpf);
}
