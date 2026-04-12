namespace CodeTicket.API.DTOs;

public class CriarCupomDto
{
    public string Codigo { get; set; } = null!;
    public decimal PorcentagemDesconto { get; set; }
    public decimal ValorMinimoRegra { get; set; }
}
