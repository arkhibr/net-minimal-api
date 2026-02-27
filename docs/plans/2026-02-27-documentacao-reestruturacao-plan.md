# Reestruturação da Documentação — Plano de Implementação

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development to implement this plan task-by-task.

**Goal:** Reestruturar toda a documentação do projeto para refletir os dois casos de uso (Produtos com camadas horizontais, Pedidos com Vertical Slice) e criar um novo guia conceitual sobre Vertical Slice Architecture e Domínio Rico.

**Architecture:** O projeto hoje tem dois padrões arquiteturais coexistindo: horizontal layers (Produtos) e Vertical Slice + Rich Domain (Pedidos). A documentação atual só cobre o primeiro. Este plano atualiza todos os docs existentes e cria um novo guia conceitual (`VERTICAL-SLICE-DOMINIO-RICO.md`).

**Tech Stack:** Markdown. Todos os arquivos são documentação pura — sem código a compilar. Verificação = `grep` para confirmar presença de seções-chave + `dotnet test` para confirmar que os snippets de código no doc correspondem ao que o projeto faz.

**Contagem de testes atual:** 121 testes passando (verificar com `dotnet test --list-tests 2>/dev/null | grep -v "^Build\|^Test run\|^$" | wc -l`).

**Caminhos de referência importantes:**
- Design doc: `docs/plans/2026-02-27-documentacao-reestruturacao-design.md`
- Domínio de Pedidos: `src/Features/Pedidos/Domain/`
- Slices: `src/Features/Pedidos/CreatePedido/`, `GetPedido/`, `ListPedidos/`, `AddItemPedido/`, `CancelPedido/`
- Common: `src/Features/Common/Result.cs`, `IEndpoint.cs`, `EndpointExtensions.cs`
- Modelo Rico: `src/Models/Produto.cs`
- Testes de domínio: `ProdutosAPI.Tests/Unit/Domain/`
- Testes de integração: `ProdutosAPI.Tests/Integration/`

---

### Task 1: Atualizar README.md

**Files:**
- Modify: `README.md`

Esta é a porta de entrada do projeto. Precisa refletir que o projeto demonstra dois padrões arquiteturais e tem 121 testes.

**Step 1: Ler o arquivo atual**

```bash
cat README.md
```

**Step 2: Reescrever o README.md com as seguintes mudanças obrigatórias**

Mudanças específicas a fazer (não alterar o que não está listado):

1. **Badge de versão**: `2.0.0` → `3.0.0`

2. **Seção "Sobre o Projeto"** — substituir o parágrafo atual por:
```markdown
**ProdutosAPI** é um projeto educacional demonstrando melhores práticas de APIs REST com **.NET 10 LTS** e **Minimal API**. O projeto cobre dois padrões arquiteturais complementares, implementados como casos de uso reais com cobertura completa de testes (121 testes).
```

3. **Seção "Principais Recursos" → "6 Endpoints REST"** — mudar título para **"11 Endpoints REST (2 casos de uso)"** e adicionar tabela de Pedidos logo abaixo da tabela de Produtos:

```markdown
### Pedidos (Vertical Slice + JWT obrigatório)

| Método | Rota | Descrição | Status |
|--------|------|-----------|---------|
| `POST` | `/api/v1/pedidos` | Criar pedido | 201/400 |
| `GET` | `/api/v1/pedidos/{id}` | Obter pedido | 200/404 |
| `GET` | `/api/v1/pedidos` | Listar pedidos | 200 |
| `POST` | `/api/v1/pedidos/{id}/itens` | Adicionar item | 200/400/404 |
| `POST` | `/api/v1/pedidos/{id}/cancelar` | Cancelar pedido | 200/400/404 |
```

4. **Contagem de testes**: `50+` → `121`

5. **Estrutura do projeto** — atualizar a árvore `src/` para incluir:
```
│   ├── Features/                            # Vertical Slice Architecture
│   │   ├── Common/
│   │   │   ├── IEndpoint.cs               # Interface de registro automático
│   │   │   ├── EndpointExtensions.cs      # Scanner de endpoints
│   │   │   └── Result.cs                  # Result pattern
│   │   └── Pedidos/
│   │       ├── Domain/                    # Aggregate root + entities
│   │       ├── Common/                    # DTOs dos slices
│   │       ├── CreatePedido/              # Slice POST /pedidos
│   │       ├── GetPedido/                 # Slice GET /pedidos/{id}
│   │       ├── ListPedidos/               # Slice GET /pedidos
│   │       ├── AddItemPedido/             # Slice POST /pedidos/{id}/itens
│   │       └── CancelPedido/              # Slice POST /pedidos/{id}/cancelar
```

E a árvore `ProdutosAPI.Tests/` para incluir:
```
│   ├── Unit/Domain/
│   │   ├── ProdutoTests.cs                # 18 testes de domínio rico
│   │   └── PedidoTests.cs                 # 16 testes do aggregate
│   ├── Builders/
│   │   └── ProdutoBuilder.cs              # Builder fluente para testes
│   └── Integration/
│       ├── ApiFactory.cs                  # WebApplicationFactory
│       ├── AuthHelper.cs                  # JWT helper
│       ├── CreatePedidoTests.cs
│       ├── GetPedidoTests.cs
│       ├── CancelPedidoTests.cs
│       ├── AddItemPedidoTests.cs
│       └── ListPedidosTests.cs
```

6. **Seção "Objetivo de Aprendizado"** — adicionar ao final da lista:
```markdown
✅ Vertical Slice Architecture
✅ Domínio Rico e Aggregate Root
✅ Result Pattern
✅ Testes de domínio e integração HTTP
```

7. **Adicionar exemplos de Pedidos** — nova seção após os exemplos de Produtos:

```markdown
### Autenticação (necessária para Pedidos)
```bash
# Obter token JWT
curl -X POST "http://localhost:5000/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@example.com", "senha": "senha123"}'
# Copie o "token" da resposta
```

```markdown
### Criar Pedido
```bash
curl -X POST "http://localhost:5000/api/v1/pedidos" \
  -H "Authorization: Bearer SEU_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{}'
```

```markdown
### Adicionar Item ao Pedido
```bash
curl -X POST "http://localhost:5000/api/v1/pedidos/1/itens" \
  -H "Authorization: Bearer SEU_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"produtoId": 1, "quantidade": 2}'
```

**Step 3: Verificar**

```bash
grep -c "Pedidos\|pedidos" README.md
# Esperado: >= 10 ocorrências

grep "121" README.md
# Esperado: linha com contagem de testes

grep "3.0.0" README.md
# Esperado: badge de versão
```

**Step 4: Commit**

```bash
git add README.md
git commit -m "docs: atualizar README com Pedidos, Vertical Slice e 121 testes"
```

---

### Task 2: Reescrever 00-LEIA-PRIMEIRO.md

**Files:**
- Modify: `docs/00-LEIA-PRIMEIRO.md`

O arquivo atual é uma listagem exaustiva de arquivos. Deve virar uma introdução narrativa que apresenta os dois casos de uso e aponta para as duas trilhas.

**Step 1: Escrever o novo conteúdo**

Substituir o arquivo inteiro por:

```markdown
# Bem-vindo ao ProdutosAPI

Este é um projeto educacional em **.NET 10 Minimal API** com dois casos de uso reais, cada um demonstrando um padrão arquitetural diferente.

---

## O Que Este Projeto Demonstra

### Caso 1 — Produtos (Camadas Horizontais)
A gestão de Produtos segue a arquitetura em camadas clássica: Endpoints → Services → Data. O modelo de domínio é **rico**: a classe `Produto` encapsula suas próprias regras de negócio com factory method e métodos comportamentais — sem setters públicos.

**Aprenda com Produtos:**
- REST API design com Minimal API
- FluentValidation, AutoMapper, Serilog
- EF Core com SQLite
- Domínio rico vs anêmico
- Testes unitários de serviço

### Caso 2 — Pedidos (Vertical Slice)
A gestão de Pedidos usa **Vertical Slice Architecture**: cada operação (criar, buscar, adicionar item, cancelar) vive em sua própria pasta com Command/Handler/Validator/Endpoint. O aggregate `Pedido` encapsula regras de negócio complexas (merge de itens, valor mínimo, cancelamento).

**Aprenda com Pedidos:**
- Vertical Slice Architecture
- Aggregate Root e Domain-Driven Design
- Result Pattern (erros sem exceptions)
- Registro automático de endpoints via `IEndpoint`
- Testes de integração HTTP com WebApplicationFactory

---

## Como Começar

### Rápido (5 min)
```bash
dotnet run
# Abra: http://localhost:5000
```

### Aprender (escolha sua trilha)
→ Veja [INDEX.md](INDEX.md) para as duas trilhas de aprendizado

### Referências
- [ARQUITETURA.md](ARQUITETURA.md) — diagramas dos dois padrões lado a lado
- [ESTRATEGIA-DE-TESTES.md](../ProdutosAPI.Tests/ESTRATEGIA-DE-TESTES.md) — 121 testes em 3 categorias
```

**Step 2: Verificar**

```bash
grep "Vertical Slice\|Pedidos\|Produtos\|trilha" docs/00-LEIA-PRIMEIRO.md | wc -l
# Esperado: >= 8
```

**Step 3: Commit**

```bash
git add docs/00-LEIA-PRIMEIRO.md
git commit -m "docs: reescrever 00-LEIA-PRIMEIRO com narrativa dos dois padrões"
```

---

### Task 3: Reescrever INDEX.md

**Files:**
- Modify: `docs/INDEX.md`

O INDEX atual tem um único caminho linear e ainda sugere "adicione Pedidos" como próximo passo. Deve ser reescrito com duas trilhas e o mapa mental atualizado.

**Step 1: Escrever o novo INDEX.md**

Estrutura obrigatória:

```markdown
# Índice Completo do Projeto

## Por Onde Começar?

### ⚡ Rápido (5 minutos)
1. Execute: `dotnet run`
2. Abra: http://localhost:5000
3. Explore o Swagger UI — veja os grupos `/api/v1/produtos` e `/api/v1/pedidos`

---

## Duas Trilhas de Aprendizado

### Trilha 1 — REST + Camadas Horizontais (Produtos)
Para quem quer aprender fundamentos de REST API com .NET 10:

1. [MELHORES-PRATICAS-API.md](MELHORES-PRATICAS-API.md) — teoria REST (30min)
2. [MELHORES-PRATICAS-MINIMAL-API.md](MELHORES-PRATICAS-MINIMAL-API.md) → seção Produtos (30min)
3. Código: `src/Endpoints/`, `src/Services/`, `src/Models/Produto.cs`
4. Testes: `ProdutosAPI.Tests/Services/` e `ProdutosAPI.Tests/Validators/`

### Trilha 2 — Vertical Slice + Domínio Rico (Pedidos)
Para quem quer ir além e estudar padrões avançados:

1. [VERTICAL-SLICE-DOMINIO-RICO.md](VERTICAL-SLICE-DOMINIO-RICO.md) — teoria (30min)
2. [MELHORES-PRATICAS-MINIMAL-API.md](MELHORES-PRATICAS-MINIMAL-API.md) → seção Pedidos (20min)
3. Código: `src/Features/Pedidos/`
4. Testes: `ProdutosAPI.Tests/Unit/Domain/` e `ProdutosAPI.Tests/Integration/`

### Profundo (completo)
Ambas as trilhas → [ARQUITETURA.md](ARQUITETURA.md) → [ESTRATEGIA-DE-TESTES.md](../ProdutosAPI.Tests/ESTRATEGIA-DE-TESTES.md)

---

## Documentação

[tabela com todos os docs e seus papéis — incluindo VERTICAL-SLICE-DOMINIO-RICO.md como novo guia]

---

## Estrutura do Código

[árvore completa de src/ incluindo src/Features/Pedidos/]

---

## Mapa Mental de Aprendizado

[diagrama ASCII atualizado com Features/ e os dois caminhos]

---

## Referências Rápidas

[tabela atualizada incluindo referências de Pedidos]
```

Pontos obrigatórios:
- Remover toda a seção "Próximos Passos Sugeridos" que sugere adicionar Pedidos (já existe)
- Incluir `VERTICAL-SLICE-DOMINIO-RICO.md` como um dos guias conceituais
- Mapa mental deve incluir `src/Features/` como ramo separado de `src/`
- Data no rodapé: `27 de Fevereiro de 2026` / Versão: `3.0.0`

**Step 2: Verificar**

```bash
grep "Trilha\|Pedidos\|Vertical Slice\|VERTICAL-SLICE" docs/INDEX.md | wc -l
# Esperado: >= 8

grep "Adicione um novo modelo\|próximo passo.*Pedido" docs/INDEX.md
# Esperado: nenhuma saída (seção removida)
```

**Step 3: Commit**

```bash
git add docs/INDEX.md
git commit -m "docs: reescrever INDEX com duas trilhas de aprendizado"
```

---

### Task 4: Reescrever ARQUITETURA.md

**Files:**
- Modify: `docs/ARQUITETURA.md`

O arquivo atual só mostra a arquitetura em camadas dos Produtos. Precisa mostrar os dois padrões lado a lado e o modelo de dados completo.

**Step 1: Ler os arquivos de referência**

```bash
cat src/Features/Common/IEndpoint.cs
cat src/Features/Common/EndpointExtensions.cs
cat src/Features/Pedidos/CreatePedido/CreatePedidoEndpoint.cs
```

**Step 2: Estrutura do novo ARQUITETURA.md**

Seções obrigatórias:

**Seção 1 — Coexistência dos Dois Padrões**
Diagrama ASCII side-by-side mostrando:
```
Produtos (Horizontal Layers)     │  Pedidos (Vertical Slice)
─────────────────────────────── │  ──────────────────────────
ProdutoEndpoints.cs              │  Features/Pedidos/
  └─ chama IProdutoService        │    CreatePedido/
       └─ usa AppDbContext         │      ├─ CreatePedidoCommand.cs
                                   │      ├─ CreatePedidoValidator.cs
                                   │      └─ CreatePedidoEndpoint.cs
                                   │           └─ usa AppDbContext
```
Ambos compartilham: mesmo `AppDbContext`, mesma pipeline JWT, mesmo middleware de exceções.

**Seção 2 — Arquitetura em Camadas (Produtos)**
Manter o diagrama vertical existente, atualizado para refletir JWT ativo (não "preparado").

**Seção 3 — Vertical Slice (Pedidos)**
Novo diagrama mostrando anatomia de um slice:
```
HTTP Request
   ↓
[CreatePedidoEndpoint]   ← só roteamento
   ↓
[CreatePedidoValidator]  ← FluentValidation do DTO
   ↓
[CreatePedidoCommand]    ← lógica de aplicação (Handler inline)
   ↓
[Pedido.AdicionarItem()] ← regras no domínio, retorna Result<T>
   ↓
[AppDbContext]           ← persistência
```

**Seção 4 — Modelo de Domínio Rico**
Comparação Produto (antes anêmico → agora rico):
- Antes: `public string Nome { get; set; }` — setter público, sem comportamento
- Depois: `public string Nome { get; private set; }` + `static Result<Produto> Criar(...)`

**Seção 5 — Data Model**
Tabela de Produtos (existente) + novas tabelas:
```
Pedidos               PedidoItens
──────────────        ──────────────────
Id (PK)               Id (PK)
Status (TEXT)         PedidoId (FK → Pedidos)
Total (DECIMAL)       ProdutoId (INT)
CriadoEm             NomeProduto (TEXT snapshot)
ConfirmadoEm         PrecoUnitario (DECIMAL snapshot)
CanceladoEm          Quantidade (INT)
MotivoCancelamento
```

**Seção 6 — DI Container**
Atualizar para incluir:
```
├─ IEndpoint scan (AddEndpointsFromAssembly)
│  ├─ CreatePedidoEndpoint
│  ├─ GetPedidoEndpoint
│  ├─ ListPedidosEndpoint
│  ├─ AddItemEndpoint
│  └─ CancelPedidoEndpoint
```

**Step 3: Verificar**

```bash
grep "Coexistência\|Vertical Slice\|PedidoItens\|AddEndpointsFromAssembly" docs/ARQUITETURA.md | wc -l
# Esperado: >= 4
```

**Step 4: Commit**

```bash
git add docs/ARQUITETURA.md
git commit -m "docs: reescrever ARQUITETURA com diagrama duplo e modelo de dados de Pedidos"
```

---

### Task 5: Reescrever ESTRATEGIA-DE-TESTES.md

**Files:**
- Modify: `ProdutosAPI.Tests/ESTRATEGIA-DE-TESTES.md`

O arquivo atual descreve apenas testes de Produto com mocks e ainda lista "Integration Tests com WebApplicationFactory" como passo futuro — quando esses testes já existem e funcionam. Precisa de reescrita completa.

**Step 1: Verificar a estrutura real de testes**

```bash
find ProdutosAPI.Tests -name "*.cs" | grep -v "\.csproj" | sort
dotnet test --list-tests 2>/dev/null | grep -v "^Build\|^Test run\|^$" | sort
```

**Step 2: Escrever novo ESTRATEGIA-DE-TESTES.md**

Estrutura obrigatória:

```markdown
# Estratégia de Testes — ProdutosAPI

## Sumário

**121 testes** organizados em 3 categorias. Execute todos com: `dotnet test`

---

## Categoria 1 — Testes de Domínio (Unit)

Sem infraestrutura, sem banco, sem HTTP. Testam as regras de negócio puras.

### ProdutoTests.cs (18 testes)
Testa o modelo rico de Produto:
- `Criar` — validações do factory method
- `AtualizarPreco` — valor > 0, diferente do atual
- `ReporEstoque` — positivo, máximo 99.999
- `Desativar` — não pode desativar já inativo
- `TemEstoqueDisponivel` — combina Ativo + Estoque

### PedidoTests.cs (16 testes)
Testa o aggregate root Pedido:
- `AdicionarItem` — só Rascunho, qtd 1-999, merge de itens, limite 20
- `Confirmar` — requer itens, valor mínimo R$ 10
- `Cancelar` — motivo obrigatório, não re-cancelar

Padrão usado: sem mocks, sem banco — objetos de domínio puro.

### ProdutoBuilder.cs
Builder fluente para criar `Produto` sem boilerplate em testes:
[exemplo de uso]

---

## Categoria 2 — Testes de Serviço (Unit com mocks)

Testam `ProdutoService` com AppDbContext e AutoMapper mockados.

### ProdutoServiceTests.cs (~16 testes)
[descrição dos cenários]

### ProdutoValidatorTests.cs (~20 testes)
[descrição dos cenários]

---

## Categoria 3 — Testes de Integração HTTP

Ponta a ponta: HTTP real, banco SQLite em memória. Sem mocks.

### ApiFactory.cs
`WebApplicationFactory<Program>` que:
- Substitui SQLite por InMemory para isolamento
- Faz seed de dados via `CreateHost` override
- Configurada via variável de ambiente `ASPNETCORE_ENVIRONMENT=Testing`

### AuthHelper.cs
Gera token JWT válido para os testes autenticados.
Credenciais: `admin@example.com` / `senha123`

### Testes de Integração (13 testes)
| Arquivo | Testes | Cobertura |
|---|---|---|
| CreatePedidoTests | 4 | POST /pedidos |
| GetPedidoTests | 2 | GET /pedidos/{id} |
| CancelPedidoTests | 3 | POST /pedidos/{id}/cancelar |
| AddItemPedidoTests | 2 | POST /pedidos/{id}/itens |
| ListPedidosTests | 2 | GET /pedidos |

---

## Como Executar

[comandos dotnet test com filtros por namespace/categoria]

---

## Convenções

[padrão MethodName_Scenario_ExpectedResult]
```

Pontos obrigatórios:
- Remover a seção "Próximos Passos" que lista WebApplicationFactory como futuro
- Refletir 121 testes no total
- Incluir exemplos do ProdutoBuilder e ApiFactory

**Step 3: Verificar**

```bash
grep "WebApplicationFactory.*futuro\|próximo.*WebApplication\|Integration Tests com WebApplication" \
  ProdutosAPI.Tests/ESTRATEGIA-DE-TESTES.md
# Esperado: nenhuma saída

grep "121\|ApiFactory\|ProdutoBuilder" ProdutosAPI.Tests/ESTRATEGIA-DE-TESTES.md | wc -l
# Esperado: >= 3
```

**Step 4: Commit**

```bash
git add ProdutosAPI.Tests/ESTRATEGIA-DE-TESTES.md
git commit -m "docs: reescrever ESTRATEGIA-DE-TESTES com 3 categorias reais e 121 testes"
```

---

### Task 6: Criar VERTICAL-SLICE-DOMINIO-RICO.md (novo guia conceitual)

**Files:**
- Create: `docs/VERTICAL-SLICE-DOMINIO-RICO.md`

Este é o maior entregável. Um guia conceitual educacional sobre Vertical Slice Architecture e Domínio Rico, usando o próprio projeto como referência. Similar em propósito ao `MELHORES-PRATICAS-API.md`, mas cobrindo padrões arquiteturais avançados.

**Step 1: Ler os arquivos de referência do projeto**

```bash
cat src/Features/Common/Result.cs
cat src/Features/Common/IEndpoint.cs
cat src/Features/Common/EndpointExtensions.cs
cat src/Features/Pedidos/Domain/Pedido.cs
cat src/Features/Pedidos/Domain/PedidoItem.cs
cat src/Features/Pedidos/CreatePedido/CreatePedidoCommand.cs
cat src/Features/Pedidos/CreatePedido/CreatePedidoValidator.cs
cat src/Features/Pedidos/CreatePedido/CreatePedidoEndpoint.cs
cat src/Models/Produto.cs
```

**Step 2: Escrever o guia com as seguintes seções obrigatórias**

**Seção 1 — O Problema com Camadas Horizontais**

Mostre que uma nova feature (ex: "criar pedido") requer tocar 5 arquivos em 5 pastas diferentes:
```
Nova feature "Criar Pedido":
  src/Models/Pedido.cs          ← novo modelo
  src/DTOs/PedidoDTO.cs         ← novo DTO
  src/Validators/...            ← novo validador
  src/Services/PedidoService.cs ← nova lógica
  src/Endpoints/PedidoEndpoints ← novo endpoint
```
O custo: coupling entre camadas, difícil de encontrar tudo de uma feature.

**Seção 2 — Vertical Slice Architecture**

Conceito: organizar por feature, não por camada.
```
src/Features/Pedidos/
  CreatePedido/   ← tudo de "criar pedido" em um lugar
  GetPedido/      ← tudo de "buscar pedido"
  ...
```

Anatomia de um slice (usando CreatePedido como exemplo real do projeto):
- **Command**: DTO de entrada + Handler com lógica de aplicação
- **Validator**: FluentValidation do DTO de entrada
- **Endpoint**: só roteamento HTTP, sem lógica

Mostrar o código real de `CreatePedidoEndpoint.cs`, `CreatePedidoCommand.cs` e `CreatePedidoValidator.cs` com anotações explicando cada responsabilidade.

**Registro automático com `IEndpoint`:**
Mostrar a interface e o `AddEndpointsFromAssembly` — nenhum slice precisa ser registrado manualmente em `Program.cs`.

**Seção 3 — Modelo de Domínio Anêmico vs Rico**

Comparação direta usando código do projeto:

```csharp
// ❌ Anêmico — Produto antes da refatoração
public class Produto
{
    public string Nome { get; set; } = "";
    public decimal Preco { get; set; }
    // sem validação, sem comportamento
}
// Quem garante que Preco > 0? O Service. E se alguém setar diretamente?
```

```csharp
// ✅ Rico — Produto atual (src/Models/Produto.cs)
public class Produto
{
    public string Nome { get; private set; } = "";
    public decimal Preco { get; private set; }

    public static Result<Produto> Criar(string nome, decimal preco, ...)
    {
        if (preco <= 0) return Result<Produto>.Fail("Preço deve ser maior que zero");
        // ...
    }

    public Result AtualizarPreco(decimal novoPreco)
    {
        if (novoPreco <= 0) return Result.Fail("Preço inválido");
        if (novoPreco == Preco) return Result.Fail("Preço já é esse valor");
        Preco = novoPreco;
        return Result.Ok();
    }
}
```
Mostrar o código real de `src/Models/Produto.cs` (métodos principais).

**Seção 4 — Result Pattern**

Por que não usar exceptions para erros de domínio:
- Exceptions são caras (stack trace)
- Exceptions como controle de fluxo são "code smell"
- Result torna o contrato explícito

Mostrar o código real de `src/Features/Common/Result.cs`.

Como usar no endpoint:
```csharp
var result = pedido.AdicionarItem(produto, request.Quantidade);
if (!result.IsSuccess)
    return TypedResults.BadRequest(new ErrorResponse { Detail = result.Error });
```

**Seção 5 — Aggregate Root com Pedido**

O que é um aggregate: fronteira de consistência. Tudo que pertence ao `Pedido` muda junto.

Regras encapsuladas no `Pedido` (listar com código real):
- `AdicionarItem`: só em Rascunho, merge de quantidade, limite 20 itens
- `Confirmar`: precisa de itens e valor >= R$ 10
- `Cancelar`: motivo obrigatório, pedido já cancelado não pode cancelar de novo

`PedidoItem` como entidade filha: preço e nome são **snapshots** do momento do pedido (o produto pode mudar de preço depois).

**Seção 6 — Coexistência no Mesmo Projeto**

```
Produtos (Horizontal)         Pedidos (Vertical Slice)
─────────────────────         ──────────────────────────
Bom para: CRUD simples        Bom para: features com
e equipes pequenas            regras de negócio ricas
```

Ambos compartilham:
- Mesmo `AppDbContext` (`DbSet<Produto>`, `DbSet<Pedido>`, `DbSet<PedidoItem>`)
- Mesma pipeline JWT
- Mesmo middleware de exceções

**Seção 7 — Onde Ver no Código**

Tabela mapeando conceito → arquivo do projeto.

**Step 3: Verificar**

```bash
# Verificar que todas as seções obrigatórias estão presentes
grep -c "Anêmico\|Result\|Aggregate\|Coexist\|IEndpoint\|Vertical Slice" \
  docs/VERTICAL-SLICE-DOMINIO-RICO.md
# Esperado: >= 6
```

**Step 4: Commit**

```bash
git add docs/VERTICAL-SLICE-DOMINIO-RICO.md
git commit -m "docs: criar guia conceitual de Vertical Slice Architecture e Domínio Rico"
```

---

### Task 7: Adicionar capítulo de Pedidos ao MELHORES-PRATICAS-MINIMAL-API.md

**Files:**
- Modify: `docs/MELHORES-PRATICAS-MINIMAL-API.md`

O guia de implementação atual só cobre Produtos. Adicionar um novo capítulo no final sobre como as práticas se aplicam ao caso Pedidos (Vertical Slice).

**Step 1: Ler o final do arquivo atual para saber onde inserir**

```bash
tail -50 docs/MELHORES-PRATICAS-MINIMAL-API.md
```

**Step 2: Adicionar no final do arquivo**

Adicionar nova seção (não alterar o conteúdo existente):

```markdown
---

## Pedidos — Vertical Slice em Ação

Esta seção demonstra como as mesmas boas práticas se aplicam com Vertical Slice Architecture.

### Registro Automático de Endpoints

Em vez de registrar cada endpoint manualmente em `Program.cs`, os slices de Pedidos usam scan automático via `IEndpoint`:

[mostrar código de IEndpoint.cs e AddEndpointsFromAssembly — ler de src/Features/Common/]

### Anatomia de um Slice: CreatePedido

**Endpoint** (`src/Features/Pedidos/CreatePedido/CreatePedidoEndpoint.cs`):
[mostrar código real — só roteamento, sem lógica]

**Command/Handler** (`src/Features/Pedidos/CreatePedido/CreatePedidoCommand.cs`):
[mostrar código real — lógica de aplicação, chama domínio]

**Validator** (`src/Features/Pedidos/CreatePedido/CreatePedidoValidator.cs`):
[mostrar código real — FluentValidation]

### Result Pattern nos Endpoints

Como o endpoint trata o `Result<T>` retornado pelo domínio:
[exemplo de código mostrando if (!result.IsSuccess)]

### Referência

Para entender os conceitos por trás destes padrões:
→ [VERTICAL-SLICE-DOMINIO-RICO.md](VERTICAL-SLICE-DOMINIO-RICO.md)
```

**Step 3: Verificar**

```bash
grep "Vertical Slice\|CreatePedido\|IEndpoint\|Result" \
  docs/MELHORES-PRATICAS-MINIMAL-API.md | wc -l
# Esperado: >= 6
```

**Step 4: Commit**

```bash
git add docs/MELHORES-PRATICAS-MINIMAL-API.md
git commit -m "docs: adicionar capítulo de Pedidos (Vertical Slice) ao guia de implementação"
```

---

### Task 8: Adicionar features dos slices ao MELHORIAS-DOTNET-10.md

**Files:**
- Modify: `docs/MELHORIAS-DOTNET-10.md`

O guia atual cobre Typed Results, MapGroup, etc. — features usadas em Produtos. Adicionar features usadas nos slices de Pedidos.

**Step 1: Ler o final do arquivo para saber onde inserir**

```bash
tail -30 docs/MELHORIAS-DOTNET-10.md
```

**Step 2: Adicionar no final**

Nova seção com três features obrigatórias:

**Feature: Scan automático de endpoints via reflection**
```csharp
// Program.cs — nenhum slice é registrado manualmente
builder.Services.AddEndpointsFromAssembly(typeof(Program).Assembly);
// ...
app.MapRegisteredEndpoints();
```
Mostrar o código real de `EndpointExtensions.cs`.

**Feature: Collection expressions (C# 12 / .NET 8+, adotado em .NET 10)**
```csharp
// No aggregate Pedido
private readonly List<PedidoItem> _itens = [];  // collection expression
```
Antes: `new List<PedidoItem>()`. Mais conciso e legível.

**Feature: Primary constructors em handlers**
```csharp
// Handler inline com primary constructor
public class CreatePedidoHandler(AppDbContext db)
{
    public async Task<Result<PedidoResponse>> Handle(CreatePedidoCommand cmd)
    { ... }
}
```
Antes: campo `private readonly AppDbContext _db;` + construtor explícito.

**Step 3: Verificar**

```bash
grep "AddEndpointsFromAssembly\|collection expression\|Primary constructor\|\[\]" \
  docs/MELHORIAS-DOTNET-10.md | wc -l
# Esperado: >= 3
```

**Step 4: Commit**

```bash
git add docs/MELHORIAS-DOTNET-10.md
git commit -m "docs: adicionar features .NET 10 usadas nos slices de Pedidos"
```

---

### Task 9: Atualizar ENTREGA-FINAL.md e CHECKLIST.md

**Files:**
- Modify: `docs/ENTREGA-FINAL.md`
- Modify: `docs/CHECKLIST.md`

**ENTREGA-FINAL.md — mudanças obrigatórias:**

1. Título "Três Pilares Educacionais" → "Quatro Pilares Educacionais" (adicionar o novo guia VERTICAL-SLICE-DOMINIO-RICO.md como pilar 4)
2. Seção "Aplicação Executável": atualizar "6 endpoints" → "11 endpoints (6 Produtos + 5 Pedidos)"
3. Seção "Arquitetura": substituir o diagrama de camadas único por:
```
Produtos: Endpoint → Service → Data (Horizontal Layers)
Pedidos:  Slice (Command+Handler+Validator+Endpoint) → Data (Vertical Slice)
```
4. Seção "O Que Você Aprenderá" — adicionar ao bloco Arquitetural:
```
- Vertical Slice Architecture
- Aggregate Root e Domain-Driven Design
- Result Pattern
```
5. Seção "Tecnologias" — corrigir versão ".NET" de "9 LTS" para "10 LTS" (erro no doc atual), remover versões desatualizadas
6. Remover "Próximos Passos" que sugere adicionar autenticação JWT e testes (ambos já existem)
7. Data/versão no rodapé: `27 de Fevereiro de 2026` / `3.0.0`

**CHECKLIST.md — mudanças obrigatórias:**

Ler o arquivo inteiro primeiro. Adicionar nova seção ao final:

```markdown
## Features/Pedidos (Vertical Slice)

### Domínio
- [x] `src/Features/Pedidos/Domain/Pedido.cs` — Aggregate root com regras de negócio
- [x] `src/Features/Pedidos/Domain/PedidoItem.cs` — Entity filha (snapshot de preço)
- [x] `src/Features/Pedidos/Domain/StatusPedido.cs` — Enum: Rascunho, Confirmado, Cancelado
- [x] `src/Features/Common/Result.cs` — Result pattern para erros de domínio

### Slices
- [x] `CreatePedido/` — POST /api/v1/pedidos (Command + Validator + Endpoint)
- [x] `GetPedido/` — GET /api/v1/pedidos/{id}
- [x] `ListPedidos/` — GET /api/v1/pedidos (paginação + filtro por status)
- [x] `AddItemPedido/` — POST /api/v1/pedidos/{id}/itens
- [x] `CancelPedido/` — POST /api/v1/pedidos/{id}/cancelar

### Infraestrutura
- [x] `IEndpoint` + `AddEndpointsFromAssembly` — registro automático
- [x] `AppDbContext` — DbSet<Pedido> e DbSet<PedidoItem> configurados

### Testes
- [x] `ProdutosAPI.Tests/Unit/Domain/PedidoTests.cs` — 16 testes de domínio
- [x] `ProdutosAPI.Tests/Integration/` — 13 testes de integração HTTP
- [x] `ProdutosAPI.Tests/Builders/ProdutoBuilder.cs` — builder para testes
```

**Step 1: Ler os arquivos**

```bash
cat docs/ENTREGA-FINAL.md
cat docs/CHECKLIST.md
```

**Step 2: Aplicar as mudanças descritas acima**

**Step 3: Verificar**

```bash
grep "Vertical Slice\|Pedidos\|11 endpoint" docs/ENTREGA-FINAL.md | wc -l
# Esperado: >= 3

grep "CreatePedido\|PedidoTests\|Integration" docs/CHECKLIST.md | wc -l
# Esperado: >= 5
```

**Step 4: Commit**

```bash
git add docs/ENTREGA-FINAL.md docs/CHECKLIST.md
git commit -m "docs: atualizar ENTREGA-FINAL e CHECKLIST para refletir Pedidos e Vertical Slice"
```

---

### Task 10: Atualizar INICIO-RAPIDO.md

**Files:**
- Modify: `docs/INICIO-RAPIDO.md`

O guia atual não menciona autenticação nem Pedidos, e ainda sugere "como adicionar autenticação JWT" como próximo passo (já existe).

**Step 1: Ler o arquivo atual**

```bash
cat docs/INICIO-RAPIDO.md
```

**Step 2: Aplicar as seguintes mudanças**

1. **Seção "Três Documentos Principais"** — adicionar como item 4:
```markdown
### 4️⃣ Vertical Slice e Domínio Rico
**Arquivo**: [VERTICAL-SLICE-DOMINIO-RICO.md](VERTICAL-SLICE-DOMINIO-RICO.md)
Guia conceitual sobre os padrões usados no caso de uso de Pedidos.
```

2. **Seção "Fluxo de Aprendizado"** — substituir o fluxo único pelas duas trilhas (apontar para INDEX.md para o detalhe).

3. **Seção "Testar a API"** — adicionar após os exemplos de Produto:

```markdown
### Testar Pedidos (requer autenticação)

**Passo 1 — Obter token JWT:**
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@example.com", "senha": "senha123"}'
```
Copie o campo `token` da resposta.

**Passo 2 — Criar um pedido:**
```bash
TOKEN="seu_token_aqui"
curl -X POST http://localhost:5000/api/v1/pedidos \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{}'
```

**Passo 3 — Adicionar item:**
```bash
curl -X POST http://localhost:5000/api/v1/pedidos/1/itens \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"produtoId": 1, "quantidade": 2}'
```

4. **Tabela "Conceitos Demonstrados"** — adicionar linhas:
```markdown
| **Vertical Slice** | [src/Features/Pedidos/](../src/Features/Pedidos/) |
| **Domínio Rico** | [src/Models/Produto.cs](../src/Models/Produto.cs) |
| **Result Pattern** | [src/Features/Common/Result.cs](../src/Features/Common/Result.cs) |
```

5. **FAQ** — substituir "Como adicionar autenticação JWT?" por "Como funciona a autenticação?" com resposta descrevendo o que já existe.

6. **"Próximos Passos"** — remover itens já implementados (autenticação JWT, testes — ambos já existem).

7. Data/versão no rodapé: `27 de Fevereiro de 2026` / `3.0.0`

**Step 3: Verificar**

```bash
grep "Pedidos\|JWT\|VERTICAL-SLICE\|Bearer" docs/INICIO-RAPIDO.md | wc -l
# Esperado: >= 5
```

**Step 4: Commit final**

```bash
git add docs/INICIO-RAPIDO.md
git commit -m "docs: atualizar INICIO-RAPIDO com auth JWT e exemplos de Pedidos"
```

---

### Task 11: Verificação final e push

**Step 1: Confirmar que todos os arquivos foram atualizados**

```bash
git log --oneline -15
# Esperado: ver os 9+ commits das tasks anteriores
```

**Step 2: Verificar que todos os links internos dos docs funcionam**

Verificar que os arquivos referenciados existem:
```bash
# Links críticos que devem existir
ls docs/VERTICAL-SLICE-DOMINIO-RICO.md
ls docs/MELHORES-PRATICAS-API.md
ls docs/MELHORES-PRATICAS-MINIMAL-API.md
ls docs/MELHORIAS-DOTNET-10.md
ls docs/ARQUITETURA.md
ls docs/CHECKLIST.md
ls docs/ENTREGA-FINAL.md
ls docs/INICIO-RAPIDO.md
ls docs/INDEX.md
ls ProdutosAPI.Tests/ESTRATEGIA-DE-TESTES.md
ls src/Features/Pedidos/Domain/Pedido.cs
ls src/Features/Common/Result.cs
```

**Step 3: Confirmar que o projeto ainda compila e testes passam**

```bash
cd /Users/marco.mendes/code/net-minimal-api
dotnet build -c Release 2>&1 | tail -5
# Esperado: Build succeeded

dotnet test 2>&1 | tail -5
# Esperado: X passed, 0 failed
```

**Step 4: Push**

```bash
git push origin main
```
