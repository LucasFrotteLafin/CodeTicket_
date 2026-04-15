namespace CodeTicket.API.DTOs;

public class EventoListarDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public string NomeUsuario { get; set; } = null!;
    public DateTime DataEvento { get; set; }
    public decimal PrecoPadrao { get; set; }
    public int CapacidadeTotal { get; set; }
    public string? Descricao { get; set; }
    public string? Local { get; set; }
    public string? ImagemUrl { get; set; }
}
