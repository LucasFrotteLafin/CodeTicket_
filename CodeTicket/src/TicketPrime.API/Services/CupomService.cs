using TicketPrime.API.DTOs;
using TicketPrime.API.Models;
using TicketPrime.API.Repository;

namespace TicketPrime.API.Service;

public class CupomService
{
    private readonly CupomRepository _repository;

    public CupomService(CupomRepository repository)
    {
        _repository = repository;
    }

    public async Task<(bool sucesso, string mensagem)> CriarCupom(CriarCupomDto dto)
    {
        if (string.IsNullOrEmpty(dto.Codigo))
            return (false, "Código é obrigatório");

        if (dto.PorcentagemDesconto <= 0 || dto.PorcentagemDesconto > 100)
            return (false, "Desconto deve ser entre 0 e 100");

        if (dto.ValorMinimoRegra < 0)
            return (false, "Valor mínimo não pode ser negativo");

        var existe = await _repository.ExisteCupom(dto.Codigo);

        if (existe)
            return (false, "Cupom já existe");

        var cupom = new Cupom
        {
            Codigo = dto.Codigo,
            PorcentagemDesconto = dto.PorcentagemDesconto,
            ValorMinimoRegra = dto.ValorMinimoRegra
        };

        await _repository.InserirCupom(cupom);

        return (true, "Cupom criado com sucesso");
    }
}
