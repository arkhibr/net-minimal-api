---
status: accepted
date: 2026-04-17
deciders: [Marco Mendes]
---

# ADR-0014 — Resiliência de Cliente HTTP com Microsoft.Extensions.Http.Resilience

## Contexto e Problema

O projeto `Catalogo.ClientDemo` demonstra como consumidores da API devem reagir a erros transientes e respostas 429. Sem uma estratégia de resiliência, o cliente falha imediatamente em qualquer erro de rede ou throttling, criando uma experiência ruim e potencialmente gerando mais carga no servidor (reconexões imediatas).

## Opções Consideradas

- **Opção A — Retry manual com `Thread.Sleep`** — simples mas bloqueia threads, não respeita `Retry-After`, não tem circuit breaker.
- **Opção B — Polly diretamente** — biblioteca madura, mas API de baixo nível requer mais código boilerplate para integrar com `HttpClient`.
- **Opção C — Microsoft.Extensions.Http.Resilience** — wrapper oficial sobre Polly v8 para `HttpClient`. Integra com DI, suporta telemetria, API fluente.

## Decisão

Escolhemos **Opção C — Microsoft.Extensions.Http.Resilience** porque é a abordagem recomendada pela Microsoft para `HttpClient` resiliente e tem integração nativa com `IHttpClientFactory`.

**Pipeline de resiliência:**
1. Timeout por tentativa (5s)
2. Retry com exponential backoff + jitter (3 tentativas, handle 429 e 503)
3. Circuit breaker (abre após 50% falhas em 30s com mínimo 5 requisições, fecha após 15s)
4. Timeout global (30s, incluindo todos os retries)

## Consequências

**Positivas:**
- Respeita `Retry-After` do servidor automaticamente ao lidar com 429.
- Jitter evita thundering herd quando múltiplos clientes retentam simultaneamente.
- Circuit breaker protege o servidor durante incidentes.
- Telemetria automática via OpenTelemetry se configurado.

**Negativas / Tradeoffs:**
- `Microsoft.Extensions.Http.Resilience` é NuGet externo (não incluso no SDK).
- Depende de Polly v8 transitivamente — adiciona ~500KB ao binário.
- Circuit breaker por instância de `HttpClient` (não distribuído).
