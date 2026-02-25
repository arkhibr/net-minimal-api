namespace ProdutosAPI.Models;

/// <summary>
/// Entidade Produto
/// Referência: Melhores-Praticas-API.md - Seção "Design de Endpoints"
/// Representa um produto no sistema
/// </summary>
public class Produto
{
    /// <summary>
    /// Identificador único do produto (PK)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nome do produto
    /// Validação: Obrigatório, mínimo 3 caracteres
    /// </summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Descrição detalhada do produto
    /// </summary>
    public string Descricao { get; set; } = string.Empty;

    /// <summary>
    /// Preço do produto em reais
    /// Validação: Deve ser maior que 0
    /// </summary>
    public decimal Preco { get; set; }

    /// <summary>
    /// Categoria do produto
    /// Exemplos: "Eletrônicos", "Livros", "Roupas"
    /// </summary>
    public string Categoria { get; set; } = string.Empty;

    /// <summary>
    /// Quantidade em estoque
    /// Validação: Não pode ser negativo
    /// </summary>
    public int Estoque { get; set; }

    /// <summary>
    /// Status do produto (Ativo/Inativo)
    /// Referência: Melhores-Praticas-API.md - Seção "RESTful Design"
    /// Permite soft delete mantendo integridade referencial
    /// </summary>
    public bool Ativo { get; set; } = true;

    /// <summary>
    /// Email de contato do fabricante/fornecedor
    /// Validação: Formato de email válido
    /// </summary>
    public string ContatoEmail { get; set; } = string.Empty;

    /// <summary>
    /// Data de criação do registro
    /// </summary>
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
}
