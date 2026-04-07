# Coexistência de Clean Architecture e Vertical Slice Architecture

## Context and Problem Statement

O projeto é uma plataforma educacional para engenheiros .NET. Engenheiros lidam com domínios de complexidade variada: alguns contextos têm regras de negócio simples e alta taxa de mudança (onde organização por funcionalidade ganha), enquanto outros têm regras complexas, múltiplas camadas de validação e necessidade de testabilidade isolada (onde separação em camadas ganha). Qual padrão arquitetural usar como base do projeto?

## Considered Options

* Vertical Slice Architecture para toda a aplicação
* Clean Architecture (camadas) para toda a aplicação
* Coexistência intencional dos dois padrões em contextos distintos

## Decision Outcome

Chosen option: "Coexistência intencional dos dois padrões em contextos distintos", porque o objetivo pedagógico é expor engenheiros a ambos os padrões side-by-side, permitindo comparar trade-offs reais no mesmo codebase.

- **Pedidos** usa Vertical Slice: cada funcionalidade (CreatePedido, AddItemPedido, CancelPedido) é um slice autossuficiente com Command, Validator, Handler e Endpoint no mesmo diretório.
- **Produtos** usa Clean Architecture: separação em camadas Domain → Application → Infrastructure → API, com interfaces, repositórios e serviços de aplicação.

### Consequences

* Good, porque engenheiros podem comparar os dois padrões em um contexto real, com o mesmo stack tecnológico.
* Good, porque demonstra que não existe padrão universalmente superior — a escolha depende da complexidade e do ciclo de vida do contexto.
* Bad, porque a coexistência aumenta a carga cognitiva para novos contribuidores, que precisam entender qual padrão se aplica a qual contexto.
* Bad, porque regras compartilhadas (ex: validação, autenticação) precisam funcionar com ambas as organizações de código, o que pode gerar duplicidade.
