---
status: accepted
date: 2024-04-10
deciders: [Marco Mendes]
---

# PIX Mock Server Customizado com mTLS

## Contexto e Declaração do Problema

O projeto precisa demonstrar integração com a API PIX do Banco Central, que exige mutual TLS (mTLS) — autenticação bidirecional onde tanto o servidor quanto o cliente apresentam certificados X.509. Como simular esse ambiente para fins educacionais sem depender de credenciais reais do Banco Central ou de infraestrutura de homologação?

## Opções Consideradas

* WireMock.Net como servidor de mock
* Mockoon ou similar (ferramenta externa)
* Servidor Minimal API customizado com Kestrel configurado para mTLS

## Decisão

Opção escolhida: "Servidor Minimal API customizado com Kestrel configurado para mTLS", porque o requisito central é demonstrar o handshake mTLS completo — validação do certificado do cliente pelo servidor e do servidor pelo cliente. WireMock.Net e ferramentas externas não oferecem controle nativo sobre a validação de certificado cliente no TLS handshake, o que tornaria a demonstração incompleta ou exigiria workarounds que obscureceriam o padrão.

O mock server (`Pix.MockServer`) é uma Minimal API separada com:
- Kestrel configurado com `ClientCertificateMode.RequireCertificate`
- Validação explícita do certificado cliente no pipeline
- Endpoint OAuth2 (`/oauth/token`) para emissão de access tokens
- Endpoints de cobrança com payloads complexos (nested objects, arrays, metadata)
- Idempotência por fingerprint de payload
- Repositórios in-memory thread-safe

O cliente (`Pix.ClientDemo`) usa `IHttpClientFactory` com certificado cliente configurado e resilience handlers.

### Consequências

* Positivo, porque demonstra o handshake mTLS completo — o artefato educacional mais importante do módulo PIX.
* Positivo, porque o mock server é um artefato de aprendizado adicional sobre como configurar Kestrel com TLS avançado.
* Negativo, porque a manutenção de um servidor Minimal API completo como mock é mais custosa do que usar uma ferramenta de mock dedicada.
* Negativo, porque os certificados de desenvolvimento precisam ser gerados e rotacionados manualmente, o que pode criar atrito no onboarding de novos desenvolvedores.

---

_Formato baseado no template [MADR — Markdown Architectural Decision Records](https://github.com/adr/madr/blob/develop/template/adr-template-minimal.md)._
