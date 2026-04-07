# Coexistência de Clean Architecture e Vertical Slice Architecture

## Contexto e Problema

O projeto é uma plataforma educacional para engenheiros .NET. Engenheiros lidam com domínios de complexidade variada: alguns contextos têm regras de negócio simples e alta taxa de mudança (onde organização por funcionalidade ganha), enquanto outros têm regras complexas, múltiplas camadas de validação e necessidade de testabilidade isolada (onde separação em camadas ganha). Qual padrão arquitetural usar como base do projeto?

## Opções Consideradas

* Vertical Slice Architecture para toda a aplicação
* Clean Architecture (camadas) para toda a aplicação
* Coexistência intencional dos dois padrões em contextos distintos

## Decisão

Opção escolhida: "Coexistência intencional dos dois padrões em contextos distintos", porque o objetivo pedagógico é expor engenheiros a ambos os padrões side-by-side, permitindo comparar trade-offs reais no mesmo codebase.

- **Pedidos** usa Vertical Slice: cada funcionalidade (CreatePedido, AddItemPedido, CancelPedido) é um slice autossuficiente com Command, Validator, Handler e Endpoint no mesmo diretório.
- **Produtos** usa Clean Architecture: separação em camadas Domain → Application → Infrastructure → API, com interfaces, repositórios e serviços de aplicação.

### Consequências

* Positivo, porque engenheiros podem comparar os dois padrões em um contexto real, com o mesmo stack tecnológico.
* Positivo, porque demonstra que não existe padrão universalmente superior — a escolha depende da complexidade e do ciclo de vida do contexto.
* Negativo, porque a coexistência aumenta a carga cognitiva para novos contribuidores, que precisam entender qual padrão se aplica a qual contexto.
* Negativo, porque regras compartilhadas (ex: validação, autenticação) precisam funcionar com ambas as organizações de código, o que pode gerar duplicidade.

---

_Formato baseado no template [MADR — Markdown Architectural Decision Records](https://github.com/adr/madr/blob/develop/template/adr-template-minimal.md)._
