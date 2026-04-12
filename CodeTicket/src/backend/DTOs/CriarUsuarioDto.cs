namespace CodeTicket.API.DTOs;

public class CriarUsuarioDto
{
    public string Cpf { get; set; } = null!;
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
}
