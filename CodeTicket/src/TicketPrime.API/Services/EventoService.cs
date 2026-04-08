using TicketPrime.API.DTOs;
using TicketPrime.API.Models;
using TicketPrime.API.Repositories;

namespace TicketPrime.API.Services;

public class EventoService(EventoRepository repository)
{
  public async Task<(bool sucesso, string mensagem)  > CriarEvento(CriarEventoDto dto)
  {
    if(string.IsNullOrWhiteSpace(dto.Nome))
      return(false, "O nome do evento é obrigatório.");

    if(dto.CapacidadeTotal <= 0)
      return(false, "A capacidade total deve ser maior que zero.");

    if (dto.DataEvento == default)
      return (false, "A data do evento é obrigatória.");

    if (dto.DataEvento.Date < DateTime.Today)
      return (false, "A data do evento não pode ser anterior ao dia de hoje.");

    if(dto.PrecoPadrao <= 0)
      return(false, "O preço do evento deve ser maior que zero.");

    await repository.Criar(new Evento
    {
      Nome = dto.Nome,
      CapacidadeTotal = dto.CapacidadeTotal,
      DataEvento = dto.DataEvento,
      PrecoPadrao = dto.PrecoPadrao
    });

    return (true, "Evento criado com sucesso!");
  }
}