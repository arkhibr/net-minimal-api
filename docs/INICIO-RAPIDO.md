# Início Rápido - Produtos API

## 🎯 Objetivo Didático

Este projeto é um **exemplo educacional completo** que demonstra como implementar uma API REST moderna seguindo as melhores práticas em .NET 10 com Minimal API. Inclui dois casos de uso: Produtos (Clean Architecture) e Pedidos (Vertical Slice + Domínio Rico).
## 📚 Três Documentos Principais

### 1️⃣ Guia Conceitual
**Arquivo**: [MELHORES-PRATICAS-API.md](MELHORES-PRATICAS-API.md)

Este é o guia **teórico** que explica as melhores práticas gerais para qualquer API REST:

- ✅ Princípios RESTful
- ✅ Design de endpoints
- ✅ HTTP status codes
- ✅ Versionamento
- ✅ Segurança
- ✅ Validação
- ✅ Tratamento de erros
- ✅ Documentação
- ✅ Performance
- ✅ Logging
- ✅ Testes

**Como usar**: Comece aqui para entender os conceitos. Cada seção tem um número que referencia a prática no código.

---

### 2️⃣ Implementação Específica
**Arquivo**: [MELHORES-PRATICAS-MINIMAL-API.md](MELHORES-PRATICAS-MINIMAL-API.md)

Este guide **prático** explica exatamente como cada conceito foi implementado neste projeto específico:

- ✅ Links para cada arquivo do projeto
- ✅ Trechos de código comentados
- ✅ Referências cruzadas com o guia conceitual
- ✅ Explicação de cada decisão arquitetural

**Como usar**: Use este arquivo para ver **como** e **onde** cada prática foi implementada. Inclui links diretos aos arquivos.

---

### 3️⃣ Instruções de Execução
**Arquivo**: [README.md](../README.md)

Este arquivo contém:

- ✅ Como instalar dependências
- ✅ Como executar a aplicação
- ✅ Exemplos de requisições HTTP
- ✅ Exemplos de respostas esperadas
- ✅ Exemplos de tratamento de erros

**Como usar**: Após entender os conceitos, use este guia para executar e testar a API.

---

## 🚀 Começar Agora (5 minutos)
*Observação: alguns endpoints de Pedidos exigem autenticação JWT. Primeiro crie um token usando `/api/v1/auth/login` (credenciais dummy) e em seguida inclua `-H "Authorization: Bearer <token>"` nos curl abaixo.*
### Pré-requisito
Instale [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Passo 1: Restaurar Dependências

```bash
cd net-minimal-api
dotnet restore
```

### Passo 2: Executar

```bash
dotnet run
```

### Passo 3: Abrir Swagger UI

```
http://localhost:5000
```

**Pronto!** A API está rodando com documentação interativa.

---

## 📖 Fluxo de Aprendizado Recomendado

```
1. Ler MELHORES-PRATICAS-API.md (30 min)
   └─ Entender os conceitos gerais

2. Explorar MELHORES-PRATICAS-MINIMAL-API.md (30 min)
   └─ Ver como cada prática foi implementada
   └─ Seguir os links para os arquivos de código

3. Executar a API (5 min)
   └─ dotnet run
   └─ Acessar http://localhost:5000

4. Testar Endpoints (15 min)
   └─ Usar exemplos do README.md
   └─ Observar respostas e status codes

5. Explorar Código (1-2 horas)
   └─ Abrir cada arquivo mencionado
   └─ Ler comentários XML referenciando o guia
   └─ Entender a arquitetura
```

---

## 🗂️ Estrutura Pronta

O projeto também possui:

```
✅ src/Pedidos/        → Vertical slices com Command/Validator/Endpoint
✅ src/Pedidos/Domain/ → Agregado rico (Pedido, PedidoItem, StatusPedido)
```
O projeto já inclui:

```
✅ Produtos.Domain          → Entidades e regras de domínio
✅ Produtos.Application     → DTOs, serviços, validadores e mappings
✅ Produtos.API             → Endpoints e rotas
✅ Produtos.Infrastructure  → Repositórios EF e seeding
✅ Shared/Data/AppDbContext → Banco de dados com EF Core
✅ Shared/Middleware        → Tratamento global de erros
✅ Logging                  → Estruturado com Serilog
✅ Documentação             → Swagger/OpenAPI
```

---

## 🎓 Conceitos Demonstrados

| Conceito | Onde Encontrar |
|----------|----------------|
| **RESTful Design** | [Endpoints](../src/Produtos/Produtos.API/Endpoints/ProdutoEndpoints.cs) |
| **HTTP Verbs** | [Endpoints](../src/Produtos/Produtos.API/Endpoints/ProdutoEndpoints.cs#L29-L60) |
| **Paginação** | [Services](../src/Produtos/Produtos.Application/Services/ProdutoService.cs#L32-L75) |
| **Validação** | [Validators](../src/Produtos/Produtos.Application/Validators/ProdutoValidator.cs) |
| **Tratamento de Erros** | [Middleware](../src/Shared/Middleware/ExceptionHandlingMiddleware.cs) |
| **Logging** | [Services](../src/Produtos/Produtos.Application/Services/ProdutoService.cs#L34) |
| **DTOs** | [DTOs](../src/Produtos/Produtos.Application/DTOs/ProdutoDTO.cs) |
| **Injeção de Dependência** | [Program.cs](Program.cs#L36-L45) |
| **Entity Framework** | [Data](../src/Shared/Data/AppDbContext.cs) |
| **Swagger/OpenAPI** | [Program.cs](Program.cs#L80-L100) |

---

## 🧪 Testar a API
Além dos exemplos a seguir, existem também endpoints de Pedidos:
```
POST /api/v1/pedidos
GET /api/v1/pedidos/{id}
PATCH /api/v1/pedidos/{id}/cancelar
POST /api/v1/pedidos/{id}/itens
GET /api/v1/pedidos
```

Use JWT conforme descrito anteriormente.
### Exemplo 1: Listar Produtos

```bash
curl http://localhost:5000/api/v1/produtos
```

### Exemplo 2: Criar Produto

```bash
curl -X POST http://localhost:5000/api/v1/produtos \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Notebook",
    "descricao": "Notebook de alta performance",
    "preco": 3000,
    "categoria": "Eletrônicos",
    "estoque": 10,
    "contatoEmail": "vendas@example.com"
  }'
```

### Exemplo 3: Validação Falha

```bash
curl -X POST http://localhost:5000/api/v1/produtos \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "A",
    "preco": -100
  }'
```

Observe a resposta com erros de validação (422 Unprocessable Entity).

---

## 💡 Dicas de Estudo

1. **Leia o Guia De Conceitos Primeiro**
   - Entender a teoria antes de explorar código

2. **Ao Explorar Código, Procure por Referências**
   - Cada arquivo tem comentários como `// Referência: MELHORES-PRATICAS-API.md - Seção "X"`
   - Isso conecta código à teoria

3. **Use Swagger UI para Entender Endpoints**
   - A documentação interativa mostra exatamente o que cada endpoint faz

4. **Teste com cURL ou Postman**
   - Veja respostas reais
   - Entenda status codes
   - Teste validações

5. **Explore Logs**
   - A aplicação gera logs estruturados em `logs/`
   - Veja o que está acontecendo internamente

---

## ❓ Perguntas Frequentes

### P: Por que Minimal API?
**R**: É a forma mais moderna e simples de criar APIs em .NET. Perfeita para microserviços e educação.

### P: Por que SQLite?
**R**: Serve para demonstração. Pode facilmente ser trocado por SQL Server, PostgreSQL, etc.

### P: Preciso instalar um servidor de banco de dados?
**R**: Não! SQLite funciona com um arquivo local (`.db`).

### P: Como adicionar autenticação JWT?
**R**: Veja a seção de Segurança em [MELHORES-PRATICAS-API.md](MELHORES-PRATICAS-API.md#autenticação). A estrutura já está preparada.

### P: Como adicionar testes?
**R**: Crie um novo projeto `dotnet new xunit --name ProdutosAPI.Tests` e siga o padrão do código existente.

---

## 📞 Próximos Passos

Após entender este projeto:

1. **Modificar o código** - Altere validações, adicione novos campos
2. **Criar novo endpoint** - Adicione uma entidade diferente
3. **Adicionar autenticação** - Implemente JWT tokens
4. **Implementar testes** - Crie testes unitários e de integração
5. **Containerizar** - Crie um Dockerfile

---

## 📄 Referência Rápida

| Arquivo | Propósito |
|---------|-----------|
| `Program.cs` | Configuração central da aplicação |
| `src/Produtos/Produtos.Domain/Produto.cs` | Entidade do domínio |
| `src/Produtos/Produtos.Application/DTOs/ProdutoDTO.cs` | Transfer objects |
| `src/Produtos/Produtos.API/Endpoints/ProdutoEndpoints.cs` | Mapeamento de rotas |
| `src/Produtos/Produtos.Application/Services/ProdutoService.cs` | Lógica de negócio |
| `src/Produtos/Produtos.Application/Validators/ProdutoValidator.cs` | Validações |
| `src/Shared/Middleware/ExceptionHandlingMiddleware.cs` | Tratamento de erros |
| `appsettings.json` | Configurações de ambiente |

---

**Versão**: 1.0.0  
**Última atualização**: 25 de Fevereiro de 2025  
**Framework**: .NET 10 LTS  
**Padrão**: Minimal API
