# 📑 Índice Completo do Projeto

## 🎯 Por Onde Começar?

Escolha seu camino de aprendizado:

### ⚡ **Rápido (5 minutos)**
1. Abra [INICIO-RAPIDO.md](INICIO-RAPIDO.md)
2. Execute: `dotnet run`
3. Abra: http://localhost:5000
4. Pronto! API rodando com documentação interativa

### 📚 **Aprender (2-3 horas)**
1. Leia: [MELHORES-PRATICAS-API.md](MELHORES-PRATICAS-API.md) (30min)
2. Leia: [MELHORES-PRATICAS-MINIMAL-API.md](MELHORES-PRATICAS-MINIMAL-API.md) (30min)
3. Execute: `dotnet run` (5min)
4. Teste endpoints usando [README.md](../README.md) (30min)
5. Explore código clicando nos links (1-2h)

### 🏗️ **Profundo (Completo)**
1. [MELHORES-PRATICAS-API.md](MELHORES-PRATICAS-API.md) - Teoria
2. [MELHORES-PRATICAS-MINIMAL-API.md](MELHORES-PRATICAS-MINIMAL-API.md) - Implementação  
3. [README.md](../README.md) - Como usar
4. Cada arquivo de código (veja estrutura abaixo)
5. [ProdutosAPI.Tests/](../ProdutosAPI.Tests/) - Como testar
6. [CHECKLIST.md](CHECKLIST.md) - Verificar cobertura

---

## 📚 Documentação (4 arquivos)

### 1. [MELHORES-PRATICAS-API.md](MELHORES-PRATICAS-API.md) ⭐⭐⭐
**Guia Conceitual - TEÓRICO**

Contém as melhores práticas universais para qualquer API REST:
- Princípios fundamentais RESTful
- Design de endpoints
- HTTP verbs e status codes
- Versionamento
- Segurança (autenticação, autorização, proteções)
- Validação de dados
- Tratamento de erros
- Documentação OpenAPI/Swagger
- Performance (caching, paginação, lazy loading)
- Logging estruturado
- Estratégias de testes

**Como usar**: Leia primeiro para entender os conceitos

---

### 2. [MELHORES-PRATICAS-MINIMAL-API.md](MELHORES-PRATICAS-MINIMAL-API.md) ⭐⭐⭐
**Guia de Implementação - PRÁTICO**

Explica exatamente como cada prática foi implementada neste projeto:
- Como RESTful foi implementado
- Como cada endpoint foi criado
- Onde estão os validadores
- Como o logging foi configurado
- Como trata erros
- Como documentação foi criada
- Links diretos para cada arquivo

**Como usar**: Após ler teoria, use este guia para ver a prática

---

### 3. [README.md](../README.md) ⭐⭐
**Guia de Uso - PRÁTICO**

Como executar e testar o projeto:
- Instalação
- Como rodar
- Exemplos de requisições HTTP
- Exemplos de respostas
- Exemplos de erros
- Testes avançados

**Como usar**: Execute e teste a API usando exemplos

---

### 4. [INICIO-RAPIDO.md](INICIO-RAPIDO.md) ⭐
**Quick Start - REFERÊNCIA**

Tudo quanto você precisa para começar em 5 minutos:
- Pre-requisitos
- 3 passos para execução
- FAQ
- Fluxo recomendado

**Como usar**: Comece aqui para ir direto ao ponto

---

## 🏗️ Estrutura do Código-Fonte

### 📄 Program.cs (Raiz)
**Arquivo principal de configuração**

Localização: `Program.cs`

Configurações:
- Logging com Serilog
- Entity Framework Core
- Dependency Injection
- CORS
- Swagger/OpenAPI
- Middleware
- Migrations
- Seeding de dados

---

### 📦 src/Models/ (1 arquivo)

#### [src/Models/Produto.cs](../src/Models/Produto.cs)
**Entidade principal do domínio**
- 11 propriedades
- Tipos adequados (int, string, decimal, bool, DateTime)
- Validação através de atributos
- Soft delete (Ativo property)
- Audit fields (DataCriacao, DataAtualizacao)
- XML comments com referências ao guia

---

### 📨 src/DTOs/ (1 arquivo - 8 classes)

#### [src/DTOs/ProdutoDTO.cs](../src/DTOs/ProdutoDTO.cs)
**Transfer Objects para dados de entrada/saída**

1. **CriarProdutoRequest** - Dados para POST
2. **AtualizarProdutoRequest** - Dados para PATCH (todos opcionais)
3. **ProdutoResponse** - Resposta de GET
4. **PaginatedResponse<T>** - Resposta paginada genérica
5. **PaginationInfo** - Informações de paginação
6. **ErrorResponse** - Erro padronizado
7. **AuthResponse** - Resposta de autenticação
8. **LoginRequest** - Requisição de login

---

### 🛣️ src/Endpoints/ (1 arquivo)

#### [src/Endpoints/ProdutoEndpoints.cs](../src/Endpoints/ProdutoEndpoints.cs)
**Mapeamento de rotas e handlers**

6 Endpoints RESTful:
1. `GET /api/v1/produtos` - Listar com paginação, filtros e busca
2. `GET /api/v1/produtos/{id}` - Obter produto específico
3. `POST /api/v1/produtos` - Criar novo produto
4. `PUT /api/v1/produtos/{id}` - Atualizar completamente (substitui todos)
5. `PATCH /api/v1/produtos/{id}` - Atualizar parcialmente
6. `DELETE /api/v1/produtos/{id}` - Deletar (soft delete)

Cada endpoint:
- Valida entrada
- Trata erros apropriadamente
- Retorna status codes corretos
- Tem descrição Swagger
- Referencia o guia conceitual

---

### 🔧 src/Services/ (1 arquivo - Interface + Implementação)

#### [src/Services/ProdutoService.cs](../src/Services/ProdutoService.cs)
**Lógica de negócio**

Interface: **IProdutoService**
```csharp
Task<PaginatedResponse<ProdutoResponse>> ListarProdutosAsync(...)
Task<ProdutoResponse?> ObterProdutoAsync(int id)
Task<ProdutoResponse> CriarProdutoAsync(CriarProdutoRequest request)
Task<ProdutoResponse?> AtualizarProdutoAsync(int id, AtualizarProdutoRequest request)
Task<ProdutoResponse?> AtualizarCompletoProdutoAsync(int id, CriarProdutoRequest request)
Task<bool> DeletarProdutoAsync(int id)
```

Implementação:
- Logging estruturado em cada operação
- Queries LINQ parametrizadas
- Validação de ranges
- Tratamento de exceções
- Mapeamento de DTOs
- Soft delete

---

### 💾 src/Data/ (3 arquivos)

#### [src/Data/AppDbContext.cs](../src/Data/AppDbContext.cs)
**Entity Framework Core DbContext**

Configurações:
- DbSet<Produto>
- Propriedades (max lengths, precision)
- Índices (Ativo, Categoria)
- Relacionamentos (pronto para expandir)
- Default values

#### [src/Data/DbSeeder.cs](../src/Data/DbSeeder.cs)
**Dados iniciais para testes**

8 produtos de exemplo:
- Notebook Dell XPS 13
- Mouse Logitech MX Master 3S
- Teclado Mecânico RGB
- Clean Code (livro)
- Design Patterns (livro)
- Camiseta técnica Azul
- Café Gourmet 500g
- Monitor LG UltraWide 34"

#### [src/Data/Migrations/](../src/Data/Migrations/)
**Entity Framework Migrations**

Arquivos:
- `20250225000000_CreateInitialSchema.cs` - Migration principal
- `AppDbContextModelSnapshot.cs` - Snapshot do modelo

Contém:
- Criação da tabela Produtos
- Índices
- Constraints

---

### ✅ src/Validators/ (1 arquivo - 3 validadores)

#### [src/Validators/ProdutoValidator.cs](../src/Validators/ProdutoValidator.cs)
**FluentValidation para business rules**

1. **CriarProdutoValidator**
   - Nome: obrigatório, 3-100 caracteres
   - Descrição: obrigatória, máx 500 caracteres
   - Preço: maior que 0
   - Categoria: lista pré-definida
   - Estoque: não negativo, máx 1M
   - Email: formato válido

2. **AtualizarProdutoValidator**
   - Todos os campos opcionais
   - Se fornecido, deve ser válido
   - Mesmas regras da criação

3. **LoginValidator**
   - Email obrigatório e válido
   - Senha obrigatória, mín 6 caracteres

---

### 🛡️ src/Middleware/ (1 arquivo)

#### [src/Middleware/ExceptionHandlingMiddleware.cs](../src/Middleware/ExceptionHandlingMiddleware.cs)
**Tratamento global de exceções**

Captura e trata:
- **ValidationException** → 422 Unprocessable Entity
- **KeyNotFoundException** → 404 Not Found
- **ArgumentException** → 400 Bad Request
- **UnauthorizedAccessException** → 401 Unauthorized
- **Exceções genéricas** → 500 Internal Server Error

Sempre retorna ErrorResponse padronizada com:
- Status code
- Título
- Descrição
- Tipo (URL)
- Instance (Path)
- Erros por campo (se validação)

---

### 🎯 src/Common/ (1 arquivo)

#### [src/Common/MappingProfile.cs](../src/Common/MappingProfile.cs)
**Configuração AutoMapper**

Mapeamentos:
- Produto → ProdutoResponse
- CriarProdutoRequest → Produto
- AtualizarProdutoRequest → Produto (ignorando nulos)

---

## ⚙️ Configuração

### [ProdutosAPI.csproj](../ProdutosAPI.csproj)
**Definição do projeto**

- Framework: .NET 10.0
- Nullable: enable
- Implicit usings: enable
- 11 dependências NuGet

Principais packages:
- Swashbuckle.AspNetCore (Swagger)
- FluentValidation (Validação)
- EntityFrameworkCore (ORM)
- Serilog (Logging)
- AutoMapper (Mapping)

### [appsettings.json](../appsettings.json)
**Configurações de runtime**

- Connection string SQLite
- Logging levels
- Serilog configuration

### [Properties/launchSettings.json](Properties/launchSettings.json)
**Configurações de execução**

- HTTP porta: 5000
- HTTPS porta: 5001
- Environment: Development

### [.gitignore](.gitignore)
**Arquivos ignorados pelo Git**

- Build outputs
- Visual Studio cache
- Rider configs
- OS files
- Arquivos .db
- node_modules

---

## 📋 Referência e Exemplos

### [ProdutosAPI.Tests/](../ProdutosAPI.Tests/)
**Exemplos de testes unitários**

Com comentários:
- Como criar projeto de teste xunit
- Padrão AAA (Arrange, Act, Assert)
- Uso de Moq para mocks
- FluentAssertions para verificações
- Helpers para async queries

Cobre:
- Testes de listagem
- Testes de criação
- Testes de atualização
- Testes de deleção
- Testes com filtros
- Testes de edge cases

---

## 📊 Arquivos de Check e Resumo

### [CHECKLIST.md](CHECKLIST.md)
**Checklist de verificação**

- ✅ Todos os arquivos criados
- ✅ Todas as práticas implementadas
- ✅ Qualidade do código
- ✅ Referências cruzadas
- ✅ Pronto para uso

### [SUMARIO.md](SUMARIO.md)
**Resumo completo do projeto**

- O que foi entregue
- Tecnologias utilizadas
- Arquitetura
- Prática por prática
- Estatísticas
- Como começar

---

## 🎓 Mapa Mental de Aprendizado

```
Melhores Práticas API REST
│
├─ Conceito Teórico
│  └─ MELHORES-PRATICAS-API.md ⭐
│
├─ Implementação Prática
│  └─ MELHORES-PRATICAS-MINIMAL-API.md ⭐
│     ├─ Links para cada arquivo
│     └─ Explicação de cada prática
│
├─ Execução e Teste
│  ├─ INICIO-RAPIDO.md (5 min)
│  └─ README.md (detalhado)
│
├─ Código-Fonte
│  ├─ Program.cs (orchestração)
│  ├─ src/Models/ (domínio)
│  ├─ src/DTOs/ (transferência)
│  ├─ src/Endpoints/ (rotas)
│  ├─ src/Services/ (lógica)
│  ├─ src/Data/ (persistência)
│  ├─ src/Validators/ (validação)
│  ├─ src/Middleware/ (transversal)
│  └─ src/Common/ (utilitários)
│
├─ Testes
│  ├─ ProdutosAPI.Tests/ (referência)
│  └─ Como criar projeto xunit
│
└─ Referência
   ├─ CHECKLIST.md (verificação)
   ├─ SUMARIO.md (resumo)
   └─ INDEX.md (este arquivo)
```

---

## 🚀 Próximos Passos Sugeridos

Após entender este projeto:

1. **Adicione um novo modelo** (ex: Pedidos)
   - Crie novo Model
   - Crie DTOs
   - Crie Validators
   - Crie Service
   - Mapeie Endpoints

2. **Implemente autenticação JWT**
   - Veja referência em [MELHORES-PRATICAS-API.md](MELHORES-PRATICAS-API.md#autenticação)
   - Use `System.IdentityModel.Tokens.Jwt`
   - Implemente AuthService

3. **Adicione testes**
   - Crie projeto `dotnet new xunit --name ProdutosAPI.Tests`
   - Use [ProdutosAPI.Tests/](../ProdutosAPI.Tests/) como referência
   - Rode com `dotnet test`

4. **Configure CI/CD**
   - GitHub Actions ou Azure DevOps
   - Build automático
   - Testes automáticos
   - Deploy automático

5. **Containerize**
   - Crie `Dockerfile`
   - Crie `docker-compose.yml`
   - Deploy em container

---

## 📞 Referências Rápidas

| Conceito | Arquivo | Seção |
|----------|---------|-------|
| HTTP Status Codes | [MELHORES-PRATICAS-MINIMAL-API.md](MELHORES-PRATICAS-MINIMAL-API.md#-http-status-codes-corretos) | Implementação |
| Paginação | [src/Services/ProdutoService.cs](../src/Services/ProdutoService.cs#L32-L75) | ListarProdutosAsync |
| Validação | [src/Validators/ProdutoValidator.cs](../src/Validators/ProdutoValidator.cs) | CriarProdutoValidator |
| Mapeamento | [src/Common/MappingProfile.cs](../src/Common/MappingProfile.cs) | AutoMapper config |
| Logging | [Program.cs](Program.cs#L17-L33) | Serilog setup |
| Endpoints | [src/Endpoints/ProdutoEndpoints.cs](../src/Endpoints/ProdutoEndpoints.cs#L29-L60) | Map methods |
| Errors | [src/Middleware/ExceptionHandlingMiddleware.cs](../src/Middleware/ExceptionHandlingMiddleware.cs#L35-L75) | Error handling |
| EF Core | [src/Data/AppDbContext.cs](../src/Data/AppDbContext.cs) | DbContext config |

---

## ✨ Destaques

✅ **Completo** - Todos os aspectos de uma API moderna  
✅ **Didático** - Comentários e referências explicam tudo  
✅ **Executável** - Pronto para rodar em 5 minutos  
✅ **Estruturado** - Padrões de mercado  
✅ **Documentado** - 4 guias complementares  
✅ **Moderno** - .NET 10 com Minimal API  
✅ **Escalável** - Fácil adicionar novos features  

---

**Data**: 25 de Fevereiro de 2025  
**Versão**: 1.0.0  
**Framework**: .NET 10 LTS  
**Padrão**: Minimal API + REST  

🎉 **Tudo pronto para começar a aprender e codificar!**
