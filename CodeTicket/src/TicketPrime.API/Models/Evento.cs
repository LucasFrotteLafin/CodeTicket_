namespace TicketPrime.API.Models;

public class Evento
{
    public int Id { get; set; }
    public string Nome { get; set; } = null!;
    public int CapacidadeTotal { get; set; }
    public DateTime DataEvento { get; set; }
    public decimal PrecoPadrao { get; set; }
}
