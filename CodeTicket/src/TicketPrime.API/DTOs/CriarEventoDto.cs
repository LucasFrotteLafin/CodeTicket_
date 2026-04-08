namespace TicketPrime.API.DTOs;

public class CriarEventoDto
{
    public string Nome { get; set; } = null!;
    public int CapacidadeTotal { get; set; }
    public DateTime DataEvento { get; set; }
    public decimal PrecoPadrao { get; set; }
}
