using System.Net.Http.Json;
using CodeTicket.Frontend.Models;

namespace CodeTicket.Frontend.Services;

public class ApiService(HttpClient http)
{
    public async Task<(bool sucesso, string mensagem)> CriarUsuario(CriarUsuarioModel model)
    {
        try
        {
            var response = await http.PostAsJsonAsync("/api/usuarios", model);
            var mensagem = await response.Content.ReadAsStringAsync();
            return (response.IsSuccessStatusCode, mensagem.Trim('"'));
        }
        catch
        {
            return (false, "Erro ao conectar com a API. Verifique se o backend está rodando.");
        }
    }

    public async Task<(bool sucesso, string mensagem)> CriarEvento(CriarEventoModel model)
    {
        try
        {
            var response = await http.PostAsJsonAsync("/api/eventos", model);
            var mensagem = await response.Content.ReadAsStringAsync();
            return (response.IsSuccessStatusCode, mensagem.Trim('"'));
        }
        catch
        {
            return (false, "Erro ao conectar com a API. Verifique se o backend está rodando.");
        }
    }

    public async Task<List<EventoListarModel>> ListarEventos()
    {
        try
        {
            return await http.GetFromJsonAsync<List<EventoListarModel>>("/api/eventos") ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<(bool sucesso, string mensagem)> CriarCupom(CriarCupomModel model)
    {
        try
        {
            var response = await http.PostAsJsonAsync("/api/cupons", model);
            var mensagem = await response.Content.ReadAsStringAsync();
            return (response.IsSuccessStatusCode, mensagem.Trim('"'));
        }
        catch
        {
            return (false, "Erro ao conectar com a API. Verifique se o backend está rodando.");
        }
    }
}
