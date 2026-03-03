# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Comandos Essenciais

```bash
# Build
dotnet build ProdutosAPI.slnx

# Executar aplicação (Swagger em http://localhost:5000)
dotnet run --project ProdutosAPI.csproj

# Todos os testes
dotnet test ProdutosAPI.slnx

# Testes de um projeto específico
dotnet test ProdutosAPI.Tests/ProdutosAPI.Tests.csproj
dotnet test Pedidos.Tests/Pedidos.Tests.csproj

# Teste único por nome (parcial match)
dotnet test --filter "FullyQualifiedName~NomeDoTeste"

# Testes de integração de Produtos (23 testes HTTP reais)
dotnet test ProdutosAPI.Tests/ProdutosAPI.Tests.csproj --filter "FullyQualifiedName~ProdutoEndpointsTests"

# Migrations (rodar da raiz do projeto)
dotnet ef migrations add NomeDaMigration --project ProdutosAPI.csproj
dotnet ef database update --project ProdutosAPI.csproj
```

## Arquitetura

Este projeto é **educacional** e demonstra **dois padrões arquiteturais coexistindo**:

### Feature Produtos — Arquitetura em Camadas (clean-ish)
```
src/Produtos/
├── Models/Produto.cs          # Entidade rica com private setters, factory method Criar()
├── DTOs/ProdutoDTO.cs         # Request/Response objects (CriarProdutoRequest, ProdutoResponse, etc.)
├── Services/ProdutoService.cs # Lógica de negócio; usa AppDbContext + AutoMapper
├── Validators/                # FluentValidation (CriarProdutoValidator, AtualizarProdutoValidator)
└── Endpoints/ProdutoEndpoints.cs  # Minimal API routes via MapGroup
```

Existe um plano de migração em `docs/plans/2026-03-02-produtos-clean-architecture.md` para reorganizar Produtos em 4 sub-projetos (`Produtos.Domain`, `Produtos.Application`, `Produtos.Infrastructure`, `Produtos.API`) abaixo de `src/Produtos/`. **Ainda não executado** — o código atual ainda é a estrutura flat original.

### Feature Pedidos — Vertical Slice + Domínio Rico
```
src/Pedidos/
├── Domain/Pedido.cs           # Agregado rico (AdicionarItem, Confirmar, Cancelar retornam Result)
├── CreatePedido/              # Uma slice completa por use case
│   ├── CreatePedidoCommand.cs
│   ├── CreatePedidoValidator.cs
│   ├── CreatePedidoEndpoint.cs   # Implementa IEndpoint
│   └── CreatePedidoHandler.cs
├── GetPedido/, ListPedidos/, CancelPedido/, AddItemPedido/  # Mesma estrutura
└── Common/PedidoResponse.cs
```

Pedidos usa **scan automático de endpoints**: qualquer classe que implemente `IEndpoint` (em `src/Shared/Common/IEndpoint.cs`) é registrada automaticamente via `services.AddEndpointsFromAssembly()` + `app.MapRegisteredEndpoints()`.

### Infraestrutura Compartilhada
```
src/Shared/
├── Common/
│   ├── Result.cs              # Result pattern: Result(IsSuccess, Error) e Result<T>(IsSuccess, Value, Error)
│   ├── IEndpoint.cs           # Interface para slices de Pedidos
│   └── MappingProfile.cs      # AutoMapper: Produto → ProdutoResponse
├── Data/
│   ├── AppDbContext.cs        # EF Core: DbSet<Produto>, DbSet<Pedido>, DbSet<PedidoItem>
│   └── DbSeeder.cs            # Popula 8 produtos de exemplo no início
└── Middleware/
    ├── ExceptionHandlingMiddleware.cs   # Captura ValidationException → 422, KeyNotFoundException → 404
    └── IdempotencyMiddleware.cs         # Header Idempotency-Key para POST/PUT/PATCH
```

## Projetos de Teste

| Projeto | O que testa | Padrão |
|---|---|---|
| `ProdutosAPI.Tests/` | Feature Produtos completo | `IClassFixture<ApiFactory>` |
| `Pedidos.Tests/` | Feature Pedidos completo | `IClassFixture<PedidosApiFactory>` |

**Testes de integração** usam `WebApplicationFactory<Program>` com banco **InMemory** (ativado pelo `Environment = "Testing"`). O `DbSeeder` é chamado no `CreateHost()` da factory e popula 8 produtos (IDs 1–8).

Cada teste recebe um `HttpClient` novo via `factory.CreateClient()` no construtor — sem estado compartilhado entre testes dentro da mesma classe.

**Autenticação nos testes:**
```csharp
var token = await AuthHelper.ObterTokenAsync(client);
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
// Credenciais hardcoded: Email="admin@example.com", Senha="senha123"
```

## Convenções Importantes

**Categorias válidas de Produto** (validadas por FluentValidation):
`"Eletrônicos"`, `"Livros"`, `"Roupas"`, `"Alimentos"`, `"Outros"`

**Soft delete:** `DELETE /api/v1/produtos/{id}` marca `Ativo = false`. Produto inativo retorna 404 em todos os endpoints.

**Result pattern:** Entidades do domínio não lançam exceções — retornam `Result` ou `Result<T>`. Verificar `IsSuccess` antes de usar `.Value`. Pedido usa `ProdutosAPI.Shared.Common.Result`; Produto usa o mesmo tipo.

**Endpoints de Produtos** retornam `null` do serviço (não lançam `KeyNotFoundException`) — o endpoint verifica `if (produto is null)` e retorna `Results.NotFound(...)`.

**Pedidos exigem autenticação** (`RequireAuthorization()`). Produtos: GET é anônimo, escrita exige JWT.

**Migrations:** Ficam em `src/Shared/Data/Migrations/`. Em produção/dev, `AppDbContext.Database.Migrate()` é chamado no startup. Em `"Testing"`, usa InMemory + `EnsureCreated()`.

**Idempotência:** POST/PUT/PATCH aceitam o header `Idempotency-Key` para evitar processamento duplicado (cache em memória).

## Estrutura de Planos de Implementação

Planos de implementação em `docs/plans/` seguem o formato da skill `superpowers:writing-plans` e são executados com `superpowers:executing-plans`. Leia o plano antes de executar qualquer migração.
