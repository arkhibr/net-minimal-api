# ProdutosAPI — .NET 10 Minimal API

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![Minimal API](https://img.shields.io/badge/Minimal_API-Enabled-1f883d)
![Tests](https://img.shields.io/badge/tests-150_passando-2ea44f)
![License](https://img.shields.io/badge/license-MIT-blue)

Projeto educacional em .NET 10 Minimal API demonstrando três bounded contexts com padrões arquiteturais distintos coexistindo no mesmo repositório. Cada contexto resolve o mesmo problema técnico (uma API REST com persistência, validação e testes) com graus diferentes de estrutura — permitindo comparação direta entre abordagens.

---
## Arquitetura de Referência
<img width="1536" height="1024" alt="image" src="https://github.com/user-attachments/assets/8e7e34ed-7037-40a7-8e79-775de33aebe4" />


---

## Princípios e padrões implementados

### Clean Architecture (módulo Catálogo)

O Catálogo é organizado em quatro camadas com responsabilidades bem definidas e separadas em sub-projetos independentes:

- **Domain** — entidades, value objects e interfaces de repositório. Nenhuma dependência de infraestrutura.
- **Application** — serviços de aplicação que orquestram casos de uso, DTOs e validadores com FluentValidation.
- **Infrastructure** — repositórios concretos com EF Core, implementando as interfaces definidas no Domain.
- **API** — endpoints Minimal API, sem lógica de negócio.

A regra de dependência é sempre de fora para dentro: a camada de API depende de Application, que depende de Domain. Domain não depende de nenhuma outra camada.

### Vertical Slice Architecture (módulo Pedidos)

O módulo de Pedidos abandona a organização por camada técnica e adota a organização por caso de uso. Cada operação (criar pedido, adicionar item, cancelar) é uma pasta autocontida com seu próprio command, validador, handler e endpoint. Alterar o comportamento de uma operação não exige tocar em nenhuma outra pasta.

### Domínio Rico vs. Modelo Anêmico

Os dois estilos coexistem intencionalmente no projeto para comparação direta.

**Modelo anêmico** (Atributo e Mídia no Catálogo): entidades são contêineres de dados com apenas propriedades get/set. Toda lógica vive nos serviços de aplicação. Adequado para CRUD sem regras de negócio.

**Domínio rico** (Produto, Categoria, Variante no Catálogo e aggregate Pedido): entidades encapsulam suas próprias invariantes em métodos de domínio. Pedido.AddItem() verifica se o pedido está aberto e se há estoque suficiente antes de adicionar o item. Categoria gera seu próprio slug e valida hierarquia. Regras de negócio vivem no lugar onde o estado é mantido.

### Value Objects

SKU em Variante e PrecoProduto e EstoqueProduto são value objects — tipos imutáveis sem identidade própria que encapsulam invariantes. Eliminam validações espalhadas e tornam o estado inválido impossível de representar.

### Result Pattern (sem exceções para erros de negócio)

O módulo de Pedidos usa Result<T> em vez de exceções para erros esperados. Métodos de domínio retornam Result<Pedido>.Fail("Pedido não está aberto") em vez de lançar InvalidOperationException. O handler sempre verifica IsSuccess antes de acessar .Value. O fluxo de controle fica linear e legível, sem blocos try/catch para erros previsíveis.

### CQRS leve — segregação de repositórios Query/Command

No Catálogo, queries e commands usam interfaces separadas (IProdutoQueryRepository e IProdutoCommandRepository). Queries retornam DTOs diretamente do banco, sem passar pela camada de domínio. Commands operam sobre entidades. Isso elimina mapeamentos desnecessários em leitura sem a complexidade de event sourcing.

### Auto-descoberta de endpoints via reflexão

Todos os endpoints implementam a interface IEndpoint. O Program.cs varre o assembly em tempo de inicialização e registra automaticamente todas as implementações. Adicionar um novo endpoint não exige nenhum registro manual — basta criar a classe.

### Idempotência

Um middleware global intercepta requisições POST, PUT e PATCH com o header Idempotency-Key. Se a chave já foi vista, a resposta cacheada é devolvida imediatamente. Se a mesma chave chegar com um payload diferente, retorna 409 Conflict. O módulo Pix demonstra isso de forma didática com exemplos de fluxos financeiros.

### Rate Limiting com três algoritmos distintos

Três políticas com algoritmos diferentes são aplicadas no Catálogo, cada uma adequada ao seu contexto:

- **Fixed Window** (leitura) — 60 requisições por janela de 60s. Para leituras com tráfego alto e previsível.
- **Sliding Window** (escrita) — 20 requisições por minuto em 6 segmentos de 10s. Distribui melhor rajadas curtas do que janela fixa.
- **Token Bucket** (criacao-produto) — 5 tokens por minuto, repostos continuamente. Controle mais granular para operações de maior custo.

Todas retornam 429 Too Many Requests com o header Retry-After.

### Pipeline de resiliência com Polly

Os módulos ClientDemo demonstram retry, circuit breaker e timeout compostos via Microsoft.Extensions.Http.Resilience (Polly v8). O pipeline do Catálogo.ClientDemo tem quatro camadas: timeout por tentativa → retry com backoff exponencial e jitter → circuit breaker → timeout global. O Pix.ClientDemo usa AddStandardResilienceHandler, que configura o mesmo conjunto automaticamente.

### Integração externa com mTLS e OAuth2 (módulo Pix)

O módulo Pix inclui um servidor mock que simula a API Pix do Banco Central com autenticação mútua TLS (mTLS) e OAuth2 (client credentials). O cliente tipado demonstra a cadeia completa: obtenção de token, injeção de X-Correlation-Id e Idempotency-Key via handlers encadeados, e consumo com resiliência. Tudo sem dependência de ambiente externo.

### Tratamento de erro padronizado (RFC 7807 Problem Details)

Um middleware global captura exceções não tratadas e retorna respostas no formato application/problem+json, com status, title, detail e type. Endpoints de validação retornam 422 Unprocessable Entity com o mesmo formato.

### Soft Delete

DELETE /produtos/{id} seta Ativo = false em vez de remover o registro. O repositório aplica o filtro automaticamente em todas as queries — um produto inativo retorna 404 em todos os endpoints, sem nenhuma lógica adicional no endpoint.

### Estratégia de testes em camadas

150 testes cobrem quatro níveis distintos:

- **Unitários de domínio** — testam entidades e agregados diretamente, sem infraestrutura. PedidoTests testa Pedido.AddItem() com pedido cancelado sem instanciar banco ou HTTP.
- **Unitários de serviço** — testam serviços de aplicação com repositório mockado.
- **Integração HTTP** — sobem a aplicação real com WebApplicationFactory e banco em memória, fazem requisições HTTP e verificam status codes, headers e payload.
- **Rate limiting isolado** — RateLimitingApiFactory substitui as políticas por limites baixos (2–3 req/janela) para testar comportamento de throttling sem depender de timing real.

---

## Módulos

| Módulo | Padrão principal | Rota base |
|---|---|---|
| Catálogo | Clean Architecture híbrida | /api/v1/catalogo/* |
| Pedidos | Vertical Slice + Domínio Rico | /api/v1/pedidos/* |
| Pix | Mock Server + HTTP Client resiliente | /pix/v1/* |

---

## Início rápido

**Pré-requisitos:** [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

```bash
git clone https://github.com/arkhibr/net-minimal-api.git
cd net-minimal-api
dotnet restore
dotnet run
```

| Endpoint                | URL                                |
| ----------------------- | ---------------------------------- |
| Swagger UI              | http://localhost:5000              |
| HTTP base               | http://localhost:5000/api/v1       |
| HTTPS base              | https://localhost:5001/api/v1      |
| Health check            | http://localhost:5000/health       |

Credenciais para JWT no Swagger: `admin@example.com` / `senha123`.

---

## Bounded Contexts

| Contexto | Padrão                        | Rotas base                                  | Descrição                                                        |
| -------- | ----------------------------- | ------------------------------------------- | ---------------------------------------------------------------- |
| Catálogo | Clean Architecture híbrida    | `/api/v1/catalogo/*`                        | 5 recursos com CRUD completo, rate limiting (3 políticas) e soft delete |
| Pedidos  | Vertical Slice + Domínio Rico | `/api/v1/pedidos/*`                         | Agregado rico, Result pattern, autenticação JWT obrigatória      |
| Pix      | Mock Server + HTTP Client     | `/pix/v1/*` (executado em processo próprio) | mTLS, OAuth2, idempotência por chave, resiliência via Polly v8   |

> O bounded context **Pix** roda como aplicação separada (`src/Pix/Pix.MockServer/`). Não compartilha pipeline HTTP com Catálogo e Pedidos. Ver [docs/04-PIX.md](docs/04-PIX.md).

---

## Estrutura de Diretórios

```
net-minimal-api/
├── Program.cs                            # composition root (ASP.NET Core)
├── ProdutosAPI.csproj                    # projeto principal
├── ProdutosAPI.slnx                      # solution
│
├── src/
│   ├── Catalogo/                         # bounded context 1 — Clean Architecture híbrida
│   │   ├── Catalogo.Domain/
│   │   ├── Catalogo.Application/
│   │   ├── Catalogo.Infrastructure/
│   │   ├── Catalogo.API/
│   │   └── Catalogo.ClientDemo/          # console app de demonstração de resiliência
│   │
│   ├── Pedidos/                          # bounded context 2 — Vertical Slice + Domínio Rico
│   │   ├── CreatePedido/                 # slice: Command, Validator, Endpoint, Handler
│   │   ├── GetPedido/
│   │   ├── ListPedidos/
│   │   ├── CancelPedido/
│   │   ├── AddItemPedido/
│   │   ├── Domain/                       # agregado Pedido + PedidoItem
│   │   ├── Repositories/
│   │   ├── Infrastructure/               # mapeamentos EF Core
│   │   └── Common/                       # DTOs e tipos compartilhados entre slices
│   │
│   ├── Pix/                              # bounded context 3 — integração externa
│   │   ├── Pix.MockServer/               # Minimal API independente (mTLS + OAuth2)
│   │   └── Pix.ClientDemo/               # console app HttpClient tipado
│   │
│   └── Shared/                           # infra usada pelos três contextos
│       ├── Common/                       # IEndpoint, Result<T>, EndpointExtensions
│       ├── Data/                         # AppDbContext + Migrations
│       └── Middleware/                   # ExceptionHandling, Idempotency
│
└── tests/
    ├── ProdutosAPI.Tests/                # 143 testes — Catálogo + Pedidos
    └── Pix.MockServer.Tests/             # 7 testes — integração HTTP PIX
```

---

## Endpoints

### Autenticação

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/api/v1/auth/login` | Retorna JWT. Body: `{"email": "admin@example.com", "senha": "senha123"}` |

### Catálogo

Todas as rotas têm prefixo `/api/v1/catalogo/`. Endpoints de escrita exigem JWT; leituras são anônimas. Políticas de rate limiting: `leitura` (60/min), `escrita` (20/min) e `criacao-produto` (5/min, exclusiva para `POST /produtos`).

| Método   | Rota                                       | Auth | Rate limit         | Observações                                |
| -------- | ------------------------------------------ | ---- | ------------------ | ------------------------------------------ |
| `GET`    | `/api/v1/catalogo/produtos`                | —    | `leitura`          | Paginado; filtros: `categoria`, `search`   |
| `GET`    | `/api/v1/catalogo/produtos/{id}`           | —    | `leitura`          | Retorna 404 se inativo                     |
| `POST`   | `/api/v1/catalogo/produtos`                | JWT  | `criacao-produto`  | TokenBucket: pico baixo, custo alto        |
| `PUT`    | `/api/v1/catalogo/produtos/{id}`           | JWT  | `escrita`          | Substituição completa                      |
| `PATCH`  | `/api/v1/catalogo/produtos/{id}`           | JWT  | `escrita`          | Atualização parcial                        |
| `DELETE` | `/api/v1/catalogo/produtos/{id}`           | JWT  | `escrita`          | Soft delete (seta `Ativo = false`)         |
| `GET`    | `/api/v1/catalogo/categorias`              | —    | `leitura`          |                                            |
| `GET`    | `/api/v1/catalogo/categorias/{id}`         | —    | `leitura`          |                                            |
| `POST`   | `/api/v1/catalogo/categorias`              | JWT  | `escrita`          |                                            |
| `PUT`    | `/api/v1/catalogo/categorias/{id}`         | JWT  | `escrita`          |                                            |
| `DELETE` | `/api/v1/catalogo/categorias/{id}`         | JWT  | `escrita`          |                                            |
| `GET`    | `/api/v1/catalogo/variantes`               | —    | `leitura`          | Query opcional: `?produtoId={id}`          |
| `GET`    | `/api/v1/catalogo/variantes/{id}`          | —    | `leitura`          |                                            |
| `POST`   | `/api/v1/catalogo/variantes`               | JWT  | `escrita`          |                                            |
| `PUT`    | `/api/v1/catalogo/variantes/{id}`          | JWT  | `escrita`          |                                            |
| `PATCH`  | `/api/v1/catalogo/variantes/{id}/estoque`  | JWT  | `escrita`          | Apenas o campo de estoque                  |
| `DELETE` | `/api/v1/catalogo/variantes/{id}`          | JWT  | `escrita`          |                                            |
| `GET`    | `/api/v1/catalogo/atributos`               | —    | `leitura`          | Query opcional: `?produtoId={id}`          |
| `POST`   | `/api/v1/catalogo/atributos`               | JWT  | `escrita`          |                                            |
| `PUT`    | `/api/v1/catalogo/atributos/{id}`          | JWT  | `escrita`          |                                            |
| `DELETE` | `/api/v1/catalogo/atributos/{id}`          | JWT  | `escrita`          |                                            |
| `GET`    | `/api/v1/catalogo/midias`                  | —    | `leitura`          | Query opcional: `?produtoId={id}`          |
| `POST`   | `/api/v1/catalogo/midias`                  | JWT  | `escrita`          |                                            |
| `PATCH`  | `/api/v1/catalogo/midias/{id}/ordem`       | JWT  | `escrita`          | Reordena a mídia                           |
| `DELETE` | `/api/v1/catalogo/midias/{id}`             | JWT  | `escrita`          |                                            |

### Pedidos

Todas as rotas exigem JWT. Erros de negócio retornam `400 Bad Request` com `Result.Error` no corpo (não usam exceções).

| Método | Rota                              | Slice          | Observações                                    |
| ------ | --------------------------------- | -------------- | ---------------------------------------------- |
| `POST` | `/api/v1/pedidos`                 | CreatePedido   | Cria pedido com itens iniciais                 |
| `GET`  | `/api/v1/pedidos`                 | ListPedidos    | Lista pedidos (consulta via Dapper)            |
| `GET`  | `/api/v1/pedidos/{id}`            | GetPedido      | Detalhe do pedido com itens                    |
| `POST` | `/api/v1/pedidos/{id}/itens`      | AddItemPedido  | Falha se pedido não está em status `Rascunho`  |
| `POST` | `/api/v1/pedidos/{id}/cancelar`   | CancelPedido   | Falha se pedido já está cancelado ou confirmado |

---

## Testes

| Projeto                | Testes | Cobertura                                                   |
| ---------------------- | -----: | ----------------------------------------------------------- |
| `ProdutosAPI.Tests`    |    143 | Catálogo (integração + unit) e Pedidos (integração)         |
| `Pix.MockServer.Tests` |      7 | Fluxo OAuth2 + cobrança + idempotência via HTTP             |
| **Total**              |  **150** |                                                           |

> `tests/Pedidos.Tests/` existe no repositório mas tem uma dependência pendente — não está incluído na contagem.

```bash
# Solução completa (150 testes)
dotnet test ProdutosAPI.slnx -v minimal

# Apenas o projeto principal (143 testes)
dotnet test tests/ProdutosAPI.Tests/

# Apenas o mock server PIX (7 testes)
dotnet test tests/Pix.MockServer.Tests/

# Filtros por categoria
dotnet test tests/ProdutosAPI.Tests/ --filter "FullyQualifiedName~Unit.Domain"
dotnet test tests/ProdutosAPI.Tests/ --filter "FullyQualifiedName~Integration.Catalogo"
dotnet test tests/ProdutosAPI.Tests/ --filter "FullyQualifiedName~RateLimitingTests"
```

---

## Documentação

| Arquivo                                          | Conteúdo                                                          |
| ------------------------------------------------ | ----------------------------------------------------------------- |
| [docs/00-VISAO-GERAL.md](docs/00-VISAO-GERAL.md) | Visão geral, trilhas de aprendizado e mapa da documentação        |
| [docs/01-ARQUITETURA.md](docs/01-ARQUITETURA.md) | Diagramas (C1/C2), fluxos de requisição e comparativo CA × VSA    |
| [docs/02-CATALOGO.md](docs/02-CATALOGO.md)       | Catálogo: Clean Architecture híbrida, recursos, rate limiting     |
| [docs/03-PEDIDOS.md](docs/03-PEDIDOS.md)         | Pedidos: Vertical Slice, domínio rico, Result pattern             |
| [docs/04-PIX.md](docs/04-PIX.md)                 | Pix: Mock Server, mTLS, OAuth2, cliente HTTP com resiliência      |
| [docs/05-TESTES.md](docs/05-TESTES.md)           | Estratégia de testes, factories, isolamento de rate limiting      |
| [docs/ADRs/](docs/ADRs/)                         | 15 ADRs (MADR 3.x) registrando as decisões arquiteturais aceitas  |
| [docs/guias/](docs/guias/)                       | 4 guias: REST, Minimal API, .NET 10, JSON complexo                |
| [CLAUDE.md](CLAUDE.md)                           | Convenções não-óbvias do projeto (soft delete, rate limiting, auth) |

---

## Licença

MIT. Ver cabeçalho em `Program.cs` (configuração de Swagger).
