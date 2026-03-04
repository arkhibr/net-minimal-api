namespace Pix.ClientDemo.Client;

public sealed class PixClientOptions
{
    public const string SectionName = "PixClient";

    public string BaseUrl { get; set; } = "http://localhost:5099";
    public string ClientId { get; set; } = "pix-demo-client";
    public string ClientSecret { get; set; } = "pix-demo-secret";
    public string Scope { get; set; } = "pix.cobranca pix.devolucao";
    public string MtlsHeaderValue { get; set; } = "demo-certificado-cliente";
}
