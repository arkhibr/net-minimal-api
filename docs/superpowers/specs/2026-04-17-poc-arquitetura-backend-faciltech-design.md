# POC Arquitetura Backend Faciltech — Design Spec

**Data:** 2026-04-17  
**Decisor:** Marco Mendes  
**Status:** Aprovado

---

## Contexto

Projeto educacional (`net-minimal-api`) demonstrando padrões arquiteturais coexistindo. Esta POC expande o projeto para cobrir:

1. **Migração Produtos → Catálogo** com arquitetura híbrida (Clean Architecture nas camadas + Vertical Slice na API)
2. **Recursos de domínio rico seletivo** — Categorias e Variantes com agregados, Atributos e Mídias como CRUD simples
3. **Tratamento de erro 429** — rate limiting no servidor + resiliência no client
4. **Versionamento manual de API** via route groups
5. **ADRs padronizadas** com template MADR, ciclo de vida e novas decisões

---

## Fase 1 — Fundação: Migração Produtos → Catálogo

### Roteamento e Versionamento

A API de Produtos deixa de existir como conceito isolado. Tudo migra para o bounded context **Catálogo** com versionamento manual via route groups aninhados:

```
/api/v1/catalogo/produtos
/api/v1/catalogo/categorias
/api/v1/catalogo/variantes
/api/v1/catalogo/atributos
/api/v1/catalogo/midias
```

```csharp
var v1 = app.MapGroup("/api/v1");
var catalogo = v1.MapGroup("/catalogo");

catalogo.MapProdutoEndpoints();
catalogo.MapCategoriaEndpoints();
catalogo.MapVarianteEndpoints();
catalogo.MapAtributoEndpoints();
catalogo.MapMidiaEndpoints();
```

Pedidos continua em `/api/v1/pedidos`. Auth em `/api/v1/auth`. Ambos intocados.

### Arquitetura Interna — Híbrida

Clean Architecture nas camadas Domain/Application/Infrastructure. Vertical Slice na camada API (um arquivo por endpoint).

```
src/Catalogo/
├── Catalogo.Domain/
│   ├── Entities/
│   │   ├── Produto.cs
│   │   ├── Categoria.cs
│   │   ├── Variante.cs
│   │   ├── Atributo.cs
│   │   └── Midia.cs
│   ├── ValueObjects/
│   │   ├── Preco.cs
│   │   ├── Estoque.cs
│   │   ├── SKU.cs
│   │   └── UrlMidia.cs
│   └── Common/
│       └── Result.cs
│
├── Catalogo.Application/
│   ├── DTOs/
│   │   ├── Produto/
│   │   ├── Categoria/
│   │   ├── Variante/
│   │   ├── Atributo/
│   │   └── Midia/
│   ├── Interfaces/
│   │   └── ICatalogoContext.cs
│   ├── Repositories/
│   │   ├── IProdutoRepository.cs
│   │   ├── ICategoriaRepository.cs
│   │   ├── IVarianteRepository.cs
│   │   ├── IAtributoRepository.cs
│   │   └── IMidiaRepository.cs
│   ├── Services/
│   │   ├── ProdutoService.cs
│   │   ├── CategoriaService.cs
│   │   ├── VarianteService.cs
│   │   ├── AtributoService.cs
│   │   └── MidiaService.cs
│   ├── Validators/
│   └── Mappings/
│
├── Catalogo.Infrastructure/
│   ├── Repositories/       ← EF Core (CQRS write)
│   ├── Queries/            ← Dapper (CQRS read)
│   └── Data/
│       └── DbSeeder.cs
│
└── Catalogo.API/
    ├── Endpoints/
    │   ├── Produtos/       ← vertical slice por recurso
    │   ├── Categorias/
    │   ├── Variantes/
    │   ├── Atributos/
    │   └── Midias/
    └── Extensions/
        └── CatalogoServiceExtensions.cs
```

### Mapa de Mudanças

| Antes | Depois |
|-------|--------|
| `src/Produtos/` (flat) | `src/Catalogo/` (4 sub-projetos) |
| `ProdutosAPI.Produtos.*` | `ProdutosAPI.Catalogo.*` |
| `/api/v1/produtos` | `/api/v1/catalogo/produtos` |
| `IProdutoContext` | `ICatalogoContext` |
| `docs/plans/2026-03-02-produtos-clean-architecture.md` | Superseded — absorvido por esta migração |

`AppDbContext` permanece em `src/Shared/Data/` e implementa `ICatalogoContext`. Pedidos, Shared e Pix: intocados.

---

## Fase 2 — Domínio: Novos Recursos do Catálogo

### Categorias — Domínio Rico com Hierarquia de Dois Níveis

```csharp
public class Categoria
{
    public int Id { get; private set; }
    public string Nome { get; private set; }
    public string Slug { get; private set; }       // gerado automaticamente
    public int? CategoriaPaiId { get; private set; }
    public bool Ativa { get; private set; }

    public static Result<Categoria> Criar(string nome, int? categoriaPaiId = null) { ... }
    public Result Renomear(string novoNome) { ... }
    public Result Desativar() { ... }
}
```

**Regras de domínio:**
- Slug gerado a partir do nome (lowercase, sem acentos, hífens no lugar de espaços)
- Categoria filha não pode ter filhas (máximo dois níveis — validado no domínio)
- Não é possível desativar categoria com produtos ativos vinculados

**Endpoints** (`/api/v1/catalogo/categorias`):

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/` | Lista categorias raiz com subcategorias aninhadas |
| GET | `/{id}` | Obtém categoria com subcategorias |
| POST | `/` | Cria categoria raiz ou subcategoria (`categoriaPaiId` opcional) |
| PUT | `/{id}` | Renomeia categoria |
| DELETE | `/{id}` | Desativa categoria (valida produtos ativos) |

### Variantes — Domínio Rico com SKU

```csharp
public class Variante
{
    public int Id { get; private set; }
    public int ProdutoId { get; private set; }
    public SKU Sku { get; private set; }
    public string Descricao { get; private set; }
    public Preco PrecoAdicional { get; private set; }
    public Estoque Estoque { get; private set; }
    public bool Ativa { get; private set; }

    public static Result<Variante> Criar(int produtoId, string sku, string descricao,
                                         decimal precoAdicional, int estoque) { ... }
    public Result AtualizarEstoque(int quantidade) { ... }
    public Result AtualizarPreco(decimal novoPrecoAdicional) { ... }
    public Result Desativar() { ... }
}
```

**Value Object SKU:**
- 6–20 caracteres
- Apenas letras maiúsculas, números e hífens (`^[A-Z0-9\-]+$`)
- Único por produto (validado na camada Application)

**Endpoints** (`/api/v1/catalogo/variantes`):

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/?produtoId={id}` | Lista variantes de um produto |
| GET | `/{id}` | Obtém variante específica |
| POST | `/` | Cria variante (`produtoId` no body) |
| PUT | `/{id}` | Atualiza preço adicional |
| PATCH | `/{id}/estoque` | Atualiza estoque da variante |
| DELETE | `/{id}` | Desativa variante |

### Atributos e Mídias — CRUD Simples

**Atributo:** chave-valor associado a produto (`"Cor": "Azul"`, `"Material": "Algodão"`). Sem regras de domínio complexas. CRUD completo em `/api/v1/catalogo/atributos`.

**Mídia:** URL externa associada ao produto, com tipo (`Imagem`, `Video`, `Documento`) e ordem de exibição. Sem upload de arquivo — apenas referência de URL. CRUD completo em `/api/v1/catalogo/midias`.

Ambos demonstram que dentro do mesmo bounded context coexistem recursos de complexidade diferente — decisão documentada em ADR-0015.

---

## Fase 3 — Resiliência: Tratamento de Erro 429

### Rate Limiting no Servidor

Usar `Microsoft.AspNetCore.RateLimiting` (built-in .NET). Três políticas intencionalmente diferentes para fins educacionais:

| Política | Onde aplica | Regra | Justificativa didática |
|----------|------------|-------|----------------------|
| `fixed-window` | GET `/catalogo/*` | 60 req/min por IP | Leitura pública, janela fixa simples |
| `sliding-window` | POST/PUT/PATCH/DELETE `/catalogo/*` | 20 req/min por IP | Escrita — janela deslizante mais justa |
| `token-bucket` | POST `/catalogo/produtos` | 5 tokens, repõe 1/min | Criação cara — bucket demonstra burst |

Resposta 429 inclui header `Retry-After` com segundos até o próximo slot.

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString();
        await context.HttpContext.Response.WriteAsync(
            "Too many requests. Please try again later.", cancellationToken: token);
    };
    // políticas configuradas aqui
});
```

Middleware adicionado ao pipeline antes do roteamento. Endpoints declaram política com `.RequireRateLimiting("policy-name")`.

### Resiliência no Client

Novo projeto `src/Catalogo/Catalogo.ClientDemo/` seguindo o padrão já existente em `Pix.ClientDemo`.

**Pipeline de resiliência:**

```csharp
builder.Services
    .AddHttpClient<CatalogoHttpClient>(client =>
        client.BaseAddress = new Uri("http://localhost:5000"))
    .AddResilienceHandler("catalogo", pipeline =>
    {
        // 1. Retry com backoff exponencial — trata 429 e 5xx, respeita Retry-After
        pipeline.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            ShouldHandle = args => args.Outcome switch
            {
                { Result.StatusCode: HttpStatusCode.TooManyRequests } => PredicateResult.True(),
                { Result.StatusCode: HttpStatusCode.InternalServerError } => PredicateResult.True(),
                _ => PredicateResult.False()
            }
        });

        // 2. Circuit Breaker — abre após falhas consecutivas
        pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            SamplingDuration = TimeSpan.FromSeconds(30),
            FailureRatio = 0.5,
            MinimumThroughput = 3,
            BreakDuration = TimeSpan.FromSeconds(30)
        });

        // 3. Timeout por requisição
        pipeline.AddTimeout(TimeSpan.FromSeconds(10));
    });
```

O demo dispara 10 requisições sequenciais, exibe os 429 recebidos, os retries automáticos com backoff, e o circuit breaker abrindo.

---

## Fase 4 — Documentação: ADRs

### Template MADR Padronizado

Todas as ADRs (existentes e novas) adotam o formato MADR 3.x:

```markdown
---
status: accepted
date: YYYY-MM-DD
deciders: [Marco Mendes]
superseded-by:
---

# NNNNN — Título

## Contexto e Problema
## Decisão
## Consequências
### Positivas
### Negativas / Trade-offs
## Alternativas Consideradas
```

### ADRs Existentes (0001–0010)

Todas recebem `date` e `deciders`. Status permanece `accepted`. Nenhuma é deprecated — o plano `2026-03-02-produtos-clean-architecture.md` é marcado como superseded internamente pelas novas ADRs, não nas ADRs antigas.

### Novas ADRs

| ADR | Título | Status |
|-----|--------|--------|
| 0011 | Arquitetura Híbrida: Clean Architecture + Vertical Slices na API | `accepted` |
| 0012 | Versionamento manual de API via route groups | `accepted` |
| 0013 | Rate Limiting com AspNetCore.RateLimiting — três políticas | `accepted` |
| 0014 | Resiliência no client HTTP com HttpResilienceHandler | `accepted` |
| 0015 | Domínio Rico Seletivo dentro do mesmo bounded context | `accepted` |

---

## Testes

### Estrutura (dentro de `ProdutosAPI.Tests/`)

```
Unit/
├── Domain/
│   ├── ProdutoTests.cs          ← namespaces atualizados
│   ├── CategoriaTests.cs
│   └── VarianteTests.cs
└── ValueObjects/
    ├── SKUTests.cs
    └── UrlMidiaTests.cs

Integration/
├── Catalogo/
│   ├── ProdutoEndpointsTests.cs ← rotas atualizadas
│   ├── CategoriaEndpointsTests.cs
│   ├── VarianteEndpointsTests.cs
│   ├── AtributoEndpointsTests.cs
│   └── MidiaEndpointsTests.cs
└── RateLimiting/
    └── RateLimitingTests.cs
```

### Convenção de IDs no DbSeeder

| Entidade | IDs Reservados | Testes criam a partir de |
|----------|---------------|--------------------------|
| Produtos | 1–8 | ID 9+ |
| Categorias | 1–5 | ID 6+ |
| Variantes | 1–3 | ID 4+ |
| Atributos | 1–4 | ID 5+ |
| Mídias | 1–2 | ID 3+ |

### Cobertura de Rate Limiting

- Dispara limite+1 requests, verifica que a última retorna 429
- Verifica presença e valor numérico do header `Retry-After`
- Verifica que POST atinge 429 antes do GET no mesmo intervalo de tempo

### O que não muda

- `Pedidos.Tests/` — intocado
- `AuthHelper.ObterTokenAsync(client)` — mesma autenticação nos testes
- `WebApplicationFactory` — mesma factory base, seeder expandido
- Ambiente `Testing` com SQLite in-memory

---

## Abordagem de Execução

**Abordagem A — Big Bang Coordenado** em 4 fases sequenciais:

1. **Fase 1 — Fundação:** Migrar Produtos → Catálogo com Clean Architecture + renomear rotas para `/api/v1/catalogo/*`
2. **Fase 2 — Domínio:** Adicionar Categorias, Variantes, Atributos e Mídias
3. **Fase 3 — Resiliência:** Rate limiting no servidor + `Catalogo.ClientDemo` com Polly
4. **Fase 4 — Documentação:** Template MADR, atualizar 10 ADRs existentes, criar 5 novas ADRs

Cada fase é um marco testável — testes devem passar integralmente ao final de cada fase antes de avançar.
