# Design: Vertical Slice Architecture + Domínio Rico (Pedidos)

**Data:** 2026-02-27
**Status:** Aprovado

---

## Contexto

O projeto ProdutosAPI é um projeto educacional em .NET 10 Minimal API com arquitetura horizontal em camadas (Endpoints → Services → Data). O modelo de domínio atual é anêmico: a classe `Produto` é um contêiner de dados sem comportamento.

Este design introduz:
1. **Vertical Slice Architecture** para o novo caso de uso de Pedidos
2. **Modelo de domínio rico** em Pedidos e na refatoração de Produtos
3. Coexistência dos dois padrões no mesmo projeto, demonstrando o contraste

---

## Decisões de Design

| Decisão | Escolha | Motivo |
|---|---|---|
| Escopo de Pedidos | Pedido + PedidoItem | Agregado clássico, didático |
| Organização dos slices | CRUD como slices | Acessível, sem overhead de casos de uso complexos |
| Handler | Inline no Command, sem MediatR | Sem dependências novas, mais simples |
| Persistência | Mesmo AppDbContext | Evita complexidade de múltiplos contextos |
| Produtos | Refatorado com domínio rico | Demonstra que domínio rico independe do padrão arquitetural |

---

## Estrutura de Pastas

```
src/
├── Endpoints/              ← Produtos (inalterado — horizontal layers)
│   ├── ProdutoEndpoints.cs
│   └── AuthEndpoints.cs
├── Services/               ← Produtos (inalterado)
├── Models/                 ← Produto (refatorado com domínio rico)
├── DTOs/                   ← Produtos
├── Validators/             ← Produtos
│
└── Features/               ← NOVO — Vertical Slice
    └── Pedidos/
        ├── Domain/
        │   ├── Pedido.cs
        │   ├── PedidoItem.cs
        │   ├── StatusPedido.cs
        │   └── PedidoErrors.cs
        ├── Common/
        │   ├── IEndpoint.cs
        │   ├── Result.cs
        │   └── PedidoResponse.cs
        ├── CreatePedido/
        │   ├── CreatePedidoCommand.cs
        │   ├── CreatePedidoValidator.cs
        │   └── CreatePedidoEndpoint.cs
        ├── GetPedido/
        │   ├── GetPedidoQuery.cs
        │   └── GetPedidoEndpoint.cs
        ├── ListPedidos/
        │   ├── ListPedidosQuery.cs
        │   └── ListPedidosEndpoint.cs
        ├── AddItemPedido/
        │   ├── AddItemCommand.cs
        │   ├── AddItemValidator.cs
        │   └── AddItemEndpoint.cs
        └── CancelPedido/
            ├── CancelPedidoCommand.cs
            └── CancelPedidoEndpoint.cs
```

---

## Modelo de Domínio Rico

### Pedido (Aggregate Root)

Regras de negócio encapsuladas:

- Itens só podem ser adicionados a pedidos em status `Rascunho`
- Quantidade por item: mínimo 1, máximo 999
- Mesmo produto adicionado duas vezes faz merge de quantidade
- Limite de 20 itens distintos por pedido
- Produto precisa estar ativo e com estoque suficiente
- Confirmar requer ao menos 1 item e valor mínimo de R$ 10,00
- Pedido `Confirmado` ou `Cancelado` não pode ser confirmado novamente
- Cancelamento exige motivo obrigatório
- Pedido já cancelado não pode ser cancelado novamente

### PedidoItem (Entity filha)

- Preço unitário e nome do produto são **snapshots** do momento do pedido
- `Subtotal` é calculado (`PrecoUnitario * Quantidade`), nunca persistido
- Construtores e mutações com acesso `internal` — protegidos fora do agregado

### Produto (refatorado)

Métodos de domínio substituem setters públicos:

- `Criar(...)` — factory method com validação (retorna `Result<Produto>`)
- `AtualizarPreco(decimal)` — valida que é > 0 e diferente do atual
- `ReporEstoque(int)` — valida positivo e não excede 99.999 unidades
- `Desativar()` — valida que não está já inativo
- `TemEstoqueDisponivel(int)` — guard usado pelo agregado Pedido

### Result Pattern

```csharp
public record Result(bool IsSuccess, string? Error = null)
public record Result<T>(bool IsSuccess, T? Value, string? Error = null)
```

Erros de domínio retornam via `Result` — sem exceptions para fluxo de negócio.

---

## Arquitetura dos Slices

### Interface IEndpoint — registro automático

```csharp
public interface IEndpoint
{
    void MapEndpoints(IEndpointRouteBuilder app);
}
```

`Program.cs` faz scan de `IEndpoint` via `AddEndpointsFromAssembly` — nenhum slice é registrado manualmente.

### Anatomia de um slice

Cada slice tem três responsabilidades separadas:

1. **Command/Query** — DTO de entrada + Handler com lógica de aplicação
2. **Validator** — FluentValidation do DTO (quando necessário)
3. **Endpoint** — apenas roteamento HTTP, sem lógica

### Coexistência dos padrões

```
Produtos (Horizontal Layers)          Pedidos (Vertical Slice)
─────────────────────────────         ──────────────────────────
ProdutoEndpoints.cs                   Features/Pedidos/CreatePedido/
  └─ chama IProdutoService              └─ CreatePedidoEndpoint.cs
       └─ usa AppDbContext               └─ CreatePedidoHandler
                                         └─ AppDbContext (mesmo)
```

Ambos usam o mesmo `AppDbContext`, mesma pipeline JWT, mesmo middleware.

---

## Recursos .NET 10

| Feature | Onde aparece |
|---|---|
| `TypedResults` | Todos os endpoints dos slices |
| `MapGroup` com metadados herdados | Grupo `/api/v1/pedidos` centraliza auth + tag |
| Primary constructors | Handlers dos slices |
| `IEndpoint` scan automático | `AddEndpointsFromAssembly` |
| `Results.ValidationProblem` (RFC 7807) | FluentValidation nos slices |
| Collection expressions `[]` | `_itens = []` no aggregate |

---

## Estratégia de Testes

### Testes de Domínio (Unit) — sem infraestrutura

- `PedidoTests.cs` — todas as regras do aggregate: merge de itens, limite de 20, valor mínimo, cancelamento, etc.
- `PedidoItemTests.cs` — snapshot de preço, incremento de quantidade
- `ProdutoTests.cs` — regras do domínio rico refatorado

### Testes de Integração (HTTP)

- `CreatePedidoTests.cs`, `GetPedidoTests.cs`, `AddItemTests.cs`, `CancelPedidoTests.cs`
- Via `WebApplicationFactory<Program>`, ponta a ponta com SQLite in-memory

### ProdutoBuilder (Test Helper)

Builder fluente para criação de `Produto` em testes de domínio, sem boilerplate.

---

## Endpoints resultantes

| Método | Rota | Slice | Auth |
|---|---|---|---|
| `POST` | `/api/v1/pedidos` | CreatePedido | Sim |
| `GET` | `/api/v1/pedidos/{id}` | GetPedido | Sim |
| `GET` | `/api/v1/pedidos` | ListPedidos | Sim |
| `POST` | `/api/v1/pedidos/{id}/itens` | AddItemPedido | Sim |
| `POST` | `/api/v1/pedidos/{id}/cancelar` | CancelPedido | Sim |
