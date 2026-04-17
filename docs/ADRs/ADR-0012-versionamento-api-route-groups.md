---
status: accepted
date: 2026-04-17
deciders: [Marco Mendes]
---

# ADR-0012 — Versionamento de API via Route Groups Manuais

## Contexto e Problema

A API precisava de versionamento explícito para o bounded context Catálogo (rota base `/api/v1/catalogo/{recurso}`). O princípio adotado é que **a versão pertence à API, não ao recurso individual** — todos os recursos do Catálogo compartilham a mesma versão.

## Opções Consideradas

- **Opção A — Asp.Versioning.Http** — biblioteca oficial de versionamento para ASP.NET Core. Suporta múltiplos esquemas (URL, header, query string). Overhead de configuração e dependência externa.
- **Opção B — Route Groups manuais** — `app.MapGroup("/api/v1").MapGroup("/catalogo")` passado como `RouteGroupBuilder` para cada conjunto de endpoints. Zero dependências externas, explícito e suficiente para um único namespace de versão.
- **Opção C — Prefixo hardcoded em cada endpoint** — cada endpoint define seu próprio prefixo `/api/v1/catalogo/...`. Simples mas viola DRY e torna mudança de versão trabalhosa.

## Decisão

Escolhemos **Opção B — Route Groups manuais** porque a API tem um único namespace de versão por enquanto, e a biblioteca de versionamento adicionaria complexidade sem benefício real neste estágio.

## Consequências

**Positivas:**
- Zero dependências externas.
- Mudança de versão requer alterar apenas o `Program.cs` (uma linha).
- Signature dos endpoints (`this RouteGroupBuilder`) força o contrato correto.
- Demonstra que versionamento é responsabilidade da API, não do recurso.

**Negativas / Tradeoffs:**
- Não suporta múltiplas versões simultâneas sem refatoração.
- Sem geração automática de documentação por versão no Swagger.
- Se a API crescer para v2 com quebras de contrato, migrar para `Asp.Versioning.Http` será necessário.
