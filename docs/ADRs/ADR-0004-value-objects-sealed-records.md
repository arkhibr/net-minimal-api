# Value Objects como Sealed Records

## Context and Problem Statement

O domínio de Produtos possui atributos com regras de validade próprias: preço não pode ser negativo, categoria deve pertencer a um conjunto válido, estoque não pode ser negativo, descrição tem comprimento mínimo. Como encapsular essas regras sem duplicá-las em validadores, serviços e entidades?

## Considered Options

* Tipos primitivos com validação no serviço de aplicação
* DTOs com DataAnnotations
* Value Objects como `sealed record` com construtor privado e factory method

## Decision Outcome

Chosen option: "Value Objects como `sealed record` com construtor privado e factory method", por fins didáticos — demonstrar o padrão DDD de Value Object em C# moderno usando `record` para igualdade estrutural automática e `sealed` para evitar herança acidental.

Cada Value Object (ex: `PrecoProduto`, `CategoriaProduto`) tem:
- Construtor privado (impede instanciação direta)
- Factory method estático (`Criar(...)`) que retorna `Result<T>` em caso de validação
- Método `Reconstituir(...)` para rehydration pelo EF Core sem re-validação

O EF Core mapeia os Value Objects via value conversions no `AppDbContext`.

### Consequences

* Good, porque as regras de validade ficam encapsuladas no próprio tipo — não há como criar um `PrecoProduto` inválido fora do próprio objeto.
* Good, porque `record` fornece igualdade estrutural e imutabilidade sem código adicional.
* Bad, porque o mapeamento EF Core requer value conversions explícitas para cada Value Object, adicionando configuração no `AppDbContext`.
* Bad, porque a complexidade adicional é justificável apenas em domínios ricos — para CRUDs simples, é over-engineering.
