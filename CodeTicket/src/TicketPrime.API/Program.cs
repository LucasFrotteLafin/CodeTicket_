using Microsoft.OpenApi.Models;
using TicketPrime.API.DTOs;
using Dapper;
using Npgsql;
using System.Data

var builder = WebApplication.CreateBuilder(args);
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "TicketPrime API", Version = "v1" });
});


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

app.UseHttpsRedirection();

app.UseCors();

app.MapPost("/api/evento", (CriarEventoDto dto) =>
{
    return Results.Ok();
});


//NÃO deixar hardcoded na entrega final, mas ok por enquanto!!!
string connectionString = "Host=localhost;Port=5432;Database=ticketprime;Username=postgres;Password=123456";

//Endpoint:POST /api/usuarios
app.MapPost("/api/usuarios", async (Usuario usuario) =>
{
using (IDbConnection db = new NpgsqlConnection(connectionString))
{
//Verifica se CPF ja existe
var usuarioExistente = await db.QueryFirstOrDefaultAsync<Usuario>(
    "SELECT * FROM Usuarios WHERE Cpf = @Cpf",
    new { usuario.Cpf }
);

if (usuarioExistente != null)
{
return Results.BadRequest("CPF já cadastrado.");
}

//insere no banco
var sql = @"INSERT INTO Usuarios (Cpf, Nome, Email)
                    VALUES (@Cpf, @Nome, @Email)";

await db.ExecuteAsync(sql, usuario);

return Results.Ok("Usuário cadastrado com sucesso!");
}
});

app.Run();

//Modelo teste
public class Usuario
{
    public string Cpf { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
}
