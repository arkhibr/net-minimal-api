namespace ProdutosAPI.DTOs;

/// <summary>
/// DTO para requisições de criação/atualização de produto
/// Referência: Melhores-Praticas-API.md - Seção "Validação de Dados"
/// Separa a representação externa da entidade interna
/// </summary>
public class CriarProdutoRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public int Estoque { get; set; }
    public string ContatoEmail { get; set; } = string.Empty;
}

/// <summary>
/// DTO para requisições de atualização parcial de produto
/// Referência: Melhores-Praticas-API.md - Seção "Design de Endpoints - PUT vs PATCH"
/// Permite atualizações de apenas alguns campos
/// </summary>
public class AtualizarProdutoRequest
{
    public string? Nome { get; set; }
    public string? Descricao { get; set; }
    public decimal? Preco { get; set; }
    public string? Categoria { get; set; }
    public int? Estoque { get; set; }
    public string? ContatoEmail { get; set; }
}

/// <summary>
/// DTO para resposta de produto
/// Referência: Melhores-Praticas-API.md - Seção "Representação de Recursos"
/// Consistência na estrutura de responses
/// </summary>
public class ProdutoResponse
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

/// <summary>
/// DTO para resposta paginada
/// Referência: Melhores-Praticas-API.md - Seção "Paginação"
/// Padroniza respostas com dados paginados
/// </summary>
public class PaginatedResponse<T>
{
    public List<T> Data { get; set; } = new();
    public PaginationInfo Pagination { get; set; } = new();
}

/// <summary>
/// Informações de paginação
/// </summary>
public class PaginationInfo
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// DTO para resposta de erro
/// Referência: Melhores-Praticas-API.md - Seção "Tratamento de Erros"
/// Formato padronizado para todas as respostas de erro
/// </summary>
public class ErrorResponse
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string Instance { get; set; } = string.Empty;
    public Dictionary<string, List<string>> Errors { get; set; } = new();
}

/// <summary>
/// DTO para resposta de autenticação
/// Referência: Melhores-Praticas-API.md - Seção "Segurança - Autenticação"
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
}

/// <summary>
/// DTO para requisição de login
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}
