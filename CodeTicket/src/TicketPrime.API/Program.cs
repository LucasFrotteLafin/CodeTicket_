using Microsoft.OpenApi.Models;
using TicketPrime.API.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "TicketPrime API", Version = "v1" });
});

// Add CORS
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

// Serve Swagger UI at application root so visiting '/' opens the docs
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TicketPrime API v1");
    options.RoutePrefix = string.Empty; // serve at '/'
});

app.UseHttpsRedirection();

app.UseCors();

app.MapPost("/api/evento", (CriarEventoDto dto) =>
{
    return Results.Ok();
});

app.Run();
