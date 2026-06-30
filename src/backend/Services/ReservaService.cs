using CodeTicket.API.DTOs;
using CodeTicket.API.Repositories;

namespace CodeTicket.API.Services;

public class ReservaService
{
    private readonly IReservaRepository _reservaRepo;
    private readonly IEventoRepository _eventoRepo;
    private readonly ICupomRepository _cupomRepo;
    private readonly IUsuarioRepository _usuarioRepo;

    public ReservaService(
        IReservaRepository reservaRepo, 
        IEventoRepository eventoRepo, 
        ICupomRepository cupomRepo,
        IUsuarioRepository usuarioRepo)
    {
        _reservaRepo = reservaRepo;
        _eventoRepo = eventoRepo;
        _cupomRepo = cupomRepo;
        _usuarioRepo = usuarioRepo;
    }

    public async Task<(bool Sucesso, string Mensagem, int? ReservaId)> ComprarIngresso(ComprarIngressoDto dto)
    {
        // VALIDAÇÃO 1: Verificar se o usuário existe
        var usuario = await _usuarioRepo.BuscarPorCpf(dto.UsuarioCpf);
        if (usuario == null)
        {
            return (false, "Usuário não encontrado. Cadastre-se antes de comprar ingressos.", null);
        }

        // VALIDAÇÃO 2: Verificar se o evento existe
        var evento = await _eventoRepo.ObterPorId(dto.EventoId);
        if (evento == null)
        {
            return (false, "Evento não encontrado.", null);
        }

        // VALIDAÇÃO 3: Verificar se o evento já passou
        if (evento.DataEvento < DateTime.Now)
        {
            return (false, "Não é possível comprar ingressos para eventos que já ocorreram.", null);
        }

        // VALIDAÇÃO 4: Verificar se há vagas disponíveis (capacidade)
        var reservasExistentes = await _reservaRepo.ContarReservasPorEvento(dto.EventoId);
        if (reservasExistentes >= evento.CapacidadeTotal)
        {
            return (false, "Ingressos esgotados para este evento.", null);
        }

        // VALIDAÇÃO 5: Verificar se o usuário já comprou ingresso para este evento (bloquear cambistas)
        var jaComprou = await _reservaRepo.UsuarioJaComprouIngresso(dto.UsuarioCpf, dto.EventoId);
        if (jaComprou)
        {
            return (false, "Você já possui um ingresso para este evento. Não é permitido comprar mais de um ingresso por CPF.", null);
        }

        decimal valorFinal = evento.PrecoPadrao;

        // VALIDAÇÃO 6: Se houver cupom, validar e aplicar desconto
        if (!string.IsNullOrWhiteSpace(dto.CupomCodigo))
        {
            var cupom = await _cupomRepo.ObterPorCodigo(dto.CupomCodigo);
            
            if (cupom == null)
            {
                return (false, $"Cupom '{dto.CupomCodigo}' não existe.", null);
            }

            // VALIDAÇÃO 7: Verificar valor mínimo para usar o cupom
            if (evento.PrecoPadrao < cupom.ValorMinimoRegra)
            {
                return (false, $"O valor do ingresso (R$ {evento.PrecoPadrao:F2}) é inferior ao valor mínimo (R$ {cupom.ValorMinimoRegra:F2}) para usar este cupom.", null);
            }

            // Aplicar desconto
            decimal desconto = evento.PrecoPadrao * (cupom.PorcentagemDesconto / 100);
            valorFinal = evento.PrecoPadrao - desconto;

            // VALIDAÇÃO 8: Garantir que o valor final não seja negativo
            if (valorFinal < 0)
            {
                valorFinal = 0;
            }
        }

        // Inserir reserva no banco
        var reservaId = await _reservaRepo.InserirReserva(
            dto.UsuarioCpf, 
            dto.EventoId, 
            dto.CupomCodigo, 
            valorFinal
        );

        return (true, $"Ingresso comprado com sucesso! Valor pago: R$ {valorFinal:F2}", reservaId);
    }

    public async Task<(bool Sucesso, string Mensagem, List<IngressoDetalheDto>? Ingressos)> ListarMeusIngressos(string cpf)
    {
        // Validar se o CPF foi informado
        if (string.IsNullOrWhiteSpace(cpf))
        {
            return (false, "CPF não informado.", null);
        }

        // Verificar se o usuário existe
        var usuario = await _usuarioRepo.BuscarPorCpf(cpf);
        if (usuario == null)
        {
            return (false, "Usuário não encontrado.", null);
        }

        var ingressos = await _reservaRepo.ListarIngressosUsuario(cpf);
        
        if (ingressos.Count == 0)
        {
            return (true, "Você ainda não possui ingressos.", new List<IngressoDetalheDto>());
        }

        return (true, $"{ingressos.Count} ingresso(s) encontrado(s).", ingressos);
    }
}
