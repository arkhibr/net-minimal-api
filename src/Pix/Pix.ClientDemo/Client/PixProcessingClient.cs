using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Pix.ClientDemo.Models;

namespace Pix.ClientDemo.Client;

public sealed class PixProcessingClient(
    HttpClient httpClient,
    IAuthTokenProvider tokenProvider,
    IOptions<PixClientOptions> options)
{
    public async Task<CobrancaResponse> CriarCobrancaAsync(CriarCobrancaRequest request, CancellationToken cancellationToken = default)
    {
        await PrepareSecureHeadersAsync(cancellationToken);
        var response = await httpClient.PostAsJsonAsync("/pix/v1/cobrancas", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CobrancaResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Resposta de cobrança inválida.");
    }

    public async Task<CobrancaResponse> ObterCobrancaAsync(string txid, CancellationToken cancellationToken = default)
    {
        await PrepareSecureHeadersAsync(cancellationToken);
        var response = await httpClient.GetAsync($"/pix/v1/cobrancas/{txid}", cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CobrancaResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Resposta de consulta de cobrança inválida.");
    }

    public async Task<CobrancaResponse> SimularLiquidacaoAsync(string txid, CancellationToken cancellationToken = default)
    {
        await PrepareSecureHeadersAsync(cancellationToken);
        var response = await httpClient.PostAsync($"/pix/v1/cobrancas/{txid}/simular-liquidacao", content: null, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CobrancaResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Resposta de liquidação inválida.");
    }

    public async Task<DevolucaoResponse> CriarDevolucaoAsync(CriarDevolucaoRequest request, CancellationToken cancellationToken = default)
    {
        await PrepareSecureHeadersAsync(cancellationToken);
        var response = await httpClient.PostAsJsonAsync("/pix/v1/devolucoes", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<DevolucaoResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Resposta de devolução inválida.");
    }

    public async Task<DevolucaoResponse> ObterDevolucaoAsync(string devolucaoId, CancellationToken cancellationToken = default)
    {
        await PrepareSecureHeadersAsync(cancellationToken);
        var response = await httpClient.GetAsync($"/pix/v1/devolucoes/{devolucaoId}", cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<DevolucaoResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Resposta de consulta de devolução inválida.");
    }

    private async Task PrepareSecureHeadersAsync(CancellationToken cancellationToken)
    {
        var token = await tokenProvider.GetTokenAsync(cancellationToken);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (httpClient.DefaultRequestHeaders.Contains("X-MTLS-Client-Cert"))
        {
            httpClient.DefaultRequestHeaders.Remove("X-MTLS-Client-Cert");
        }

        httpClient.DefaultRequestHeaders.Add("X-MTLS-Client-Cert", options.Value.MtlsHeaderValue);
    }
}
