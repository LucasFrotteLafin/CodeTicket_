using System.Text.RegularExpressions;
using CodeTicket.API.DTOs;
using CodeTicket.API.Models;
using CodeTicket.API.Repositories;

namespace CodeTicket.API.Services;

public class UsuarioService(IUsuarioRepository repository)
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public async Task<(bool sucesso, string mensagem)> CriarUsuario(CriarUsuarioDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Cpf))
            return (false, "CPF é obrigatório.");

        if (!Regex.IsMatch(dto.Cpf, @"^\d{11}$"))
            return (false, "CPF deve conter exatamente 11 dígitos numéricos.");

        if (string.IsNullOrWhiteSpace(dto.Nome))
            return (false, "Nome é obrigatório.");

        if (string.IsNullOrWhiteSpace(dto.Email))
            return (false, "Email é obrigatório.");

        if (!EmailRegex.IsMatch(dto.Email))
            return (false, "Email inválido.");

        var existente = await repository.BuscarPorCpf(dto.Cpf);
        if (existente != null)
            return (false, "CPF já cadastrado.");

        await repository.Criar(new Usuario
        {
            Cpf = dto.Cpf,
            Nome = dto.Nome,
            Email = dto.Email
        });
        return (true, "Usuário cadastrado com sucesso!");
    }
}
