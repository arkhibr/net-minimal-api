---
status: accepted
date: 2026-04-17
deciders: [Marco Mendes]
---

# ADR-0015 — Domínio Rico Seletivo: Complexidade onde ela Importa

## Contexto e Problema

O bounded context Catálogo tem 5 recursos: Produto, Categoria, Variante, Atributo, Mídia. Aplicar DDD completo (aggregates, value objects, domain events, specifications) a todos os recursos seria over-engineering. Alguns recursos têm regras de negócio ricas e invariantes que valem ser modeladas no domínio; outros são essencialmente CRUD com pouca lógica.

## Opções Consideradas

- **Opção A — DDD completo para todos os recursos** — todos os recursos têm aggregates, value objects, domain events. Máxima consistência arquitetural, mas complexidade desnecessária para recursos simples.
- **Opção B — CRUD puro para todos** — sem domain model, lógica nos controllers/handlers. Simples mas perde proteção de invariantes em recursos complexos.
- **Opção C — Domínio rico seletivo** — Produto, Categoria e Variante recebem domain modeling completo; Atributo e Mídia são CRUD simples sem value objects.

## Decisão

Escolhemos **Opção C — Domínio rico seletivo** baseado na análise de invariantes de negócio:

| Recurso | Decisão | Justificativa |
|---|---|---|
| Produto | Rico | Preço > 0, estoque ≥ 0, categorias válidas, SKU único |
| Categoria | Rico | Hierarquia máx 2 níveis, slug auto-gerado, não pode desativar com produtos ativos |
| Variante | Rico | SKU obrigatório e único por produto, estoque próprio |
| Atributo | CRUD | Apenas nome+valor, sem invariantes de negócio |
| Mídia | CRUD | URL + tipo de mídia, sem regras de domínio |

## Consequências

**Positivas:**
- Invariantes de negócio críticas são protegidas pelo domínio (não podem ser bypassadas).
- Recursos simples não têm overhead de value objects desnecessários.
- Demonstra o princípio de "aplicar DDD onde a complexidade justifica".

**Negativas / Tradeoffs:**
- Inconsistência aparente entre recursos (alguns ricos, outros CRUD) pode confundir.
- Pressão para "normalizar" pode levar futuramente a enriquecer Atributo/Mídia sem necessidade real.
- Regras como "hierarquia máx 2 níveis" ficam no service (necessitam DB) e não no domínio puro — limite da abordagem.
