using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Pedidos.Domain;
using ProdutosAPI.Produtos.Models;

namespace ProdutosAPI.Shared.Data;

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
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoItem> PedidoItens => Set<PedidoItem>();

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
                .HasDatabaseName("idx_produto_ativo");

            entity.HasIndex(p => p.Categoria)
                .HasDatabaseName("idx_produto_categoria");

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

            // Permite EF Core ler/escrever em propriedades com private setters
            entity.Property(p => p.Nome).UsePropertyAccessMode(PropertyAccessMode.Property);
            entity.Property(p => p.Descricao).UsePropertyAccessMode(PropertyAccessMode.Property);
            entity.Property(p => p.Preco).UsePropertyAccessMode(PropertyAccessMode.Property);
            entity.Property(p => p.Categoria).UsePropertyAccessMode(PropertyAccessMode.Property);
            entity.Property(p => p.Estoque).UsePropertyAccessMode(PropertyAccessMode.Property);
            entity.Property(p => p.Ativo).UsePropertyAccessMode(PropertyAccessMode.Property);
            entity.Property(p => p.ContatoEmail).UsePropertyAccessMode(PropertyAccessMode.Property);
            entity.Property(p => p.DataCriacao).UsePropertyAccessMode(PropertyAccessMode.Property);
            entity.Property(p => p.DataAtualizacao).UsePropertyAccessMode(PropertyAccessMode.Property);
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Status)
                .HasConversion<string>()
                .UsePropertyAccessMode(PropertyAccessMode.Property);

            entity.Property(p => p.Total)
                .HasPrecision(10, 2)
                .UsePropertyAccessMode(PropertyAccessMode.Property);

            entity.Property(p => p.CriadoEm)
                .UsePropertyAccessMode(PropertyAccessMode.Property);

            entity.Property(p => p.ConfirmadoEm)
                .UsePropertyAccessMode(PropertyAccessMode.Property);

            entity.Property(p => p.CanceladoEm)
                .UsePropertyAccessMode(PropertyAccessMode.Property);

            entity.Property(p => p.MotivoCancelamento)
                .HasMaxLength(500)
                .UsePropertyAccessMode(PropertyAccessMode.Property);

            entity.HasMany(p => p.Itens)
                .WithOne()
                .HasForeignKey(i => i.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation(p => p.Itens).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<PedidoItem>(entity =>
        {
            entity.HasKey(i => i.Id);

            entity.Property(i => i.NomeProduto)
                .IsRequired()
                .HasMaxLength(100)
                .UsePropertyAccessMode(PropertyAccessMode.Property);

            entity.Property(i => i.PrecoUnitario)
                .HasPrecision(10, 2)
                .UsePropertyAccessMode(PropertyAccessMode.Property);

            entity.Property(i => i.Quantidade)
                .UsePropertyAccessMode(PropertyAccessMode.Property);

            entity.Property(i => i.ProdutoId)
                .UsePropertyAccessMode(PropertyAccessMode.Property);

            entity.Ignore(i => i.Subtotal);
        });
    }
}
