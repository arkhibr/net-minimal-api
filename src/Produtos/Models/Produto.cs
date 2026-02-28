using ProdutosAPI.Shared.Common;

namespace ProdutosAPI.Produtos.Models;

public class Produto
{
    public static readonly decimal PrecoMinimo = 0.01m;
    public static readonly int EstoqueMaximo = 99_999;

    private Produto() { }

    public int Id { get; private set; }
    public string Nome { get; private set; } = "";
    public string Descricao { get; private set; } = "";
    public decimal Preco { get; private set; }
    public string Categoria { get; private set; } = "";
    public int Estoque { get; private set; }
    public bool Ativo { get; private set; } = true;
    public string ContatoEmail { get; private set; } = "";
    public DateTime DataCriacao { get; private set; }
    public DateTime DataAtualizacao { get; private set; }

    public static Result<Produto> Criar(
        string nome, string descricao, decimal preco,
        string categoria, int estoque, string email)
    {
        if (string.IsNullOrWhiteSpace(nome) || nome.Length < 3)
            return Result<Produto>.Fail("Nome deve ter ao menos 3 caracteres.");
        if (preco < PrecoMinimo)
            return Result<Produto>.Fail("Preço deve ser maior que zero.");
        if (estoque < 0)
            return Result<Produto>.Fail("Estoque não pode ser negativo.");
        if (string.IsNullOrWhiteSpace(email))
            return Result<Produto>.Fail("Email de contato é obrigatório.");

        var agora = DateTime.UtcNow;
        return Result<Produto>.Ok(new Produto
        {
            Nome = nome,
            Descricao = descricao,
            Preco = preco,
            Categoria = categoria,
            Estoque = estoque,
            ContatoEmail = email,
            Ativo = true,
            DataCriacao = agora,
            DataAtualizacao = agora
        });
    }

    public Result AtualizarPreco(decimal novoPreco)
    {
        if (novoPreco < PrecoMinimo)
            return Result.Fail("Preço deve ser maior que zero.");
        if (novoPreco == Preco)
            return Result.Fail("Novo preço é igual ao preço atual.");
        Preco = novoPreco;
        DataAtualizacao = DateTime.UtcNow;
        return Result.Ok();
    }

    public Result AtualizarDados(
        string? nome = null, string? descricao = null,
        string? categoria = null, string? email = null)
    {
        if (nome is not null)
        {
            if (nome.Length < 3) return Result.Fail("Nome deve ter ao menos 3 caracteres.");
            Nome = nome;
        }
        if (descricao is not null) Descricao = descricao;
        if (categoria is not null) Categoria = categoria;
        if (email is not null) ContatoEmail = email;
        DataAtualizacao = DateTime.UtcNow;
        return Result.Ok();
    }

    public Result ReporEstoque(int quantidade)
    {
        if (quantidade <= 0)
            return Result.Fail("Quantidade de reposição deve ser positiva.");
        if (Estoque + quantidade > EstoqueMaximo)
            return Result.Fail($"Estoque não pode exceder {EstoqueMaximo} unidades.");
        Estoque += quantidade;
        DataAtualizacao = DateTime.UtcNow;
        return Result.Ok();
    }

    public Result Desativar()
    {
        if (!Ativo)
            return Result.Fail("Produto já está inativo.");
        Ativo = false;
        DataAtualizacao = DateTime.UtcNow;
        return Result.Ok();
    }

    public bool TemEstoqueDisponivel(int qtd) => Ativo && Estoque >= qtd;

    internal void AjustarEstoque(int quantidade)
    {
        if (quantidade < 0) throw new InvalidOperationException("Estoque não pode ser negativo.");
        if (quantidade > EstoqueMaximo) throw new InvalidOperationException($"Estoque não pode exceder {EstoqueMaximo} unidades.");
        Estoque = quantidade;
        DataAtualizacao = DateTime.UtcNow;
    }

    // For testing purposes only
    internal void SetIdForTesting(int id) => Id = id;
}
