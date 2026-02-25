# Estratégia de Testes - ProdutosAPI

## 📋 Sumário Executivo

Este documento descreve a estratégia de testes implementada para o projeto **ProdutosAPI**, uma aplicação educacional demonstrando best practices de APIs REST com .NET 10 e Minimal API.

**Framework de Testes**: xUnit  
**Mocking**: Moq + NSubstitute  
**Assertions**: FluentAssertions  
**Target Coverage**: 80%+ das operações críticas  

---

## 🎯 Objetivos dos Testes

1. **Validação de Funcionalidade**: Garantir que todos os endpoints REST funcionam conforme especificado
2. **Cobertura de Casos de Erro**: Testar tratamento de exceções e status HTTP corretos
3. **Verifição de Validações**: Assegurar que todas as regras de negócio são aplicadas
4. **Regressão**: Prevenir quebras em funcionalidades existentes durante refatoração
5. **Documentação Viva**: Os testes servem como exemplos de como usar a API

---

## 📁 Estrutura de Projeto de Testes

```
ProdutosAPI.Tests/
├── ProdutosAPI.Tests.csproj          # Arquivo de projeto .NET 10
├── Services/
│   └── ProdutoServiceTests.cs        # ~350 linhas, 16 testes
├── Endpoints/
│   └── ProdutoEndpointsTests.cs      # ~400 linhas, 18 testes
├── Validators/
│   └── ProdutoValidatorTests.cs      # ~400 linhas, 20 testes
└── README.md                          # Esta documentação
```

---

## 🧪 Categorias de Testes

### 1. **Unit Tests - ProdutoService** (`ProdutoServiceTests.cs`)

Testa a camada de negócio com **16 casos de teste** organizados em 6 métodos principais.

#### **ListarProdutosAsync**
- ✅ Retorna paginação válida com 10 produtos
- ❌ Rejeita número de página inválido (0, -1)
- ✅ Retorna lista vazia quando banco está vazio

#### **ObterProdutoAsync**
- ✅ Retorna produto com ID válido
- ❌ Lança KeyNotFoundException para ID inválido
- ❌ Rejeita produtos inativos (soft delete)

#### **CriarProdutoAsync**
- ✅ Cria produto com request válida
- ❌ Rejeita request sem Nome (ValidationException)
- ❌ Rejeita preço negativo ou zero
- ❌ Valida formato de email

#### **AtualizarProdutoAsync** (PATCH)
- ✅ Atualiza apenas campos fornecidos
- ❌ Rejeita ID inexistente

#### **AtualizarCompletoProdutoAsync** (PUT)
- ✅ Substitui todos os campos do produto
- ❌ Rejeita ID inexistente

#### **DeletarProdutoAsync**
- ✅ Executa soft delete (marca como inativo)
- ❌ Rejeita ID inexistente
- ❌ Rejeita produto já deletado

**Padrão: AAA (Arrange-Act-Assert)**
```csharp
// Arrange - Setup dados e mocks
var request = new CriarProdutoRequest { ... };

// Act - Executar ação
var result = await service.CriarProdutoAsync(request);

// Assert - Verificar resultado
result.Should().NotBeNull();
result.Nome.Should().Be("...");
```

---

### 2. **Integration Tests - Endpoints** (`ProdutoEndpointsTests.cs`)

Testa a camada HTTP com **18 casos de teste** distribuídos entre os 6 endpoints.

#### **GET /produtos**
- ✅ Retorna 200 OK com lista paginada
- ❌ Retorna 400 Bad Request com página inválida
- ✅ Retorna 200 OK com lista vazia

#### **GET /produtos/{id}**
- ✅ Retorna 200 OK com produto específico
- ❌ Retorna 404 Not Found para ID inválido
- ❌ Retorna 404 para produto deletado

#### **POST /produtos**
- ✅ Retorna 201 Created com novo ID
- ❌ Retorna 422 Unprocessable Entity para validação falha
- ❌ Retorna 400 Bad Request para preço inválido

#### **PUT /produtos/{id}**
- ✅ Retorna 200 OK com atualização completa
- ❌ Retorna 404 Not Found para ID inválido

#### **PATCH /produtos/{id}**
- ✅ Retorna 200 OK com atualização parcial
- ❌ Retorna 404 Not Found para ID inválido

#### **DELETE /produtos/{id}**
- ✅ Retorna 204 No Content após soft delete
- ❌ Retorna 404 Not Found para ID inválido
- ❌ Retorna 404 para produto já deletado

#### **Error Response Format**
- ✅ Todos os erros seguem formato padrão (Status, Message, Details, Timestamp, TraceId)

**Validação de Status HTTP**:
- `200 OK` - Sucesso de GET/PUT/PATCH
- `201 Created` - Criação bem-sucedida de recurso
- `204 No Content` - Deleção bem-sucedida
- `400 Bad Request` - Erro na requisição (formato inválido)
- `404 Not Found` - Recurso não encontrado
- `422 Unprocessable Entity` - Falha de validação

---

### 3. **Validation Tests** (`ProdutoValidatorTests.cs`)

Testa regras de negócio com **20+ casos de teste** para validadores.

#### **CriarProdutoValidator**
- ✅ Request válida passa
- ❌ Nome vazio/nulo falha
- ❌ Nome > 255 caracteres falha
- ❌ Preço zero/negativo falha
- ❌ Email inválido falha
- ✅ Emails válidos passam (user@domain.com, nome+tag@empresa.co.uk)
- ❌ Estoque negativo falha
- ✅ Estoque zero passa (produto sem stock)

#### **AtualizarProdutoValidator**
- ✅ Atualização com apenas Nome passa
- ❌ Nome vazio falha
- ✅ Atualização com apenas Preco válido passa
- ❌ Preço negativo falha
- ✅ Múltiplos campos válidos passam
- ❌ Múltiplos campos inválidos reportam todos os erros

---

## 🚀 Como Executar Testes

### **Executar Todos os Testes**
```bash
cd net-minimal-api
dotnet test
```

### **Executar Testes de um Namespace Específico**
```bash
dotnet test --filter "FullyQualifiedName~ProdutosAPI.Tests.Services"
```

### **Executar um Teste Específico**
```bash
dotnet test --filter "Name=ListarProdutosAsync_WithValidPagination_ReturnsPaginatedProducts"
```

### **Executar com Output Detalhado**
```bash
dotnet test --verbosity detailed
```

### **Cobertura de Código (requer dotnet-reportgenerator)**
```bash
dotnet add package OpenCover
dotnet add package ReportGenerator

dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
reportgenerator -reports:"coverage.opencover.xml" -targetdir:"coveragereport"
```

---

## 🔍 Estratégia de Mocking

### **Mocks Utilizados**

**AppDbContext Mock:**
```csharp
var mockDbContext = new Mock<AppDbContext>();
mockDbContext
    .Setup(db => db.Produtos.FindAsync(id))
    .ReturnsAsync(produto);
```

**IMapper Mock (AutoMapper):**
```csharp
var mockMapper = new Mock<IMapper>();
mockMapper
    .Setup(m => m.Map<ProdutoResponse>(produto))
    .Returns(response);
```

**ILogger Mock:**
```csharp
var mockLogger = new Mock<ILogger<ProdutoService>>();
// Logger apenas registra, não afeta comportamento
```

**IProdutoService Mock (Endpoint Tests):**
```csharp
var mockService = new Mock<IProdutoService>();
mockService
    .Setup(s => s.ListarProdutosAsync(1, 10))
    .ReturnsAsync(paginatedResponse);
```

---

## 📊 Cobertura de Código - Alvo

| Componente | Tipo | Alvo | Status |
|-----------|------|------|--------|
| **ProdutoService** | Métodos | 100% | ✅ |
| **ProdutoValidator** | Regras | 95%+ | ✅ |
| **Endpoints** | Paths HTTP | 100% | ✅ |
| **Error Handling** | Middleware | 90%+ | ✅ |
| **DTOs** | Mapping | 80%+ | ✅ |

---

## 🎨 Convenções de Nomenclatura de Testes

Todos os testes seguem o padrão: **MethodName_Scenario_ExpectedResult**

```csharp
// ✅ BOM
public async Task ListarProdutosAsync_WithValidPagination_ReturnsPaginatedProducts()

// ✅ BOM
public async Task PostProduto_WithInvalidEmail_Returns422UnprocessableEntity()

// ❌ RUIM
public async Task TestListarProdutos()

// ❌ RUIM
public async Task Test1()
```

---

## 🔄 Ciclo de Vida dos Testes

### **Setup (Arrange)**
1. Criar dados mock representativos
2. Configurar comportamento dos mocks
3. Preparar objeto sob teste (Service/Validator)

### **Executar (Act)**
1. Chamar método sendo testado
2. Capturar resultado ou exceção

### **Verificar (Assert)**
1. Validar resultado com FluentAssertions
2. Verificar chamadas de mocks com `.Verify()`
3. Validar exceções com `.ThrowsAsync<T>()`

---

## 🛠️ Exemplo Completo: Teste de Criação

```csharp
[Fact]
public async Task CriarProdutoAsync_WithValidRequest_CreatesProduto()
{
    // Arrange - Preparar dados
    var request = new CriarProdutoRequest
    {
        Nome = "Mouse Logitech",
        Descricao = "Wireless USB",
        Preco = 150.00m,
        Estoque = 50,
        ContatoEmail = "vendor@example.com"
    };

    var produto = new Produto
    {
        Id = 1, 
        Nome = request.Nome,
        // ... demais campos
        DataCriacao = DateTime.UtcNow
    };

    // Configurar mocks
    _mockDbContext
        .Setup(db => db.Produtos.AddAsync(It.IsAny<Produto>(), default))
        .Returns(ValueTask.FromResult((EntityEntry<Produto>)null!));

    _mockMapper
        .Setup(m => m.Map<Produto>(request))
        .Returns(produto);

    // Act - Executar
    var result = await _service.CriarProdutoAsync(request);

    // Assert - Validar
    result.Should().NotBeNull();
    result.Id.Should().Be(1);
    result.Nome.Should().Be("Mouse Logitech");
    
    // Verificar que SaveChanges foi chamado
    _mockDbContext.Verify(db => db.SaveChangesAsync(default), Times.Once);
}
```

---

## ⚠️ Tratamento de Exceções em Testes

### **Teste de Exceção Esperada**
```csharp
[Fact]
public async Task CriarProdutoAsync_WithInvalidName_ThrowsValidationException()
{
    var request = new CriarProdutoRequest { Nome = "" };
    
    await Assert.ThrowsAsync<ValidationException>(() =>
        _service.CriarProdutoAsync(request)
    );
}
```

### **Teste com Múltiplas Exceções Possíveis**
```csharp
[Theory]
[InlineData(0)]
[InlineData(-1)]
public async Task ListarProdutos_WithInvalidPage_ThrowsArgumentException(int page)
{
    await Assert.ThrowsAsync<ArgumentException>(() =>
        _service.ListarProdutosAsync(page, 10)
    );
}
```

---

## 📈 Próximos Passos para Melhorias de Teste

1. **Integration Tests com WebApplicationFactory**
   - Testes de ponta a ponta sem mocks
   - Testes com banco de dados SQLite em memória
   
2. **Performance Tests**
   - Benchmark para paginação com 100k produtos
   - Testes de stress para endpoints

3. **Security Tests**
   - Validação de JWT/Authentication
   - Testes de autorização

4. **Data-Driven Tests**
   - Mais uso de `[Theory]` com `[InlineData]`
   - CSV/JSON data sources

---

## 📚 Referências

- [xUnit Documentation](https://xunit.net/docs/getting-started/netcore)
- [Moq Quick Start](https://github.com/moq/moq4/wiki/Quickstart)
- [FluentAssertions](https://fluentassertions.com/)
- [REST API Best Practices](../Melhores-Praticas-API.md)
- [.NET 10 Testing Guide](https://learn.microsoft.com/en-us/dotnet/core/testing/)

---

## 📝 Autor

Criado como parte do projeto educacional **ProdutosAPI**  
Framework: .NET 10 + Minimal API  
Versão: 2.0.0

**Última Atualização**: 2025
