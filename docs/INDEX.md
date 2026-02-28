# 📑 Índice Completo do Projeto

## 🎯 Por Onde Começar?

Este projeto demonstra **dois padrões arquiteturais paralelos e apartados** no mesmo codebase. Escolha a trilha de aprendizado de acordo com seu foco:

---

## 🟢 Trilha 1 – Produtos (Clean Architecture em Camadas Horizontais)

**Localização:** `src/Produtos/Endpoints/`, `src/Produtos/Services/`, `src/Produtos/Models/`, `src/Produtos/Validators/`, `src/Produtos/DTOs/`

Ideal para aprender:
- ✅ Separação clara de responsabilidades (Endpoints → Services → Data)
- ✅ API REST tradicional com Minimal API
- ✅ Padrões consolidados no mercado
- ✅ Escalabilidade horizontal (fácil adicionar mais endpoints)

**Struct:**
```
src/Produtos/
  ├─ Endpoints/ProdutoEndpoints.cs    # 6 rotas REST (GET, POST, PUT, PATCH, DELETE)
  ├─ Endpoints/AuthEndpoints.cs       # Autenticação JWT
  ├─ Services/ProdutoService.cs       # Orquestração e lógica de negócio
  ├─ Models/Produto.cs               # Entidade anêmica (apenas dados)
  ├─ Validators/ProdutoValidator.cs   # Regras de validação (separadas)
  └─ DTOs/ProdutoDTO.cs              # Transferência de dados
```

**Fluxo de Requisição:**
```
HTTP Request
    ↓
ProdutoEndpoints (rota)
    ↓
ProdutoValidator (entrada)
    ↓
ProdutoService (orquestração)
    ↓
AppDbContext (persistência)
    ↓
HTTP Response
```

**Começar aqui:**
1. Abra [src/Produtos/Endpoints/ProdutoEndpoints.cs](../src/Produtos/Endpoints/ProdutoEndpoints.cs)
2. Veja como cada rota é mapeada
3. Siga para [src/Produtos/Services/ProdutoService.cs](../src/Produtos/Services/ProdutoService.cs)
4. Explore [src/Shared/Data/AppDbContext.cs](../src/Shared/Data/AppDbContext.cs)

---

## 🔵 Trilha 2 – Pedidos (Vertical Slice Architecture + Domínio Rico)

**Localização:** `src/Pedidos/`

Ideal para aprender:
- ✅ Organização por feature/caso de uso (não por camada)
- ✅ Domain-Driven Design e modelos de domínio ricos
- ✅ Independência de features (cada slice é autossuficiente)
- ✅ Result pattern para tratamento de erros
- ✅ Padrões modernos de arquitetura

**Struct:**
```
src/Pedidos/
  ├─ Domain/
  │  ├─ Pedido.cs                    # Aggregate root (rico, com regras)
  │  ├─ PedidoItem.cs               # Entidade interna
  │  └─ StatusPedido.cs             # Value object (enum)
  ├─ CreatePedido/
  │  ├─ CreatePedidoCommand.cs      # DTO de entrada
  │  ├─ CreatePedidoValidator.cs    # Validações
  │  ├─ CreatePedidoHandler.cs      # Orquestração + domínio
  │  └─ CreatePedidoEndpoint.cs     # Rota HTTP
  ├─ GetPedido/                      # Slice: obter pedido único
  ├─ ListPedidos/                    # Slice: listar pedidos
  ├─ AddItemPedido/                  # Slice: adicionar item
  ├─ CancelPedido/                   # Slice: cancelar
  └─ Common/                         # DTOs compartilhadas
```

**Fluxo de Requisição:**
```
HTTP Request
    ↓
CreatePedidoEndpoint (rota + descuberta automática via IEndpoint)
    ↓
CreatePedidoValidator (entrada)
    ↓
CreatePedidoHandler (orquestração)
    ↓
Pedido.Create() → Result (validação de domínio)
    ↓
AppDbContext (persistência)
    ↓
HTTP Response
```

**Começar aqui:**
1. Abra [src/Pedidos/Domain/Pedido.cs](../src/Pedidos/Domain/Pedido.cs)
2. Veja como o domínio encapsula regras
3. Explore um slice completo: [src/Pedidos/CreatePedido/](../src/Pedidos/CreatePedido/)
4. Compare estrutura com Trilha 1

---

## 🔗 Compartilhado (ambas as trilhas usam)

```
src/Shared/
  ├─ Common/                         # Interfaces e padrões
  │  ├─ IEndpoint.cs                # Discover automático de rotas
  │  ├─ EndpointExtensions.cs       # Scanner via reflexão
  │  ├─ Result.cs                   # Result pattern
  │  └─ MappingProfile.cs           # AutoMapper config
  ├─ Data/
  │  ├─ AppDbContext.cs             # EF Core (um banco para ambos)
  │  ├─ Migrations/                 # Versionamento do schema
  │  └─ DbSeeder.cs                 # Dados iniciais
  └─ Middleware/
     ├─ ExceptionHandlingMiddleware.cs  # Erro global
     └─ IdempotencyMiddleware.cs       # Idempotência
```

---

## 📚 Documentação (6 guias)

### 1. [ARQUITETURA.md](ARQUITETURA.md) ⭐⭐⭐
**Guia de Estrutura Comparativa — VISUAL**
- ✅ Mostra separação de diretórios com árvore
- ✅ Tabela comparando Clean Architecture vs Vertical Slice
- ✅ Fluxos de requisição lado a lado
- ✅ Por que ambas coexistem
- **Leia primeiro para entender a visão geral**

### 2. [MELHORES-PRATICAS-API.md](MELHORES-PRATICAS-API.md) ⭐⭐⭐
**Guia Conceitual — TEÓRICO**
- ✅ Princípios REST
- ✅ Status codes
- ✅ Paginação
- ✅ Validação
- ✅ Tratamento de erros
- **Leia para fundamentação teórica**

### 3. [MELHORES-PRATICAS-MINIMAL-API.md](MELHORES-PRATICAS-MINIMAL-API.md) ⭐⭐⭐
**Guia de Implementação — PRÁTICO**
- ✅ Como implementar cada prática em .NET
- ✅ Exemplos de código real do projeto
- ✅ Configuração de middleware, logging, etc.
- ✅ Covers Clean Architecture (Produtos)
- **Leia para aprender implementação**

### 4. [VERTICAL-SLICE-DOMINIO-RICO.md](VERTICAL-SLICE-DOMINIO-RICO.md) 🧩
**Guia de Vertical Slice — CONCEITUAL + PRÁTICO**
- ✅ Problema com camadas horizontais
- ✅ Definição e anatomia de uma slice
- ✅ Diferença: modelo anêmico vs rico
- ✅ IEndpoint e descoberta automática
- ✅ Covers Vertical Slice (Pedidos)
- **Leia DEPOIS de entender Clean Architecture**

### 5. [README.md](../README.md) ⭐⭐
**Guia de Uso — PRÁTICO**
- ✅ Como executar o projeto
- ✅ Endpoints disponíveis (Produtos + Pedidos)
- ✅ Como testar via cURL ou Swagger
- ✅ Autenticação JWT
- **Consulte para treinar a API**

### 6. [INICIO-RAPIDO.md](INICIO-RAPIDO.md) ⭐
**Quick Start — REFERÊNCIA**
- ✅ 5 minutos para rodar
- ✅ Primeiros endpoints para testar
- ✅ Acesso ao Swagger
- **Comece aqui se está com pressa**

---

## ⚡ Roteiros de Aprendizado

### Roteiro 1: Para iniciantes (2-3 horas)

1. **Quick Start (5 min)**
   - Abra [INICIO-RAPIDO.md](INICIO-RAPIDO.md)
   - Execute `dotnet run`
   - Teste via Swagger

2. **Conceitos Teóricos (30 min)**
   - Leia [MELHORES-PRATICAS-API.md](MELHORES-PRATICAS-API.md)

3. **Clean Architecture — Produtos (45 min)**
   - Leia [ARQUITETURA.md](ARQUITETURA.md) — seção "Camadas Horizontais"
   - Leia [MELHORES-PRATICAS-MINIMAL-API.md](MELHORES-PRATICAS-MINIMAL-API.md) — Trilha 1
   - Explore [src/Produtos/Endpoints/ProdutoEndpoints.cs](../src/Produtos/Endpoints/ProdutoEndpoints.cs)
   - Explore [src/Produtos/Services/ProdutoService.cs](../src/Produtos/Services/ProdutoService.cs)

4. **Prática (30 min)**
   - Execute `dotnet run`
   - Teste endpoints de Produtos via Swagger
   - Leia logs no terminal

### Roteiro 2: Para aprofundamento (2-3 horas adicionais)

1. **Vertical Slice — Pedidos (60 min)**
   - Releia [ARQUITETURA.md](ARQUITETURA.md) — tabela comparativa
   - Leia [VERTICAL-SLICE-DOMINIO-RICO.md](VERTICAL-SLICE-DOMINIO-RICO.md) — completo
   - Explore [src/Pedidos/Domain/Pedido.cs](../src/Pedidos/Domain/Pedido.cs)
   - Explore um slice: [src/Pedidos/CreatePedido/](../src/Pedidos/CreatePedido/)

2. **Testes (30 min)**
   - Execute `dotnet test` (111 testes)
   - Explore [ProdutosAPI.Tests/](../ProdutosAPI.Tests/) — estrutura de testes

3. **Prática Comparativa (30 min)**
   - Teste endpoints de Pedidos (requer JWT)
   - Compare estrutura de código entre Produtos (camadas) e Pedidos (slices)
   - Veja em Swagger como ambas funcionam lado a lado

### Roteiro 3: Para arquitetos/mentores (full)

1. **Visão Geral**
   - [ARQUITETURA.md](ARQUITETURA.md) — completo

2. **Conceitos**
   - [MELHORES-PRATICAS-API.md](MELHORES-PRATICAS-API.md) — completo
   - [MELHORES-PRATICAS-MINIMAL-API.md](MELHORES-PRATICAS-MINIMAL-API.md) — completo
   - [VERTICAL-SLICE-DOMINIO-RICO.md](VERTICAL-SLICE-DOMINIO-RICO.md) — completo

3. **Código Completo**
   - Leia toda `src/` de ambas as trilhas
   - Leia testes em `ProdutosAPI.Tests/`

4. **Validação**
   - Execute `dotnet test`
   - Use [CHECKLIST.md](CHECKLIST.md) para conferir cobertura

---

## 🏗️ Estrutura de Código-Fonte Completa

### 📄 Program.cs
Arquivo principal de orquestração.
- Configuração de middleware
- Dependency Injection
- Entity Framework
- Swagger
- Endpoints (descuberta automática via `IEndpoint`)

### 📦 Trilha 1: Clean Architecture (Produtos)

#### [src/Produtos/Endpoints/ProdutoEndpoints.cs](../src/Produtos/Endpoints/ProdutoEndpoints.cs)
- 6 endpoints RESTful
- GET /api/v1/produtos (com paginação)
- GET /api/v1/produtos/{id}
- POST /api/v1/produtos
- PUT /api/v1/produtos/{id}
- PATCH /api/v1/produtos/{id}
- DELETE /api/v1/produtos/{id}

#### [src/Produtos/Services/ProdutoService.cs](../src/Produtos/Services/ProdutoService.cs)
- Interface `IProdutoService`
- Lógica de negócio centralizada
- Orquestração de operações
- Mapeamento de DTOs

#### [src/Produtos/Models/Produto.cs](../src/Produtos/Models/Produto.cs)
- Entidade anêmica (apenas dados)
- 11 propriedades
- Sem regras de negócio encapsuladas
- Soft delete

#### [src/Produtos/Validators/ProdutoValidator.cs](../src/Produtos/Validators/ProdutoValidator.cs)
- `CriarProdutoValidator`
- `AtualizarProdutoValidator`
- Regras centralizadas em validadores
- FluentValidation

#### [src/Produtos/DTOs/ProdutoDTO.cs](../src/Produtos/DTOs/ProdutoDTO.cs)
- `CriarProdutoRequest`
- `AtualizarProdutoRequest`
- `ProdutoResponse`
- `PaginatedResponse<T>`

### 📦 Trilha 2: Vertical Slice (Pedidos)

#### [src/Pedidos/Domain/Pedido.cs](../src/Pedidos/Domain/Pedido.cs)
- Aggregate root rico
- Encapsula regras de negócio
- Métodos: `Create()`, `AddItem()`, `Cancel()`
- Retorna `Result<T>` para validações

#### [src/Pedidos/CreatePedido/](../src/Pedidos/CreatePedido/)
- `CreatePedidoCommand.cs` — DTO
- `CreatePedidoValidator.cs` — Validações
- `CreatePedidoHandler.cs` — Handler
- `CreatePedidoEndpoint.cs` — Rota (IEndpoint)

#### [src/Pedidos/GetPedido/](../src/Pedidos/GetPedido/)
Padrão similar: Query → Handler → Endpoint

#### [src/Pedidos/ListPedidos/](../src/Pedidos/ListPedidos/)
Slice para listar com paginação

#### [src/Pedidos/AddItemPedido/](../src/Pedidos/AddItemPedido/)
Slice para adicionar item ao pedido

#### [src/Pedidos/CancelPedido/](../src/Pedidos/CancelPedido/)
Slice para cancelar pedido

### 🔗 Compartilhado

#### [src/Shared/Common/IEndpoint.cs](../src/Shared/Common/IEndpoint.cs)
- Interface para descoberta automática
- Implementada por todos os endpoints (Pedidos)

#### [src/Shared/Common/Result.cs](../src/Shared/Common/Result.cs)
- Result pattern
- `Result.Ok()` e `Result.Fail()`
- `Result<T>.Ok(value)` e `Result<T>.Fail(error)`

#### [src/Shared/Data/AppDbContext.cs](../src/Shared/Data/AppDbContext.cs)
- DbSet para Produtos e Pedidos
- Configurações EF Core
- Índices e constraints

#### [src/Shared/Middleware/ExceptionHandlingMiddleware.cs](../src/Shared/Middleware/ExceptionHandlingMiddleware.cs)
- Tratamento global de exceções
- Retorna ErrorResponse padronizada

---

## 🧪 Testes (111 testes)

### ProdutosAPI.Tests/

#### Unit Tests
- [Domain/](../ProdutosAPI.Tests/Unit/Domain/) — testes de modelos e agregados
- [Services/](../ProdutosAPI.Tests/Services/) — testes de serviços

#### Integration Tests
- [Endpoints/](../ProdutosAPI.Tests/Endpoints/) — testes HTTP dos endpoints
- [Pedidos/](../ProdutosAPI.Tests/Integration/Pedidos/) — testes de slices

---

## 📊 Comparação em Uma Tabela

| Aspecto | Produtos (Clean) | Pedidos (Vertical Slice) |
|---------|------------------|------------------------|
| **Diretório** | `src/Produtos/Endpoints/`, `src/Produtos/Services/`, etc | `src/Pedidos/` |
| **Organização** | Por camada | Por feature |
| **Modelo** | Anêmico | Rico |
| **Regras de negócio** | Em `Service` e `Validator` | Em `Domain` |
| **Alterar um campo** | Toca: Service, DTO, Endpoint | Toca: Domain, Handler, Command |
| **Coesão** | Baixa (espalhado) | Alta (tudo junto) |
| **Escalabilidade** | Boa até ~50 endpoints | Excelente (features isoladas) |
| **Teste** | Testa serviço isolado | Testa handler + domínio |
| **Independência de feature** | Baixa (mudanças globais) | Alta (cada slice é autossuficiente) |
| **Quando usar** | Domínio simples, muitos endpoints | Domínio complexo, features nítidas |

---

## ⚙️ Executar e Testar

### Executar a API
```bash
dotnet run
```
Acesso: http://localhost:5000
Swagger: http://localhost:5000/swagger

### Executar Testes
```bash
dotnet test
```
Resultado: 111 testes passando

### Clonar e Começar
```bash
git clone <repo>
cd net-minimal-api
dotnet run
# Agora acesse Swagger e teste ambas as trilhas
```

---

## 📞 Referência Rápida

| Quero aprender... | Arquivo | Seção |
|-------------------|---------|-------|
| REST Principles | [MELHORES-PRATICAS-API.md](MELHORES-PRATICAS-API.md) | Introdução |
| Endpoints Produtos | [src/Produtos/Endpoints/ProdutoEndpoints.cs](../src/Produtos/Endpoints/ProdutoEndpoints.cs) | Todas as rotas |
| Serviços | [src/Produtos/Services/ProdutoService.cs](../src/Produtos/Services/ProdutoService.cs) | Implementação |
| Validadores | [src/Produtos/Validators/ProdutoValidator.cs](../src/Produtos/Validators/ProdutoValidator.cs) | Rules |
| Domínio Rico | [src/Pedidos/Domain/Pedido.cs](../src/Pedidos/Domain/Pedido.cs) | Aggregate |
| Vertical Slice | [src/Pedidos/CreatePedido/](../src/Pedidos/CreatePedido/) | Exemplo completo |
| Testes | [ProdutosAPI.Tests/](../ProdutosAPI.Tests/) | Exemplos |
| Middleware | [src/Shared/Middleware/](../src/Shared/Middleware/) | Global processing |
| EF Core | [src/Shared/Data/AppDbContext.cs](../src/Shared/Data/AppDbContext.cs) | Configuration |

---

## ✨ O Que Você Vai Aprender

✅ **Clean Architecture** — Separação por responsabilidades  
✅ **Vertical Slice Architecture** — Organização por feature  
✅ **Domain-Driven Design** — Modelos ricos com regras  
✅ **Minimal API** — API REST sem controllers  
✅ **Validação Fluente** — FluentValidation  
✅ **Entity Framework Core** — ORM moderno  
✅ **AutoMapper** — Mapeamento de DTOs  
✅ **Logging Estruturado** — Serilog  
✅ **Middleware** — Processamento transversal  
✅ **Result Pattern** — Tratamento de erros  
✅ **Testes Unitários** — xUnit + Moq  
✅ **Testes de Integração** — WebApplicationFactory  

---

## 🎓 Próximos Passos

1. **Escolha uma trilha** (Produtos ou Pedidos) e comece a explorar
2. **Execute o projeto** e teste via Swagger
3. **Leia o código** de um endpoint até entender completamente
4. **Execute testes** e veja como testam diferentes aspectos
5. **Implemente uma mudança** (ex: adicionar um campo) em cada trilha e veja a diferença
6. **Compreendia trade-offs** — quando usar cada padrão

---

**Versão:** 3.0.0  
**Data:** 28 de fevereiro de 2026  
**Framework:** .NET 10 LTS  
**Padrões:** Clean Architecture + Vertical Slice + DDD  

🎉 Escolha sua trilha e comece a aprender!
