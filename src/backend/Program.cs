using Microsoft.OpenApi.Models;
using CodeTicket.API.DTOs;
using CodeTicket.API.Repositories;
using CodeTicket.API.Services;

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CodeTicket API", Version = "v1" });
});

builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<IEventoRepository, EventoRepository>();
builder.Services.AddScoped<EventoService>();
builder.Services.AddScoped<ICupomRepository, CupomRepository>();
builder.Services.AddScoped<CupomService>();
builder.Services.AddScoped<IReservaRepository, ReservaRepository>();
builder.Services.AddScoped<ReservaService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "5007";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "CodeTicket API v1");
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

// COMPRAR INGRESSO - Endpoint com múltiplas validações de negócio
app.MapPost("/api/ingressos/comprar", async (ComprarIngressoDto dto, ReservaService service) =>
{
    var (sucesso, mensagem, reservaId) = await service.ComprarIngresso(dto);
    
    if (sucesso)
    {
        return Results.Ok(new { mensagem, reservaId });
    }
    
    return Results.BadRequest(new { mensagem });
});

// LISTAR MEUS INGRESSOS - Endpoint com JOIN entre reservas, usuarios e eventos
app.MapGet("/api/ingressos/meus/{cpf}", async (string cpf, ReservaService service) =>
{
    var (sucesso, mensagem, ingressos) = await service.ListarMeusIngressos(cpf);
    
    if (sucesso)
    {
        return Results.Ok(new { mensagem, ingressos });
    }
    
    return Results.BadRequest(new { mensagem });
});

app.Run();
