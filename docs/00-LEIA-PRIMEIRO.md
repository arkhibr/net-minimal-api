# 📋 PROJETO COMPLETO — VISÃO GERAL
================================================

## 🎯 OBJETIVO DO PROJETO
=====================

✅ Demonstrar duas abordagens arquiteturais modernas em uma API .NET 10:
   • **Clean Architecture** (camadas horizontais) para o caso de Produtos
   • **Vertical Slice Architecture + Domínio Rico** para o caso de Pedidos
✅ Fornecer guia conceitual, código de exemplo e cobertura de testes
✅ Ser um ponto de partida para aprender Minimal API, EF Core e padrões DDD

---

## 📚 DOCUMENTAÇÃO (pasta `docs/`)
===================================

1. ⭐ **MELHORES-PRATICAS-API.md** (10 seções teóricas)
   └─ Guia universal de melhores práticas REST

2. ⭐ **MELHORES-PRATICAS-MINIMAL-API.md** (implementação)
   └─ Explica como cada prática foi implementada para Produtos e Pedidos

3. 📖 **MELHORIAS-DOTNET-10.md**
   └─ Features novas do .NET 10 aplicadas no projeto

4. 📘 **VERTICAL-SLICE-DOMINIO-RICO.md**
   └─ Arquitetura Vertical Slice explicada com agregados de domínio

5. 🚀 **INICIO-RAPIDO.md** (quick start)
   └─ 5 minutos para rodar, incluindo autenticação e endpoints de Pedidos

6. 📑 **INDEX.md** (índice completo)
   └─ Mapa mental de aprendizado com duas trilhas

7. ✅ **CHECKLIST.md** (verificação)
   └─ Todas as práticas implementadas para os dois casos de uso

8. 🏗️ **ARQUITETURA.md** (diagramas)
   └─ Diagramas de camadas e slices, fluxo de requisições

9. 🎉 **ENTREGA-FINAL.md** (resumo executivo)
   └─ O que foi criado, como começar e o que você vai aprender

---

## ⚙️ CONFIGURAÇÃO DO PROJETO
==============================

- `ProdutosAPI.csproj`             [Definição do projeto .NET 10]
- `Program.cs`                     [Configuração central]
- `appsettings.json`               [Configurações de ambiente]
- `Properties/launchSettings.json` [Configurações de execução]
- `.gitignore`                     [Arquivos ignorados]
- `.env.example`                   [Variáveis de ambiente]
- `setup.sh`                       [Script auxiliar]
- `README.md`                      [Guia de uso]

---

## ✨ CÓDIGO-FONTE (`src/`)
===========================

### Camadas Horizontais (Produtos)
```
└─ src/Endpoints/ProdutoEndpoints.cs      # 6 endpoints REST
└─ src/Services/ProdutoService.cs         # Lógica de negócios
└─ src/Models/Produto.cs                  # Entidade de domínio
└─ src/Validators/ProdutoValidator.cs     # Regras FluentValidation
```

### Vertical Slice (Pedidos)
```
└─ src/Features/Pedidos/                  # Cada operação é um slice
   ├─ CreatePedido/                       # Command, Handler, Validator, Endpoint
   ├─ GetPedido/
   ├─ ListPedidos/
   ├─ AddItemPedido/
   └─ CancelPedido/
└─ src/Features/Common/                   # Regras e utilitários compartilhados
└─ src/Features/Pedidos/Domain/           # Agregado Pedido, PedidoItem, Result<T>
```

Ambas as abordagens compartilham `AppDbContext`, o pipeline de middleware e a mesma instância do contêiner de DI.

---

## 🧪 TESTES (`ProdutosAPI.Tests/`)
=====================================

- ✅ **111 testes automatizados** (versão 3.0.0)
- 🧱 **3 categorias**:
  1. Domain Unit Tests (agregados, regras de negócio) – 40+ testes
  2. Service Unit Tests (serviços individuais) – 35 testes
  3. Integration HTTP Tests (endpoints Produtos + Pedidos) – 36 testes
- 🛡️ Validator Tests para garantir consistência dos comandos

Localização:
```
ProdutosAPI.Tests/
├── Services/
├── Endpoints/
├── Validators/
└── Domain/           # testes de domínio rico
```

---

## 🛠️ TECNOLOGIAS
==================

| Tecnologia | Versão | Propósito |
|------------|--------|-----------|
| **.NET 10 LTS** | 10.0 | Framework principal |
| **Minimal API** | — | Web framework |
| **Entity Framework Core** | 10.0 | ORM |
| **SQLite** | — | Banco de dados local |
| **FluentValidation** | 11.10 | Validação |
| **AutoMapper** | 13.0.1 | Mapeamento de objetos |
| **Serilog** | 4.1.1 | Logging estruturado |
| **Swagger/OpenAPI** | 6.9.0 | Documentação interativa |
| **xUnit** | 2.7.0 | Framework de testes |
| **Moq** | 4.20.70 | Mocking |
| **FluentAssertions** | 6.12.0 | Assertivas fluentes |

---

## 🚀 COMO COMEÇAR
==================

**Opção 1: Rápido**
```bash
cd net-minimal-api
dotnet run
# Abra: http://localhost:5000
```

**Opção 2: Aprender**
1. Leia: `docs/MELHORES-PRATICAS-API.md`
2. Leia: `docs/MELHORES-PRATICAS-MINIMAL-API.md`
3. Experimente os endpoints de Produtos e Pedidos
4. Execute os testes para ver os 111 casos

---

## ⚙️ CONFIGURAÇÃO DO PROJETO
==============================

- `ProdutosAPI.csproj`             [Definição do projeto .NET 10]
- `Program.cs`                     [Configuração central]
- `appsettings.json`               [Configurações de runtime]
- `Properties/launchSettings.json` [Configurações de execução]
- `.gitignore`                     [Arquivos ignorados]
- `.env.example`                   [Variáveis de ambiente]
- `setup.sh`                       [Script auxiliar]
- `README.md`                      [Guia de uso]

---

## ✨ CÓDIGO-FONTE (`src/`)
===========================

**Models:**
```
└─ src/Models/Produto.cs
   └─ Entidade principal com 11 propriedades
   └─ Soft delete, audit fields, XML comments
```

**DTOs (8 classes):**
```
└─ src/DTOs/ProdutoDTO.cs
   ├─ CriarProdutoRequest
   ├─ AtualizarProdutoRequest
   ├─ ProdutoResponse
   ├─ PaginatedResponse<T>
   ├─ PaginationInfo
   ├─ ErrorResponse
   ├─ AuthResponse
   └─ LoginRequest
```

**Endpoints (6 rotas):**
```
└─ src/Endpoints/ProdutoEndpoints.cs
   ├─ GET    /       (listar com paginação)
   ├─ GET    /{id}   (obter específico)
   ├─ POST   /       (criar)
   ├─ PUT    /{id}   (atualizar completo)
   ├─ PATCH  /{id}   (atualizar parcial)
   └─ DELETE /{id}   (deletar)
```

**Services:**
```
└─ src/Services/ProdutoService.cs
   └─ 6 métodos async com logging e validação
```

**Data Access:**
```
├─ src/Data/AppDbContext.cs         [EF Core context com índices]
├─ src/Data/DbSeeder.cs             [8 produtos de exemplo]
└─ src/Data/Migrations/
   ├─ 20250225000000_CreateInitialSchema.cs
   └─ AppDbContextModelSnapshot.cs
```

**Validação (3 validadores):**
```
└─ src/Validators/ProdutoValidator.cs
   ├─ CriarProdutoValidator
   ├─ AtualizarProdutoValidator
   └─ LoginValidator
```

**Middleware:**
```
└─ src/Middleware/ExceptionHandlingMiddleware.cs
   └─ Tratamento global de exceções
```

**Common:**
```
└─ src/Common/MappingProfile.cs
   └─ Configuração AutoMapper
```

---

## 🧪 TESTES (`ProdutosAPI.Tests/`)
=====================================

- `Services/ProdutoServiceTests.cs`     [Testes de serviço (InMemory DB)]
- `Endpoints/ProdutoEndpointsTests.cs`  [Testes de endpoints]
- `Validators/ProdutoValidatorTests.cs` [Testes de validação]
- `ESTRATEGIA-DE-TESTES.md`             [Estratégia completa de testes]

---

## 🛠️ TECNOLOGIAS
==================

| Tecnologia | Versão | Propósito |
|------------|--------|-----------|
| **.NET 10 LTS** | 10.0 | Framework principal |
| **Minimal API** | — | Web framework |
| **Entity Framework Core** | 10.0 | ORM |
| **SQLite** | — | Banco de dados local |
| **FluentValidation** | 11.10 | Validação |
| **AutoMapper** | 12.0 | Mapeamento de objetos |
| **Serilog** | 4.2 | Logging estruturado |
| **Swashbuckle (Swagger)** | 6.9 | Documentação interativa |
| **xUnit** | 2.7 | Framework de testes |
| **Moq** | 4.20 | Mocking |
| **FluentAssertions** | 6.12 | Assertivas fluentes |

---

## ✅ PRÁTICAS IMPLEMENTADAS
============================

✓ **RESTful Design**         → Endpoints seguem REST  
✓ **HTTP Verbs Corretos**    → GET, POST, PUT, PATCH, DELETE  
✓ **HTTP Status Codes**      → 200, 201, 204, 400, 404, 422, 500  
✓ **Paginação**              → page, pageSize, metadata  
✓ **Filtros e Busca**        → categoria, search  
✓ **Versionamento**          → /api/v1/  
✓ **Validação de Dados**     → FluentValidation com regras  
✓ **Tratamento de Erros**    → Middleware global, respostas padronizadas  
✓ **Logging Estruturado**    → Serilog (console + arquivo)  
✓ **Documentação**           → Swagger/OpenAPI, XML comments  
✓ **Performance**            → Async/await, índices, paginação  
✓ **Segurança**              → ORM (SQL Injection), CORS, input validation  
✓ **Arquitetura**            → Clean Architecture, separation of concerns  
✓ **DTOs**                   → Separação entre modelos internos e externos  
✓ **Injeção de Dependência** → Services registrados em Program.cs  
✓ **Testes Automatizados**   → 111 testes (unitários de domínio/serviço e integrações HTTP)  

---

## 🚀 COMO COMEÇAR
==================

**Opção 1: Rápido**
```bash
cd net-minimal-api
dotnet run
# Abra: http://localhost:5000
```

**Opção 2: Aprender**
1. Leia: `docs/MELHORES-PRATICAS-API.md`
2. Leia: `docs/MELHORES-PRATICAS-MINIMAL-API.md`
3. Execute: `dotnet run`
4. Teste: exemplos do `README.md`
5. Explore: código-fonte em `src/`

**Opção 3: Completo**
1. Leia: `docs/INICIO-RAPIDO.md`
2. Siga: todos os documentos em sequência
3. Estude: cada arquivo de código

---

## 📂 ESTRUTURA FINAL
====================

```
net-minimal-api/
│
├── README.md                      [Guia principal]
├── Program.cs                     [Configuração central]
├── ProdutosAPI.csproj             [Projeto .NET 10]
├── appsettings.json
│
├── 📁 docs/                       [Documentação]
│   ├── 00-LEIA-PRIMEIRO.md   ← você está aqui
│   ├── MELHORES-PRATICAS-API.md ⭐
│   ├── MELHORES-PRATICAS-MINIMAL-API.md ⭐
│   ├── MELHORIAS-DOTNET-10.md
│   ├── ARQUITETURA.md
│   ├── INICIO-RAPIDO.md
│   ├── INDEX.md
│   ├── CHECKLIST.md
│   └── ENTREGA-FINAL.md
│
├── 📁 src/                        [Código-fonte]
│   ├── Models/Produto.cs
│   ├── DTOs/ProdutoDTO.cs
│   ├── Endpoints/ProdutoEndpoints.cs
│   ├── Features/Pedidos/  # Vertical slices (5 operações)
│   │   ├── CreatePedido/
│   │   ├── GetPedido/
│   │   ├── ListPedidos/
│   │   ├── AddItemPedido/
│   │   └── CancelPedido/
│   ├── Services/ProdutoService.cs
│   ├── Data/AppDbContext.cs
│   ├── Data/DbSeeder.cs
│   ├── Data/Migrations/
│   ├── Validators/ProdutoValidator.cs
│   ├── Validators/*Pedido*  # validadores de comando
│   ├── Middleware/ExceptionHandlingMiddleware.cs
│   └── Common/MappingProfile.cs
│
└── 📁 ProdutosAPI.Tests/          [Testes]
    ├── Services/ProdutoServiceTests.cs
    ├── Endpoints/ProdutoEndpointsTests.cs
    ├── Validators/ProdutoValidatorTests.cs
    └── ESTRATEGIA-DE-TESTES.md
```

---
═══════════════════════════════════════════════════════════

**PRONTO PARA COMEÇAR!**

1. Abra o terminal em: `net-minimal-api`
2. Execute: `dotnet run`
3. Abra: `http://localhost:5000`
4. Explore a documentação em `docs/`

Ou comece lendo: `docs/INICIO-RAPIDO.md`


