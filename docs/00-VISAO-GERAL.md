# Visão Geral do Projeto

## Sobre o Projeto

Este é um projeto educacional em **.NET 10 Minimal API** que demonstra três bounded contexts coexistindo no mesmo repositório, cada um seguindo um padrão arquitetural distinto. O objetivo é permitir comparação direta entre abordagens — o mesmo problema (uma API REST com persistência, validação e testes) resolvido com diferentes graus de estrutura e separação de responsabilidades.

## Os Três Bounded Contexts

### Catálogo

| | |
|---|---|
| **Padrão** | Clean Architecture híbrida (CA nas camadas, VSA na API) |
| **Diretório** | `src/Catalogo/` |
| **Rotas** | `/api/v1/catalogo/*` |

Demonstra separação em sub-projetos (Domain, Application, Infrastructure, API), entidades com domínio rico, value objects, repositórios abstraídos por interfaces e rate limiting por política de rota. Contém 5 recursos: Produto, Categoria, Variante, Atributo e Mídia.

Caminhos relevantes:
- `src/Catalogo/Catalogo.Domain/` — entidades e value objects
- `src/Catalogo/Catalogo.Application/` — serviços, DTOs e validadores
- `src/Catalogo/Catalogo.Infrastructure/` — repositórios EF Core e DbSeeder
- `src/Catalogo/Catalogo.API/Endpoints/` — um arquivo por grupo de recursos

---

### Pedidos

| | |
|---|---|
| **Padrão** | Vertical Slice Architecture + Domínio Rico |
| **Diretório** | `src/Pedidos/` |
| **Rotas** | `/api/v1/pedidos/*` |

Demonstra organização por caso de uso (cada operação é uma pasta isolada), aggregate com regras de negócio encapsuladas, o padrão `Result<T>` em vez de exceções, e auto-descoberta de endpoints via reflection. Autenticação JWT obrigatória.

Caminhos relevantes:
- `src/Pedidos/Features/` — pastas por caso de uso (CreatePedido, GetPedido, etc.)
- `src/Pedidos/Domain/` — aggregate Pedido
- `src/Shared/Common/IEndpoint.cs` — contrato de auto-registro

---

### Pix

| | |
|---|---|
| **Padrão** | Mock Server + HTTP Client com resiliência |
| **Diretório** | `src/Pix/` |
| **Rotas** | — (integração externa) |

Demonstra como integrar com APIs externas usando mTLS, OAuth2 e pipelines de resiliência (Polly via `Microsoft.Extensions.Http.Resilience`). Inclui um servidor mock que simula a API Pix do BCB e um console app que o consome.

Caminhos relevantes:
- `src/Pix/Pix.MockServer/` — Minimal API simulando BCB Pix
- `src/Pix/Pix.ClientDemo/` — console app com HttpClient tipado e resiliência

---

## Caminhos de Aprendizado

### Iniciante

Foco em entender a estrutura básica do projeto e as boas práticas de API REST.

```
README.md
  → docs/01-ARQUITETURA.md
  → docs/02-CATALOGO.md
  → docs/guias/MELHORES-PRATICAS-API.md
```

### Intermediário

Adiciona o estudo de padrões arquiteturais mais sofisticados e domínio rico.

```
(Iniciante, mais:)
  → docs/03-PEDIDOS.md        (Vertical Slice Architecture na prática)
  → Leitura: VSA vs. Layered  (teoria de organização por caso de uso)
  → src/Shared/Common/        (Result pattern, IEndpoint)
```

### Arquiteto

Estudo completo incluindo integração externa, estratégia de testes e decisões arquiteturais registradas.

```
(Intermediário, mais:)
  → docs/04-PIX.md            (mTLS, OAuth2, resiliência)
  → docs/05-TESTES.md         (estratégia e execução)
  → docs/ADRs/                (15 ADRs no formato MADR 3.x)
  → src/Pix/Pix.ClientDemo/   (resiliência com Polly/Http.Resilience)
```

---

## Mapa da Documentação

| Arquivo | Objetivo | Público |
|---|---|---|
| `README.md` | Visão geral e início rápido | Todos |
| `docs/01-ARQUITETURA.md` | Padrões e fluxo de dados | Intermediário / Arquiteto |
| `docs/02-CATALOGO.md` | Deep-dive do Catálogo | Intermediário |
| `docs/03-PEDIDOS.md` | Vertical Slice e domínio rico | Intermediário / Arquiteto |
| `docs/04-PIX.md` | Integração externa e resiliência | Intermediário |
| `docs/05-TESTES.md` | Estratégia e execução de testes | Todos |
| `docs/guias/` | Guias conceituais de REST e .NET | Iniciante / Intermediário |
| `docs/ADRs/` | Decisões arquiteturais registradas | Arquiteto |

---

## Tecnologias

| Tecnologia | Versão | Papel |
|---|---|---|
| .NET | 10 LTS | Runtime e SDK |
| EF Core | 10 | ORM e migrações |
| SQLite | — | Banco de dados (dev/testes) |
| FluentValidation | 11 | Validação de entrada |
| Polly / Http.Resilience | v8 | Retry, circuit breaker |
| JWT Bearer | — | Autenticação em Pedidos |
| xUnit | — | Framework de testes |
