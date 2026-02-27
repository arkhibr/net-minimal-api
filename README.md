# ProdutosAPI - Projeto para Aprendizado com .NET 10 e Minimal API [![.NET 10](https://img.shields.io/badge/.NET-10.0%20LTS-blue?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com)

![Version](https://img.shields.io/badge/version-3.0.0-success?style=flat-square)
![Status](https://img.shields.io/badge/status-Production%20Ready-brightgreen?style=flat-square)
![License](https://img.shields.io/badge/license-MIT-blue?style=flat-square)

## 📚 Sobre o Projeto

**ProdutosAPI** é um projeto educacional demonstrando melhores práticas de APIs REST com **.NET 10 LTS** e **Minimal API**. O projeto cobre dois padrões arquiteturais complementares, implementados como casos de uso reais com cobertura completa de testes (121 testes).

### Objetivo
Fornecer um recurso abrangente incluindo:
- 📖 Guia conceitual de melhores práticas de APIs REST
- 💻 Implementação pronta para produção com padrões modernos (.NET 10 e Minimal API)

---

## 🚀 Quick Start

### Pré-requisitos
- **.NET 10 SDK** ou superior
- **Visual Studio 2024**, **VS Code**, ou similar

### Instalação e Execução

```bash
# 1. Clone ou navegue para o diretório do projeto
cd net-minimal-api

# 2. Restaurar dependências
dotnet restore

# 3. Build do projeto
dotnet build -c Release

# 4. Executar testes
dotnet test
# Se preferir detalhes...  dotnet test -l "console;verbosity=detailed"

# 5. Executar a aplicação
dotnet run

# 6. Acessar a API
# - Swagger UI: http://localhost:5000
# - Health Check: http://localhost:5000/health
# - API Base: http://localhost:5000/api/v1
```

## � Estrutura do Projeto

```
net-minimal-api/
├── Program.cs                              # Configuração principal (.NET 10)
├── ProdutosAPI.csproj                      # Arquivo de projeto (net10.0)
├── appsettings.json                        # Configurações de ambiente
│
├── src/                                     # Código principal
│   ├── Common/MappingProfile.cs            # AutoMapper
│   ├── Data/
│   │   ├── AppDbContext.cs                # EF Core DbContext
│   │   └── DbSeeder.cs                    # Dados iniciais
│   ├── DTOs/ProdutoDTO.cs                 # 8 classes DTO
│   ├── Endpoints/ProdutoEndpoints.cs      # 6 endpoints (Typed Results)
│   ├── Middleware/ExceptionHandlingMiddleware.cs
│   ├── Models/Produto.cs                  # Domain model
│   ├── Services/ProdutoService.cs         # Business logic
│   ├── Validators/ProdutoValidator.cs     # FluentValidation
│   └── Features/                            # Vertical Slice Architecture
│       ├── Common/
│       │   ├── IEndpoint.cs               # Interface de registro automático
│       │   ├── EndpointExtensions.cs      # Scanner de endpoints
│       │   └── Result.cs                  # Result pattern
│       └── Pedidos/
│           ├── Domain/                    # Aggregate root + entities
│           ├── Common/                    # DTOs dos slices
│           ├── CreatePedido/              # Slice POST /pedidos
│           ├── GetPedido/                 # Slice GET /pedidos/{id}
│           ├── ListPedidos/               # Slice GET /pedidos
│           ├── AddItemPedido/             # Slice POST /pedidos/{id}/itens
│           └── CancelPedido/              # Slice POST /pedidos/{id}/cancelar
│
├── ProdutosAPI.Tests/                      # Testes abrangentes
│   ├── ProdutosAPI.Tests.csproj
│   ├── ESTRATEGIA-DE-TESTES.md           # Estratégia completa de testes
│   ├── Unit/Domain/
│   │   ├── ProdutoTests.cs                # 18 testes de domínio rico
│   │   └── PedidoTests.cs                 # 16 testes do aggregate
│   ├── Builders/
│   │   └── ProdutoBuilder.cs              # Builder fluente para testes
│   ├── Services/ProdutoServiceTests.cs    # Unit tests com mocks
│   ├── Endpoints/ProdutoEndpointsTests.cs # Endpoint tests
│   ├── Validators/ProdutoValidatorTests.cs
│   └── Integration/
│       ├── ApiFactory.cs                  # WebApplicationFactory
│       ├── AuthHelper.cs                  # JWT helper
│       ├── CreatePedidoTests.cs
│       ├── GetPedidoTests.cs
│       ├── CancelPedidoTests.cs
│       ├── AddItemPedidoTests.cs
│       └── ListPedidosTests.cs
│
├── docs/                                   # 📖 Documentação completa
│   ├── 00-LEIA-PRIMEIRO.md               # Índice geral do projeto
│   ├── MELHORES-PRATICAS-API.md          # Guia conceitual
│   ├── MELHORES-PRATICAS-MINIMAL-API.md  # Implementação das práticas
│   ├── MELHORIAS-DOTNET-10.md            # Features .NET 10
│   ├── ARQUITETURA.md                    # Diagramas de arquitetura
│   ├── INICIO-RAPIDO.md                  # Quick start
│   ├── INDEX.md                          # Índice completo
│   ├── CHECKLIST.md                      # Verificação de práticas
│   └── ENTREGA-FINAL.md                  # Resumo executivo
│
├── logs/                                   # Logs estruturados (runtime)
└── produtos-api.db                         # Banco SQLite (runtime)
```

---

## 🎯 Principais Recursos

### ✅ 11 Endpoints REST (2 casos de uso)

| Método | Rota | Descrição | Status |
|--------|------|-----------|---------|
| `GET` | `/api/v1/produtos` | Listar com paginação | 200 OK |
| `GET` | `/api/v1/produtos/{id}` | Obter específico | 200/404 |
| `POST` | `/api/v1/produtos` | Criar novo | 201/422 |
| `PUT` | `/api/v1/produtos/{id}` | Atualizar completo | 200/404/422 |
| `PATCH` | `/api/v1/produtos/{id}` | Atualizar parcial | 200/404/422 |
| `DELETE` | `/api/v1/produtos/{id}` | Soft delete | 204/404 |

### Pedidos (Vertical Slice + JWT obrigatório)

| Método | Rota | Descrição | Status |
|--------|------|-----------|---------|
| `POST` | `/api/v1/pedidos` | Criar pedido | 201/400 |
| `GET` | `/api/v1/pedidos/{id}` | Obter pedido | 200/404 |
| `GET` | `/api/v1/pedidos` | Listar pedidos | 200 |
| `POST` | `/api/v1/pedidos/{id}/itens` | Adicionar item | 200/400/404 |
| `POST` | `/api/v1/pedidos/{id}/cancelar` | Cancelar pedido | 200/400/404 |

### ✅ 121 Testes Automatizados

- **Testes de Domínio** - Regras de negócio puras (Produto + Pedido aggregate)
- **Unit Tests** - Testa lógica de serviços com mocking
- **Integration Tests HTTP** - Ponta a ponta com WebApplicationFactory
- **Validator Tests** - Testa regras de validação

Execute com: `dotnet test`

### ✅ .NET 10 Minimal API Enhancements (NOVO)

- **Typed Results** - Type-safety em compile-time
- **MapGroup com Prefix** - Organize endpoints sem duplicação
- **Discriminated Union Results** - Múltiplos return types seguros
- **Enhanced OpenAPI** - Swagger preciso com todos status codes

---

## 🧪 Executando Testes

```bash
# Todos os testes
dotnet test

# Teste específico
dotnet test --filter "Name=ObterProdutoAsync_WithValidId_ReturnsProduto"

# Com detalhes
dotnet test --verbosity detailed
```

## � Documentação Completa

### Guias Disponíveis

1. **[docs/MELHORES-PRATICAS-API.md](./docs/MELHORES-PRATICAS-API.md)** 📖
   - RESTful Design principles
   - HTTP Status Codes
   - Validação de dados
   - Segurança e autenticação
   - Tratamento de erros
   - Logging e monitoramento

2. **[docs/MELHORIAS-DOTNET-10.md](./docs/MELHORIAS-DOTNET-10.md)** 🚀
   - Typed Results para type-safety
   - Discriminated Union Results
   - MapGroup com Prefix
   - Enhanced OpenAPI
   - Comparativas antes/depois

3. **[ProdutosAPI.Tests/ESTRATEGIA-DE-TESTES.md](./ProdutosAPI.Tests/ESTRATEGIA-DE-TESTES.md)** 🧪
   - Estratégia completa de testes
   - Como executar testes
   - Padrão AAA (Arrange-Act-Assert)
   - Cobertura esperada

4. **Outros Guias** (em `docs/`)
   - [ARQUITETURA.md](./docs/ARQUITETURA.md) - Diagramas de arquitetura
   - [INICIO-RAPIDO.md](./docs/INICIO-RAPIDO.md) - Quick start guide
   - [INDEX.md](./docs/INDEX.md) - Índice completo
 
---

## 🛠️ Stack Técnico

```
.NET 10 LTS (Versão 2.0.0)
├── ASP.NET Core Minimal API
├── Entity Framework Core 10.0.0
├── SQLite (Demo)
├── FluentValidation 11.10.0
├── AutoMapper 13.0.1
├── Serilog 4.1.1 (Structured Logging)
└── Swagger/OpenAPI 6.9.0

📊 Testes (Novo)
├── xUnit 2.7.0
├── Moq 4.20.70
└── FluentAssertions 6.12.0
```

---

## 📊 Exemplos Rápidos

### Listar Produtos
```bash
curl -X GET "http://localhost:5000/api/v1/produtos?page=1&pageSize=10"
```

### Criar Produto
```bash
curl -X POST "http://localhost:5000/api/v1/produtos" \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Mouse Logitech",
    "descricao": "Wireless USB",
    "preco": 150.00,
    "categoria": "Periféricos",
    "estoque": 50,
    "contatoEmail": "vendor@example.com"
  }'
```

### Obter Produto
```bash
curl -X GET "http://localhost:5000/api/v1/produtos/1"
```

### Atualizar (PATCH)
```bash
curl -X PATCH "http://localhost:5000/api/v1/produtos/1" \
  -H "Content-Type: application/json" \
  -d '{"preco": 160.00}'
```

### Deletar
```bash
curl -X DELETE "http://localhost:5000/api/v1/produtos/1"
```

---

## 🔐 Exemplos com Pedidos (requer JWT)

### Autenticação
```bash
# Obter token JWT
curl -X POST "http://localhost:5000/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@example.com", "senha": "senha123"}'
# Copie o campo "token" da resposta
```

### Criar Pedido
```bash
curl -X POST "http://localhost:5000/api/v1/pedidos" \
  -H "Authorization: Bearer SEU_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{}'
```

### Adicionar Item ao Pedido
```bash
curl -X POST "http://localhost:5000/api/v1/pedidos/1/itens" \
  -H "Authorization: Bearer SEU_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"produtoId": 1, "quantidade": 2}'
```

### Listar Pedidos
```bash
curl -X GET "http://localhost:5000/api/v1/pedidos" \
  -H "Authorization: Bearer SEU_TOKEN"
```

---

## 🔐 Configuração Avançada

### Database Alternatives

**SQL Server**:
```csharp
options.UseSqlServer(connectionString)
```

**PostgreSQL**:
```csharp
options.UseNpgsql(connectionString)
```

### CORS

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecific", policy =>
    {
        policy.WithOrigins("https://example.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

---

## 🧪 Padrão AAA para Testes

**Arrange**: Preparar dados e mocks  
**Act**: Executar ação  
**Assert**: Validar resultado

```csharp
[Fact]
public async Task ObterProduto_WithValidId_ReturnsProduto()
{
    // Arrange
    var id = 1;
    var produto = new Produto { Id = id, Nome = "Test" };
    
    // Act
    var result = await service.ObterProdutoAsync(id);
    
    // Assert
    result.Should().NotBeNull();
    result.Id.Should().Be(id);
}
```

## 📚 Recursos de Aprendizado

- 📖 [Documentação .NET 10](https://learn.microsoft.com/en-us/dotnet/)
- 📖 [Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- 📖 [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- 📖 [FluentValidation](https://docs.fluentvalidation.net/)
- 📖 [xUnit Testing](https://xunit.net/)

---

## 🎓 Objetivo de Aprendizado

Este projeto foi criado com fins **didáticos** para demonstrar:

✅ Arquitetura Clean em ASP.NET Core
✅ Melhores práticas de REST API design
✅ Features modernas do .NET 10
✅ Minimal API patterns
✅ Testes automatizados completos
✅ Documentação profissional
✅ Vertical Slice Architecture
✅ Domínio Rico e Aggregate Root
✅ Result Pattern
✅ Testes de domínio e integração HTTP

