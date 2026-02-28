# Estratégia de Testes - ProdutosAPI

## 📋 Sumário Executivo

A suite de testes é distribuída em **2 projetos paralelos**:
- **ProdutosAPI.Tests**: Cobre 111 casos para Clean Architecture (Produtos)
- **Pedidos.Tests**: Cobre 11 casos para Vertical Slice + Domínio Rico (Pedidos)

**Total**: **122 casos automatizados** em três camadas diferentes para validar os dois padrões arquiteturais.

**Framework de Testes**: xUnit  
**Mocking**: Moq + NSubstitute  
**Assertions**: FluentAssertions  
**Categorias**: Domain Unit, Service Unit, Integration HTTP  
**Cobertura alvo**: ≥ 80% das operações críticas  

---

## 🎯 Objetivos dos Testes

1. **Validação de Funcionalidade**: Endpoints de Produtos e Pedidos respondem com códigos e corpos corretos.
2. **Cobertura de Regras de Negócio**: Agregado `Pedido` e serviços de produto mantêm invariantes.
3. **Teste de Validações**: Todos os validadores (incluindo comandos de slice) são exercitados.
4. **Regressão**: Prevenir quebras em refatorações das duas arquiteturas.
5. **Documentação Viva**: Testes servem como exemplos de chamadas HTTP e uso de API.

---

## 📁 Estrutura de Projetos de Testes

### **ProdutosAPI.Tests/** (Clean Architecture - Produtos)
```
ProdutosAPI.Tests/
├── ProdutosAPI.Tests.csproj
├── Domain/
│   └── ProdutoTests.cs               # Testes de modelo de Produtos
├── Services/
│   └── ProdutoServiceTests.cs        # 35 testes de serviço
├── Endpoints/
│   └── ProdutoEndpointsTests.cs      # 18 testes HTTP
├── Validators/
│   └── ProdutoValidatorTests.cs      # 20+ testes de validação
├── Unit/
│   ├── Common/
│   ├── Domain/
│   └── Services/
├── Integration/
│   └── Pedidos/
├── Builders/
│   └── ProdutoBuilder.cs
└── ESTRATEGIA-DE-TESTES.md           # Documentação
```

### **Pedidos.Tests/** (Vertical Slice - Pedidos)
```
Pedidos.Tests/
├── Pedidos.Tests.csproj
├── Builders/
│   └── ProdutoTestBuilder.cs         # Construtor para testes
├── Domain/
│   └── PedidoTests.cs                # 11 testes de agregado Pedido
└── ESTRATEGIA-TESTES-PEDIDOS.md      # Documentação
```

---

## 🧪 Categorias de Testes

### 1. **Domain Unit Tests**
Localizadas em `ProdutosAPI.Tests/Domain/`.
Cobrem os comportamentos do agregado `Pedido` e classes de valor associadas.
- ✅ Criação de pedido com todos os campos válidos
- ❌ Rejeita pedido com total negativo
- ✅ Adição de item valida estoque e preço
- ❌ Impede cancelamento de pedido já enviado
- ✅ Cálculo de total incorporando quantidade e preço

(40+ casos diferentes definem invariantes e transformar exceções em `Result`.)

### 2. **Service Unit Tests**
Localizadas em `ProdutosAPI.Tests/Services/`.
Testam cada serviço isoladamente usando banco em memória ou mocks.
- **ProdutoServiceTests** (35 testes): cobertura completa de métodos CRUD, paginação, filtros, soft-delete.
- **PedidoServiceTests** (se aplicável): executar uso de `Pedido` agregado com handlers.

Exemplo de padrão AAA:
```csharp
// Arrange
var service = new ProdutoService(...);
var request = new CriarProdutoRequest { Nome = "X" };

// Act
var result = await service.CriarProdutoAsync(request);

// Assert
result.Nome.Should().Be("X");
```

### 3. **Integration HTTP Tests**
Localizadas em `ProdutosAPI.Tests/Endpoints/`.
Testam a API como cliente usando `WebApplicationFactory`.
- 18 testes para Produtos (mapeamento dos 6 endpoints)
- 18 testes para Pedidos, incluindo fluxo de autenticação JWT
- Verificação de status codes, esquemas de resposta e headers
- Simulação de erros (404, 422, 401, 409)

Exemplo:
```csharp
var response = await _client.GetAsync("/api/v1/pedidos/1");
response.StatusCode.Should().Be(HttpStatusCode.NotFound);
```

### 4. **Validation Tests**
Localizadas em `ProdutosAPI.Tests/Validators/`.
Confiram regras de todos os validadores:
- `ProdutoValidator` (mais de 20 casos)
- `PedidoCommand` validadores (10+ casos) verificando obrigatoriedade, ranges e formatos

---

## 🚀 Como Executar os Testes

### Todos os testes
```bash
cd net-minimal-api
dotnet test
```

### Executar categorias específicas
```bash
dotnet test --filter "Category=Domain"
dotnet test --filter "Namespace~=Endpoints"
```

### Um teste específico
```bash
dotnet test --filter "Name=CriarPedidoAsync_WithValidCommand_ReturnsSuccess"
```

### Cobertura de código (requer dotnet-reportgenerator)
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
reportgenerator -reports:"coverage.opencover.xml" -targetdir:"coveragereport"
```

---

## 🔍 Estratégia de Mocking

### AppDbContext Mock
```csharp
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase("TestDb")
    .Options;
var context = new AppDbContext(options);
```
*...continua...*

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
