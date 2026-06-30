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

    public async Task<(bool sucesso, string mensagem, int? reservaId)> ComprarIngresso(ComprarIngressoModel model)
    {
        try
        {
            var response = await http.PostAsJsonAsync("/api/ingressos/comprar", model);
            
            if (response.IsSuccessStatusCode)
            {
                var resultado = await response.Content.ReadFromJsonAsync<ComprarIngressoResponse>();
                return (true, resultado?.Mensagem ?? "Compra realizada!", resultado?.ReservaId);
            }
            else
            {
                var erro = await response.Content.ReadFromJsonAsync<ErroResponse>();
                return (false, erro?.Mensagem ?? "Erro ao comprar ingresso.", null);
            }
        }
        catch
        {
            return (false, "Erro ao conectar com a API. Verifique se o backend está rodando.", null);
        }
    }

    public async Task<(bool sucesso, string mensagem, List<IngressoDetalheModel>? ingressos)> ListarMeusIngressos(string cpf)
    {
        try
        {
            var response = await http.GetAsync($"/api/ingressos/meus/{cpf}");
            
            if (response.IsSuccessStatusCode)
            {
                var resultado = await response.Content.ReadFromJsonAsync<ListarIngressosResponse>();
                return (true, resultado?.Mensagem ?? "", resultado?.Ingressos);
            }
            else
            {
                var erro = await response.Content.ReadFromJsonAsync<ErroResponse>();
                return (false, erro?.Mensagem ?? "Erro ao listar ingressos.", null);
            }
        }
        catch
        {
            return (false, "Erro ao conectar com a API. Verifique se o backend está rodando.", null);
        }
    }

    private class ComprarIngressoResponse
    {
        public string Mensagem { get; set; } = "";
        public int ReservaId { get; set; }
    }

    private class ErroResponse
    {
        public string Mensagem { get; set; } = "";
    }
}
