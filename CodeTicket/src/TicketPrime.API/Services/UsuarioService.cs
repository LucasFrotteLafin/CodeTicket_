using System.Text.RegularExpressions;
using TicketPrime.API.DTOs;
using TicketPrime.API.Models;
using TicketPrime.API.Repositories;

namespace TicketPrime.API.Services;

public class UsuarioService(UsuarioRepository repository)
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
