using ProdutosAPI.Produtos.Domain.Common;

namespace ProdutosAPI.Produtos.Domain.ValueObjects;

public sealed record DescricaoProduto
{
    public const int MaxLength = 500;

    private DescricaoProduto(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<DescricaoProduto> Criar(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<DescricaoProduto>.Fail("Descrição é obrigatória.");

        var normalizado = value.Trim();
        if (normalizado.Length > MaxLength)
            return Result<DescricaoProduto>.Fail($"Descrição não pode exceder {MaxLength} caracteres.");

        return Result<DescricaoProduto>.Ok(new DescricaoProduto(normalizado));
    }

    public static DescricaoProduto Reconstituir(string? value)
    {
        var result = Criar(value);
        if (!result.IsSuccess || result.Value is null)
            throw new InvalidOperationException(result.Error);
        return result.Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(DescricaoProduto descricao) => descricao.Value;
}
