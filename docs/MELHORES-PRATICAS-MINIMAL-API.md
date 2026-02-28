# Melhores Práticas de Minimal API em .NET - Guia de Implementação

## Introdução

Este documento explica como as melhores práticas de API REST apresentadas em `MELHORES-PRATICAS-API.md` foram implementadas no projeto de exemplo usando .NET 10 e Minimal API.

## Versão do .NET

- **.NET 10.0** - Versão LTS mais moderna com suporte estendido
- **Minimal API** - Abordagem simplificada para criar APIs sem controllers

### Por que Minimal API?

A Minimal API é ideal para:
- ✅ APIs simples e diretas
- ✅ Microserviços
- ✅ APIs com poucos endpoints
- ✅ Prototipagem rápida
- ✅ Menor overhead de framework

---

## Estrutura do Projeto

```
net-minimal-api/
├── Program.cs                          # Configuração central da aplicação
├── ProdutosAPI.csproj                 # Configuração do projeto
├── appsettings.json                   # Configurações de ambiente
├── MELHORES-PRATICAS-API.md          # Guia conceitual (referência)
├── src/
│   ├── Models/
│   │   └── Produto.cs                # Entidade principal
│   ├── DTOs/
│   │   └── ProdutoDTO.cs             # Data Transfer Objects
│   ├── Endpoints/
│   │   └── ProdutoEndpoints.cs       # Endpoints da API
│   ├── Services/
│   │   └── ProdutoService.cs         # Lógica de negócio
│   ├── Data/
│   │   └── AppDbContext.cs           # Entity Framework Context
│   ├── Validators/
│   │   └── ProdutoValidator.cs       # FluentValidation
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs  # Tratamento de erros
│   └── Common/
│       └── MappingProfile.cs         # AutoMapper mappings
└── logs/                              # Diretório de logs
```

---

## Implementação das Melhores Práticas

### 1. RESTful Design

#### ✅ Identificação de Recursos

**Referência**: MELHORES-PRATICAS-API.md - Seção "RESTful Design"

Os endpoints seguem a convenção REST com recursos bem definidos:

```csharp
// src/Produtos/Endpoints/ProdutoEndpoints.cs
const string BaseRoute = "/api/v1/produtos";

// Recursos identificados por URI
GET    /api/v1/produtos              → Listar produtos
GET    /api/v1/produtos/{id}         → Obter específico
POST   /api/v1/produtos              → Criar novo
PUT    /api/v1/produtos/{id}         → Atualizar completo
PATCH  /api/v1/produtos/{id}         → Atualizar parcial
DELETE /api/v1/produtos/{id}         → Deletar
```

**Implementação**: [src/Produtos/Endpoints/ProdutoEndpoints.cs](../src/Produtos/Endpoints/ProdutoEndpoints.cs#L15-L20)

#### ✅ Operações Padrão HTTP

Cada endpoint usa o verbo HTTP correto:

```csharp
// POST - Criar (idempotência violada inticionalmente)
group.MapPost("/", CriarProduto)
    .WithName("CriarProduto")
    .Produces<ProdutoResponse>(StatusCodes.Status201Created);

// GET - Recuperar (idempotente, sem efeitos colaterais)
group.MapGet("/{id}", ObterProduto)
    .WithName("ObterProduto")
    .Produces<ProdutoResponse>(StatusCodes.Status200OK);

// PUT - Substituir completamente
group.MapPut("/{id}", AtualizarCompletoProduto)
    .Produces<ProdutoResponse>(StatusCodes.Status200OK);

// PATCH - Atualizar parcialmente
group.MapPatch("/{id}", AtualizarParcialProduto)
    .Produces<ProdutoResponse>(StatusCodes.Status200OK);

// DELETE - Remover
group.MapDelete("/{id}", DeletarProduto)
    .Produces(StatusCodes.Status204NoContent);
```

**Implementação**: [src/Produtos/Endpoints/ProdutoEndpoints.cs](../src/Produtos/Endpoints/ProdutoEndpoints.cs#L29-L60)

#### ✅ Representação Padronizada

**Referência**: MELHORES-PRATICAS-API.md - Seção "Representação de Recursos"

Respostas padronizadas em JSON usando DTOs:

```csharp
// src/Produtos/DTOs/ProdutoDTO.cs
public class ProdutoResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public int Estoque { get; set; }
    public bool Ativo { get; set; }
    public string ContatoEmail { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
    public DateTime DataAtualizacao { get; set; }
}
```

**Implementação**: [src/Produtos/DTOs/ProdutoDTO.cs](../src/Produtos/DTOs/ProdutoDTO.cs#L25)

#### ✅ Statelessness

Cada requisição deve conter todas as informações necessárias:

```csharp
// Não mantém estado de sessão
// Autenticação futura: JWT token em header Authorization

private static async Task<IResult> ListarProdutos(
    IProdutoService produtoService,
    int page = 1,
    int pageSize = 20,
    string? categoria = null,
    string? search = null)
{
    // Toda informação está na requisição
    var resultado = await produtoService.ListarProdutosAsync(
        page, pageSize, categoria, search);
    
    return Results.Ok(resultado);
}
```

**Implementação**: [src/Produtos/Endpoints/ProdutoEndpoints.cs](../src/Produtos/Endpoints/ProdutoEndpoints.cs#L70-L80)

---

### 2. Design de Endpoints

#### ✅ Nomenclatura de URLs

**Referência**: MELHORES-PRATICAS-API.md - Seção "Nomenclatura de URLs"

```csharp
// ✅ CORRETO: Nomes em plural
GET /api/v1/produtos

// ✅ CORRETO: Minúsculas
GET /api/v1/produtos/123

// ✅ CORRETO: Hífens para separar palavras  
GET /api/v1/produtos?status=produto-ativo

// ❌ EVITAR: Verbos nas URLs
// GET /api/v1/obter-produtos
// GET /api/v1/deletar-produto/123
```

**Implementação**: [src/Produtos/Endpoints/ProdutoEndpoints.cs](../src/Produtos/Endpoints/ProdutoEndpoints.cs#L14-L15)

#### ✅ Paginação

**Referência**: MELHORES-PRATICAS-API.md - Seção "Paginação"

```csharp
// Requisição com paginação
GET /api/v1/produtos?page=1&pageSize=20&sortBy=nome

// Resposta paginada
{
  "data": [...],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalItems": 150,
    "totalPages": 8
  }
}
```

**Implementação da resposta**: [src/Produtos/DTOs/ProdutoDTO.cs](../src/Produtos/DTOs/ProdutoDTO.cs#L46-L57)

**Implementação do endpoint**: [src/Produtos/Endpoints/ProdutoEndpoints.cs](../src/Produtos/Endpoints/ProdutoEndpoints.cs#L70-L86)

**Implementação do serviço**:

```csharp
// src/Produtos/Services/ProdutoService.cs
public async Task<PaginatedResponse<ProdutoResponse>> ListarProdutosAsync(
    int page, int pageSize, string? categoria = null, string? search = null)
{
    if (page < 1) page = 1;
    if (pageSize < 1 || pageSize > 100) pageSize = 20; // Máximo 100

    var query = _context.Produtos.Where(p => p.Ativo).AsQueryable();

    var totalItems = await query.CountAsync();
    var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

    var produtos = await query
        .OrderByDescending(p => p.DataCriacao)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return new PaginatedResponse<ProdutoResponse>
    {
        Data = _mapper.Map<List<ProdutoResponse>>(produtos),
        Pagination = new PaginationInfo
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        }
    };
}
```

**Implementação**: [src/Produtos/Services/ProdutoService.cs](../src/Produtos/Services/ProdutoService.cs#L32-L75)

#### ✅ Filtros e Busca

**Referência**: MELHORES-PRATICAS-API.md - Seção "Filtros e Busca"

```csharp
// Suporte a filtros e busca na mesma requisição
GET /api/v1/produtos?categoria=eletrônicos&search=notebook

// No serviço:
if (!string.IsNullOrEmpty(categoria))
{
    query = query.Where(p => p.Categoria == categoria);
}

if (!string.IsNullOrEmpty(search))
{
    query = query.Where(p => p.Nome.Contains(search) || 
                             p.Descricao.Contains(search));
}
```

**Implementação**: [src/Produtos/Services/ProdutoService.cs](../src/Produtos/Services/ProdutoService.cs#L48-L55)

---

### 3. Versionamento

**Referência**: MELHORES-PRATICAS-API.md - Seção "Versionamento"

#### ✅ URL Path Versionamento (Recomendado)

```csharp
// Program.cs
const string BaseRoute = "/api/v1/produtos";

// Endpoints começam com /api/v1/
// Fácil evoluir para /api/v2/ no futuro
```

**Implementação**: [src/Produtos/Endpoints/ProdutoEndpoints.cs](../src/Produtos/Endpoints/ProdutoEndpoints.cs#L14)

#### ✅ Versionamento Semântico do Projeto

```xml
<!-- ProdutosAPI.csproj -->
<Version>1.0.0</Version>
<Description>API REST de Produtos com Minimal API e .NET 10</Description>
```

**Implementação**: [ProdutosAPI.csproj](ProdutosAPI.csproj#L8-L9)

---

### 4. Segurança

#### ✅ Validação de Inputs

**Referência**: MELHORES-PRATICAS-API.md - Seção "Segurança - Validação"

Usando **FluentValidation** para validações robustas:

```csharp
// src/Produtos/Validators/ProdutoValidator.cs
public class CriarProdutoValidator : AbstractValidator<CriarProdutoRequest>
{
    public CriarProdutoValidator()
    {
        RuleFor(p => p.Nome)
            .NotEmpty()
            .WithMessage("Nome é obrigatório")
            .MinimumLength(3)
            .WithMessage("Nome deve ter no mínimo 3 caracteres")
            .MaximumLength(100)
            .WithMessage("Nome não pode exceder 100 caracteres");

        RuleFor(p => p.Preco)
            .GreaterThan(0)
            .WithMessage("Preço deve ser maior que zero");

        RuleFor(p => p.ContatoEmail)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email de contato inválido");
    }
}
```

**Implementação do validador**: [src/Produtos/Validators/ProdutoValidator.cs](../src/Produtos/Validators/ProdutoValidator.cs)

**Uso no endpoint**:

```csharp
// src/Produtos/Endpoints/ProdutoEndpoints.cs
private static async Task<IResult> CriarProduto(
    CriarProdutoRequest request,
    IValidator<CriarProdutoRequest> validator,
    ...)
{
    var resultado = await validator.ValidateAsync(request);
    if (!resultado.IsValid)
    {
        throw new ValidationException(resultado.Errors);
    }
    
    var produto = await produtoService.CriarProdutoAsync(request);
    return Results.Created($"/api/v1/produtos/{produto.Id}", produto);
}
```

**Implementação**: [src/Produtos/Endpoints/ProdutoEndpoints.cs](../src/Produtos/Endpoints/ProdutoEndpoints.cs#L125-L145)

#### ✅ Proteção contra SQL Injection

**Referência**: MELHORES-PRATICAS-API.md - Seção "Segurança - SQL Injection"

Usando **Entity Framework Core** (ORM) ao invés de SQL raw:

```csharp
// ✅ SEGURO: Usando EF Core com queries LINQ
var produtos = await _context.Produtos
    .Where(p => p.Nome.Contains(search)) // Parametrizado automaticamente
    .ToListAsync();

// ❌ NÃO FAZER: Raw SQL sem parametrização
// var produtos = _context.Produtos.FromSqlRaw(
//     $"SELECT * FROM Produtos WHERE Nome LIKE '%{search}%'");
```

**Implementação**: [src/Produtos/Services/ProdutoService.cs](../src/Produtos/Services/ProdutoService.cs#L48)

#### ✅ CORS Configurado

**Referência**: MELHORES-PRATICAS-API.md - Seção "Segurança - CORS"

```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Use CORS
app.UseCors("AllowAll");
```

**Implementação**: [Program.cs](Program.cs#L42-L51)

---

### 5. Validação de Dados

**Referência**: MELHORES-PRATICAS-API.md - Seção "Validação de Dados"

#### ✅ Input Validation

Campos validados conforme business rules:

```csharp
// src/Produtos/Models/Produto.cs
public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;     // Min 3 caracteres
    public decimal Preco { get; set; }                   // Deve ser > 0
    public int Estoque { get; set; }                     // Não pode ser negativo
    public string ContatoEmail { get; set; } = string.Empty; // Email válido
    public bool Ativo { get; set; } = true;              // Status soft delete
}
```

**Implementação**: [src/Produtos/Models/Produto.cs](../src/Produtos/Models/Produto.cs)

#### ✅ Mensagens de Erro de Validação

```csharp
// Resposta de validação
{
  "errors": {
    "nome": ["Campo obrigatório", "Mínimo 3 caracteres"],
    "preco": ["Deve ser maior que 0"],
    "email": ["Email inválido"]
  }
}
```

**Implementação**: [src/Shared/Middleware/ExceptionHandlingMiddleware.cs](../src/Shared/Middleware/ExceptionHandlingMiddleware.cs#L47-L60)

#### ✅ Sanitização

AutoMapper e FluentValidation garantem sanitização:

```csharp
// AutoMapper mapeia e converte tipos
CreateMap<CriarProdutoRequest, Produto>();

// FluentValidation valida formato
RuleFor(p => p.ContatoEmail)
    .EmailAddress()
    .WithMessage("Email de contato inválido");
```

**Implementação**: [src/Common/MappingProfile.cs](../src/Common/MappingProfile.cs)

---

### 6. Tratamento de Erros

**Referência**: MELHORES-PRATICAS-API.md - Seção "Tratamento de Erros"

#### ✅ HTTP Status Codes Corretos

```csharp
// GET - 200 OK
Results.Ok(produto)

// POST - 201 Created
Results.Created($"/api/v1/produtos/{produto.Id}", produto)

// DELETE - 204 No Content
Results.NoContent()

// 4xx Erros do Cliente
Results.NotFound(...)           // 404
Results.BadRequest(...)         // 400
Results.UnprocessableEntity(...) // 422

// 5xx Erro do Servidor
// Middleware captura e retorna 500
```

**Implementação**: [src/Produtos/Endpoints/ProdutoEndpoints.cs](../src/Produtos/Endpoints/ProdutoEndpoints.cs#L88-L180)

#### ✅ Respostas de Erro Padronizadas

**Referência**: MELHORES-PRATICAS-API.md - Seção "Resposta de Erro Padronizada"

```csharp
// src/Produtos/DTOs/ProdutoDTO.cs
public class ErrorResponse
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string Instance { get; set; } = string.Empty;
    public Dictionary<string, List<string>> Errors { get; set; } = new();
}
```

**Exemplo de resposta de erro**:

```json
{
  "type": "https://example.com/errors/validation-error",
  "title": "Validation Failed",
  "status": 422,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/v1/produtos",
  "errors": {
    "nome": ["Campo obrigatório"],
    "preco": ["Deve ser maior que 0"]
  }
}
```

**Implementação**: [src/Shared/Middleware/ExceptionHandlingMiddleware.cs](../src/Shared/Middleware/ExceptionHandlingMiddleware.cs#L35-L75)

#### ✅ Middleware Global de Tratamento de Erros

```csharp
// src/Shared/Middleware/ExceptionHandlingMiddleware.cs
public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção não tratada");
            await HandleExceptionAsync(context, ex);
        }
    }
}

// Program.cs
app.UseExceptionHandling();
```

**Implementação**: [src/Shared/Middleware/ExceptionHandlingMiddleware.cs](../src/Shared/Middleware/ExceptionHandlingMiddleware.cs)

---

### 7. Documentação

**Referência**: MELHORES-PRATICAS-API.md - Seção "Documentação"

#### ✅ OpenAPI/Swagger

```csharp
// Program.cs
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Produtos API",
        Version = "v1.0.0",
        Description = "API REST de produtos com Minimal API em .NET 10"
    });
});

// Use Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

**Implementação**: [Program.cs](Program.cs#L80-L100)

#### ✅ Endpoints com Descrição

```csharp
// src/Produtos/Endpoints/ProdutoEndpoints.cs
group.MapGet("/", ListarProdutos)
    .WithName("ListarProdutos")
    .WithDescription("Lista todos os produtos com paginação")
    .WithSummary("Listar produtos")
    .Produces<PaginatedResponse<ProdutoResponse>>(StatusCodes.Status200OK)
    .AllowAnonymous();
```

**Acesso**: http://localhost:5000 (Swagger UI)

**Implementação**: [src/Produtos/Endpoints/ProdutoEndpoints.cs](../src/Produtos/Endpoints/ProdutoEndpoints.cs#L22-L27)

#### ✅ XML Comments

```csharp
// src/Produtos/Models/Produto.cs
/// <summary>
/// Entidade Produto
/// Referência: MELHORES-PRATICAS-API.md - Seção "Design de Endpoints"
/// Representa um produto no sistema
/// </summary>
public class Produto
{
    /// <summary>
    /// Identificador único do produto (PK)
    /// </summary>
    public int Id { get; set; }
}
```

**Implementação**: [src/Produtos/Models/Produto.cs](../src/Produtos/Models/Produto.cs#L1)

---

### 8. Performance

**Referência**: MELHORES-PRATICAS-API.md - Seção "Performance"

#### ✅ Paginação Obrigatória

```csharp
// Máximo 100 itens por página
if (pageSize < 1 || pageSize > 100) pageSize = 20;

// Padrão: 20 itens
int pageSize = 20
```

**Implementação**: [src/Produtos/Services/ProdutoService.cs](../src/Produtos/Services/ProdutoService.cs#L41-L42)

#### ✅ Async/Await

```csharp
// Todas as operações I/O são assíncronas
public async Task<PaginatedResponse<ProdutoResponse>> ListarProdutosAsync(...)
{
    var totalItems = await query.CountAsync();
    var produtos = await query.ToListAsync();
    // ...
}
```

**Implementação**: [src/Produtos/Services/ProdutoService.cs](../src/Produtos/Services/ProdutoService.cs#L32)

#### ✅ Índices de Banco de Dados

```csharp
// src/Shared/Data/AppDbContext.cs
entity.HasIndex(p => p.Ativo)
    .HasName("idx_produto_ativo");

entity.HasIndex(p => p.Categoria)
    .HasName("idx_produto_categoria");
```

**Implementação**: [src/Shared/Data/AppDbContext.cs](../src/Shared/Data/AppDbContext.cs#L26-L30)

---

### 9. Logging e Monitoramento

**Referência**: MELHORES-PRATICAS-API.md - Seção "Logging e Monitoramento"

#### ✅ Structured Logging com Serilog

```csharp
// Program.cs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/api-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        fileSizeLimitBytes: 10_000_000)
    .WriteTo.File(
        new JsonFormatter(),
        path: "logs/api-.json",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

**Implementação**: [Program.cs](Program.cs#L17-L33)

#### ✅ Logging em Serviços

```csharp
// src/Produtos/Services/ProdutoService.cs
_logger.LogInformation("Listando produtos - Page: {Page}, PageSize: {PageSize}", 
    page, pageSize);

_logger.LogWarning("Produto com ID {ProductId} não encontrado", id);

_logger.LogError(ex, "Erro ao listar produtos");
```

**Implementação**: [src/Produtos/Services/ProdutoService.cs](../src/Produtos/Services/ProdutoService.cs#L34)

#### ✅ Correlação de Requisições

```csharp
// Middleware captura request ID
var requestId = context.TraceIdentifier;

// Disponível em logs
_logger.LogInformation("Requisição: {RequestId}", requestId);
```

---

### 10. Testes

**Referência**: MELHORES-PRATICAS-API.md - Seção "Testes"

Para implementar testes, crie um projeto de teste:

```bash
# Criar projeto de teste
dotnet new xunit --name ProdutosAPI.Tests

# Adicionar referências
dotnet add package Moq
dotnet add package FluentAssertions
```

#### ✅ Exemplo de Teste Unitário

```csharp
// ProdutosAPI.Tests/Services/ProdutoServiceTests.cs
[Fact]
public async Task ListarProdutos_DeveRetornarPaginado()
{
    // Arrange
    var mockContext = new Mock<AppDbContext>();
    var mockLogger = new Mock<ILogger<ProdutoService>>();
    var mockMapper = new Mock<IMapper>();
    
    var service = new ProdutoService(mockContext.Object, mockMapper.Object, mockLogger.Object);
    
    // Act
    var resultado = await service.ListarProdutosAsync(1, 20);
    
    // Assert
    resultado.Pagination.Page.Should().Be(1);
    resultado.Pagination.PageSize.Should().Be(20);
}
```

---

## Como Executar o Projeto

### Pré-requisitos

- .NET 10 SDK
- SQLite (incluído no Entity Framework)

### Passos

1. **Clonar/Abrir projeto**

```bash
cd net-minimal-api
```

2. **Restaurar dependências**

```bash
dotnet restore
```

3. **Configurar banco de dados**

```bash
# As migrations serão aplicadas automaticamente ao iniciar
# Ou manualmente:
dotnet ef database update
```

4. **Executar aplicação**

```bash
dotnet run
```

5. **Acessar Swagger UI**

```
http://localhost:5000
```

6. **Exemplos de requisições**

```bash
# Listar produtos
curl -X GET "http://localhost:5000/api/v1/produtos?page=1&pageSize=20"

# Criar produto
curl -X POST "http://localhost:5000/api/v1/produtos" \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Notebook Dell",
    "descricao": "Notebook de alto desempenho",
    "preco": 3500.00,
    "categoria": "Eletrônicos",
    "estoque": 5,
    "contatoEmail": "vendas@dell.com"
  }'

# Obter produto
curl -X GET "http://localhost:5000/api/v1/produtos/1"

# Atualizar completamente (PUT)
curl -X PUT "http://localhost:5000/api/v1/produtos/1" \
  -H "Content-Type: application/json" \
  -d '{...}'

# Atualizar parcialmente (PATCH)
curl -X PATCH "http://localhost:5000/api/v1/produtos/1" \
  -H "Content-Type: application/json" \
  -d '{"preco": 3200.00}'

# Deletar
curl -X DELETE "http://localhost:5000/api/v1/produtos/1"

# Health check
curl -X GET "http://localhost:5000/health"
```

---

## Estrutura de Pastas

```
src/
├── Models/
│   └── Produto.cs
│       - Entidade principal
│       - Propriedades da tabela de produtos
│       - Comentários XML com referências ao guia
│
├── DTOs/
│   └── ProdutoDTO.cs
│       - CriarProdutoRequest: Dados de entrada
│       - ProdutoResponse: Dados de saída
│       - AtualizarProdutoRequest: Para PATCH
│       - PaginatedResponse: Resposta paginada
│       - ErrorResponse: Erro padronizado
│
├── Endpoints/
│   └── ProdutoEndpoints.cs
│       - MapGet: Listar e obter
│       - MapPost: Criar
│       - MapPut: Atualizar completo
│       - MapPatch: Atualizar parcial
│       - MapDelete: Deletar
│       - Comentários referenciando guia
│
├── Services/
│   └── ProdutoService.cs
│       - IProdutoService interface
│       - Implementação com lógica de negócio
│       - Logging estruturado
│       - Tratamento de exceções
│
├── Data/
│   └── AppDbContext.cs
│       - DbContext do Entity Framework
│       - Configuração de entidades
│       - Definiçao de índices
│
├── Validators/
│   └── ProdutoValidator.cs
│       - CriarProdutoValidator
│       - AtualizarProdutoValidator
│       - Validações de negócio
│
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs
│       - Tratamento global de exceções
│       - Respostas padronizadas
│       - Logging de erros
│
└── Common/
    └── MappingProfile.cs
        - Configuração AutoMapper
        - Mapeamentos entre entidades e DTOs
```

---

## Referências Cruzadas

| Aspecto | Documento | Implementação |
|---------|-----------|------------------|
| RESTful Design | [Seção 1](MELHORES-PRATICAS-API.md#princípios-fundamentais) | [Endpoints](../src/Produtos/Endpoints/ProdutoEndpoints.cs) |
| HTTP Verbs | [Seção 1](MELHORES-PRATICAS-API.md#operações-padrão) | [Endpoints](../src/Produtos/Endpoints/ProdutoEndpoints.cs#L29-L60) |
| Paginação | [Seção 2](MELHORES-PRATICAS-API.md#paginação) | [Service](../src/Produtos/Services/ProdutoService.cs#L32-L75) |
| Versionamento | [Seção 3](MELHORES-PRATICAS-API.md#versionamento) | [Routes](../src/Produtos/Endpoints/ProdutoEndpoints.cs#L14) |
| Segurança | [Seção 4](MELHORES-PRATICAS-API.md#segurança) | [Validators](../src/Produtos/Validators/ProdutoValidator.cs) |
| Validação | [Seção 5](MELHORES-PRATICAS-API.md#validação-de-dados) | [Validators](../src/Produtos/Validators/ProdutoValidator.cs) |
| Erros | [Seção 6](MELHORES-PRATICAS-API.md#tratamento-de-erros) | [Middleware](../src/Shared/Middleware/ExceptionHandlingMiddleware.cs) |
| Documentação | [Seção 7](MELHORES-PRATICAS-API.md#documentação) | [Program.cs](Program.cs#L80-L100) |
| Performance | [Seção 8](MELHORES-PRATICAS-API.md#performance) | [Service](../src/Produtos/Services/ProdutoService.cs#L41) |
| Logging | [Seção 9](MELHORES-PRATICAS-API.md#logging-e-monitoramento) | [Service](../src/Produtos/Services/ProdutoService.cs#L34) |

---

## Checklist Final

Verificar se todas as práticas foram implementadas:

- ✅ Endpoints seguem convenção RESTful
- ✅ Versionamento em URL (/api/v1/)
- ✅ Autenticação preparada para JWT
- ✅ Validação com FluentValidation
- ✅ Erros retornam status codes corretos
- ✅ Tratamento global de exceções
- ✅ Logging estruturado com Serilog
- ✅ Documentação com Swagger/OpenAPI
- ✅ Async/Await em operações I/O
- ✅ Paginação implementada
- ✅ CORS configurado
- ✅ DTOs separados de entidades
- ✅ Serviços com dependências injetadas
- ✅ Entity Framework para proteção SQL Injection

---

## Capítulo Extra: Vertical Slice + Domínio Rico (Pedidos)

O projeto utiliza **Vertical Slice Architecture** para o caso de uso de Pedidos, um padrão em que cada operação é auto-contida em sua própria pasta com
componente Command/Handler/Validator/Endpoint. O objetivo é reduzir acoplamento e melhorar a navegabilidade em APIs mais complexas.

### Anatomia de um Slice

Cada slice está localizado em `src/Pedidos/<Operação>/`:

```
src/Pedidos/CreatePedido/
├─ CreatePedidoCommand.cs      # DTO de entrada
├─ CreatePedidoValidator.cs    # FluentValidation do comando
├─ CreatePedidoHandler.cs      # Lógica de negócio (usa domínio rico)
└─ CreatePedidoEndpoint.cs     # Mapeia rota e resultados
```

Os endpoints são registrados automaticamente por scan de `IEndpoint` no startup:
```csharp
builder.Services.AddEndpointsFromAssembly(typeof(Program).Assembly);
```

### Domínio Rico

O agregado `Pedido` reside em `src/Pedidos/Domain/` e encapsula regras:

```csharp
public sealed class Pedido
{
    private readonly List<PedidoItem> _itens = new();
    public Result AddItem(PedidoItem novo)
    {
        if (novo.Preco <= 0) return Result.Fail("Preço inválido");
        if (_itens.Sum(i => i.Total) + novo.Total > Limite) 
            return Result.Fail("Total excede limite");
        _itens.Add(novo);
        return Result.Ok();
    }
    // outras invariantes
}
```

Todos os métodos retornam `Result<T>` em vez de lançar exceções, seguindo a **Result Pattern**.

### Quando usar este padrão?

- Recursos com várias operações independentes
- Projetos que crescerão em escala
- Deseja-se manter cada caso de uso isolado e testável

Os slices coexistem pacificamente com os endpoints de Produtos baseados em camadas; ambos compartilham o mesmo contexto de dados e pipeline de middleware.

