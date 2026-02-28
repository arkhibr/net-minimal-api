# Vertical Slice Architecture & Domínio Rico

Este guia conceitual explica a **segunda abordagem arquitetural** adotada no projeto para o caso de uso de **Pedidos**, enquanto os produtos continuam usando a arquitetura em camadas.

Se você ainda não entendeu a primeira abordagem (Clean Architecture em camadas), comece por [MELHORES-PRATICAS-MINIMAL-API.md](MELHORES-PRATICAS-MINIMAL-API.md) e explore `src/Produtos/Endpoints/ProdutoEndpoints.cs`.

---

## 1. O Problema com Camadas Horizontais

Arquiteturas tradicionais em camadas (Endpoints → Services → Data) funcionam bem até um ponto. Uma mudança no domínio exige edições em múltiplos lugares:

> **Exemplo:** Adicionar um novo campo `Desconto` aos Produtos exigia tocar em:
> 1. `Produto.cs` — adicionar propriedade
> 2. `CriarProdutoValidator.cs` — adicionar regra
> 3. `AtualizarProdutoValidator.cs` — idem
> 4. `ProdutoDTO.cs` — adicionar Request/Response
> 5. `MappingProfile.cs` — adicionar mapping
> 6. `AppDbContext.cs` — configurar
> 7. Database — executar migration

Essa dispersão acontece porque o domínio é **anêmico** — entidades são apenas contêineres de dados, e toda a lógica vive em serviços genéricos.

---

## 2. Vertical Slice Architecture

### O que é?

Uma **slice** (fatia) representa **um único caso de uso** ou funcionalidade. Todas as peças necessárias para executá-la residem em uma pasta isolada:

```
src/Pedidos/CreatePedido/
  ├─ CreatePedidoCommand.cs      # Input (DTO)
  ├─ CreatePedidoValidator.cs    # Validações de entrada
  ├─ CreatePedidoHandler.cs      # Orquestração
  └─ CreatePedidoEndpoint.cs     # Rota HTTP
```

Cada slice é **independente**: alterar o comportamento de criação de pedido não afeta diretamente outras operações.

### Benefícios

| Benefício | Descrição |
|-----------|-----------|
| **Coesão Alta** | Tudo para fazer uma tarefa está num lugar |
| **Independência** | Cada slice pode evoluir isoladamente |
| **Escalabilidade** | Fácil adicionar novos casos de uso |
| **Onboarding** | Novo dev consegue entender um caso de uso completo rápido |
| **Low Coupling** | Mexer em uma slice não quebra outras |

### Anatomia de um Slice (exemplo: CreatePedido)

#### 1. **Command** (DTO de entrada)
```csharp
public sealed record CreatePedidoCommand(
    string ClienteNome,
    List<AddItemCommand> Itens
);

public sealed record AddItemCommand(
    int ProdutoId,
    int Quantidade
);
```

#### 2. **Validator** (FluentValidation)
```csharp
public sealed class CreatePedidoValidator : AbstractValidator<CreatePedidoCommand>
{
    public CreatePedidoValidator()
    {
        RuleFor(x => x.ClienteNome)
            .NotEmpty().WithMessage("Nome do cliente é obrigatório")
            .Length(3, 100);

        RuleForEach(x => x.Itens)
            .NotNull()
            .DependentRules(() =>
            {
                RuleFor(x => x.ProdutoId).GreaterThan(0);
                RuleFor(x => x.Quantidade).GreaterThan(0);
            });
    }
}
```

#### 3. **Handler** (Orquestração com domínio)
```csharp
public sealed class CreatePedidoHandler(
    AppDbContext context,
    IValidator<CreatePedidoCommand> validator
)
{
    public async Task<Result<int>> HandleAsync(CreatePedidoCommand command)
    {
        var validationResult = await validator.ValidateAsync(command);
        if (!validationResult.IsValid)
            return Result<int>.Fail("Validação falhou");

        var pedido = Pedido.Create(command.ClienteNome);

        foreach (var item in command.Itens)
        {
            var produto = await context.Produtos.FindAsync(item.ProdutoId);
            if (produto == null)
                return Result<int>.Fail($"Produto {item.ProdutoId} não encontrado");

            var result = pedido.AddItem(produto, item.Quantidade);
            if (!result.IsSuccess)
                return Result<int>.Fail(result.Error);
        }

        context.Pedidos.Add(pedido);
        await context.SaveChangesAsync();
        return Result<int>.Ok(pedido.Id);
    }
}
```

#### 4. **Endpoint** (Rota HTTP)
```csharp
public sealed class CreatePedidoEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder routes) =>
        routes
            .MapPost("/api/v1/pedidos")
            .Produces<PedidoResponse>(StatusCodes.Status201Created)
            .WithName("Create Pedido")
            .WithOpenApi()
            .RequireAuthorization();

    public async Task<IResult> Handle(
        CreatePedidoCommand command,
        CreatePedidoHandler handler
    )
    {
        var result = await handler.HandleAsync(command);
        if (!result.IsSuccess)
            return Results.BadRequest(new { error = result.Error });

        return Results.Created($"/api/v1/pedidos/{result.Value}", new { id = result.Value });
    }
}
```

### IEndpoint e Descoberta Automática

**O desafio:** Em Vertical Slice, cada slice tem seu próprio endpoint. Registrá-los manualmente seria tedioso.

**A solução:** Interface comum `IEndpoint` + descoberta via reflexão

```csharp
// src/Shared/Common/IEndpoint.cs
public interface IEndpoint
{
    void Map(IEndpointRouteBuilder routes);
}
```

No `Program.cs`:
```csharp
builder.Services.AddEndpointsFromAssembly(typeof(Program).Assembly);
```

Isso varre todos os tipos implementando `IEndpoint` e chama `.Map()` automaticamente. Basta criar `NovoSliceEndpoint : IEndpoint` e ela será descoberta!

---

## 3. Modelo Anêmico vs Domínio Rico

### Produto (Anêmico) — Clean Architecture

```csharp
public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public decimal Preco { get; set; }
    public int Estoque { get; set; }
    public bool Ativo { get; set; }
    // Nenhuma regra de negócio encapsulada aqui!
}
```

**Características:**
- Apenas propriedades (get/set)
- Sem métodos de negócio
- Validações em `ProdutoValidator`
- Lógica em `ProdutoService`

**Onde as regras vivem:**
- "Preço não pode ser negativo" → `ProdutoValidator`
- "Não pode vender fora do estoque" → `ProdutoService`
- "Ativo garante disponibilidade" → `ProdutoService`

### Pedido (Rico) — Vertical Slice

```csharp
public sealed class Pedido
{
    private readonly List<PedidoItem> _itens = new();

    public int Id { get; set; }
    public string ClienteNome { get; set; }
    public PedidoStatus Status { get; set; }
    
    // Propriedade calculada!
    public decimal Total => _itens.Sum(i => i.Total);

    // **Regras encapsuladas em métodos:**

    public static Result<Pedido> Create(string clienteNome)
    {
        if (string.IsNullOrWhiteSpace(clienteNome))
            return Result<Pedido>.Fail("Nome do cliente obrigatório");

        if (clienteNome.Length > 100)
            return Result<Pedido>.Fail("Nome muito longo");

        return Result<Pedido>.Ok(new Pedido 
        { 
            ClienteNome = clienteNome,
            Status = PedidoStatus.Aberto,
            DataCriacao = DateTime.Now
        });
    }

    public Result AddItem(Produto produto, int quantidade)
    {
        if (Status != PedidoStatus.Aberto)
            return Result.Fail("Pedido não está aberto");

        if (quantidade <= 0)
            return Result.Fail("Quantidade deve ser positiva");

        if (produto.Estoque < quantidade)
            return Result.Fail("Estoque insuficiente");

        _itens.Add(new PedidoItem(produto, quantidade));
        return Result.Ok();
    }

    public Result Cancel()
    {
        if (Status != PedidoStatus.Aberto)
            return Result.Fail("Só pedidos abertos podem ser cancelados");

        Status = PedidoStatus.Cancelado;
        return Result.Ok();
    }
}
```

**Características:**
- Propriedades + métodos
- Métodos retornam `Result<T>` para sucesso/falha
- Identidade própria (invariantes)
- Validações integradas

| Aspecto | Produto (Anêmico) | Pedido (Rico) |
|---------|-------------------|---------------|
| **Define-se em** | Apenas propriedades | Propriedades + métodos |
| **Validação "Preço > 0"** | Em `ProdutoValidator` | Em `Pedido.Create()` |
| **"Não vender sem estoque"** | Em `ProdutoService` | Em `Pedido.AddItem()` |
| **Quem orquestra?** | `ProdutoService` | `Pedido.Create()`, `Pedido.AddItem()` |
| **Total de Pedido** | Calculado em `Service` | Propriedade `Total` do próprio agregado |
| **Teste** | Testa `Service.CancelarAsync()` | Testa `Pedido.Cancel()` direto |
| **Classe tem identidade?** | Não, é apenas storage | Sim, entidade com regras |

---

## 4. Result Pattern

Para distinguir entre sucesso e erro **sem lançar exceções**, Vertical Slice usa o **Result pattern**:

```csharp
public abstract record Result(bool IsSuccess, string? Error)
{
    public static Result Ok() => new SuccessResult();
    public static Result Fail(string error) => new FailureResult(error);

    public sealed record SuccessResult : Result(true, null);
    public sealed record FailureResult(string ErrorMessage) : Result(false, ErrorMessage);
}

public abstract record Result<T>(bool IsSuccess, T? Value, string? Error)
{
    public static Result<T> Ok(T value) => new SuccessResult(value);
    public static Result<T> Fail(string error) => new FailureResult(error);

    public sealed record SuccessResult(T Value) : Result<T>(true, Value, null);
    public sealed record FailureResult(string ErrorMessage) : Result<T>(false, default, ErrorMessage);
}
```

**Vantagens:**
- Sem overhead de exception handling
- Erros de negócio são esperados
- Code flow é linear e legível
- Performance melhor

---

## 5. Quando Usar Cada Padrão?

### Use Clean Architecture (Camadas) quando:
- ✅ Domínio é simples (poucos agregados, poucas regras)
- ✅ Muitos endpoints genéricos (CRUD tradicional)
- ✅ Equipe pequena / projeto pequeno
- ✅ Mudanças são raras e isoladas

**Exemplo:** Produtos — 6 endpoints simples de CRUD

### Use Vertical Slice (Feature Folders) quando:
- ✅ Domínio é complexo (muitos agregados, invariantes)
- ✅ Cada feature tem lógica específica
- ✅ Equipe média/grande
- ✅ Escalabilidade horizontal (features independentes)

**Exemplo:** Pedidos — lógica de negócio embarcada no agregado

---

## 6. Testes em Ambas as Arquiteturas

### Testando Clean Architecture (Produto)

```csharp
[Fact]
public async Task DeletarProduto_DeveRetornarTrue()
{
    // Arrange
    var service = new ProdutoService(context);
    var produto = new Produto { Nome = "Test", Preco = 10 };
    context.Produtos.Add(produto);
    await context.SaveChangesAsync();

    // Act
    var result = await service.DeletarProdutoAsync(produto.Id);

    // Assert
    result.Should().BeTrue();
}
```

**Foco:** Testa comportamento de um serviço isolado.

### Testando Vertical Slice (Pedido)

```csharp
[Fact]
public void Pedido_AddItem_QuandoStatusNaoAberto_DeveRetornarFalha()
{
    // Arrange
    var pedido = new Pedido { ClienteNome = "Cliente", Status = PedidoStatus.Cancelado };
    var produto = new Produto { Nome = "Test", Preco = 10, Estoque = 100 };

    // Act
    var result = pedido.AddItem(produto, 1);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Error.Should().Be("Pedido não está aberto");
}
```

**Foco:** Testa invariantes do agregado direto.

---

## 7. Fluxo Completo: Criar Pedido

### Clean Architecture (Produtos)

```
GET /api/v1/produtos/{id}
  ↓
ProdutoEndpoints mapeia rota
  ↓
Chama ProdutoService.ObterProdutoAsync()
  ↓
Service query context
  ↓
Mapeia resultado para ProdutoResponse
  ↓
Returns 200 + DTO
```

### Vertical Slice (Pedidos)

```
POST /api/v1/pedidos (requer JWT)
  ↓
CreatePedidoEndpoint descobre rota via IEndpoint
  ↓
CreatePedidoValidator valida entrada
  ↓
CreatePedidoHandler orquestra:
    ├─ Pedido.Create(clienteNome) → Result<Pedido>
    ├─ Para cada item, pedido.AddItem(...) → Result
    ├─ Se falha, retorna erro
    └─ context.SaveChangesAsync()
  ↓
Endpoint checka result.IsSuccess
  ↓
Returns 201 + DTO ou 400 + erro
```

---

## 8. Checklist: Montando um Novo Slice

Quando for adicionar um novo slice de Pedidos:

- [ ] Criar pasta `src/Pedidos/NovoSlice/`
- [ ] Criar `NovoSliceCommand.cs` (DTO)
- [ ] Criar `NovoSliceValidator.cs` (FluentValidation)
- [ ] Criar `NovoSliceHandler.cs` (orquestração)
- [ ] Criar `NovoSliceEndpoint.cs` (implementa `IEndpoint`)
- [ ] Adicionar método ao agregado `Pedido` (se necessário)
- [ ] Criar testes em `ProdutosAPI.Tests/Integration/Pedidos/`
- [ ] Testar via `dotnet run` + Swagger

---

## 9. Referências no Código

### Clean Architecture (Produtos)
- Endpoints: [src/Produtos/Endpoints/ProdutoEndpoints.cs](../src/Produtos/Endpoints/ProdutoEndpoints.cs)
- Service: [src/Produtos/Services/ProdutoService.cs](../src/Produtos/Services/ProdutoService.cs)
- Validators: [src/Validators/ProdutoValidator.cs](../src/Validators/ProdutoValidator.cs)
- Testes: [ProdutosAPI.Tests/Services/ProdutoServiceTests.cs](../ProdutosAPI.Tests/Services/ProdutoServiceTests.cs)

### Vertical Slice (Pedidos)
- Domain: [src/Pedidos/Domain/](../src/Pedidos/Domain/)
- CreatePedido: [src/Pedidos/CreatePedido/](../src/Pedidos/CreatePedido/)
- Result Pattern: [src/Shared/Common/Result.cs](../src/Shared/Common/Result.cs)
- Testes: [ProdutosAPI.Tests/Integration/Pedidos/](../ProdutosAPI.Tests/Integration/Pedidos/)

---

## 10. Resumo Comparativo Final

| Dimensão | Clean (Produtos) | Vertical Slice (Pedidos) |
|----------|-----------------|--------------------------|
| **Organização** | Por camada | Por feature |
| **Diretório** | `src/Produtos/Endpoints/`, `src/Produtos/Services/` | `src/Pedidos/` |
| **Independência** | Fraca (mudanças globais) | Forte (slice isolada) |
| **Modelo** | Anêmico | Rico |
| **Validação** | Em Validator + Service | No agregado + Validator |
| **Erro** | Exception | Result pattern |
| **Coesão** | Baixa (espalhada) | Alta (tudo junto) |
| **Teste** | Testa serviço isolado | Testa agregado direto |
| **Escalabilidade** | Até ~50 endpoints | 100+ features |
| **Quando usar** | Domínio simples | Domínio complexo |

---

## 🎓 Próximas Leituras

1. **DDD (Domain-Driven Design)** — Aprofunde em agregados e bounded contexts
2. **CQRS** — Separe reads e writes
3. **Event Sourcing** — Rastreie histórico de mudanças
4. **Mediator Pattern** — Distribua handlers (MediatR)

---

**Este projeto é um playground real para aprender ambos os padrões lado a lado. Explore, modifique, e veja as diferenças na prática!**

🎉 **Feliz aprendizado!**
