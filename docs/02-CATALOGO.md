# Catálogo — Bounded Context

## 1. Estrutura de Projetos

O bounded context **Catálogo** adota uma arquitetura híbrida entre Clean Architecture e Vertical Slice, organizada em 5 sub-projetos:

| Projeto | Responsabilidade |
|---|---|
| `src/Catalogo/Catalogo.Domain/` | Entidades, value objects e regras de domínio |
| `src/Catalogo/Catalogo.Application/` | Serviços de aplicação, DTOs e validators (FluentValidation) |
| `src/Catalogo/Catalogo.Infrastructure/` | Repositórios EF Core, migrations e DbSeeder |
| `src/Catalogo/Catalogo.API/` | Endpoints Minimal API e extensões (ex: `RateLimitingExtensions`) |
| `src/Catalogo/Catalogo.ClientDemo/` | Console app demonstrando pipeline de resiliência |

---

## 2. Recursos e Endpoints

Todos os endpoints seguem o prefixo `/api/v1/catalogo/`.

| Recurso | GET (anon) | GET /{id} (anon) | POST [auth] | PUT/PATCH [auth] | DELETE [auth] |
|---|---|---|---|---|---|
| Produtos | `GET /produtos` | `GET /produtos/{id}` | `POST /produtos` | `PUT /produtos/{id}` | `DELETE /produtos/{id}` |
| Categorias | `GET /categorias` | `GET /categorias/{id}` | `POST /categorias` | `PUT /categorias/{id}` | `DELETE /categorias/{id}` |
| Variantes | `GET /produtos/{id}/variantes` | `GET /produtos/{id}/variantes/{vid}` | `POST /produtos/{id}/variantes` | `PUT /produtos/{id}/variantes/{vid}` | `DELETE /produtos/{id}/variantes/{vid}` |
| Atributos | `GET /produtos/{id}/atributos` | `GET /produtos/{id}/atributos/{aid}` | `POST /produtos/{id}/atributos` | `PUT /produtos/{id}/atributos/{aid}` | `DELETE /produtos/{id}/atributos/{aid}` |
| Mídias | `GET /produtos/{id}/midias` | `GET /produtos/{id}/midias/{mid}` | `POST /produtos/{id}/midias` | `PUT /produtos/{id}/midias/{mid}` | `DELETE /produtos/{id}/midias/{mid}` |

### Políticas de Rate Limiting por verbo

| Verbo / Rota | Política |
|---|---|
| `GET` (todos) | `leitura` — FixedWindow, 60 req/min |
| `POST /produtos` | `criacao-produto` — TokenBucket, 5 req/min |
| `POST`, `PUT`, `PATCH`, `DELETE` (demais rotas) | `escrita` — SlidingWindow, 20 req/min |

---

## 3. Modelo de Domínio

### Produto

Campos: `Id`, `Nome`, `Descricao`, `Preco`, `Estoque`, `Categoria`, `Ativo`, `DataCriacao`.

Implementa **soft delete**: `DELETE /produtos/{id}` seta `Ativo = false`. Um produto inativo retorna `404` em todos os endpoints — o filtro é aplicado no repositório, não no endpoint.

### Categoria

Campos: `Id`, `Nome`, `Slug` (gerado automaticamente), `Descricao`, `CategoriaPaiId`, `Ativo`.

O `Slug` é gerado por normalização Unicode do nome (remoção de diacríticos + lowercase + hífens). A hierarquia suporta no máximo 2 níveis. Uma categoria com produtos ativos não pode ser desativada.

### Variante

Pertence a um `Produto`. Campos principais: `Id`, `ProdutoId`, `SKU`, `PrecoProduto`, `EstoqueProduto`.

- **SKU** é um value object (`sealed record`), validado por regex `^[A-Z0-9\-]+$` com comprimento entre 6 e 20 caracteres.
- **PrecoProduto** e **EstoqueProduto** são value objects que encapsulam invariantes de negócio (ex: preço não negativo, estoque não negativo).

### Atributo

Pertence a um `Produto`. Campos: `Id`, `ProdutoId`, `Nome`, `Valor`. CRUD simples — sem regras de domínio adicionais.

### Midia

Pertence a um `Produto`. Campos: `Id`, `ProdutoId`, `Url`, `TipoMidia` (enum: `Imagem`, `Video`, `Documento`), `Ordem`. CRUD simples — sem regras de domínio adicionais.

### Domínio rico vs. CRUD simples

`Produto`, `Categoria` e `Variante` são modelados com **domínio rico**: possuem invariantes explícitas, value objects e métodos de domínio que protegem o estado interno. `Atributo` e `Mídia` são **CRUD simples** porque representam dados descritivos sem regras de negócio que justifiquem a complexidade adicional.

---

## 4. Rate Limiting

As três políticas são registradas em `RateLimitingExtensions` e não se sobrepõem:

### `leitura` — FixedWindow

- Janela fixa de 60 segundos, limite de 60 requisições, `QueueLimit = 0`.
- Aplicada a todos os endpoints `GET`.
- Adequada para leitura: tráfego mais alto e previsível.

### `escrita` — SlidingWindow

- 20 requisições por minuto, 6 segmentos de 10 segundos cada, `QueueLimit = 0`.
- Aplicada a `POST` (exceto `/produtos`), `PUT`, `PATCH` e `DELETE`.
- A janela deslizante distribui melhor rajadas curtas do que uma janela fixa.

### `criacao-produto` — TokenBucket

- 5 tokens por minuto, `QueueLimit = 0`.
- Aplicada exclusivamente a `POST /produtos`.
- Controle mais granular para operações de maior custo (validação, persistência, indexação futura).

### Comportamento em caso de limite excedido

Retorna `429 Too Many Requests` com o header `Retry-After` indicando quando o cliente pode tentar novamente.

### Ambiente de testes

Em `Environment = "Testing"`, o `Program.cs` não registra `AddCatalogoRateLimiting()`. Cada factory de teste registra suas próprias políticas:

- `ApiFactory`: registra as três políticas com limite `10000` para não interferir nos testes funcionais.
- `RateLimitingApiFactory`: sobrescreve `AddRateLimiting()` com limites baixos (`leitura = 3`, `escrita = 3`, `criacao-produto = 2`) para os testes de throttling.

---

## 5. Autenticação

Todos os endpoints de escrita (`POST`, `PUT`, `PATCH`, `DELETE`) exigem JWT Bearer. Endpoints `GET` são anônimos.

```bash
# 1. Obter token
POST /api/v1/auth/login
Content-Type: application/json

{"email": "admin@example.com", "senha": "senha123"}

# 2. Usar nas requisições
Authorization: Bearer {token}
```

---

## 6. ClientDemo — Resiliência

`Catalogo.ClientDemo` é um console app que demonstra o consumo da API com um pipeline de resiliência composto (Polly v8):

| Camada | Política | Configuração |
|---|---|---|
| 1ª | Timeout interno | 5 segundos por tentativa |
| 2ª | Retry | 3 tentativas, backoff exponencial + jitter; ativa em `429` e `503` |
| 3ª | Circuit Breaker | Abre se 50% das requisições falham em 30s; permanece aberto por 15s |
| 4ª | Timeout global | 30 segundos para toda a operação |

As políticas são aplicadas de fora para dentro: o timeout global envolve tudo, incluindo as retentativas.

Para executar:

```bash
dotnet run --project src/Catalogo/Catalogo.ClientDemo -- https://localhost:5001
```

---

## 7. Seeding

O `DbSeeder` em `Catalogo.Infrastructure` popula os dados iniciais ao subir a aplicação (e nos testes de integração via `ApiFactory`):

| Entidade | Quantidade | IDs reservados |
|---|---|---|
| Produtos | 8 | 1–8 |
| Categorias | 5 | 1–5 |

Categorias iniciais: **Eletrônicos** (1), **Vestuário** (2), **Alimentos** (3), **Casa & Jardim** (4), **Esportes** (5).

Testes que criam novos produtos começam a partir do ID 9.
