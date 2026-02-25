using ProdutosAPI.Models;

namespace ProdutosAPI.Data;

/// <summary>
/// Classe para popular dados iniciais no banco de dados
/// Referência: Melhores-Praticas-API.md - Seção "Documentação"
/// Dados de exemplo para facilitar testes
/// </summary>
public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        // Se já há dados, não inserir novamente
        if (context.Produtos.Any())
        {
            return;
        }

        var produtos = new List<Produto>
        {
            new()
            {
                Nome = "Notebook Dell XPS 13",
                Descricao = "Notebook de alta performance com processador Intel Core i7, 16GB RAM e 512GB SSD",
                Preco = 4500.00m,
                Categoria = "Eletrônicos",
                Estoque = 5,
                ContatoEmail = "vendas@dell.com",
                Ativo = true,
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            },
            new()
            {
                Nome = "Mouse Logitech MX Master 3S",
                Descricao = "Mouse wireless de precisão profissional com múltiplos botões e rastreamento avançado",
                Preco = 450.00m,
                Categoria = "Eletrônicos",
                Estoque = 25,
                ContatoEmail = "suporte@logitech.com",
                Ativo = true,
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            },
            new()
            {
                Nome = "Teclado Mecânico RGB",
                Descricao = "Teclado mecânico com iluminação RGB, switches Cherry MX e design compacto",
                Preco = 350.00m,
                Categoria = "Eletrônicos",
                Estoque = 15,
                ContatoEmail = "contato@keyboards.com.br",
                Ativo = true,
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            },
            new()
            {
                Nome = "Clean Code",
                Descricao = "Guia prático para escrever código limpo e manutenível. Essencial para todo desenvolvedor",
                Preco = 89.90m,
                Categoria = "Livros",
                Estoque = 30,
                ContatoEmail = "vendas@books.com",
                Ativo = true,
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            },
            new()
            {
                Nome = "Design Patterns",
                Descricao = "Padrões de design reutilizáveis para desenvolvimento de software. Referência obrigatória",
                Preco = 75.00m,
                Categoria = "Livros",
                Estoque = 20,
                ContatoEmail = "vendas@books.com",
                Ativo = true,
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            },
            new()
            {
                Nome = "Camiseta técnica Azul",
                Descricao = "Camiseta de poliéster com tecnologia anti-transpiração, disponível em vários tamanhos",
                Preco = 79.90m,
                Categoria = "Roupas",
                Estoque = 50,
                ContatoEmail = "vendas@clothing.com.br",
                Ativo = true,
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            },
            new()
            {
                Nome = "Café Gourmet 500g",
                Descricao = "Café gourmet especial com grãos selecionados de plantações premium da região",
                Preco = 45.00m,
                Categoria = "Alimentos",
                Estoque = 100,
                ContatoEmail = "vendas@coffee.com.br",
                Ativo = true,
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            },
            new()
            {
                Nome = "Monitor LG UltraWide 34\"",
                Descricao = "Monitor curvo ultrawide com resolução 3440x1440, ideal para produtividade e games",
                Preco = 1899.00m,
                Categoria = "Eletrônicos",
                Estoque = 3,
                ContatoEmail = "suporte@lg.com.br",
                Ativo = true,
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            }
        };

        context.Produtos.AddRange(produtos);
        context.SaveChanges();
    }
}
