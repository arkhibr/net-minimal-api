using System.Net.Http.Json;

namespace ProdutosAPI.Catalogo.ClientDemo;

public class CatalogoHttpClient(HttpClient http)
{
    public async Task<string?> GetProdutosAsync(CancellationToken ct = default)
    {
        var response = await http.GetAsync("/api/v1/catalogo/produtos", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(ct);
    }

    public async Task<HttpResponseMessage> CreateProdutoAsync(object payload, CancellationToken ct = default)
    {
        return await http.PostAsJsonAsync("/api/v1/catalogo/produtos", payload, ct);
    }
}
