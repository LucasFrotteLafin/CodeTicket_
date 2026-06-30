namespace CodeTicket.API.DTOs;

public class IngressoDetalheDto
{
    public int ReservaId { get; set; }
    public string UsuarioNome { get; set; } = string.Empty;
    public string UsuarioCpf { get; set; } = string.Empty;
    public string EventoNome { get; set; } = string.Empty;
    public DateTime DataEvento { get; set; }
    public string? LocalEvento { get; set; }
    public decimal PrecoPadrao { get; set; }
    public string? CupomUtilizado { get; set; }
    public decimal? DescontoAplicado { get; set; }
    public decimal ValorFinalPago { get; set; }
}
