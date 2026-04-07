using Microsoft.AspNetCore.Mvc;
using TicketPrime.API.Models;

namespace TicketPrime.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventoController : ControllerBase
{
    [HttpPost]
    public void AdicionaEvento([FromBody] Evento evento)
    {

    }
}
