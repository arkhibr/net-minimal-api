using ProdutosAPI.Produtos.Domain.Common;

namespace ProdutosAPI.Produtos.Domain.ValueObjects;

public sealed record EstoqueProduto
{
    public const int Maximo = 99_999;

    private EstoqueProduto(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static Result<EstoqueProduto> Criar(int value)
    {
        if (value < 0)
            return Result<EstoqueProduto>.Fail("Estoque não pode ser negativo.");

        return Result<EstoqueProduto>.Ok(new EstoqueProduto(value));
    }

    public static EstoqueProduto Reconstituir(int value)
    {
        var result = Criar(value);
        if (!result.IsSuccess || result.Value is null)
            throw new InvalidOperationException(result.Error);
        return result.Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator int(EstoqueProduto estoque) => estoque.Value;
}
