# 🧪 Estratégia de Testes - ProdutosAPI.Pedidos

## 📋 Visão Geral

Projeto de testes dedicado para a **Vertical Slice Architecture** implementada em Pedidos. Projeto paralelo e independente de ProdutosAPI.Tests (focado em Produtos), com estrutura simplificada para o domínio do Pedido.

**Framework**: xUnit  
**Mocking**: Moq + NSubstitute  
**Assertions**: FluentAssertions  
**Cobertura**: Handlers, Validators, Domain, Integration HTTP  

---

## 📁 Estrutura de Projeto

```
Pedidos.Tests/
├── Pedidos.Tests.csproj
├── Builders/
│   └── ProdutoTestBuilder.cs           # Builder para dados de teste dos Produtos
├── Domain/
│   └── PedidoTests.cs                  # 11 testes de agregado Pedido
│       ├── Criar (Rascunho)
│       ├── AdicionarItem (validações)
│       ├── Confirmar (regras de negócio)
│       └── Cancelar (transição de estado)
└── ESTRATEGIA-TESTES-PEDIDOS.md        # Esta documentação
```

**Total**: 11 testes unitários de domínio rico

---

## 🧫 Categorias de Testes

### 1. **Domain Unit Tests** (`Domain/`)
Testam o agregado `Pedido` em isolamento com 11 casos.

- ✅ Criar pedido em status Rascunho
- ✅ Adicionar item com produto válido
- ✅ Rejeitar item com estoque insuficiente
- ✅ Validar quantidade mínima de itens
- ✅ Confirmar pedido com validações (valor mínimo 10.00)
- ✅ Rejeitar operações em estado inválido
- ✅ Cancelar pedido com motivo
- ✅ Transições de estado (Rascunho → Confirmado → Cancelado)

**Exemplos**:
```csharp
[Fact]
public void Criar_ComClienteValido_RetornaResultOk()
{
    var result = Pedido.Create("Cliente");
    result.IsSuccess.Should().BeTrue();
}
```

### 2. **Test Builders** (`Builders/`)
Facilitam criação de dados de teste.

- ✅ `ProdutoTestBuilder`: Cria instâncias de Produto para testes
  - `Padrao()`: Produto default com preço e estoque
  - `ComPreco(decimal)`: Define preço customizado
  - `ComEstoque(int)`: Define estoque customizado
  - `ComNome(string)`: Define nome customizado
  - `Build()`: Retorna Produto construído

**Exemplos**:
```csharp
[Fact]
public async Task ValidarComando_SemItens_FalhaValidacao()
{
    var comando = new CreatePedidoCommand([]);
    var result = await _validator.ValidateAsync(comando);
    result.IsValid.Should().BeFalse();
}
```

### 3. **Integração com Resultado**
Todos os testes usam o pattern `Result` para validação:

```csharp
// Sucesso
resultado.IsSuccess.Should().BeTrue();

// Falha
resultado.IsSuccess.Should().BeFalse();
resultado.Error.Should().Contain("mensagem");
```

**Exemplos**:
```csharp
[Fact]
public async Task Handle_ComComandoValido_CriaPedidoComSucesso()
{
    var handler = new CreatePedidoHandler(context, validator);
    var result = await handler.HandleAsync(comando);
    result.IsSuccess.Should().BeTrue();
}
```

### 4. **Namespace**
Os testes usam namespace normalizado:

```csharp
namespace Pedidos.Tests.Domain;
using Pedidos.Tests.Builders;
using ProdutosAPI.Pedidos.Domain;  // Domínio da aplicação
using ProdutosAPI.Shared.Common;    // Result pattern
```

**Exemplos**:
```csharp
[Fact]
public async Task POST_Pedidos_ComDadosValidos_Retorna201()
{
    var response = await _client.PostAsJsonAsync("/api/v1/pedidos", comando);
    response.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

### 5. **Exemplo de Teste**

```csharp
[Fact]
public void Criar_RetornaPedidoEmRascunho()
{
    // Act
    var pedido = Pedido.Criar();

    // Assert
    pedido.Status.Should().Be(StatusPedido.Rascunho);
    pedido.Itens.Should().BeEmpty();
    pedido.Total.Should().Be(0m);
}

[Fact]
public void AdicionarItem_ProdutoComEstoqueInsuficiente_RetornaFalha()
{
    // Arrange
    var pedido = Pedido.Criar();
    var produto = ProdutoTestBuilder.Padrao()
        .ComEstoque(1)
        .ComPreco(100m)
        .Build();

    // Act
    var resultado = pedido.AdicionarItem(produto, 5);

    // Assert
    resultado.IsSuccess.Should().BeFalse();
    resultado.Error.Should().Contain("estoque");
}
```

---

## 🚀 Como Executar

### Executar todos os testes de Pedidos
```bash
dotnet test Pedidos.Tests
```

### Executar categoria específica
```bash
# Testes de Domain
dotnet test Pedidos.Tests --filter "FullyQualifiedName~Domain"
```

### Com saída detalhada
```bash
dotnet test Pedidos.Tests --logger "console;verbosity=detailed"
```

### Executar todos os testes da solução (incluindo ProdutosAPI.Tests)
```bash
dotnet test ProdutosAPI.slnx
```

---

## 📊 Cobertura de Testes

| Teste | Status | Quantidade |
|-------|--------|------------|
| Criar pedido | ✅ | 1 |
| Adicionar item | ✅ | 4 |
| Confirmar pedido | ✅ | 3 |
| Cancelar pedido | ✅ | 3 |
| **Total** | ✅ | **11** |

**Status**: ✅ Completo para operações básicas do agregado

---

## 🎯 Próximos Passos

1. **Handlers Tests**: Adicionar testes para handlers de cada slice
2. **Validators Tests**: Testar FluentValidation rules para comandos
3. **Integration Tests**: Testes HTTP dos endpoints de Pedidos
4. **Performance Tests**: Medir tempo de resposta em operações
5. **Testes de Concorrência**: Simular múltiplas requisições simultâneas

---

## 📚 Referências

- Framework: [xUnit](https://xunit.net/)
- Assertions: [FluentAssertions](https://fluentassertions.com/)
- Mocking: [Moq](https://github.com/moq/moq4)
- Documentação Pedidos: [src/Pedidos/](../../src/Pedidos/)

---

**Versão**: 2.0.0  
**Data**: 28 de fevereiro de 2026  
**Status**: ✅ Funcional - Testes de domínio implementados
