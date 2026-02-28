using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using ProdutosAPI.Produtos.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProdutosAPI.Produtos.Endpoints;

/// <summary>
/// Endpoint para Simulação de Autenticação.
/// Referência: Melhores-Praticas-API.md - Seção "Segurança"
/// Demonstra a geração de um Token JWT (JSON Web Token) válido para ser usado no cabeçalho Authorization.
/// </summary>
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Auth")
            .WithDescription("Endpoints para autenticação de usuários e geração de tokens JWT");

        group.MapPost("/login", Login)
            .WithName("UserLogin")
            .WithSummary("Realiza login e retorna um JWT Token")
            .Accepts<LoginRequest>("application/json")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();
    }

    private static IResult Login(LoginRequest req, IConfiguration configuration)
    {
        // Para simplificar o projeto educativo, o login é "mockado".
        // Em um projeto real, você verificaria na base de dados (Identity)
        if (req.Email != "admin@example.com" || req.Senha != "senha123")
        {
            return Results.Unauthorized();
        }

        // Criando as informações contidas no "recheio" do Token (Claims)
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "admin_id"),
            new Claim(JwtRegisteredClaimNames.Email, req.Email),
            new Claim(ClaimTypes.Role, "Admin")
        };

        // Pegamos a "Chave Secreta" que fica blindada no servidor (appsettings.json)
        var secretKey = configuration["Jwt:Key"] ?? "MinhaChaveSuperSecretaDePeloMenos32BytesAki123!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        // Assinamos o Token com Algoritmo (Criptografia simétrica forte)
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"] ?? "ProdutosAPI",
            audience: configuration["Jwt:Audience"] ?? "TodosOsClientes",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2), // O Token tem um prazo de validade (2 horas)
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Results.Ok(new AuthResponse
        {
            Token = tokenString,
            ExpiresIn = (int)TimeSpan.FromHours(2).TotalSeconds
        });
    }
}
