using FluentValidation;
using ProdutosAPI.DTOs;

namespace ProdutosAPI.Validators;

/// <summary>
/// Validador para criação de produto
/// Referência: Melhores-Praticas-API.md - Seção "Validação de Dados"
/// Implementa business rules para criação de novo produto
/// </summary>
public class CriarProdutoValidator : AbstractValidator<CriarProdutoRequest>
{
    public CriarProdutoValidator()
    {
        // Validação: Nome obrigatório e com tamanho mínimo
        RuleFor(p => p.Nome)
            .NotEmpty()
            .WithMessage("Nome é obrigatório")
            .MinimumLength(3)
            .WithMessage("Nome deve ter no mínimo 3 caracteres")
            .MaximumLength(100)
            .WithMessage("Nome não pode exceder 100 caracteres");

        // Validação: Descrição obrigatória
        RuleFor(p => p.Descricao)
            .NotEmpty()
            .WithMessage("Descrição é obrigatória")
            .MaximumLength(500)
            .WithMessage("Descrição não pode exceder 500 caracteres");

        // Validação: Preço deve ser maior que zero
        RuleFor(p => p.Preco)
            .GreaterThan(0)
            .WithMessage("Preço deve ser maior que zero")
            .LessThan(999999.99m)
            .WithMessage("Preço não pode ser tão alto");

        // Validação: Categoria obrigatória
        RuleFor(p => p.Categoria)
            .NotEmpty()
            .WithMessage("Categoria é obrigatória")
            .Must(c => new[] { "Eletrônicos", "Livros", "Roupas", "Alimentos", "Outros" }.Contains(c))
            .WithMessage("Categoria inválida");

        // Validação: Estoque não pode ser negativo
        RuleFor(p => p.Estoque)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Estoque não pode ser negativo")
            .LessThan(1000000)
            .WithMessage("Estoque muito alto");

        // Validação: Email válido
        RuleFor(p => p.ContatoEmail)
            .NotEmpty()
            .WithMessage("Email de contato é obrigatório")
            .EmailAddress()
            .WithMessage("Email de contato inválido");
    }
}

/// <summary>
/// Validador para atualização parcial de produto
/// Referência: Melhores-Praticas-API.md - Seção "Design de Endpoints - PATCH"
/// </summary>
public class AtualizarProdutoValidator : AbstractValidator<AtualizarProdutoRequest>
{
    public AtualizarProdutoValidator()
    {
        // Todos os campos são opcionais, mas se fornecidos, devem ser válidos
        
        RuleFor(p => p.Nome)
            .MinimumLength(3)
            .WithMessage("Nome deve ter no mínimo 3 caracteres")
            .MaximumLength(100)
            .WithMessage("Nome não pode exceder 100 caracteres")
            .When(p => !string.IsNullOrEmpty(p.Nome));

        RuleFor(p => p.Descricao)
            .MaximumLength(500)
            .WithMessage("Descrição não pode exceder 500 caracteres")
            .When(p => !string.IsNullOrEmpty(p.Descricao));

        RuleFor(p => p.Preco)
            .GreaterThan(0)
            .WithMessage("Preço deve ser maior que zero")
            .When(p => p.Preco.HasValue);

        RuleFor(p => p.Categoria)
            .Must(c => new[] { "Eletrônicos", "Livros", "Roupas", "Alimentos", "Outros" }.Contains(c))
            .WithMessage("Categoria inválida")
            .When(p => !string.IsNullOrEmpty(p.Categoria));

        RuleFor(p => p.Estoque)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Estoque não pode ser negativo")
            .When(p => p.Estoque.HasValue);

        RuleFor(p => p.ContatoEmail)
            .EmailAddress()
            .WithMessage("Email de contato inválido")
            .When(p => !string.IsNullOrEmpty(p.ContatoEmail));
    }
}

/// <summary>
/// Validador para login
/// </summary>
public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(l => l.Email)
            .NotEmpty()
            .WithMessage("Email é obrigatório")
            .EmailAddress()
            .WithMessage("Email inválido");

        RuleFor(l => l.Senha)
            .NotEmpty()
            .WithMessage("Senha é obrigatória")
            .MinimumLength(6)
            .WithMessage("Senha deve ter no mínimo 6 caracteres");
    }
}
