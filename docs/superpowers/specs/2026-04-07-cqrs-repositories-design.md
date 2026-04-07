# Design Spec — CQRS Repositories (Produtos + Pedidos)

**Data:** 2026-04-07  
**Status:** Aprovado  
**Branch de implementação:** `feature/cqrs-repositories` (worktree em `.worktrees/feature/cqrs-repositories`)

---

## Contexto

O projeto demonstra dois padrões arquiteturais coexistindo: Produtos (layered) e Pedidos (vertical slices). Atualmente no `main`:

- **Produtos** usa `DapperProdutoRepository` — interface única que mistura leitura e escrita, retorna entidades de domínio em todas as operações
- **Pedidos** usa `AppDbContext` diretamente nos handlers — sem abstração de repositório

O objetivo é aplicar **CQRS no nível de repositório** em ambas as features, separando explicitamente operações de leitura (Dapper → DTO) de operações de escrita (EF Core → entidade rastreada).

---

## Arquitetura

### Padrão adotado

Cada feature expõe **dois repositórios com responsabilidades distintas:**

| Repositório | Tecnologia | Retorna | Responsabilidade |
|-------------|------------|---------|-----------------|
| `IXxxQueryRepository` | Dapper + raw SQL | DTO (`XxxResponse`) | Leituras — sem instanciar entidades de domínio |
| `IXxxCommandRepository` | EF Core (tracking) | Entidade de domínio | Escritas — carrega, muta, persiste |

### Estrutura de arquivos (feature branch)

```
src/Produtos/
├── Produtos.Application/Repositories/
│   ├── IProdutoQueryRepository.cs      # ObterPorIdAsync, ListarAsync → ProdutoResponse
│   └── IProdutoCommandRepository.cs    # ObterPorIdAsync, AdicionarAsync, DeletarAsync, SaveChangesAsync
└── Produtos.Infrastructure/Repositories/
    ├── DapperProdutoQueryRepository.cs  # Dapper, mapeia para ProdutoResponse inline (ProdutoRow)
    └── EfProdutoCommandRepository.cs    # IProdutoContext (EF Core tracking)

src/Pedidos/
├── Repositories/
│   ├── IPedidoQueryRepository.cs       # ObterPorIdAsync, ListarAsync → PedidoResponse
│   └── IPedidoCommandRepository.cs     # ObterPorIdAsync, ObterProdutoParaItemAsync, AdicionarAsync, SaveChangesAsync
└── Infrastructure/
    ├── PedidoQueryRepository.cs         # Dapper, mapeia inline (PedidoRow, PedidoItemRow)
    └── PedidoCommandRepository.cs       # AppDbContext (EF Core tracking)
```

---

## Fluxo de Dados

### Leitura
```
Endpoint → Handler → IXxxQueryRepository → Dapper SQL → DTO
```
- Query repo retorna `null` quando não encontrado; endpoint converte em `404`
- Nenhuma entidade de domínio é instanciada no caminho de leitura
- Paginação via `LIMIT/OFFSET` + `COUNT(1)` separado

### Escrita
```
Endpoint → Handler/Service → IXxxCommandRepository.ObterPorIdAsync → [mutação no domínio] → SaveChangesAsync
```
- Command repo carrega entidade **rastreada** pelo EF Core
- Handler aplica mutações via domain methods (`Desativar()`, `AdicionarItem()`, etc.)
- EF detecta mudanças automaticamente via change tracking
- `SaveChangesAsync` é chamado pelo handler (não pelo repo) — exceto em `AdicionarAsync` de Produtos, que persiste internamente

### Caso especial — AddItemPedido
`IPedidoCommandRepository` expõe `ObterProdutoParaItemAsync(produtoId)` porque `pedido.AdicionarItem(produto, quantidade)` precisa da entidade `Produto` rastreada. Isso mantém a lógica de domínio intacta sem vazar `AppDbContext` para fora da infraestrutura.

---

## Camada de Serviço

**Produtos:** `ProdutoService` mantém-se como orquestrador, agora injetando ambos os repositórios:
```csharp
public ProdutoService(
    IProdutoQueryRepository queryRepo,
    IProdutoCommandRepository commandRepo,
    IMapper mapper,
    ILogger<ProdutoService> logger)
```

**Pedidos:** handlers de cada slice injetam o repositório diretamente — sem service layer (padrão vertical slice mantido).

---

## Registro de Dependências

**Produtos** (`ProdutosServiceExtensions.cs`):
```csharp
services.AddScoped<IProdutoService, ProdutoService>();
services.AddScoped<IProdutoQueryRepository, DapperProdutoQueryRepository>();
services.AddScoped<IProdutoCommandRepository, EfProdutoCommandRepository>();
```

**Pedidos** (`Program.cs`):
```csharp
services.AddScoped<IPedidoQueryRepository, PedidoQueryRepository>();
services.AddScoped<IPedidoCommandRepository, PedidoCommandRepository>();
```

---

## Estratégia de Validação

1. `dotnet build` na worktree — zero erros
2. `dotnet test` — todos os testes devem passar:
   - `ProdutosAPI.Tests` — 23+ testes HTTP (endpoints, auth, paginação, soft delete)
   - `Pedidos.Tests` — testes de todos os slices (Create, Get, List, AddItem, Cancel)
   - Testes unitários de domínio (value objects, FSM de Pedido)
3. Corrigir qualquer falha antes do merge

---

## O que NÃO muda

- Endpoints (Produtos e Pedidos)
- Validators (FluentValidation)
- Domain model (Produto, Pedido, value objects)
- Migrations e AppDbContext
- Middleware (Idempotency, ExceptionHandling)
- IEndpoint auto-registration pattern

---

## Plano de Integração

- Abordagem: merge direto (sem squash) de `feature/cqrs-repositories` → `main`
- Criar PR para checkpoint de revisão
- Após merge aprovado, remover worktree
