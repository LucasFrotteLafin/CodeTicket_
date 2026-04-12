using CodeTicket.API.DTOs;
using CodeTicket.API.Models;
using CodeTicket.API.Repositories;

namespace CodeTicket.API.Services;

public class CupomService(ICupomRepository repository)
{
    public async Task<(bool sucesso, string mensagem)> CriarCupom(CriarCupomDto dto)
    {
        if (string.IsNullOrEmpty(dto.Codigo))
            return (false, "Código é obrigatório");

        if (dto.PorcentagemDesconto <= 0 || dto.PorcentagemDesconto > 100)
            return (false, "Desconto deve ser entre 0 e 100");

        if (dto.ValorMinimoRegra < 0)
            return (false, "Valor mínimo não pode ser negativo");

        if (await repository.ExisteCupom(dto.Codigo))
            return (false, "Cupom já existe");

        await repository.InserirCupom(new Cupom
        {
            Codigo = dto.Codigo,
            PorcentagemDesconto = dto.PorcentagemDesconto,
            ValorMinimoRegra = dto.ValorMinimoRegra
        });

        return (true, "Cupom criado com sucesso");
    }
}
