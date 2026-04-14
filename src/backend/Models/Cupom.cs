namespace CodeTicket.API.Models;

public class Cupom
{
    public string Codigo { get; set; } = null!;
    public decimal PorcentagemDesconto { get; set; }
    public decimal ValorMinimoRegra { get; set; }
}
