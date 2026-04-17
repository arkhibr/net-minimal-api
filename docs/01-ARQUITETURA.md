# Arquitetura do Projeto

## Visão Estrutural

O projeto hospeda três bounded contexts no mesmo repositório. Cada um segue um padrão arquitetural independente — eles não se comunicam diretamente entre si. O que compartilham é restrito a `src/Shared/`: o `AppDbContext`, a interface `IEndpoint`, o padrão `Result<T>` e o pipeline de middleware.

Essa coexistência é intencional: permite comparar abordagens diferentes aplicadas ao mesmo stack tecnológico, sem a complexidade de múltiplos repositórios ou serviços distribuídos.

---

## Catálogo — Clean Architecture Híbrida

O Catálogo usa Clean Architecture nas camadas internas e Vertical Slice na camada de API (um arquivo por grupo de recursos, em vez de controllers monolíticos).

### Sub-projetos

| Sub-projeto | Responsabilidade |
|---|---|
| `Catalogo.Domain` | Entidades, value objects, interfaces de repositório |
| `Catalogo.Application` | Serviços, DTOs, validadores FluentValidation |
| `Catalogo.Infrastructure` | Repositórios EF Core, DbSeeder, configurações de mapeamento |
| `Catalogo.API` | Endpoints Minimal API, políticas de rate limiting |

### Domain

Contém 5 entidades: **Produto**, **Categoria**, **Variante**, **Atributo** e **Mídia**.

- **Produto**, **Categoria** e **Variante** têm domínio rico — lógica de negócio encapsulada nos próprios agregados (ex.: `Produto.Desativar()`, `Categoria` com slug gerado e hierarquia pai/filho, `Variante` com value object `SKU`).
- **Atributo** e **Mídia** são CRUD simples — entidades anêmicas com persistência direta via repositório.

### Application

Serviços de aplicação orquestram casos de uso: recebem DTOs, invocam o domínio e delegam persistência ao repositório. Validadores FluentValidation ficam nesta camada, junto com os DTOs de entrada e saída.

### Infrastructure

Repositórios concretos implementam as interfaces definidas no Domain. O `DbSeeder` popula o banco com dados iniciais (IDs 1–5 reservados para categorias, IDs 1–8 para produtos).

### API

Cada grupo de recursos tem seu próprio arquivo de endpoints (`ProdutoEndpoints.cs`, `CategoriaEndpoints.cs`, etc.). Rate limiting é aplicado por política de rota — três políticas registradas: `leitura`, `escrita` e `criacao-produto`.

### Fluxo de dados

```
HTTP → Catalogo.API/Endpoints → Catalogo.Application/Services → Catalogo.Infrastructure/Repositories → AppDbContext
                              ↓
                     Catalogo.Domain (entities, value objects)
```

---

## Pedidos — Vertical Slice + Domínio Rico

O bounded context de Pedidos organiza o código por caso de uso, não por camada técnica. Cada operação é uma pasta autocontida dentro de `src/Pedidos/Features/`.

### Estrutura de uma feature

```
Features/
  CreatePedido/
    CreatePedidoCommand.cs     ← DTO de entrada
    CreatePedidoValidator.cs   ← FluentValidation
    CreatePedidoHandler.cs     ← orquestração do caso de uso
    CreatePedidoEndpoint.cs    ← implementa IEndpoint, auto-descoberto
```

Cada pasta contém tudo o que aquela operação precisa — nenhuma dependência cruzada entre features.

### Domínio Rico

O aggregate `Pedido` encapsula as regras de negócio. Métodos como `Pedido.Create()`, `Pedido.AddItem()` e `Pedido.Cancel()` nunca lançam exceções — retornam `Result` ou `Result<T>`. O handler sempre verifica `IsSuccess` antes de acessar `.Value`.

### Auto-descoberta de endpoints

Todos os endpoints implementam a interface `IEndpoint` (`src/Shared/Common/IEndpoint.cs`). O `Program.cs` usa reflection para descobrir e registrar automaticamente todas as implementações — nenhum endpoint precisa ser cadastrado manualmente.

### Fluxo de dados

```
HTTP → CreatePedidoEndpoint (IEndpoint, auto-discovered) → CreatePedidoValidator → CreatePedidoHandler → Pedido.Create() → AppDbContext
```

---

## Pix — Integração Externa

### Pix.MockServer

Minimal API autocontida que simula a API Pix do Banco Central do Brasil. Implementa OAuth2 para emissão de tokens e mTLS para autenticação mútua. Persiste dados em memória (sem banco de dados). Seu propósito é permitir desenvolvimento e testes da integração sem dependência de ambiente externo.

Localização: `src/Pix/Pix.MockServer/`

### Pix.ClientDemo

Console app que consome o `Pix.MockServer` via `HttpClient` tipado. Demonstra:
- Configuração de mTLS com certificado de cliente
- Fluxo OAuth2 (client credentials)
- Pipeline de resiliência com `AddStandardResilienceHandler` (retry + circuit breaker via `Microsoft.Extensions.Http.Resilience`)

Localização: `src/Pix/Pix.ClientDemo/`

### Catalogo.ClientDemo

Console app que demonstra retry e circuit breaker consumindo a API do Catálogo. Usa `Microsoft.Extensions.Http.Resilience` diretamente, sem mTLS, como exemplo mais acessível do padrão de resiliência para APIs internas.

---

## Middleware e Shared

Componentes em `src/Shared/` utilizados por todos os bounded contexts:

| Componente | Descrição |
|---|---|
| `IdempotencyMiddleware` | Intercepta POST/PUT/PATCH com header `Idempotency-Key`. Evita reprocessamento de requisições duplicadas. |
| Global exception handling | Captura exceções não tratadas e retorna respostas padronizadas (RFC 7807 Problem Details). |
| JWT Bearer auth | Configurado globalmente. Pedidos exigem `RequireAuthorization()`. GET do Catálogo é anônimo; escrita exige token. |
| Rate limiting | 3 políticas: `leitura`, `escrita`, `criacao-produto`. Registradas no `Program.cs`. Desativadas quando `Environment = "Testing"`. |
| `AppDbContext` | Único contexto EF Core, compartilhado pelos três bounded contexts. |
| `IEndpoint` | Interface com método `Map(IEndpointRouteBuilder)`. Implementações são descobertas por reflection. |
| `Result<T>` | Tipo discriminado que representa sucesso ou falha sem lançar exceções. Usado exclusivamente no bounded context de Pedidos. |

---

## Decisões Arquiteturais

As decisões arquiteturais estão registradas em 15 ADRs no formato MADR 3.x em `docs/ADRs/`. ADR-0011 a ADR-0015 documentam as decisões das Fases 1 a 3 (migração do Catálogo para Clean Architecture, novos recursos, rate limiting e estrutura de documentação).
