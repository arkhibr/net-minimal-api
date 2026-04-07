# Value Objects como Sealed Records

## Contexto e Problema

O domínio de Produtos possui atributos com regras de validade próprias: preço não pode ser negativo, categoria deve pertencer a um conjunto válido, estoque não pode ser negativo, descrição tem comprimento mínimo. Como encapsular essas regras sem duplicá-las em validadores, serviços e entidades?

## Opções Consideradas

* Tipos primitivos com validação no serviço de aplicação
* DTOs com DataAnnotations
* Value Objects como `sealed record` com construtor privado e factory method

## Decisão

Opção escolhida: "Value Objects como `sealed record` com construtor privado e factory method", por fins didáticos — demonstrar o padrão DDD de Value Object em C# moderno usando `record` para igualdade estrutural automática e `sealed` para evitar herança acidental.

Cada Value Object (ex: `PrecoProduto`, `CategoriaProduto`) tem:
- Construtor privado (impede instanciação direta)
- Factory method estático (`Criar(...)`) que retorna `Result<T>` em caso de validação
- Método `Reconstituir(...)` para rehydration pelo EF Core sem re-validação

O EF Core mapeia os Value Objects via value conversions no `AppDbContext`.

### Consequências

* Positivo, porque as regras de validade ficam encapsuladas no próprio tipo — não há como criar um `PrecoProduto` inválido fora do próprio objeto.
* Positivo, porque `record` fornece igualdade estrutural e imutabilidade sem código adicional.
* Negativo, porque o mapeamento EF Core requer value conversions explícitas para cada Value Object, adicionando configuração no `AppDbContext`.
* Negativo, porque a complexidade adicional é justificável apenas em domínios ricos — para CRUDs simples, é over-engineering.

---

_Formato baseado no template [MADR — Markdown Architectural Decision Records](https://github.com/adr/madr/blob/develop/template/adr-template-minimal.md)._
