---
status: accepted
date: 2024-03-05
deciders: [Marco Mendes]
---

# FluentValidation para Validação de Entrada

## Contexto e Declaração do Problema

Endpoints de escrita precisam validar os dados de entrada antes de invocar a lógica de negócio: campos obrigatórios, formatos, comprimentos, intervalos numéricos, categorias válidas. Como implementar essa validação de forma declarativa, testável e desacoplada dos handlers?

## Opções Consideradas

* DataAnnotations (`[Required]`, `[Range]`, `[StringLength]`)
* Validação manual com `if` nos handlers
* FluentValidation com validators por tipo de request

## Decisão

Opção escolhida: "FluentValidation com validators por tipo de request", porque oferece validação declarativa com sintaxe encadeada, suporte a regras assíncronas, mensagens de erro customizáveis e validators facilmente testáveis de forma isolada.

Validators são registrados por assembly scanning (`AddValidatorsFromAssemblyContaining<Program>()`). Cada endpoint invoca `IValidator<T>.ValidateAsync()` explicitamente antes de passar o controle ao handler, retornando 422 Unprocessable Entity com os erros por campo em caso de falha.

### Consequências

* Positivo, porque as regras de validação são centralizadas em uma classe dedicada por tipo de request, facilitando leitura e manutenção.
* Positivo, porque validators são testáveis de forma isolada, sem necessidade de subir a aplicação completa.
* Positivo, porque o assembly scanning elimina o registro manual de cada validator no DI.
* Negativo, porque a validação explícita nos endpoints (chamada manual a `ValidateAsync`) pode ser esquecida em novos endpoints — não há enforcement automático como em controllers MVC com `[ApiController]`.
* Negativo, porque adiciona uma dependência de biblioteca para um problema que DataAnnotations resolveria para casos simples.

---

_Formato baseado no template [MADR — Markdown Architectural Decision Records](https://github.com/adr/madr/blob/develop/template/adr-template-minimal.md)._
