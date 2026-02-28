# Vertical Slice Architecture & Domínio Rico

Este guia conceitual explica a segunda abordagem arquitetural adotada no projeto: **Vertical Slice Architecture** combinada com um modelo de **Domínio Rico**. Ela já está implementada no código para o caso de uso de **Pedidos**, enquanto os produtos continuam usando a arquitetura em camadas.

---

## 1. O Problema

Arquiteturas tradicionais em camadas (Endpoints → Services → Data) funcionam bem até o ponto em que um requisito exige mudanças em diversos lugares: adicionar um novo campo ao domínio, modificar um validator, atualizar o DTO, alterar um endpoint. Cada nova feature atravessa 4 ou 5 arquivos diferentes, tornando o desenvolvimento lento e a onboarding de novos desenvolvedores mais difícil.

> "Adicionar um campo de `Desconto` exigia tocar no endpoint, no serviço, no mapeamento, no contexto e no banco."

Essa dispersão de responsabilidade é chamada de *anemic domain* quando as entidades são meros contêineres de dados e a lógica é espalhada em vários serviços.

---

## 2. Vertical Slice Architecture

### O que é?

Uma *slice* (fatia) representa **uma única funcionalidade** ou caso de uso da aplicação. Todas as partes necessárias para executá-la residem em uma pasta:

```
src/Features/Pedidos/CreatePedido/
  ├─ CreatePedidoCommand.cs      # Input DTO
  ├─ CreatePedidoValidator.cs    # Regras de validação
  ├─ CreatePedidoHandler.cs      # Lógica de negócio
  └─ CreatePedidoEndpoint.cs     # Mapeia requisição HTTP
```

Cada slice é independente: alterar o comportamento de criação de pedido não afeta diretamente outros slices.

### Anatomia de um Slice

1. **Command/Query** – DTO que descreve a intenção do cliente.
2. **Validator** – Regras do FluentValidation para o comando.
3. **Handler** – Implementa a lógica usando entidades de domínio.
4. **Endpoint** – Exposição HTTP (Minimal API) usando Typed Results.
5. **Tests** – cada slice possui seus próprios testes de unidade e integração.

### IEndpoint e scan automático

Todos os endpoints implementam a interface `IEndpoint`:

```csharp
public interface IEndpoint
{
    void Map(IEndpointRouteBuilder routes);
}
```

No `Program.cs` a descoberta ocorre via reflexão:

```csharp
builder.Services.AddEndpointsFromAssembly(typeof(Program).Assembly);
```

Isso elimina a necessidade de registrar manualmente cada rota; basta adicionar uma classe `CreatePedidoEndpoint` que já será mapeada.

---

## 3. Modelo Anêmico vs Domínio Rico

### Produto (anêmico)

```csharp
public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public decimal Preco { get; set; }
    // nenhuma regra de negócio aqui
}
```

A lógica de validação e alteração vive em `ProdutoService` e em validadores separados.

### Pedido (rico)

```csharp
public sealed class Pedido
{
    private readonly List<PedidoItem> _itens = new();
    public int Id { get; private set; }
    public PedidoStatus Status { get; private set; }
    public decimal Total => _itens.Sum(i => i.Total);

    public Result AddItem(Produto produto, int quantidade)
    {
        if (Status != PedidoStatus.Aberto)
            return Result.Fail("Só é possível adicionar itens ao pedido aberto");
        if (quantidade <= 0)
            return Result.Fail("Quantidade deve ser positiva");

        _itens.Add(new PedidoItem(produto, quantidade));
        return Result.Ok();
    }

    public Result Cancel()
    {
        if (Status == PedidoStatus.Cancelado) 
            return Result.Fail("Pedido já cancelado");
        Status = PedidoStatus.Cancelado;
        return Result.Ok();
    }
}
```

Repare que regras de negócio — adicionar item, cancelar pedido, limites — estão **dentro** da classe. Nenhum serviço externo precisa conhecê-las.

---

## 4. Result Pattern

Em vez de lançar exceções para violar invariantes, o domínio retorna um `Result<T>` legível:

```csharp
public record Result(bool IsSuccess, string? Error = null);
public record Result<T>(bool IsSuccess, T? Value = default, string? Error = null) : Result(IsSuccess, Error);
```

Handlers e endpoints testam `result.IsSuccess` e propagam erros para respostas HTTP apropriadas (422, 409, etc.) sem poluir o fluxo principal com try/catch.

---

## 5. Aggregate Root

O agregado `Pedido` é o ponto de entrada único para modificações relacionadas a pedidos. Ele garante consistência interna e encapsula todos os invariantes.

- **Invariantes:** total ≥ 0, status válido, somente itens ativos adicionados.
- **Encapsulamento:** `_itens` é privado; só pode ser alterado via métodos.
- **Transações:** alterações são aplicadas em memória e depois persistidas via `AppDbContext` pelo handler.

---

## 6. Coexistência dos Padrões

O código do projeto não é "ou isto ou aquilo". Produtos continuam usando a arquitetura horizontal enquanto Pedidos usam slices.

- Mesmo `AppDbContext` com DbSet<Produto>, DbSet<Pedido>, DbSet<PedidoItem>
- Middleware (CORS, JWT, exceptions) é compartilhado
- Service layer de Produtos convive com handlers de Pedidos
- Permite evolução gradual: novas features podem adotar slices sem reescrever tudo

---

## 7. Referências no Código

- Slices: `src/Features/Pedidos/` (cada pasta é um slice)
- Agregado rico: `src/Features/Pedidos/Domain/Pedido.cs`
- Pattern Result: `src/Features/Common/Result.cs`
- Endpoint scan: `Program.cs` linha `builder.Services.AddEndpointsFromAssembly(...)`
- Testes: `ProdutosAPI.Tests/` contém pastas `Domain`, `Endpoints/Pedidos` etc.

---

Use este guia como ponto de partida para entender e aplicar Vertical Slice e Domínio Rico em seus próprios projetos. A arquitetura facilita o crescimento, torna o código mais navegável e mantém a lógica de negócio onde ela pertence.