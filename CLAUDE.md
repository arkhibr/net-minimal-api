# CLAUDE.md

## Contexto do Projeto

Projeto **educacional** demonstrando dois padrões arquiteturais coexistindo intencionalmente:
- **Produtos** — arquitetura em camadas (flat, sem sub-projetos)
- **Pedidos** — vertical slice + domínio rico

Existe um plano de migração de Produtos para Clean Architecture em `docs/plans/2026-03-02-produtos-clean-architecture.md` — **ainda não executado**. Não migre sem ler o plano.

## Convenções Não-Óbvias

**Soft delete:** `DELETE /produtos/{id}` seta `Ativo = false`. Produto inativo retorna 404 em **todos** os endpoints — o filtro é responsabilidade do service/repositório.

**Error handling assimétrico:**
- `ProdutoService` retorna `null` para not found (endpoint faz `if (produto is null)`)
- `Pedido` (domínio) retorna `Result`/`Result<T>` — nunca lança exceções; sempre checar `IsSuccess` antes de `.Value`

**Categorias válidas de Produto:** definidas no `CriarProdutoValidator` — não duplicar aqui.

**Auth:** Pedidos exigem `RequireAuthorization()`. Produtos: GET anônimo, escrita exige JWT.

**Idempotência automática:** `IdempotencyMiddleware` intercepta POST/PUT/PATCH via header `Idempotency-Key`. Não reimplemente no handler.

**Novos endpoints em Pedidos** devem implementar `IEndpoint` (`src/Shared/Common/IEndpoint.cs`) — são registrados automaticamente por reflection. **Não** cadastre manualmente no `Program.cs`.

## Testes

Banco InMemory ativado por `Environment = "Testing"`. O `DbSeeder` roda no setup da factory e popula **8 produtos (IDs 1–8)** — testes que criam produtos começam do ID 9.

Autenticação nos testes via `AuthHelper.ObterTokenAsync(client)` — credenciais hardcoded `admin@example.com` / `senha123`.

## Planos de Implementação

Planos ficam em `docs/plans/`. Leia o plano antes de qualquer migração estrutural.
