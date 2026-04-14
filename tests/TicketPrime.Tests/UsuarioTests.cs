using CodeTicket.API.DTOs;
using CodeTicket.API.Models;
using CodeTicket.API.Repositories;
using CodeTicket.API.Services;
using NSubstitute;
using Xunit;

namespace CodeTicket.Tests;

public class UsuarioTests
{
    private UsuarioService CriarService(Usuario? usuarioExistente = null)
    {
        var repo = Substitute.For<IUsuarioRepository>();
        repo.BuscarPorCpf(Arg.Any<string>()).Returns(usuarioExistente);
        return new UsuarioService(repo);
    }

    [Fact]
    public async Task CriarUsuario_CpfValido_RetornaSucesso()
    {
        var service = CriarService();
        var dto = new CriarUsuarioDto { Cpf = "12345678901", Nome = "Lucas", Email = "lucas@email.com" };

        var (sucesso, _) = await service.CriarUsuario(dto);

        Assert.True(sucesso);
    }

    [Fact]
    public async Task CriarUsuario_CpfJaExistente_RetornaErro()
    {
        var service = CriarService(usuarioExistente: new Usuario { Cpf = "12345678901", Nome = "Lucas", Email = "lucas@email.com" });
        var dto = new CriarUsuarioDto { Cpf = "12345678901", Nome = "Lucas", Email = "lucas@email.com" };

        var (sucesso, mensagem) = await service.CriarUsuario(dto);

        Assert.False(sucesso);
        Assert.Equal("CPF já cadastrado.", mensagem);
    }

    [Fact]
    public async Task CriarUsuario_CpfInvalido_RetornaErro()
    {
        var service = CriarService();
        var dto = new CriarUsuarioDto { Cpf = "123", Nome = "Lucas", Email = "lucas@email.com" };

        var (sucesso, _) = await service.CriarUsuario(dto);

        Assert.False(sucesso);
    }

    [Fact]
    public async Task CriarUsuario_EmailInvalido_RetornaErro()
    {
        var service = CriarService();
        var dto = new CriarUsuarioDto { Cpf = "12345678901", Nome = "Lucas", Email = "emailinvalido" };

        var (sucesso, _) = await service.CriarUsuario(dto);

        Assert.False(sucesso);
    }
}
