namespace CodeTicket.Frontend.Models;

public class CriarUsuarioModel
{
    public string Cpf { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
}

public class CriarEventoModel
{
    public string Nome { get; set; } = "";
    public int CapacidadeTotal { get; set; }
    public DateTime? DataEvento { get; set; }
    public decimal PrecoPadrao { get; set; }
    public string UsuarioCpf { get; set; } = "";
    public string? Descricao { get; set; }
    public string? Local { get; set; }
    public string? ImagemUrl { get; set; }
}

public class CriarCupomModel
{
    public string Codigo { get; set; } = "";
    public decimal PorcentagemDesconto { get; set; }
    public decimal ValorMinimoRegra { get; set; }
}

public class EventoListarModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string NomeUsuario { get; set; } = "";
    public DateTime DataEvento { get; set; }
    public decimal PrecoPadrao { get; set; }
    public int CapacidadeTotal { get; set; }
    public string? Descricao { get; set; }
    public string? Local { get; set; }
    public string? ImagemUrl { get; set; }
}
