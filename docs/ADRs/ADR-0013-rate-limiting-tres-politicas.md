---
status: accepted
date: 2026-04-17
deciders: [Marco Mendes]
---

# ADR-0013 — Rate Limiting no Servidor com Três Políticas Diferenciadas

## Contexto e Problema

A API Catálogo expõe endpoints públicos de leitura e endpoints protegidos de escrita. Sem rate limiting, a API fica vulnerável a abuso — tanto leitura excessiva (scraping, DDoS acidental) quanto criação em massa de recursos. Leituras e escritas têm comportamentos de tráfego diferentes e merecem políticas distintas.

## Opções Consideradas

- **Opção A — Uma política global** — um único limite aplica-se a todos os endpoints. Simples, mas impede ajuste fino por tipo de operação.
- **Opção B — Três políticas diferenciadas** — `leitura` (fixed window, 60/min), `escrita` (sliding window, 20/min), `criacao-produto` (token bucket, 5/min). Cada tipo de operação tem limite adequado ao seu impacto.
- **Opção C — Biblioteca externa (AspRateLimit)** — mais recursos (por IP, por usuário, por API key). Overhead de configuração desnecessário para este estágio.

## Decisão

Escolhemos **Opção B — três políticas** usando `AspNetCore.RateLimiting` (built-in .NET 7+) porque reflete a diferença real de custo e risco entre tipos de operação.

**Políticas:**
| Política | Algoritmo | Limite | Endpoints |
|---|---|---|---|
| `leitura` | Fixed Window | 60 req/min | GET * |
| `escrita` | Sliding Window | 20 req/min | PUT, DELETE, PATCH |
| `criacao-produto` | Token Bucket | 5 req/min | POST /produtos |

## Consequências

**Positivas:**
- Sem dependências externas — `AspNetCore.RateLimiting` incluso no SDK.
- Resposta 429 com header `Retry-After` padronizado.
- Leitura pode ter limite 3× maior que escrita sem configuração complexa.
- Token bucket naturalmente lida com rajadas curtas em criação de produto.

**Negativas / Tradeoffs:**
- Limites são por processo (não distribuídos) — em múltiplas instâncias, cada instância tem seu próprio contador.
- Para produção com múltiplas réplicas, seria necessário rate limiting no API Gateway ou Redis-backed limiter.
