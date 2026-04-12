using CodeTicket.API.Models;

namespace CodeTicket.API.Repositories;

public interface ICupomRepository
{
    Task<bool> ExisteCupom(string codigo);
    Task InserirCupom(Cupom cupom);
}
