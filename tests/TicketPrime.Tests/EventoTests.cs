using CodeTicket.API.DTOs;
using CodeTicket.API.Repositories;
using CodeTicket.API.Services;
using NSubstitute;
using Xunit;

namespace CodeTicket.Tests;

public class EventoTests
{
    private EventoService CriarService()
    {
        var repo = Substitute.For<IEventoRepository>();
        return new EventoService(repo);
    }

    [Fact]
    public async Task CriarEvento_DadosValidos_RetornaSucesso()
    {
        var service = CriarService();
        var dto = new CriarEventoDto
        {
            Nome = "Show do Ano",
            CapacidadeTotal = 1000,
            DataEvento = DateTime.Today.AddDays(10),
            PrecoPadrao = 150.00m
        };

        var (sucesso, _) = await service.CriarEvento(dto);

        Assert.True(sucesso);
    }

    [Fact]
    public async Task CriarEvento_CapacidadeZero_RetornaErro()
    {
        var service = CriarService();
        var dto = new CriarEventoDto
        {
            Nome = "Show do Ano",
            CapacidadeTotal = 0,
            DataEvento = DateTime.Today.AddDays(10),
            PrecoPadrao = 150.00m
        };

        var (sucesso, _) = await service.CriarEvento(dto);

        Assert.False(sucesso);
    }

    [Fact]
    public async Task CriarEvento_DataPassada_RetornaErro()
    {
        var service = CriarService();
        var dto = new CriarEventoDto
        {
            Nome = "Show do Ano",
            CapacidadeTotal = 500,
            DataEvento = DateTime.Today.AddDays(-1),
            PrecoPadrao = 150.00m
        };

        var (sucesso, _) = await service.CriarEvento(dto);

        Assert.False(sucesso);
    }

    [Fact]
    public async Task CriarEvento_PrecoZero_RetornaErro()
    {
        var service = CriarService();
        var dto = new CriarEventoDto
        {
            Nome = "Show do Ano",
            CapacidadeTotal = 500,
            DataEvento = DateTime.Today.AddDays(10),
            PrecoPadrao = 0
        };

        var (sucesso, _) = await service.CriarEvento(dto);

        Assert.False(sucesso);
    }
}
