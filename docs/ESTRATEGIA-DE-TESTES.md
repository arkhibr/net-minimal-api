# Estratégia de Testes Global - API

## 📋 Sumário Executivo

A solução está organizada em **3 projetos de teste** cobrindo as 3 trilhas do repositório:

- **ProdutosAPI.Tests**: Clean Architecture (Produtos) e integração de endpoints.
- **Pedidos.Tests**: Vertical Slice + Domínio Rico (Pedidos).
- **Pix.MockServer.Tests**: integração HTTP da trilha PIX (mock server).

> Organização adotada: **`src/` para código de produção e `tests/` para projetos de teste**, alinhada com a prática recomendada pela Microsoft para soluções .NET.

### Contagem real de testes (executada em 4 de março de 2026)

Comando de referência:

```bash
dotnet test ProdutosAPI.slnx -v minimal
```

Resultado consolidado:

| Projeto | Aprovados | Falhas | Ignorados | Total |
|---|---:|---:|---:|---:|
| `ProdutosAPI.Tests` | 112 | 0 | 0 | 112 |
| `Pedidos.Tests` | 47 | 0 | 0 | 47 |
| `Pix.MockServer.Tests` | 7 | 0 | 0 | 7 |
| **Total da solução** | **166** | **0** | **0** | **166** |

---

## 🧪 Matriz por Tipo de Teste

### 1) ProdutosAPI.Tests (112)

| Tipo | Escopo | Qtde |
|---|---|---:|
| Unit Domain | Entidades/regras de domínio | 34 |
| Unit Common | Tipos utilitários comuns | 4 |
| Services | Regras de aplicação e orquestração | 14 |
| Endpoints | Contrato HTTP de Produtos | 23 |
| Integration/Pedidos | Fluxos HTTP de Pedidos dentro deste projeto | 13 |
| Validators | Regras FluentValidation | 24 |
| **Total** |  | **112** |

### 2) Pedidos.Tests (47)

| Tipo | Escopo | Qtde |
|---|---|---:|
| Domain (agregado) | `PedidoTests` (invariantes e transições) | 11 |
| Unit Domain (avançado) | `PedidoAdvancedTests` | 4 |
| Unit Common | `CommonTypesTests` | 3 |
| Validators | `CreatePedidoValidatorTests` | 3 |
| Endpoints | Contratos HTTP por endpoint de pedido | 17 |
| Integration | Workflows e cenários E2E de pedidos | 9 |
| **Total** |  | **47** |

### 3) Pix.MockServer.Tests (7)

| Tipo | Escopo | Qtde |
|---|---|---:|
| Integration HTTP | Auth mock, segurança, idempotência, liquidação e devolução | 7 |
| **Total** |  | **7** |

---

## 🧱 Objetivo de Cada Camada de Teste

### Unit (Domain/Common)
- Garantir invariantes de negócio sem dependência de infraestrutura.
- Validar transições de estado do agregado (`Rascunho`, `Confirmado`, `Cancelado`, etc.).
- Verificar tipos compartilhados usados por endpoints e respostas.

### Services / Handlers
- Validar regras de aplicação (fluxo, orquestração, efeitos de operação).
- Garantir que persistência e mapeamentos ocorram conforme esperado.
- Evitar regressão em regras de atualização, paginação e soft delete.

### Validators
- Garantir contrato de entrada estável e mensagens de erro consistentes.
- Cobrir limites, obrigatoriedade e formatos inválidos.

### Integration HTTP
- Validar contrato real de API (status code, payload, headers).
- Cobrir fluxos críticos fim a fim (`POST` + `GET` + transições de estado).
- Na trilha PIX: validar segurança com `Bearer` + mTLS real e idempotência (`Idempotency-Key`).
- Em `Testing`, validar fallback controlado por header para manter testes de integração sem handshake TLS real.

---

## 🔐 Cenários Críticos Cobertos

### Produtos
- CRUD completo com validações e paginação.
- Erros de contrato (`400`, `404`, `422`) e retornos de sucesso (`200`, `201`, `204`).

### Pedidos
- Criação, consulta, listagem, cancelamento e adição de item.
- Regras de domínio (valor mínimo, estado permitido, consistência do pedido).

### PIX Mock
- Emissão de token OAuth2 mock.
- Falha de segurança por ausência de token e/ou certificado de cliente (`401`/`403`).
- Idempotência com replay seguro e conflito `409` para payload divergente.
- Fluxo financeiro: criar cobrança, simular liquidação, criar devolução, consultar devolução.

---

## 🛠️ Execução e Diagnóstico

### Executar tudo

```bash
dotnet test ProdutosAPI.slnx -v minimal
```

### Executar por projeto

```bash
dotnet test tests/ProdutosAPI.Tests/ProdutosAPI.Tests.csproj -v minimal
dotnet test tests/Pedidos.Tests/Pedidos.Tests.csproj -v minimal
dotnet test tests/Pix.MockServer.Tests/Pix.MockServer.Tests.csproj -v minimal
```

### Executar por tipo (filtro)

```bash
# ProdutosAPI.Tests
dotnet test tests/ProdutosAPI.Tests/ProdutosAPI.Tests.csproj --filter "FullyQualifiedName~ProdutosAPI.Tests.Unit.Domain"
dotnet test tests/ProdutosAPI.Tests/ProdutosAPI.Tests.csproj --filter "FullyQualifiedName~ProdutosAPI.Tests.Services"
dotnet test tests/ProdutosAPI.Tests/ProdutosAPI.Tests.csproj --filter "FullyQualifiedName~ProdutosAPI.Tests.Endpoints"
dotnet test tests/ProdutosAPI.Tests/ProdutosAPI.Tests.csproj --filter "FullyQualifiedName~ProdutosAPI.Tests.Validators"

# Pedidos.Tests
dotnet test tests/Pedidos.Tests/Pedidos.Tests.csproj --filter "FullyQualifiedName~Pedidos.Tests.Domain"
dotnet test tests/Pedidos.Tests/Pedidos.Tests.csproj --filter "FullyQualifiedName~Pedidos.Tests.Endpoints"

# PIX
dotnet test tests/Pix.MockServer.Tests/Pix.MockServer.Tests.csproj --filter "FullyQualifiedName~Pix.MockServer.Tests"
```

---

## 📈 Diretrizes de Evolução da Suíte

- Ao adicionar endpoint novo, incluir:
  - teste de sucesso (2xx),
  - teste de validação (4xx),
  - teste de recurso inexistente (404 quando aplicável).
- Ao adicionar regra de domínio, incluir teste unitário do agregado antes do teste HTTP.
- Ao alterar contrato JSON, atualizar teste de integração e exemplos de documentação juntos.
- Em integrações externas (PIX), nunca remover cobertura de `Idempotency-Key`, `X-Correlation-Id` e segurança mock.

---

## ✅ Status Atual

- Suíte íntegra e verde na data de referência.
- Cobertura distribuída entre domínio, aplicação, validação e contrato HTTP.
- Estratégia preparada para crescimento das 3 trilhas sem perder rastreabilidade.
