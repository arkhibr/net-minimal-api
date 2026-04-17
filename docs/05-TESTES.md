# Estratégia de Testes

## 1. Projetos de Teste

| Projeto | Testes | Escopo |
|---|---|---|
| `ProdutosAPI.Tests` | 143 | Catálogo (integração + unitários) |
| `Pix.MockServer.Tests` | 7 | Integração HTTP PIX |
| **Total** | **150** | |

> `Pedidos.Tests` existe no repositório mas tem uma dependência pendente de correção — não está incluído na contagem acima.

---

## 2. Distribuição por Tipo — ProdutosAPI.Tests

| Tipo | Escopo | Aprox. |
|---|---|---|
| Integração (Catálogo) | Endpoints HTTP completos via `HttpClient` | ~80 |
| Rate limiting | Políticas de throttling via `RateLimitingApiFactory` | 3 |
| Unitários (domínio) | Entidades, value objects, invariantes | ~30 |
| Validators | Regras FluentValidation | ~30 |

---

## 3. ApiFactory e Isolamento de Rate Limiting

Este é o ponto de maior atenção ao escrever novos testes de integração para o Catálogo.

### `ApiFactory` (base)

Factory base para todos os testes funcionais. Configura `Environment = "Testing"`, sobe o banco InMemory e executa o `DbSeeder`. Registra as três políticas de rate limiting com limite `10000` para que nunca interfiram nos testes de comportamento funcional.

### `RateLimitingApiFactory`

Estende `ApiFactory` e sobrescreve o registro de `AddRateLimiting()`, aplicando limites baixos propositalmente:

| Política | Limite |
|---|---|
| `leitura` | 3 req/janela |
| `escrita` | 3 req/janela |
| `criacao-produto` | 2 req/janela |

O objetivo é permitir que os testes atinjam o limite `429` com poucas requisições, sem depender de timing real.

### Por que o `Program.cs` não registra rate limiting em Testing

Em `Environment = "Testing"`, a chamada `AddCatalogoRateLimiting()` é omitida no `Program.cs`. Isso evita conflito de chave duplicada (`InvalidOperationException`) quando a factory tenta registrar suas próprias políticas durante o `WebApplicationFactory.CreateHost()`.

### Isolamento entre classes de teste

Cada classe de teste usa `IClassFixture<T>` com sua própria instância de factory. Isso garante que o estado interno do rate limiter (contadores de janela) não vaze entre classes de teste diferentes, evitando falhas intermitentes por ordem de execução.

---

## 4. Executar Testes

```bash
# Todos os testes funcionais
dotnet test tests/ProdutosAPI.Tests/ProdutosAPI.Tests.csproj

# Apenas testes de rate limiting
dotnet test tests/ProdutosAPI.Tests/ --filter "FullyQualifiedName~RateLimitingTests"

# Testes PIX
dotnet test tests/Pix.MockServer.Tests/
```

---

## 5. Cenários Críticos Cobertos

### Catálogo

- CRUD completo para todos os 5 recursos (Produto, Categoria, Variante, Atributo, Mídia)
- Paginação (tamanho de página, cursor/offset)
- Soft delete: produto inativo retorna `404` em todos os endpoints, incluindo GET por ID
- `422 Unprocessable Entity` para payloads que violam validators FluentValidation
- `404 Not Found` para recursos inexistentes
- Rate limiting: sequência de requisições que excede o limite retorna `429` com header `Retry-After`

### Pedidos

- Criar pedido, consultar por ID, listar
- Adicionar item a pedido existente
- Cancelar pedido
- Invariantes de domínio: não é permitido adicionar item a pedido cancelado, nem cancelar pedido já entregue

### PIX

- Fluxo OAuth2: obtenção de token e uso em requisições subsequentes
- Segurança mTLS: rejeição de requisições sem certificado cliente válido
- Idempotency key: mesma chave retorna a resposta cacheada
- Conflito `409`: payload divergente para a mesma chave de idempotência
- Fluxo de liquidação: criação de cobrança, webhook de liquidação e consulta de status atualizado

---

## 6. Diretrizes para Novos Testes

| Situação | Diretriz |
|---|---|
| Novo endpoint | Cobrir: resposta `2xx` com sucesso, `4xx` de validação, `404` quando aplicável |
| Nova regra de domínio | Escrever teste unitário no agregado **antes** do teste de integração |
| Endpoint de escrita no Catálogo | Obter token com `AuthHelper.ObterTokenAsync(client)` antes de chamar o endpoint |
| Asserção de rate limiting | Usar `RateLimitingApiFactory`, nunca `ApiFactory` |
| Novos produtos criados em teste | IDs começam a partir de 9 (DbSeeder reserva 1–8) |
