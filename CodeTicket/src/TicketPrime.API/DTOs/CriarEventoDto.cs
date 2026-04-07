namespace TicketPrime.API.DTOs;

public record CriarEventoDto(
    string Nome,
    int CapacidadeTotal,
    DateTime DataEvento,
    decimal PrecoDoIngresso
);
