using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProdutosAPI.Common;
using ProdutosAPI.Data;
using ProdutosAPI.Features.Common;
using ProdutosAPI.Features.Pedidos.CreatePedido;
using ProdutosAPI.Features.Pedidos.GetPedido;
using ProdutosAPI.Features.Pedidos.ListPedidos;
using ProdutosAPI.DTOs;
using ProdutosAPI.Endpoints;
using ProdutosAPI.Middleware;
using ProdutosAPI.Services;
using ProdutosAPI.Validators;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// CONFIGURAÇÃO DE LOGGING
// ==========================================
// Referência: Melhores-Praticas-API.md - Seção "Logging e Monitoramento"
// Structured logging com Serilog

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/api-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        fileSizeLimitBytes: 10_000_000,
        retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();

// ==========================================
// CONFIGURAÇÃO DE BANCO DE DADOS
// ==========================================
// Referência: Melhores-Praticas-API.md - Seção "Segurança - SQL Injection"
// Using Entity Framework Core para proteção contra SQL Injection

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=produtos-api.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString)
);

// ==========================================
// CONFIGURAÇÃO DE DEPENDENCY INJECTION
// ==========================================
// Registrar serviços

builder.Services.AddScoped<IProdutoService, ProdutoService>();

// Registrar slices de Pedidos via scan automático
builder.Services.AddEndpointsFromAssembly(typeof(Program).Assembly);

// Handlers dos slices de Pedidos
builder.Services.AddScoped<CreatePedidoHandler>();
builder.Services.AddScoped<GetPedidoHandler>();
builder.Services.AddScoped<ListPedidosHandler>();

// ==========================================
// CONFIGURAÇÃO DE VALIDAÇÃO
// ==========================================
// Referência: Melhores-Praticas-API.md - Seção "Validação de Dados"
// FluentValidation para validações robustas

builder.Services.AddValidatorsFromAssemblyContaining<CriarProdutoValidator>();

// ==========================================
// CONFIGURAÇÃO DE MAPEAMENTO
// ==========================================
// AutoMapper para mapeamento entre entidades e DTOs

builder.Services.AddAutoMapper(typeof(MappingProfile));

// ==========================================
// CONFIGURAÇÃO DE CORS
// ==========================================
// Referência: Melhores-Praticas-API.md - Seção "Segurança - CORS"

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Cache em memória para armazenar as chaves de idempotência
builder.Services.AddMemoryCache();

// ==========================================
// CONFIGURAÇÃO DE SEGURANÇA (JWT)
// ==========================================
// Referência: Melhores-Praticas-API.md - Seção "Segurança"

var jwtKey = builder.Configuration["Jwt:Key"] ?? "MinhaChaveSuperSecretaDePeloMenos32BytesAki123!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "ProdutosAPI",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "TodosOsClientes",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ==========================================
// CONFIGURAÇÃO DE DOCUMENTAÇÃO (SWAGGER)
// ==========================================
// Referência: Melhores-Praticas-API.md - Seção "Documentação"

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Produtos API",
        Version = "v2.0.0",
        Description = "API REST educacional de produtos com Minimal API em .NET 10 LTS",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "API Support",
            Email = "support@example.com"
        },
        License = new Microsoft.OpenApi.Models.OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Adicionar XML comments se existir
    var xmlFile = Path.Combine(AppContext.BaseDirectory, "ProdutosAPI.xml");
    if (File.Exists(xmlFile))
    {
        c.IncludeXmlComments(xmlFile);
    }

    // Informar ao Swagger como mandar o Token nas rotas para teste pelo UI (Botão "Authorize" no Swagger)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT neste campo da seguinte forma: 'Bearer {seu_token_aqui}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ==========================================
// CRIAR APLICAÇÃO
// ==========================================

var app = builder.Build();

// ==========================================
// EXECUTAR MIGRATIONS E SEED
// ==========================================
// Aplicar migrations e popular dados iniciais

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Aplicar migrations
    dbContext.Database.Migrate();
    
    // Popular dados iniciais
    DbSeeder.Seed(dbContext);
}

// ==========================================
// CONFIGURAR MIDDLEWARE
// ==========================================
// Referência: Melhores-Praticas-API.md - Seção "Tratamento de Erros"

// Middleware de tratamento global de exceções
app.UseExceptionHandling();

// CORS
app.UseCors("AllowAll");

// HTTPS redirect
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// Swagger/OpenAPI
// Referência: Melhores-Praticas-API.md - Seção "Documentação"
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Produtos API v1");
        c.RoutePrefix = string.Empty;
    });
}

// Segurança (Identifica e Protege rotas baseadas no Token)
app.UseAuthentication();
app.UseAuthorization();

// Idempotência (Evita reprocessamentos duplo de escrita de dados)
app.UseMiddleware<IdempotencyMiddleware>();

// ==========================================
// CONFIGURAR ENDPOINTS
// ==========================================
// Referência: Melhores-Praticas-API.md - Seção "Design de Endpoints"

app.MapAuthEndpoints();
app.MapProdutoEndpoints();

// Slices de Pedidos (IEndpoint)
app.MapRegisteredEndpoints();

// Health check simples
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("Health")
    .AllowAnonymous();

// ==========================================
// EXECUTAR APLICAÇÃO
// ==========================================

app.Run();
