using ProdutosAPI.Produtos.Domain.Common;

namespace ProdutosAPI.Produtos.Domain.ValueObjects;

public sealed record CategoriaProduto
{
    public const int MaxLength = 50;

    private CategoriaProduto(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<CategoriaProduto> Criar(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<CategoriaProduto>.Fail("Categoria é obrigatória.");

        var normalizado = value.Trim();
        if (normalizado.Length > MaxLength)
            return Result<CategoriaProduto>.Fail($"Categoria não pode exceder {MaxLength} caracteres.");

        return Result<CategoriaProduto>.Ok(new CategoriaProduto(normalizado));
    }

    public static CategoriaProduto Reconstituir(string? value)
    {
        var result = Criar(value);
        if (!result.IsSuccess || result.Value is null)
            throw new InvalidOperationException(result.Error);
        return result.Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(CategoriaProduto categoria) => categoria.Value;
}
