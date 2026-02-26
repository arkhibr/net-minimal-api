# ProdutosAPI - Projeto para Aprendizado com .NET 10 e Minimal API [![.NET 10](https://img.shields.io/badge/.NET-10.0%20LTS-blue?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com)

![Version](https://img.shields.io/badge/version-2.0.0-success?style=flat-square)
![Status](https://img.shields.io/badge/status-Production%20Ready-brightgreen?style=flat-square)
![License](https://img.shields.io/badge/license-MIT-blue?style=flat-square)

## 📚 Sobre o Projeto

**ProdutosAPI** é um projeto educacional completo demonstrando melhores práticas de desenvolvimento de APIs REST usando **.NET 10 LTS** e **Minimal API** com cobertura completa de testes.

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
│   └── Validators/ProdutoValidator.cs     # FluentValidation
│
├── ProdutosAPI.Tests/                      # Testes abrangentes
│   ├── ProdutosAPI.Tests.csproj          # xUnit + Moq + FluentAssertions
│   ├── ESTRATEGIA-DE-TESTES.md           # Documentação estratégia
│   ├── Services/ProdutoServiceTests.cs     # Unit tests
│   ├── Endpoints/ProdutoEndpointsTests.cs  # Integration tests
│   └── Validators/ProdutoValidatorTests.cs # Validator tests
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

### ✅ 6 Endpoints REST Completos com Typed Results

| Método | Rota | Descrição | Status |
|--------|------|-----------|---------|
| `GET` | `/api/v1/produtos` | Listar com paginação | 200 OK |
| `GET` | `/api/v1/produtos/{id}` | Obter específico | 200/404 |
| `POST` | `/api/v1/produtos` | Criar novo | 201/422 |
| `PUT` | `/api/v1/produtos/{id}` | Atualizar completo | 200/404/422 |
| `PATCH` | `/api/v1/produtos/{id}` | Atualizar parcial | 200/404/422 |
| `DELETE` | `/api/v1/produtos/{id}` | Soft delete | 204/404 |

### ✅ 50+ Testes Automatizados (NOVO em v2.0.0)

- **Unit Tests** - Testa lógica de serviços com mocking
- **Integration Tests** - Valida endpoints e status HTTP codes
- **Validator Tests** - Testa regras de negócio

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

