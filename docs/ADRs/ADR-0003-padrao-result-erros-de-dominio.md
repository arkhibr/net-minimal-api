---
status: accepted
date: 2024-02-01
deciders: [Marco Mendes]
---

# Padrão Result&lt;T&gt; para Erros de Domínio

## Contexto e Problema

Operações de domínio podem falhar por regras de negócio (ex: pedido já cancelado, item acima do limite permitido). Como comunicar essas falhas ao chamador sem lançar exceções — que interrompem o fluxo e carregam overhead de stack trace — mas mantendo o código legível e o fluxo explícito?

## Opções Consideradas

* Lançar exceptions de domínio (ex: `DomainException`)
* Retornar `null` para ausência e lançar exceção para violações
* Usar `Result<T>` como tipo de retorno explícito (monad Either)

## Decisão

Opção escolhida: "Usar `Result<T>` como tipo de retorno explícito", porque torna o contrato do método explícito — o chamador é forçado a tratar tanto o caminho de sucesso quanto o de falha, sem depender de documentação ou convenção de exceções.

O tipo `Result<T>` é implementado como `record` imutável com `IsSuccess`, `Value` e `Error`, sem herança de exceção.

**Nota sobre uso heterogêneo entre bounded contexts:** O módulo de `Pedidos` adota `Result<T>` de forma consistente — todos os métodos do agregado `Pedido` (`Criar`, `AdicionarItem`, `Confirmar`, `Cancelar`) e o `CreatePedidoHandler` retornam `Result`/`Result<T>`. Já o `Catálogo` opera com modelo híbrido: alguns recursos (Produto, Categoria, Variante) usam domínio rico com value objects que lançam exceções via construtor para invariantes, enquanto Atributo e Mídia seguem CRUD anêmico com `null` para not found. Essa heterogeneidade é intencional — cada bounded context escolhe a estratégia de erro proporcional à densidade de regras de domínio que possui.

**Nota sobre completude do monad:** Para o padrão ser plenamente aproveitado, métodos de encadeamento como `.Bind()`, `.Map()` e `.Match()` deveriam ser implementados no tipo `Result<T>`. Sem eles, o ganho em relação a um nullable é marginal — apenas estrutural. A implementação atual é intencional para fins educacionais (introdução ao conceito), não como implementação de produção completa.

### Consequências

* Positivo, porque o compilador força o tratamento explícito de falhas — não é possível ignorar um `Result<T>` sem inspecioná-lo.
* Positivo, porque elimina o uso de exceções para controle de fluxo de negócio, reservando-as para erros verdadeiramente excepcionais.
* Negativo, porque a ausência de métodos de encadeamento (`.Bind`, `.Map`) limita a composição funcional e reduz o ganho de legibilidade em cadeias de operações.
* Negativo, porque o uso heterogêneo entre Catálogo (modelo híbrido: exceções no construtor de value objects + `null` em CRUD anêmico) e Pedidos (`Result<T>` consistente) aumenta a carga cognitiva — o desenvolvedor precisa saber qual estratégia de erro é usada em cada contexto.

---

_Formato baseado no template [MADR — Markdown Architectural Decision Records](https://github.com/adr/madr/blob/develop/template/adr-template-minimal.md)._
