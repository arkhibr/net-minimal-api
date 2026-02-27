# Design: Reestruturação da Documentação

**Data:** 2026-02-27
**Status:** Aprovado

---

## Contexto

A documentação do projeto reflete apenas a primeira fase (Produtos com camadas horizontais). A segunda fase (Pedidos com Vertical Slice + Domínio Rico) está no código mas invisível nos docs. Todos os guias omitem: Vertical Slice Architecture, Domínio Rico, Result Pattern, o agregado Pedido, e os 13 testes de integração HTTP. A contagem de testes nos docs diz "50+" quando o projeto tem 111.

---

## Objetivo

Reestruturar toda a documentação para:
1. Refletir fielmente o estado atual do código
2. Organizar o aprendizado em duas trilhas complementares
3. Criar um novo guia conceitual sobre Vertical Slice e Domínio Rico

---

## Nova Arquitetura de Informação

```
docs/
├── Navegação
│   ├── 00-LEIA-PRIMEIRO.md     ← reescrita: narrativa dos 2 casos de uso
│   ├── INDEX.md                ← reescrita: 2 trilhas de aprendizado
│   └── INICIO-RAPIDO.md        ← atualização: endpoints de Pedidos + auth
│
├── Guias Conceituais
│   ├── MELHORES-PRATICAS-API.md           ← sem alterações
│   └── VERTICAL-SLICE-DOMINIO-RICO.md     ← NOVO
│
├── Guias de Implementação
│   ├── MELHORES-PRATICAS-MINIMAL-API.md   ← adição: capítulo Pedidos
│   └── MELHORIAS-DOTNET-10.md             ← adição: features dos slices
│
└── Referência
    ├── ARQUITETURA.md     ← reescrita: diagrama duplo + coexistência
    ├── CHECKLIST.md       ← adição: seção Pedidos
    └── ENTREGA-FINAL.md   ← atualização: reflete os 2 casos de uso

ProdutosAPI.Tests/
└── ESTRATEGIA-DE-TESTES.md  ← reescrita: 3 categorias, 111 testes

README.md  ← atualização pesada: porta de entrada para os 2 padrões
```

---

## Duas Trilhas de Aprendizado

### Trilha 1 — REST + Camadas Horizontais (Produtos)
1. `MELHORES-PRATICAS-API.md` — teoria REST universal
2. `MELHORES-PRATICAS-MINIMAL-API.md` → seção Produtos
3. Código: `src/Endpoints/` + `src/Services/` + `src/Models/Produto.cs`
4. Testes: unit tests de serviço e validação

### Trilha 2 — Vertical Slice + Domínio Rico (Pedidos)
1. `VERTICAL-SLICE-DOMINIO-RICO.md` — teoria: slices + domínio rico + Result pattern
2. `MELHORES-PRATICAS-MINIMAL-API.md` → seção Pedidos
3. Código: `src/Features/Pedidos/`
4. Testes: domain unit tests + integration tests HTTP

---

## Escopo de Mudanças por Arquivo

| Arquivo | Tipo | O que muda |
|---|---|---|
| `README.md` | Atualização pesada | +Pedidos endpoints, +111 testes, estrutura atualizada, v3.0.0 |
| `00-LEIA-PRIMEIRO.md` | Reescrita | Introdução narrativa aos 2 casos de uso, sem lista exaustiva de arquivos |
| `INDEX.md` | Reescrita | 2 trilhas, mapa mental atualizado, remove sugestões já implementadas |
| `ARQUITETURA.md` | Reescrita | Diagrama duplo lado a lado, coexistência, data model com Pedidos+PedidoItens |
| `ESTRATEGIA-DE-TESTES.md` | Reescrita | 3 categorias reais: Domain Unit, Service Unit, Integration HTTP; 111 testes |
| `MELHORES-PRATICAS-MINIMAL-API.md` | Adição | Novo capítulo sobre Pedidos (slices, handlers, Result pattern) |
| `MELHORIAS-DOTNET-10.md` | Adição | IEndpoint scan, collection expressions `[]`, primary constructors em handlers |
| `ENTREGA-FINAL.md` | Atualização | Reflete os 2 casos de uso, domínio rico, testes de integração |
| `CHECKLIST.md` | Adição | Seção Pedidos (5 slices, domínio, testes) |
| `INICIO-RAPIDO.md` | Atualização | Exemplos curl com JWT + endpoints de Pedidos |
| `VERTICAL-SLICE-DOMINIO-RICO.md` | **NOVO** | Guia conceitual completo |

---

## Conteúdo do Novo Guia (VERTICAL-SLICE-DOMINIO-RICO.md)

1. **O Problema** — por que camadas horizontais têm limites (feature atravessa 5 arquivos)
2. **Vertical Slice Architecture** — o que é, anatomia de um slice (Command/Handler/Validator/Endpoint), IEndpoint + scan automático
3. **Modelo Anêmico vs Domínio Rico** — comparação direta com código do próprio projeto (`Produto` antes/depois, `Pedido` como exemplo maduro)
4. **Result Pattern** — erros de domínio sem exceptions, `Result<T>` records
5. **Aggregate Root** — `Pedido` como exemplo: invariantes, encapsulamento, regras de negócio no domínio
6. **Coexistência dos Padrões** — quando usar cada um, como compartilham AppDbContext e middleware
7. **Referências** para o código em `src/Features/Pedidos/`
