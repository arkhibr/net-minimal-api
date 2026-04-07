# FluentValidation para Validação de Entrada

## Context and Problem Statement

Endpoints de escrita precisam validar os dados de entrada antes de invocar a lógica de negócio: campos obrigatórios, formatos, comprimentos, intervalos numéricos, categorias válidas. Como implementar essa validação de forma declarativa, testável e desacoplada dos handlers?

## Considered Options

* DataAnnotations (`[Required]`, `[Range]`, `[StringLength]`)
* Validação manual com `if` nos handlers
* FluentValidation com validators por tipo de request

## Decision Outcome

Chosen option: "FluentValidation com validators por tipo de request", porque oferece validação declarativa com sintaxe encadeada, suporte a regras assíncronas, mensagens de erro customizáveis e validators facilmente testáveis de forma isolada.

Validators são registrados por assembly scanning (`AddValidatorsFromAssemblyContaining<Program>()`). Cada endpoint invoca `IValidator<T>.ValidateAsync()` explicitamente antes de passar o controle ao handler, retornando 422 Unprocessable Entity com os erros por campo em caso de falha.

### Consequences

* Good, porque as regras de validação são centralizadas em uma classe dedicada por tipo de request, facilitando leitura e manutenção.
* Good, porque validators são testáveis de forma isolada, sem necessidade de subir a aplicação completa.
* Good, porque o assembly scanning elimina o registro manual de cada validator no DI.
* Bad, porque a validação explícita nos endpoints (chamada manual a `ValidateAsync`) pode ser esquecida em novos endpoints — não há enforcement automático como em controllers MVC com `[ApiController]`.
* Bad, porque adiciona uma dependência de biblioteca para um problema que DataAnnotations resolveria para casos simples.
