using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Pedidos.Domain;
using ProdutosAPI.Catalogo.Application.Interfaces;
using ProdutosAPI.Catalogo.Domain;
using ProdutosAPI.Catalogo.Domain.ValueObjects;

namespace ProdutosAPI.Shared.Data;

public class AppDbContext : DbContext, ICatalogoContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoItem> PedidoItens => Set<PedidoItem>();

    // ICatalogoContext: explicit implementation — DbSet<T> não satisfaz IQueryable<T> implicitamente
    IQueryable<Produto> ICatalogoContext.Produtos => Set<Produto>();
    IQueryable<Categoria> ICatalogoContext.Categorias => Set<Categoria>();

    public void AddProduto(Produto produto) => this.Add(produto);
    public void AddCategoria(Categoria categoria) => this.Add(categoria);

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
                .HasConversion(
                    descricao => descricao.Value,
                    value => DescricaoProduto.Reconstituir(value))
                .HasMaxLength(500);

            entity.Property(p => p.Preco)
                .HasPrecision(10, 2)
                .HasConversion(
                    preco => preco.Value,
                    value => PrecoProduto.Reconstituir(value))
                .IsRequired();

            entity.Property(p => p.Categoria)
                .IsRequired()
                .HasConversion(
                    categoria => categoria.Value,
                    value => CategoriaProduto.Reconstituir(value))
                .HasMaxLength(50);

            entity.Property(p => p.Estoque)
                .HasConversion(
                    estoque => estoque.Value,
                    value => EstoqueProduto.Reconstituir(value))
                .IsRequired();

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

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Nome).IsRequired().HasMaxLength(100).UsePropertyAccessMode(PropertyAccessMode.Property);
            entity.Property(c => c.Slug).IsRequired().HasMaxLength(120).UsePropertyAccessMode(PropertyAccessMode.Property);
            entity.Property(c => c.CategoriaPaiId).UsePropertyAccessMode(PropertyAccessMode.Property);
            entity.Property(c => c.Ativa).HasDefaultValue(true).UsePropertyAccessMode(PropertyAccessMode.Property);
            entity.Property(c => c.DataCriacao).UsePropertyAccessMode(PropertyAccessMode.Property);
            entity.Property(c => c.DataAtualizacao).UsePropertyAccessMode(PropertyAccessMode.Property);

            entity.HasIndex(c => c.Slug).IsUnique().HasDatabaseName("idx_categoria_slug");

            entity.HasOne<Categoria>()
                .WithMany()
                .HasForeignKey(c => c.CategoriaPaiId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
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
