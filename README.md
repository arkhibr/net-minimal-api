# ProdutosAPI - Projeto para Aprendizado com .NET 10 e Minimal API [![.NET 10](https://img.shields.io/badge/.NET-10.0%20LTS-blue?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com)

![Version](https://img.shields.io/badge/version-3.0.0-success?style=flat-square)
![Status](https://img.shields.io/badge/status-Production%20Ready-brightgreen?style=flat-square)
![License](https://img.shields.io/badge/license-MIT-blue?style=flat-square)

## 📚 Sobre o Projeto

**ProdutosAPI** é um projeto educacional completo demonstrando melhores práticas de desenvolvimento de APIs REST usando **.NET 10 LTS** e **Minimal API** com cobertura completa de testes. Ele ilustra dois estilos arquiteturais coexistindo no mesmo código: **Clean Architecture** em camadas para o caso de Produtos e **Vertical Slice Architecture com Domínio Rico** para o caso de Pedidos.

### Objetivo
Fornecer um recurso abrangente incluindo:
- 📖 Guia conceitual de melhores práticas de APIs REST
- 💻 Implementação pronta para produção com padrões modernos (.NET 10 e Minimal API)
- 🎯 Demonstração de dois padrões arquiteturais: **Clean Architecture** (Produtos) e **Vertical Slice + Domínio Rico** (Pedidos)

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
---

## 🏗️ Dois Padrões Arquiteturais — Apartados e Paralelos

Este projeto não escolhe **um** padrão — ele demonstra **dois** lado a lado, cada um em sua estrutura de diretório, facilitando comparação educacional:

### 🟢 Trilha 1: Clean Architecture (Produtos)

**Diretórios:** `src/Produtos/Produtos.API/`, `src/Produtos/Produtos.Application/`, `src/Produtos/Produtos.Domain/`, `src/Produtos/Produtos.Infrastructure/`

Padrão tradicional com separação por responsabilidade:
```
HTTP → Produtos.API/Endpoints → Produtos.Application (Validators + Services) → Produtos.Infrastructure/Repositories → AppDbContext → Database
```

**Explore:**
- Rota simples: [src/Produtos/Produtos.API/Endpoints/ProdutoEndpoints.cs](src/Produtos/Produtos.API/Endpoints/ProdutoEndpoints.cs)
- Lógica: [src/Produtos/Produtos.Application/Services/ProdutoService.cs](src/Produtos/Produtos.Application/Services/ProdutoService.cs)
- Entidade rica: [src/Produtos/Produtos.Domain/Produto.cs](src/Produtos/Produtos.Domain/Produto.cs)
- Testes: [ProdutosAPI.Tests/Services/](ProdutosAPI.Tests/Services/)

### 🔵 Trilha 2: Vertical Slice + Domínio Rico (Pedidos)

**Diretório:** `src/Pedidos/`

Padrão moderno com organização por feature:
```
HTTP → src/Pedidos/CreatePedido/CreatePedidoEndpoint → CreatePedidoValidator → CreatePedidoHandler → Pedido.Create() → src/Shared/Data/AppDbContext
```

**Explore:**
- Agregado rico: [src/Pedidos/Domain/Pedido.cs](src/Pedidos/Domain/Pedido.cs)
- Uma slice completa: [src/Pedidos/CreatePedido/](src/Pedidos/CreatePedido/)
- Result pattern: [src/Shared/Common/Result.cs](src/Shared/Common/Result.cs)
- Testes: [ProdutosAPI.Tests/Integration/Pedidos/](ProdutosAPI.Tests/Integration/Pedidos/)

### 📊 Comparação Rápida

| Aspecto | Produtos (Clean) | Pedidos (Vertical Slice) |
|---------|-----------------|--------------------------|
| **Organização** | Por camada | Por feature |
| **Modelo** | Rico (dados + regras)| Rico (dados + regras) |
| **Validação** | Em separado | Encapsulada |
| **Ideal para** | Domínio simples | Domínio complexo |

📖 **Saiba mais:** [ARQUITETURA.md](docs/ARQUITETURA.md) | [VERTICAL-SLICE-DOMINIO-RICO.md](docs/VERTICAL-SLICE-DOMINIO-RICO.md)

---
## � Estrutura do Projeto

```
net-minimal-api/
├── Program.cs                              # Configuração principal (.NET 10)
├── ProdutosAPI.csproj                      # Arquivo de projeto principal
├── ProdutosAPI.slnx                        # Solution explorer setup
├── appsettings.json                        # Configurações de ambiente
│
├── src/                                    # Código principal particionado
│   ├── Pedidos/                            # Módulo Pedidos (Vertical Slice + Domínio Rico)
│   │   ├── CreatePedido/                   # Slices (Create, Get, List, Cancel)
│   │   └── Domain/                         # Agregado Pedido, Entidades e Regras de Negócio
│   ├── Produtos/                           # Módulo Produtos (Clean Architecture)
│   │   ├── Produtos.Domain/                # Entidades e regras de domínio
│   │   ├── Produtos.Application/           # DTOs, serviços, validadores, mappings
│   │   ├── Produtos.Infrastructure/        # Repositórios EF e seeding
│   │   └── Produtos.API/                   # Endpoints e composição de DI
│   └── Shared/                             # Infraestrutura e Código Comum
│       ├── Common/                         # Helper classes, Result pattern
│       ├── Data/                           # Entity Framework DbContext e migrations
│       └── Middleware/                     # ExceptionHandling e Idempotency
│
├── ProdutosAPI.Tests/                      # Testes do módulo Produtos (Clean Architecture)
│   ├── Domain/                             # Domain tests
│   ├── Services/                           # Unit tests (35 testes)
│   ├── Endpoints/                          # Integration tests HTTP (18 testes)
│   └── Validators/                         # Validator tests (20+ testes)
│
├── Pedidos.Tests/                          # Testes do módulo Pedidos (Vertical Slice + Domínio Rico)
│   ├── Domain/                             # Testes de agregado (11 testes)
│   └── Builders/                           # Construtores para massa de testes
│
├── docs/                                   # 📖 Documentação completa
│   ├── 00-LEIA-PRIMEIRO.md               # Índice geral do projeto
│   ├── MELHORES-PRATICAS-API.md          # Guia conceitual
│   ├── MELHORES-PRATICAS-MINIMAL-API.md  # Implementação das práticas
│   ├── MELHORIAS-DOTNET-10.md            # Features .NET 10
│   ├── ARQUITETURA.md                    # Diagramas de arquitetura
│   ├── VERTICAL-SLICE-DOMINIO-RICO.md    # Detalhamento de Vertical Slice
│   ├── ESTRATEGIA-DE-TESTES.md           # Planejamento global de testes
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

### ✅ 11 Endpoints REST Completos com Typed Results

#### Produtos (Clean Architecture)

| Método | Rota | Descrição | Status |
|--------|------|-----------|---------|
| `GET` | `/api/v1/produtos` | Listar com paginação | 200 OK |
| `GET` | `/api/v1/produtos/{id}` | Obter específico | 200/404 |
| `POST` | `/api/v1/produtos` | Criar novo | 201/422 |
| `PUT` | `/api/v1/produtos/{id}` | Atualizar completo | 200/404/422 |
| `PATCH` | `/api/v1/produtos/{id}` | Atualizar parcial | 200/404/422 |
| `DELETE` | `/api/v1/produtos/{id}` | Soft delete | 204/404 |

#### Pedidos (vertical slice, domínio rico)

| Método | Rota | Descrição | Status |
|--------|------|-----------|---------|
| `POST` | `/api/v1/pedidos` | Criar novo pedido | 201/422/401 |
| `GET` | `/api/v1/pedidos/{id}` | Obter pedido | 200/404/401 |
| `PATCH` | `/api/v1/pedidos/{id}/cancelar` | Cancelar pedido | 204/404/409/401 |
| `POST` | `/api/v1/pedidos/{id}/itens` | Adicionar item | 201/404/422/401 |
| `GET` | `/api/v1/pedidos` | Listar pedidos | 200/401 |

### ✅ 122 Testes Automatizados (NOVO em v3.0.0)

Distribuídos em **2 projetos paralelos**:

**ProdutosAPI.Tests** (111 testes - Clean Architecture):
- **Domain Unit Tests** – regras de negócio de agregados (40+ testes)
- **Service Unit Tests** – casos de serviço individuais (35 testes)
- **Integration HTTP Tests** – endpoints Produtos (18 testes)
- **Validator Tests** – validações (20+ testes)

**Pedidos.Tests** (11 testes - Vertical Slice + Domínio Rico):
- **Domain Unit Tests** – agregado Pedido com Result pattern (11 testes)
  - Criar, AdicionarItem, Confirmar, Cancelar com validações

Execute com: `dotnet test ProdutosAPI.slnx`

### ✅ .NET 10 Minimal API Enhancements (NOVO)

- **Typed Results** - Type-safety em compile-time
- **MapGroup com Prefix** - Organize endpoints sem duplicação
- **Discriminated Union Results** - Múltiplos return types seguros
- **Enhanced OpenAPI** - Swagger preciso com todos status codes

---

## 🧪 Executando Testes

```bash
# Todos os testes (ProdutosAPI.Tests + Pedidos.Tests)
dotnet test

# Ou explicitamente a solução
dotnet test ProdutosAPI.slnx

# Testes específicos do projeto Produtos
dotnet test ProdutosAPI.Tests

# Testes específicos do projeto Pedidos
dotnet test Pedidos.Tests

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

3. **[docs/ESTRATEGIA-DE-TESTES.md](./docs/ESTRATEGIA-DE-TESTES.md)** 🧪
   - Estratégia completa de testes (ProdutosAPI.Tests + Pedidos.Tests)
   - Como executar testes em Clean Architecture e Vertical Slice
   - Padrão AAA (Arrange-Act-Assert)
   - Cobertura esperada e documentação do Domínio

4. **[docs/VERTICAL-SLICE-DOMINIO-RICO.md](./docs/VERTICAL-SLICE-DOMINIO-RICO.md)** 🚀
   - Fundamentos do Vertical Slice
   - Detalhamento prático das implementações de Request Handlers
   - Utilização do Padrão Result no design do Domínio

   - **Outros recursos:**
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
