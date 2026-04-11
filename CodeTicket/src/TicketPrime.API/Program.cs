using Microsoft.OpenApi.Models;
using TicketPrime.API.DTOs;
using TicketPrime.API.Repositories;
using TicketPrime.API.Services;

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "TicketPrime API", Version = "v1" });
});

builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<EventoRepository>();
builder.Services.AddScoped<EventoService>();
builder.Services.AddScoped<CupomRepository>();
builder.Services.AddScoped<CupomService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TicketPrime API v1");
    options.RoutePrefix = string.Empty;
});

app.UseCors();

// Endpoint Criar Evento
app.MapPost("/api/eventos", async (CriarEventoDto dto, EventoService service) =>
{
    var (sucesso, mensagem) = await service.CriarEvento(dto);
    return sucesso ? Results.Ok(mensagem) : Results.BadRequest(mensagem);
});

//Endpoint Criar Usuario
app.MapPost("/api/usuarios", async (CriarUsuarioDto dto, UsuarioService service) =>
{
    var (sucesso, mensagem) = await service.CriarUsuario(dto);
    return sucesso ? Results.Ok(mensagem) : Results.BadRequest(mensagem);
});

// Endpoint Criar Cupom
app.MapPost("/api/cupons", async (CriarCupomDto dto, CupomService service) =>
{
    var (sucesso, mensagem) = await service.CriarCupom(dto);
    return sucesso ? Results.Ok(mensagem) : Results.BadRequest(mensagem);
});

//  LISTAR EVENTOS
app.MapGet("/api/eventos", async (EventoService service) =>
{
    var eventos = await service.ListarEventos();
    return Results.Ok(eventos);
});

app.Run();
