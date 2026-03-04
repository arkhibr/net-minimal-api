# 📑 Índice Completo do Projeto

## 🎯 Por Onde Começar?

Este projeto demonstra **três trilhas complementares** no mesmo codebase. Escolha o caminho de aprendizado de acordo com seu foco:

---

## 🟢 Trilha 1 – Produtos (Clean Architecture em Projetos Separados)

**Localização:** `src/Produtos/Produtos.API/`, `src/Produtos/Produtos.Application/`, `src/Produtos/Produtos.Domain/`, `src/Produtos/Produtos.Infrastructure/`

Ideal para aprender:
- ✅ Separação clara de responsabilidades (Endpoints → Services → Data)
- ✅ API REST tradicional com Minimal API
- ✅ Padrões consolidados no mercado
- ✅ Escalabilidade horizontal (fácil adicionar mais endpoints)

**Struct:**
```
src/Produtos/
  ├─ Produtos.API/Endpoints/ProdutoEndpoints.cs
  ├─ Produtos.API/Endpoints/AuthEndpoints.cs
  ├─ Produtos.Application/Services/ProdutoService.cs
  ├─ Produtos.Application/Validators/ProdutoValidator.cs
  ├─ Produtos.Application/DTOs/ProdutoDTO.cs
  ├─ Produtos.Domain/Produto.cs
  └─ Produtos.Infrastructure/Repositories/EfProdutoRepository.cs
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
1. Abra [src/Produtos/Produtos.API/Endpoints/ProdutoEndpoints.cs](../src/Produtos/Produtos.API/Endpoints/ProdutoEndpoints.cs)
2. Veja como cada rota é mapeada
3. Siga para [src/Produtos/Produtos.Application/Services/ProdutoService.cs](../src/Produtos/Produtos.Application/Services/ProdutoService.cs)
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

## 🟣 Trilha 3 – PIX Mock Processing (Servidor + Cliente HTTP)

**Localização:** `src/Pix/Pix.MockServer/`, `src/Pix/Pix.ClientDemo/`, `tests/Pix.MockServer.Tests/`

Ideal para aprender:
- ✅ Modelagem de JSON complexo com payloads aninhados
- ✅ Boas práticas de chamadas HTTP com `HttpClientFactory`
- ✅ Resiliência com `AddStandardResilienceHandler`
- ✅ Idempotência, correlação e segurança com mTLS real (OAuth2 + certificado de cliente)
- ✅ Testes de integração para APIs financeiras simuladas

**Struct:**
```
src/Pix/
  ├─ Pix.MockServer/
  │  ├─ Contracts/                  # Requests/responses complexos
  │  ├─ Application/                # Regras de negócio e validações
  │  ├─ Domain/                     # Estados e entidades do mock
  │  ├─ Infrastructure/InMemory/    # Repositórios thread-safe
  │  ├─ Security/                   # Token + mTLS real
  │  └─ Program.cs                  # Endpoints /oauth e /pix/v1
  ├─ Pix.ClientDemo/
  │  ├─ Client/                     # Cliente tipado e token provider
  │  ├─ Client/Handlers/            # Correlation, idempotency, logging
  │  ├─ Scenarios/                  # Fluxo fim-a-fim didático
  │  └─ Program.cs                  # Execução do cenário

tests/
  └─ Pix.MockServer.Tests/          # Testes de integração do mock
```

**Começar aqui:**
1. Abra [docs/PIX-DEMO.md](PIX-DEMO.md)
2. Suba [src/Pix/Pix.MockServer/Program.cs](../src/Pix/Pix.MockServer/Program.cs)
3. Execute [src/Pix/Pix.ClientDemo/Program.cs](../src/Pix/Pix.ClientDemo/Program.cs)
4. Rode os testes em [tests/Pix.MockServer.Tests/PixMockServerTests.cs](../tests/Pix.MockServer.Tests/PixMockServerTests.cs)

---

## 🔗 Compartilhado (trilhas internas usam)

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

## 📚 Documentação (7 guias)

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

### 7. [PIX-DEMO.md](PIX-DEMO.md) 💸
**Trilha de Integração HTTP — PRÁTICO**
- ✅ Mock PIX auto-contido
- ✅ Cliente com `HttpClientFactory` e resiliência
- ✅ Payload JSON complexo
- ✅ Fluxo: autenticar, cobrar, liquidar e devolver
- **Leia para estudar integrações externas**

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
   - Explore [src/Produtos/Produtos.API/Endpoints/ProdutoEndpoints.cs](../src/Produtos/Produtos.API/Endpoints/ProdutoEndpoints.cs)
   - Explore [src/Produtos/Produtos.Application/Services/ProdutoService.cs](../src/Produtos/Produtos.Application/Services/ProdutoService.cs)

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
   - Execute `dotnet test` (166 testes)
   - Explore [tests/ProdutosAPI.Tests/](../tests/ProdutosAPI.Tests/) — estrutura de testes

3. **Prática Comparativa (30 min)**
   - Teste endpoints de Pedidos (requer JWT)
   - Compare estrutura de código entre Produtos (camadas) e Pedidos (slices)
   - Veja em Swagger como ambas funcionam lado a lado

4. **Integração Externa (30 min)**
   - Leia [PIX-DEMO.md](PIX-DEMO.md)
   - Execute `dotnet run --project src/Pix/Pix.MockServer/Pix.MockServer.csproj`
   - Execute `dotnet run --project src/Pix/Pix.ClientDemo/Pix.ClientDemo.csproj`

### Roteiro 3: Para arquitetos/mentores (full)

1. **Visão Geral**
   - [ARQUITETURA.md](ARQUITETURA.md) — completo

2. **Conceitos**
   - [MELHORES-PRATICAS-API.md](MELHORES-PRATICAS-API.md) — completo
   - [MELHORES-PRATICAS-MINIMAL-API.md](MELHORES-PRATICAS-MINIMAL-API.md) — completo
   - [VERTICAL-SLICE-DOMINIO-RICO.md](VERTICAL-SLICE-DOMINIO-RICO.md) — completo
   - [PIX-DEMO.md](PIX-DEMO.md) — completo

3. **Código Completo**
   - Leia toda `src/` de ambas as trilhas
   - Leia testes em `tests/ProdutosAPI.Tests/`

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

#### [src/Produtos/Produtos.API/Endpoints/ProdutoEndpoints.cs](../src/Produtos/Produtos.API/Endpoints/ProdutoEndpoints.cs)
- 6 endpoints RESTful
- GET /api/v1/produtos (com paginação)
- GET /api/v1/produtos/{id}
- POST /api/v1/produtos
- PUT /api/v1/produtos/{id}
- PATCH /api/v1/produtos/{id}
- DELETE /api/v1/produtos/{id}

#### [src/Produtos/Produtos.Application/Services/ProdutoService.cs](../src/Produtos/Produtos.Application/Services/ProdutoService.cs)
- Interface `IProdutoService`
- Lógica de negócio centralizada
- Orquestração de operações
- Mapeamento de DTOs

#### [src/Produtos/Produtos.Domain/Produto.cs](../src/Produtos/Produtos.Domain/Produto.cs)
- Entidade de domínio com invariantes
- Métodos de comportamento (`Criar`, `AtualizarPreco`, `ReporEstoque`, etc.)
- Regras encapsuladas sem dependência de infraestrutura
- Soft delete por `Desativar()`

#### [src/Produtos/Produtos.Application/Validators/ProdutoValidator.cs](../src/Produtos/Produtos.Application/Validators/ProdutoValidator.cs)
- `CriarProdutoValidator`
- `AtualizarProdutoValidator`
- Regras centralizadas em validadores
- FluentValidation

#### [src/Produtos/Produtos.Application/DTOs/ProdutoDTO.cs](../src/Produtos/Produtos.Application/DTOs/ProdutoDTO.cs)
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

### 📦 Trilha 3: Integração PIX (Mock + Cliente)

#### [src/Pix/Pix.MockServer/Program.cs](../src/Pix/Pix.MockServer/Program.cs)
- Endpoints de autenticação mock (`/oauth/token`)
- Endpoints PIX (`/pix/v1/cobrancas`, `/pix/v1/devolucoes`)
- Segurança com mTLS real + `Bearer` token
- Erros padronizados com `problem+json`

#### [src/Pix/Pix.ClientDemo/Client/PixProcessingClient.cs](../src/Pix/Pix.ClientDemo/Client/PixProcessingClient.cs)
- Cliente HTTP tipado com `HttpClientFactory`
- Chamadas para criar/consultar cobrança e devolução
- Preparação de headers de segurança

#### [src/Pix/Pix.ClientDemo/Scenarios/PixScenarioRunner.cs](../src/Pix/Pix.ClientDemo/Scenarios/PixScenarioRunner.cs)
- Cenário fim-a-fim didático
- Geração de payload complexo com `metadata` e `split`
- Resumo final em console

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

## 🧪 Testes (166 testes)

### tests/ProdutosAPI.Tests/

#### Unit Tests
- [Domain/](../tests/ProdutosAPI.Tests/Unit/Domain/) — testes de modelos e agregados
- [Services/](../tests/ProdutosAPI.Tests/Services/) — testes de serviços

#### Integration Tests
- [Endpoints/](../tests/ProdutosAPI.Tests/Endpoints/) — testes HTTP dos endpoints
- [Pedidos/](../tests/ProdutosAPI.Tests/Integration/Pedidos/) — testes de slices

### tests/Pedidos.Tests/
- [Unit/Domain/](../tests/Pedidos.Tests/Unit/Domain/) — agregado `Pedido`
- [Integration/](../tests/Pedidos.Tests/Integration/) — apoio para testes HTTP

### tests/Pix.MockServer.Tests/
- [PixMockServerTests.cs](../tests/Pix.MockServer.Tests/PixMockServerTests.cs) — 7 cenários de aceite (auth, idempotência, liquidação, devolução)

---

## 📊 Comparação em Uma Tabela

| Aspecto | Produtos (Clean) | Pedidos (Vertical Slice) |
|---------|------------------|------------------------|
| **Diretório** | `src/Produtos/Produtos.*` | `src/Pedidos/` |
| **Organização** | Por camada | Por feature |
| **Modelo** | Rico | Rico |
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
Resultado esperado: 166 testes passando

### Executar Trilha PIX
```bash
dotnet run --project src/Pix/Pix.MockServer/Pix.MockServer.csproj
dotnet run --project src/Pix/Pix.ClientDemo/Pix.ClientDemo.csproj
dotnet test tests/Pix.MockServer.Tests/Pix.MockServer.Tests.csproj
```

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
| Endpoints Produtos | [src/Produtos/Produtos.API/Endpoints/ProdutoEndpoints.cs](../src/Produtos/Produtos.API/Endpoints/ProdutoEndpoints.cs) | Todas as rotas |
| Serviços | [src/Produtos/Produtos.Application/Services/ProdutoService.cs](../src/Produtos/Produtos.Application/Services/ProdutoService.cs) | Implementação |
| Validadores | [src/Produtos/Produtos.Application/Validators/ProdutoValidator.cs](../src/Produtos/Produtos.Application/Validators/ProdutoValidator.cs) | Rules |
| Domínio Rico | [src/Pedidos/Domain/Pedido.cs](../src/Pedidos/Domain/Pedido.cs) | Aggregate |
| Vertical Slice | [src/Pedidos/CreatePedido/](../src/Pedidos/CreatePedido/) | Exemplo completo |
| Testes | [tests/ProdutosAPI.Tests/](../tests/ProdutosAPI.Tests/) | Exemplos |
| Middleware | [src/Shared/Middleware/](../src/Shared/Middleware/) | Global processing |
| EF Core | [src/Shared/Data/AppDbContext.cs](../src/Shared/Data/AppDbContext.cs) | Configuration |

---

## ✨ O Que Você Vai Aprender

✅ **Clean Architecture** — Separação por responsabilidades  
✅ **Vertical Slice Architecture** — Organização por feature  
✅ **Integração HTTP Externa** — Cliente tipado + mock server  
✅ **JSON Complexo** — Contratos aninhados com metadata  
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

**Versão:** 3.1.0  
**Data:** 4 de março de 2026  
**Framework:** .NET 10 LTS  
**Padrões:** Clean Architecture + Vertical Slice + DDD + API Client Patterns  

🎉 Escolha sua trilha e comece a aprender!
