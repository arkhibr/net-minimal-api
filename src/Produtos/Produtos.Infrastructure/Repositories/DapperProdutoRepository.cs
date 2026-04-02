using System.Data;
using System.Data.Common;
using System.Text;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProdutosAPI.Produtos.Application.Interfaces;
using ProdutosAPI.Produtos.Application.Repositories;
using ProdutosAPI.Produtos.Domain;

namespace ProdutosAPI.Produtos.Infrastructure.Repositories;

public class DapperProdutoRepository : IProdutoRepository
{
    private readonly DbContext _dbContext;
    private readonly ILogger<DapperProdutoRepository> _logger;

    public DapperProdutoRepository(IProdutoContext context, ILogger<DapperProdutoRepository> logger)
    {
        _dbContext = context as DbContext
            ?? throw new InvalidOperationException(
                "IProdutoContext precisa ser uma implementação de DbContext para suportar Dapper.");
        _logger = logger;
    }

    public Task<(IReadOnlyList<Produto> Items, int Total)> ListarAsync(
        int page, int pageSize, string? categoria = null, string? search = null) =>
        WithConnectionAsync(async connection =>
        {
            var where = new StringBuilder("WHERE Ativo = 1");
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                where.Append(" AND Categoria = @Categoria");
                parameters.Add("Categoria", categoria);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                where.Append(" AND (Nome LIKE @Search OR Descricao LIKE @Search)");
                parameters.Add("Search", $"%{search}%");
            }

            parameters.Add("Limit", pageSize);
            parameters.Add("Offset", (page - 1) * pageSize);

            var totalSql = $"SELECT COUNT(1) FROM Produtos {where};";
            var itemsSql = $@"
SELECT
    Id,
    Nome,
    Descricao,
    Preco,
    Categoria,
    Estoque,
    Ativo,
    ContatoEmail,
    DataCriacao,
    DataAtualizacao
FROM Produtos
{where}
ORDER BY DataCriacao DESC
LIMIT @Limit OFFSET @Offset;";

            var total = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(totalSql, parameters));

            var rows = await connection.QueryAsync<ProdutoRow>(
                new CommandDefinition(itemsSql, parameters));

            return ((IReadOnlyList<Produto>)rows.Select(MapToDomain).ToList(), total);
        });

    public Task<Produto?> ObterPorIdAsync(int id) =>
        WithConnectionAsync(async connection =>
        {
            const string sql = @"
SELECT
    Id,
    Nome,
    Descricao,
    Preco,
    Categoria,
    Estoque,
    Ativo,
    ContatoEmail,
    DataCriacao,
    DataAtualizacao
FROM Produtos
WHERE Id = @Id AND Ativo = 1;";

            var row = await connection.QuerySingleOrDefaultAsync<ProdutoRow>(
                new CommandDefinition(sql, new { Id = id }));

            return row is null ? null : MapToDomain(row);
        });

    public Task<Produto> AdicionarAsync(Produto produto) =>
        WithConnectionAsync(async connection =>
        {
            const string insertSql = @"
INSERT INTO Produtos (
    Nome,
    Descricao,
    Preco,
    Categoria,
    Estoque,
    Ativo,
    ContatoEmail,
    DataCriacao,
    DataAtualizacao
)
VALUES (
    @Nome,
    @Descricao,
    @Preco,
    @Categoria,
    @Estoque,
    @Ativo,
    @ContatoEmail,
    @DataCriacao,
    @DataAtualizacao
);

SELECT CAST(last_insert_rowid() AS INTEGER);";

            var newId = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(insertSql, new
                {
                    produto.Nome,
                    Descricao = produto.Descricao.Value,
                    Preco = produto.Preco.Value,
                    Categoria = produto.Categoria.Value,
                    Estoque = produto.Estoque.Value,
                    produto.Ativo,
                    produto.ContatoEmail,
                    produto.DataCriacao,
                    produto.DataAtualizacao
                }));

            var persisted = await QueryByIdAsync(connection, newId);
            return persisted ?? throw new InvalidOperationException(
                $"Falha ao carregar produto recém inserido. Id={newId}");
        });

    public Task AtualizarAsync(Produto produto) =>
        WithConnectionAsync(async connection =>
        {
            const string sql = @"
UPDATE Produtos
SET
    Nome = @Nome,
    Descricao = @Descricao,
    Preco = @Preco,
    Categoria = @Categoria,
    Estoque = @Estoque,
    ContatoEmail = @ContatoEmail,
    DataAtualizacao = @DataAtualizacao
WHERE Id = @Id AND Ativo = 1;";

            var affected = await connection.ExecuteAsync(
                new CommandDefinition(sql, new
                {
                    produto.Id,
                    produto.Nome,
                    Descricao = produto.Descricao.Value,
                    Preco = produto.Preco.Value,
                    Categoria = produto.Categoria.Value,
                    Estoque = produto.Estoque.Value,
                    produto.ContatoEmail,
                    produto.DataAtualizacao
                }));

            if (affected == 0)
            {
                _logger.LogWarning("Produto {Id} não encontrado para atualização", produto.Id);
            }
        });

    public Task<bool> DeletarAsync(int id) =>
        WithConnectionAsync(async connection =>
        {
            const string sql = @"
UPDATE Produtos
SET
    Ativo = 0,
    DataAtualizacao = @DataAtualizacao
WHERE Id = @Id AND Ativo = 1;";

            var affected = await connection.ExecuteAsync(
                new CommandDefinition(sql, new
                {
                    Id = id,
                    DataAtualizacao = DateTime.UtcNow
                }));

            if (affected == 0)
            {
                _logger.LogWarning("Produto {Id} não encontrado", id);
                return false;
            }

            return true;
        });

    private async Task<Produto?> QueryByIdAsync(DbConnection connection, int id)
    {
        const string sql = @"
SELECT
    Id,
    Nome,
    Descricao,
    Preco,
    Categoria,
    Estoque,
    Ativo,
    ContatoEmail,
    DataCriacao,
    DataAtualizacao
FROM Produtos
WHERE Id = @Id;";

        var row = await connection.QuerySingleOrDefaultAsync<ProdutoRow>(
            new CommandDefinition(sql, new { Id = id }));

        return row is null ? null : MapToDomain(row);
    }

    private async Task<T> WithConnectionAsync<T>(Func<DbConnection, Task<T>> action)
    {
        var connection = _dbContext.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
        {
            await connection.OpenAsync();
        }

        try
        {
            return await action(connection);
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }

    private Task WithConnectionAsync(Func<DbConnection, Task> action) =>
        WithConnectionAsync(async connection =>
        {
            await action(connection);
            return true;
        });

    private static Produto MapToDomain(ProdutoRow row) =>
        Produto.Reconstituir(
            row.Id,
            row.Nome,
            row.Descricao,
            row.Preco,
            row.Categoria,
            row.Estoque,
            row.Ativo,
            row.ContatoEmail,
            row.DataCriacao,
            row.DataAtualizacao);

    private sealed class ProdutoRow
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public int Estoque { get; set; }
        public bool Ativo { get; set; }
        public string ContatoEmail { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
        public DateTime DataAtualizacao { get; set; }
    }
}
