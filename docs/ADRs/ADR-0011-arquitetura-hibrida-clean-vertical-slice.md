---
status: accepted
date: 2026-04-17
deciders: [Marco Mendes]
---

# ADR-0011 — Arquitetura Híbrida: Clean Architecture nas Camadas + Vertical Slice na API

## Contexto e Problema

O bounded context Catálogo precisava ser reestruturado para suportar múltiplos recursos (Produto, Categoria, Variante, Atributo, Mídia) com regras de domínio ricas. A arquitetura anterior (flat em Produtos) não escalava bem para esse nível de complexidade. Precisávamos de uma estrutura que equilibrasse separação de responsabilidades com praticidade de implementação.

## Opções Consideradas

- **Opção A — Clean Architecture pura** — 4 camadas (Domain, Application, Infrastructure, API), com interfaces e abstrações em todas as camadas. Alta separação mas overhead de implementação para endpoints simples.
- **Opção B — Vertical Slice pura** — cada feature como um arquivo autocontido. Máxima coesão por feature, mas duplicação de código de domínio entre features do mesmo bounded context.
- **Opção C — Híbrido: CA nas camadas + VSA na API** — Domain/Application/Infrastructure em Clean Architecture; camada de API com um arquivo por endpoint (Vertical Slice). Combina reutilização de domínio com isolamento de endpoint.

## Decisão

Escolhemos **Opção C — Híbrido** porque permite reutilizar entidades de domínio e serviços entre endpoints enquanto mantém cada endpoint isolado para mudanças independentes.

## Consequências

**Positivas:**
- Domain e Application são reutilizados por todos os endpoints sem duplicação.
- Cada endpoint pode evoluir independentemente (adicionar validações, políticas de rate limiting, autorização diferenciada).
- Estrutura familiar para quem conhece Clean Architecture; fácil onboarding.
- Permite demonstrar CQRS com EF Core (writes) e Dapper (reads) de forma natural.

**Negativas / Tradeoffs:**
- Mais arquivos que a abordagem flat original.
- Desenvolvedores precisam entender dois padrões (CA e VSA) para contribuir.
- Risco de misturar responsabilidades se a disciplina de camadas não for mantida.
