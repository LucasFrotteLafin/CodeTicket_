using CodeTicket.API.DTOs;
using CodeTicket.API.Repositories;
using CodeTicket.API.Services;
using NSubstitute;
using Xunit;

namespace CodeTicket.Tests;

public class CupomTests
{
    private CupomService CriarService(bool cupomExiste = false)
    {
        var repo = Substitute.For<ICupomRepository>();
        repo.ExisteCupom(Arg.Any<string>()).Returns(cupomExiste);
        return new CupomService(repo);
    }

    [Fact]
    public async Task CriarCupom_DadosValidos_RetornaSucesso()
    {
        var service = CriarService();
        var dto = new CriarCupomDto { Codigo = "PROMO10", PorcentagemDesconto = 10, ValorMinimoRegra = 100 };

        var (sucesso, _) = await service.CriarCupom(dto);

        Assert.True(sucesso);
    }

    [Fact]
    public async Task CriarCupom_CodigoJaExistente_RetornaErro()
    {
        var service = CriarService(cupomExiste: true);
        var dto = new CriarCupomDto { Codigo = "PROMO10", PorcentagemDesconto = 10, ValorMinimoRegra = 100 };

        var (sucesso, mensagem) = await service.CriarCupom(dto);

        Assert.False(sucesso);
        Assert.Equal("Cupom já existe", mensagem);
    }

    [Fact]
    public async Task CriarCupom_DescontoAcimaDe100_RetornaErro()
    {
        var service = CriarService();
        var dto = new CriarCupomDto { Codigo = "PROMO10", PorcentagemDesconto = 110, ValorMinimoRegra = 100 };

        var (sucesso, _) = await service.CriarCupom(dto);

        Assert.False(sucesso);
    }

    [Fact]
    public async Task CriarCupom_ValorMinimoNegativo_RetornaErro()
    {
        var service = CriarService();
        var dto = new CriarCupomDto { Codigo = "PROMO10", PorcentagemDesconto = 10, ValorMinimoRegra = -1 };

        var (sucesso, _) = await service.CriarCupom(dto);

        Assert.False(sucesso);
    }
}
