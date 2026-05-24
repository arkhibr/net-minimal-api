# Pedidos — Vertical Slice e Domínio Rico

> Complemento didático: para integração externa com APIs e JSON complexo, veja [04-PIX.md](04-PIX.md), que cobre `HttpClientFactory`, idempotência e servidor mock auto-contido.

Para entender a arquitetura do Catálogo (CA híbrida em camadas), explore `src/Catalogo/Catalogo.API/Endpoints/`.

---

## 1. O Problema com Camadas Horizontais

Arquiteturas tradicionais em camadas (Endpoints → Services → Data) funcionam bem até um ponto. Uma mudança no domínio exige edições em múltiplos lugares:

> **Exemplo:** Adicionar um novo campo `Desconto` ao Catálogo exigiria tocar em:
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
  ├─ CreatePedidoCommand.cs      # Command + Handler (no mesmo arquivo)
  ├─ CreatePedidoValidator.cs    # Validações de entrada
  └─ CreatePedidoEndpoint.cs     # Rota HTTP (implementa IEndpoint)
```

> **Padrão do projeto:** Handler e Command vivem no mesmo arquivo. Isso reduz a contagem de arquivos por slice sem perder coesão.

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

#### 2.1 Command (DTO de entrada)

```csharp
public sealed record CreatePedidoCommand(List<CreatePedidoItemDto> Itens);
public sealed record CreatePedidoItemDto(int ProdutoId, int Quantidade);
```

#### 2.2 Validator (FluentValidation)

```csharp
public sealed class CreatePedidoValidator : AbstractValidator<CreatePedidoCommand>
{
    public CreatePedidoValidator()
    {
        RuleFor(x => x.Itens)
            .NotEmpty().WithMessage("Pedido precisa ter ao menos um item");

        RuleForEach(x => x.Itens).ChildRules(item =>
        {
            item.RuleFor(i => i.ProdutoId).GreaterThan(0);
            item.RuleFor(i => i.Quantidade).GreaterThan(0);
        });
    }
}
```

#### 2.3 Handler (Orquestração com domínio)

```csharp
public sealed class CreatePedidoHandler(IPedidoCommandRepository repository)
{
    public async Task<Result<PedidoResponse>> HandleAsync(
        CreatePedidoCommand cmd, CancellationToken ct = default)
    {
        var pedido = Pedido.Criar();

        foreach (var itemDto in cmd.Itens)
        {
            var produto = await repository.ObterProdutoParaItemAsync(itemDto.ProdutoId, ct);
            if (produto == null)
                return Result<PedidoResponse>.Fail($"Produto {itemDto.ProdutoId} não encontrado");

            var result = pedido.AdicionarItem(produto, itemDto.Quantidade);
            if (!result.IsSuccess)
                return Result<PedidoResponse>.Fail(result.Error!);
        }

        await repository.AdicionarAsync(pedido, ct);
        await repository.SaveChangesAsync(ct);
        return Result<PedidoResponse>.Ok(PedidoResponse.From(pedido));
    }
}
```

#### 2.4 Endpoint (Rota HTTP)

```csharp
public sealed class CreatePedidoEndpoint : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app) =>
        app.MapPost("/api/v1/pedidos", async (
            CreatePedidoCommand cmd,
            CreatePedidoHandler handler,
            IValidator<CreatePedidoCommand> validator,
            CancellationToken ct) =>
        {
            var validation = await validator.ValidateAsync(cmd, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var result = await handler.HandleAsync(cmd, ct);
            return result.IsSuccess
                ? Results.Created($"/api/v1/pedidos/{result.Value!.Id}", result.Value)
                : Results.BadRequest(new { error = result.Error });
        })
        .RequireAuthorization()
        .WithTags("Pedidos");
}
```

---

## 3. IEndpoint e Auto-Discovery

**O desafio:** Em Vertical Slice, cada slice tem seu próprio endpoint. Registrá-los manualmente seria tedioso.

**A solução:** Interface comum `IEndpoint` + descoberta via reflexão.

```csharp
// src/Shared/Common/IEndpoint.cs
public interface IEndpoint
{
    void MapEndpoints(IEndpointRouteBuilder app);
}
```

No `Program.cs`:
```csharp
builder.Services.AddEndpointsFromAssembly(typeof(Program).Assembly);
```

Isso varre todos os tipos implementando `IEndpoint` e chama `.MapEndpoints()` automaticamente. Basta criar `NovoSliceEndpoint : IEndpoint` e ela será descoberta — sem cadastro manual.

---

## 4. Modelo Anêmico vs Domínio Rico

### Produto Hipotético (Anêmico)

O exemplo abaixo mostra como seria um `Produto` puramente anêmico — sem regras encapsuladas:

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

    public int Id { get; private set; }
    public StatusPedido Status { get; private set; } = StatusPedido.Rascunho;
    public decimal Total { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public IReadOnlyCollection<PedidoItem> Itens => _itens.AsReadOnly();

    // Factory — pedido nasce em Rascunho, sem cliente atrelado nessa versão
    public static Pedido Criar() => new() { CriadoEm = DateTime.UtcNow };

    public Result AdicionarItem(Produto produto, int quantidade)
    {
        if (Status != StatusPedido.Rascunho)
            return Result.Fail("Só é possível adicionar itens em pedido em rascunho");

        if (quantidade <= 0)
            return Result.Fail("Quantidade deve ser positiva");

        if (produto.Estoque < quantidade)
            return Result.Fail("Estoque insuficiente");

        _itens.Add(new PedidoItem(produto, quantidade));
        Total = _itens.Sum(i => i.Total);
        return Result.Ok();
    }

    public Result Confirmar()
    {
        if (Status != StatusPedido.Rascunho)
            return Result.Fail("Apenas pedidos em rascunho podem ser confirmados");

        if (_itens.Count == 0)
            return Result.Fail("Pedido precisa ter ao menos um item");

        if (Total < 10m)
            return Result.Fail("Total mínimo do pedido é R$ 10,00");

        Status = StatusPedido.Confirmado;
        return Result.Ok();
    }

    public Result Cancelar(string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            return Result.Fail("Motivo do cancelamento é obrigatório");

        if (Status == StatusPedido.Cancelado)
            return Result.Fail("Pedido já está cancelado");

        Status = StatusPedido.Cancelado;
        return Result.Ok();
    }
}

public enum StatusPedido { Rascunho, Confirmado, Cancelado }
```

**Características:**
- Setters privados — estado só muda via métodos do agregado
- Métodos retornam `Result<T>` para sucesso/falha
- Status nasce em `Rascunho`; transições controladas por `Confirmar()` e `Cancelar(motivo)`
- `Cancelar` exige motivo explícito — invariante de domínio
- Validações integradas e invariantes verificadas em cada transição

| Aspecto | Produto (Anêmico) | Pedido (Rico) |
|---------|-------------------|---------------|
| **Define-se em** | Apenas propriedades | Propriedades + métodos |
| **Validação "Preço > 0"** | Em `ProdutoValidator` | Em construtor / value object |
| **"Não vender sem estoque"** | Em `ProdutoService` | Em `Pedido.AdicionarItem()` |
| **Quem orquestra?** | `ProdutoService` | `Pedido.Criar()`, `Pedido.AdicionarItem()`, `Pedido.Confirmar()`, `Pedido.Cancelar()` |
| **Total de Pedido** | Calculado em `Service` | Recalculado pelo agregado a cada item adicionado |
| **Teste** | Testa `Service.CancelarAsync()` | Testa `Pedido.Cancelar(motivo)` direto |
| **Classe tem identidade?** | Não, é apenas storage | Sim, entidade com regras |

---

## 5. Result Pattern

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

## 6. Quando Usar Cada Padrão

### Use Clean Architecture (Camadas) quando:
- Domínio é simples (poucos agregados, poucas regras)
- Muitos endpoints genéricos (CRUD tradicional)
- Equipe pequena / projeto pequeno
- Mudanças são raras e isoladas

**Exemplo:** Catálogo — `Atributo` e `Mídia` (CRUD simples, sem invariantes de negócio)

### Use Vertical Slice (Feature Folders) quando:
- Domínio é complexo (muitos agregados, invariantes)
- Cada feature tem lógica específica
- Equipe média/grande
- Escalabilidade horizontal (features independentes)

**Exemplo:** Pedidos — lógica de negócio embarcada no agregado

---

## 7. Testes em Ambas as Arquiteturas

### Testando Clean Architecture (Catálogo)

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
public void Pedido_AdicionarItem_QuandoCancelado_DeveRetornarFalha()
{
    // Arrange
    var pedido = Pedido.Criar();
    pedido.Cancelar("teste");
    var produto = new Produto { Nome = "Test", Preco = 10, Estoque = 100 };

    // Act
    var result = pedido.AdicionarItem(produto, 1);

    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Error.Should().Be("Só é possível adicionar itens em pedido em rascunho");
}
```

**Foco:** Testa invariantes do agregado direto, sem dependência de banco ou HTTP.

---

## 8. Checklist: Montando um Novo Slice

Quando for adicionar um novo slice de Pedidos (3 arquivos por slice — Handler vive no mesmo arquivo do Command):

- [ ] Criar pasta `src/Pedidos/NovoSlice/`
- [ ] Criar `NovoSliceCommand.cs` (contém **Command + Handler** no mesmo arquivo)
- [ ] Criar `NovoSliceValidator.cs` (FluentValidation, quando aplicável)
- [ ] Criar `NovoSliceEndpoint.cs` (implementa `IEndpoint.MapEndpoints`)
- [ ] Adicionar método ao agregado `Pedido` (se necessário)
- [ ] Adicionar nova operação à `IPedidoCommandRepository` ou `IPedidoQueryRepository` (se necessário)
- [ ] Criar testes em `tests/ProdutosAPI.Tests/Integration/Pedidos/`
- [ ] Testar via `dotnet run` + Swagger

---

## 9. Referências no Código

### Catálogo (CA Híbrida)
- Endpoints: [src/Catalogo/Catalogo.API/Endpoints/Produtos/ProdutoEndpoints.cs](../src/Catalogo/Catalogo.API/Endpoints/Produtos/ProdutoEndpoints.cs)
- Service: [src/Catalogo/Catalogo.Application/Services/ProdutoService.cs](../src/Catalogo/Catalogo.Application/Services/ProdutoService.cs)
- Testes: [tests/ProdutosAPI.Tests/Integration/](../tests/ProdutosAPI.Tests/Integration/)

### Vertical Slice (Pedidos)
- Domain: [src/Pedidos/Domain/](../src/Pedidos/Domain/)
- CreatePedido: [src/Pedidos/CreatePedido/](../src/Pedidos/CreatePedido/)
- Result Pattern: [src/Shared/Common/Result.cs](../src/Shared/Common/Result.cs)
- Testes: [tests/ProdutosAPI.Tests/Integration/](../tests/ProdutosAPI.Tests/Integration/)

---

## 10. Comparativo Final

| Dimensão | Catálogo (Produto) | Vertical Slice (Pedidos) |
|----------|--------------------|--------------------------|
| **Organização** | Por camada | Por feature |
| **Diretório** | `src/Catalogo/Catalogo.*` | `src/Pedidos/` |
| **Independência** | Fraca (mudanças globais) | Forte (slice isolada) |
| **Modelo** | Anêmico / híbrido | Rico |
| **Validação** | Em Validator + Service | No agregado + Validator |
| **Erro** | Exception | Result pattern |
| **Coesão** | Baixa (espalhada) | Alta (tudo junto) |
| **Teste** | Testa serviço isolado | Testa agregado direto |
| **Escalabilidade** | Até ~50 endpoints | 100+ features |
| **Quando usar** | Domínio simples | Domínio complexo |
