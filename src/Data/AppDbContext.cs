using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Models;

namespace ProdutosAPI.Data;

/// <summary>
/// Contexto do Entity Framework
/// Referência: Melhores-Praticas-API.md - Seção "Segurança - SQL Injection"
/// Usando ORM para proteção contre SQL Injection
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da entidade Produto
        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasKey(p => p.Id);

            // Índices para melhor performance
            // Referência: Melhores-Praticas-API.md - Seção "Performance"
            entity.HasIndex(p => p.Ativo)
                .HasName("idx_produto_ativo");
            
            entity.HasIndex(p => p.Categoria)
                .HasName("idx_produto_categoria");

            // Configuração de propriedades
            entity.Property(p => p.Nome)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.Descricao)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(p => p.Preco)
                .HasPrecision(10, 2)
                .IsRequired();

            entity.Property(p => p.Categoria)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(p => p.ContatoEmail)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.Ativo)
                .HasDefaultValue(true);
        });
    }
}
