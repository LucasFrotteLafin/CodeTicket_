using NSubstitute;
using Xunit;
using CodeTicket.API.DTOs;
using CodeTicket.API.Models;
using CodeTicket.API.Repositories;
using CodeTicket.API.Services;

namespace TicketPrime.Tests;

public class ReservaTests
{
    [Fact]
    public async Task ComprarIngresso_QuandoUsuarioNaoExiste_DeveRetornarErro()
    {
        // Arrange
        var reservaRepo = Substitute.For<IReservaRepository>();
        var eventoRepo = Substitute.For<IEventoRepository>();
        var cupomRepo = Substitute.For<ICupomRepository>();
        var usuarioRepo = Substitute.For<IUsuarioRepository>();
        
        usuarioRepo.ObterPorCpf(Arg.Any<string>()).Returns((Usuario?)null);
        
        var service = new ReservaService(reservaRepo, eventoRepo, cupomRepo, usuarioRepo);
        var dto = new ComprarIngressoDto 
        { 
            UsuarioCpf = "12345678901", 
            EventoId = 1 
        };

        // Act
        var (sucesso, mensagem, _) = await service.ComprarIngresso(dto);

        // Assert
        Assert.False(sucesso);
        Assert.Contains("Usuário não encontrado", mensagem);
    }

    [Fact]
    public async Task ComprarIngresso_QuandoEventoNaoExiste_DeveRetornarErro()
    {
        // Arrange
        var reservaRepo = Substitute.For<IReservaRepository>();
        var eventoRepo = Substitute.For<IEventoRepository>();
        var cupomRepo = Substitute.For<ICupomRepository>();
        var usuarioRepo = Substitute.For<IUsuarioRepository>();
        
        usuarioRepo.ObterPorCpf(Arg.Any<string>()).Returns(new Usuario { Cpf = "12345678901" });
        eventoRepo.ObterPorId(Arg.Any<int>()).Returns((Evento?)null);
        
        var service = new ReservaService(reservaRepo, eventoRepo, cupomRepo, usuarioRepo);
        var dto = new ComprarIngressoDto 
        { 
            UsuarioCpf = "12345678901", 
            EventoId = 999 
        };

        // Act
        var (sucesso, mensagem, _) = await service.ComprarIngresso(dto);

        // Assert
        Assert.False(sucesso);
        Assert.Contains("Evento não encontrado", mensagem);
    }

    [Fact]
    public async Task ComprarIngresso_QuandoEventoJaPassou_DeveRetornarErro()
    {
        // Arrange
        var reservaRepo = Substitute.For<IReservaRepository>();
        var eventoRepo = Substitute.For<IEventoRepository>();
        var cupomRepo = Substitute.For<ICupomRepository>();
        var usuarioRepo = Substitute.For<IUsuarioRepository>();
        
        usuarioRepo.ObterPorCpf(Arg.Any<string>()).Returns(new Usuario { Cpf = "12345678901" });
        eventoRepo.ObterPorId(Arg.Any<int>()).Returns(new Evento 
        { 
            Id = 1, 
            DataEvento = DateTime.Now.AddDays(-1) 
        });
        
        var service = new ReservaService(reservaRepo, eventoRepo, cupomRepo, usuarioRepo);
        var dto = new ComprarIngressoDto 
        { 
            UsuarioCpf = "12345678901", 
            EventoId = 1 
        };

        // Act
        var (sucesso, mensagem, _) = await service.ComprarIngresso(dto);

        // Assert
        Assert.False(sucesso);
        Assert.Contains("já ocorreram", mensagem);
    }

    [Fact]
    public async Task ComprarIngresso_QuandoIngressosEsgotados_DeveRetornarErro()
    {
        // Arrange
        var reservaRepo = Substitute.For<IReservaRepository>();
        var eventoRepo = Substitute.For<IEventoRepository>();
        var cupomRepo = Substitute.For<ICupomRepository>();
        var usuarioRepo = Substitute.For<IUsuarioRepository>();
        
        usuarioRepo.ObterPorCpf(Arg.Any<string>()).Returns(new Usuario { Cpf = "12345678901" });
        eventoRepo.ObterPorId(Arg.Any<int>()).Returns(new Evento 
        { 
            Id = 1, 
            DataEvento = DateTime.Now.AddDays(30),
            CapacidadeTotal = 100,
            PrecoPadrao = 50
        });
        reservaRepo.ContarReservasPorEvento(Arg.Any<int>()).Returns(100);
        
        var service = new ReservaService(reservaRepo, eventoRepo, cupomRepo, usuarioRepo);
        var dto = new ComprarIngressoDto 
        { 
            UsuarioCpf = "12345678901", 
            EventoId = 1 
        };

        // Act
        var (sucesso, mensagem, _) = await service.ComprarIngresso(dto);

        // Assert
        Assert.False(sucesso);
        Assert.Contains("esgotados", mensagem);
    }

    [Fact]
    public async Task ComprarIngresso_QuandoUsuarioJaPossuiIngresso_DeveBloquearCambista()
    {
        // Arrange
        var reservaRepo = Substitute.For<IReservaRepository>();
        var eventoRepo = Substitute.For<IEventoRepository>();
        var cupomRepo = Substitute.For<ICupomRepository>();
        var usuarioRepo = Substitute.For<IUsuarioRepository>();
        
        usuarioRepo.ObterPorCpf(Arg.Any<string>()).Returns(new Usuario { Cpf = "12345678901" });
        eventoRepo.ObterPorId(Arg.Any<int>()).Returns(new Evento 
        { 
            Id = 1, 
            DataEvento = DateTime.Now.AddDays(30),
            CapacidadeTotal = 100,
            PrecoPadrao = 50
        });
        reservaRepo.ContarReservasPorEvento(Arg.Any<int>()).Returns(50);
        reservaRepo.UsuarioJaComprouIngresso(Arg.Any<string>(), Arg.Any<int>()).Returns(true);
        
        var service = new ReservaService(reservaRepo, eventoRepo, cupomRepo, usuarioRepo);
        var dto = new ComprarIngressoDto 
        { 
            UsuarioCpf = "12345678901", 
            EventoId = 1 
        };

        // Act
        var (sucesso, mensagem, _) = await service.ComprarIngresso(dto);

        // Assert
        Assert.False(sucesso);
        Assert.Contains("já possui um ingresso", mensagem);
    }

    [Fact]
    public async Task ComprarIngresso_ComCupomInvalido_DeveRetornarErro()
    {
        // Arrange
        var reservaRepo = Substitute.For<IReservaRepository>();
        var eventoRepo = Substitute.For<IEventoRepository>();
        var cupomRepo = Substitute.For<ICupomRepository>();
        var usuarioRepo = Substitute.For<IUsuarioRepository>();
        
        usuarioRepo.ObterPorCpf(Arg.Any<string>()).Returns(new Usuario { Cpf = "12345678901" });
        eventoRepo.ObterPorId(Arg.Any<int>()).Returns(new Evento 
        { 
            Id = 1, 
            DataEvento = DateTime.Now.AddDays(30),
            CapacidadeTotal = 100,
            PrecoPadrao = 50
        });
        reservaRepo.ContarReservasPorEvento(Arg.Any<int>()).Returns(50);
        reservaRepo.UsuarioJaComprouIngresso(Arg.Any<string>(), Arg.Any<int>()).Returns(false);
        cupomRepo.ObterPorCodigo(Arg.Any<string>()).Returns((Cupom?)null);
        
        var service = new ReservaService(reservaRepo, eventoRepo, cupomRepo, usuarioRepo);
        var dto = new ComprarIngressoDto 
        { 
            UsuarioCpf = "12345678901", 
            EventoId = 1,
            CupomCodigo = "INVALIDO"
        };

        // Act
        var (sucesso, mensagem, _) = await service.ComprarIngresso(dto);

        // Assert
        Assert.False(sucesso);
        Assert.Contains("não existe", mensagem);
    }

    [Fact]
    public async Task ComprarIngresso_ComValorAbaixoDoMinimo_DeveRejeitarCupom()
    {
        // Arrange
        var reservaRepo = Substitute.For<IReservaRepository>();
        var eventoRepo = Substitute.For<IEventoRepository>();
        var cupomRepo = Substitute.For<ICupomRepository>();
        var usuarioRepo = Substitute.For<IUsuarioRepository>();
        
        usuarioRepo.ObterPorCpf(Arg.Any<string>()).Returns(new Usuario { Cpf = "12345678901" });
        eventoRepo.ObterPorId(Arg.Any<int>()).Returns(new Evento 
        { 
            Id = 1, 
            DataEvento = DateTime.Now.AddDays(30),
            CapacidadeTotal = 100,
            PrecoPadrao = 50
        });
        reservaRepo.ContarReservasPorEvento(Arg.Any<int>()).Returns(50);
        reservaRepo.UsuarioJaComprouIngresso(Arg.Any<string>(), Arg.Any<int>()).Returns(false);
        cupomRepo.ObterPorCodigo(Arg.Any<string>()).Returns(new Cupom 
        { 
            Codigo = "DESC20",
            PorcentagemDesconto = 20,
            ValorMinimoRegra = 100
        });
        
        var service = new ReservaService(reservaRepo, eventoRepo, cupomRepo, usuarioRepo);
        var dto = new ComprarIngressoDto 
        { 
            UsuarioCpf = "12345678901", 
            EventoId = 1,
            CupomCodigo = "DESC20"
        };

        // Act
        var (sucesso, mensagem, _) = await service.ComprarIngresso(dto);

        // Assert
        Assert.False(sucesso);
        Assert.Contains("valor mínimo", mensagem);
    }

    [Fact]
    public async Task ComprarIngresso_ComTodosOsDadosValidos_DeveCriarReservaComSucesso()
    {
        // Arrange
        var reservaRepo = Substitute.For<IReservaRepository>();
        var eventoRepo = Substitute.For<IEventoRepository>();
        var cupomRepo = Substitute.For<ICupomRepository>();
        var usuarioRepo = Substitute.For<IUsuarioRepository>();
        
        usuarioRepo.ObterPorCpf(Arg.Any<string>()).Returns(new Usuario { Cpf = "12345678901" });
        eventoRepo.ObterPorId(Arg.Any<int>()).Returns(new Evento 
        { 
            Id = 1, 
            DataEvento = DateTime.Now.AddDays(30),
            CapacidadeTotal = 100,
            PrecoPadrao = 50
        });
        reservaRepo.ContarReservasPorEvento(Arg.Any<int>()).Returns(50);
        reservaRepo.UsuarioJaComprouIngresso(Arg.Any<string>(), Arg.Any<int>()).Returns(false);
        reservaRepo.InserirReserva(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<decimal>())
            .Returns(123);
        
        var service = new ReservaService(reservaRepo, eventoRepo, cupomRepo, usuarioRepo);
        var dto = new ComprarIngressoDto 
        { 
            UsuarioCpf = "12345678901", 
            EventoId = 1
        };

        // Act
        var (sucesso, mensagem, reservaId) = await service.ComprarIngresso(dto);

        // Assert
        Assert.True(sucesso);
        Assert.Contains("sucesso", mensagem);
        Assert.Equal(123, reservaId);
    }

    [Fact]
    public async Task ListarMeusIngressos_QuandoCpfVazio_DeveRetornarErro()
    {
        // Arrange
        var reservaRepo = Substitute.For<IReservaRepository>();
        var eventoRepo = Substitute.For<IEventoRepository>();
        var cupomRepo = Substitute.For<ICupomRepository>();
        var usuarioRepo = Substitute.For<IUsuarioRepository>();
        
        var service = new ReservaService(reservaRepo, eventoRepo, cupomRepo, usuarioRepo);

        // Act
        var (sucesso, mensagem, _) = await service.ListarMeusIngressos("");

        // Assert
        Assert.False(sucesso);
        Assert.Contains("CPF não informado", mensagem);
    }

    [Fact]
    public async Task ListarMeusIngressos_QuandoUsuarioExisteComIngressos_DeveRetornarLista()
    {
        // Arrange
        var reservaRepo = Substitute.For<IReservaRepository>();
        var eventoRepo = Substitute.For<IEventoRepository>();
        var cupomRepo = Substitute.For<ICupomRepository>();
        var usuarioRepo = Substitute.For<IUsuarioRepository>();
        
        usuarioRepo.ObterPorCpf(Arg.Any<string>()).Returns(new Usuario { Cpf = "12345678901" });
        reservaRepo.ListarIngressosUsuario(Arg.Any<string>()).Returns(new List<IngressoDetalheDto>
        {
            new IngressoDetalheDto { ReservaId = 1, EventoNome = "Show" }
        });
        
        var service = new ReservaService(reservaRepo, eventoRepo, cupomRepo, usuarioRepo);

        // Act
        var (sucesso, mensagem, ingressos) = await service.ListarMeusIngressos("12345678901");

        // Assert
        Assert.True(sucesso);
        Assert.NotNull(ingressos);
        Assert.Single(ingressos);
    }
}
