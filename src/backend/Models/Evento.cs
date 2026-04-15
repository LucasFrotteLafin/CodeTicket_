namespace CodeTicket.API.Models;

public class Evento
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public int CapacidadeTotal { get; set; }
    public DateTime DataEvento { get; set; }
    public decimal PrecoPadrao { get; set; }
    public string UsuarioCpf { get; set; } = null!;
    public string? Descricao { get; set; }
    public string? Local { get; set; }
    public string? ImagemUrl { get; set; }
}
