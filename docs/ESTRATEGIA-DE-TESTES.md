# Estratégia de Testes Global - API

## 📋 Sumário Executivo

A suite de testes da solução é distribuída em **2 projetos paralelos** para validar os diferentes padrões arquiteturais empregados:
- **ProdutosAPI.Tests**: Cobre os casos para Clean Architecture (focado no domínio de Produtos).
- **Pedidos.Tests**: Cobre os casos para Vertical Slice + Domínio Rico (focado no domínio de Pedidos).

**Total de testes**: **150+ casos automatizados** (abrangendo Domain, Services, Handlers, Integrations e Validations).

**Framework de Testes**: xUnit  
**Mocking**: Moq + NSubstitute  
**Assertions**: FluentAssertions  
**Cobertura alvo**: ≥ 80% das operações críticas  

---

## 📁 Estrutura de Projetos de Testes

### **ProdutosAPI.Tests/** (Clean Architecture - Produtos)
Focado no CRUD de Produtos, testando a Clean Architecture em isolamento.
- **Domain**: Valida as entidades e regras de domínio puras.
- **Services**: Maior volume de testes (unitários de serviço).
- **Endpoints**: Testes de integração HTTP completos.
- **Validators**: Regras de validação de Request/DTO.

### **Pedidos.Tests/** (Vertical Slice - Pedidos)
Focado nas transações de Pedidos usando o modelo Vertical Slice.
- **Builders**: Utilitários para criação de massa de dados complexa (`ProdutoTestBuilder`).
- **Domain**: Testes de Agregado e regras de negócio complexas do `Pedido` (Rascunho, Confirmar, Cancelar).
- **Endpoints/Integração**: Testes dos fluxos completos via HTTP.

---

## 🧪 Categorias de Testes

### 1. **Domain Unit Tests**
Testam as entidades de domínio ricas, invariantes estruturais e transições de estado.

*Em Produtos:*
- Criação de produtos válidos e rejeição de estados inválidos.

*Em Pedidos:*
- Criação de pedido em status `Rascunho`.
- Adição de item valida estoque e preço.
- Regras de confirmação de pedido (ex: Valor mínimo R$ 10,00).
- Transições de estado (Rascunho → Confirmado → Cancelado).

### 2. **Service / Handler Unit Tests**
Testam a lógica de aplicação isolada do banco de dados (mock).

*Em Produtos (Services):*
- Regras de CRUD: Obter, Listar paginado, Criar, Atualizar (PUT/PATCH), e Soft Delete.

*Em Pedidos (Handlers):*
- Fluxo de execução de Comandos (MediatR/Handlers) como `CreatePedidoHandler`.
- Validação de comandos antes da persistência.

### 3. **Integration HTTP Tests**
Testam a API como cliente usando `WebApplicationFactory`.

- **Produtos**: 18+ testes (mapeamento dos 6 endpoints), lidando com 200 OK, 400 Bad Request, 404 Not Found e 422 Unprocessable Entity.
- **Pedidos**: Testes de todo o Workflow do Pedido.

### 4. **Validation Tests**
Garante o funcionamento do modelo de validação por FluentValidation. Covers requests structure.

---

## 🔍 Estratégia de Mocking

### 1. AppDbContext Mock
Para serviços, usamos o padrão em memória ou EntityFramework in-memory para evitar complexidade excessiva com Mocks puros em queries linq.
```csharp
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase("TestDb")
    .Options;
```

### 2. Builders de Mocks
O `ProdutoTestBuilder` e padrões semelhantes são empregados para criar massa de dados realista e flexível sem poluir o teste:
```csharp
var produto = ProdutoTestBuilder.Padrao()
    .ComEstoque(10)
    .ComPreco(100m)
    .Build();
```

---

## 📊 Cobertura de Código - Alvo

| Componente | Tipo | Alvo | Status |
|-----------|------|------|--------|
| **ProdutoService** | Métodos | 100% | ✅ |
| **ProdutoValidator** | Regras | 95%+ | ✅ |
| **Agregado Pedido**| Regras de Negócio | 100% | ✅ |
| **Endpoints (Ambos)** | Paths HTTP | 100% | ✅ |
| **Error Handling** | Middleware | 90%+ | ✅ |

---

## 🛠️ Exemplos Completos

### Exemplo de Teste de Domínio (Pedidos) - Padrão AAA
```csharp
[Fact]
public void AdicionarItem_ProdutoComEstoqueInsuficiente_RetornaFalha()
{
    // Arrange
    var pedido = Pedido.Criar();
    var produto = ProdutoTestBuilder.Padrao()
        .ComEstoque(1)
        .Build();

    // Act
    var resultado = pedido.AdicionarItem(produto, 5);

    // Assert
    resultado.IsSuccess.Should().BeFalse();
    resultado.Error.Should().Contain("estoque");
}
```

### Exemplo de Teste de Serviço (Produtos)
```csharp
[Fact]
public async Task CriarProdutoAsync_WithValidRequest_CreatesProduto()
{
    // Arrange
    var request = new CriarProdutoRequest { Nome = "Mouse", Preco = 150m };
    // Config Mocks...
    
    // Act
    var result = await _service.CriarProdutoAsync(request);

    // Assert
    result.Should().NotBeNull();
    _mockDbContext.Verify(db => db.SaveChangesAsync(default), Times.Once);
}
```

---

## 🚀 Como Executar os Testes

**Console (Todos os testes)**
```bash
dotnet test
```

**Por Categoria**
```bash
dotnet test --filter "Category=Domain"
dotnet test --filter "Namespace~=Endpoints"
```

**Cobertura (Requer ReportGenerator / Coverlet)**
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
reportgenerator -reports:"coverage.opencover.xml" -targetdir:"coveragereport"
```
