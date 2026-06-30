namespace CodeTicket.API.DTOs;

public class ComprarIngressoDto
{
    public string UsuarioCpf { get; set; } = string.Empty;
    public int EventoId { get; set; }
    public string? CupomCodigo { get; set; }
}
