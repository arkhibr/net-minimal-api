---
status: accepted
date: 2024-02-15
deciders: [Marco Mendes]
---

# CQRS: EF Core para Escrita e Dapper para Leitura

## Contexto e Problema

O projeto utiliza um banco de dados relacional (SQLite em desenvolvimento) para persistência. Operações de escrita (inserção, atualização, remoção) tendem a ser simples e se beneficiam de um ORM com rastreamento de mudanças e geração automática de SQL. Operações de leitura, especialmente em telas de listagem com filtros, paginação e joins, tendem a gerar queries pesadas quando delegadas ao ORM sem consciência do SQL gerado. Como equilibrar produtividade de escrita com controle de performance em leitura?

## Opções Consideradas

* EF Core para todas as operações (leitura e escrita)
* Dapper para todas as operações (leitura e escrita)
* CQRS com EF Core para comandos e Dapper para queries

## Decisão

Opção escolhida: "CQRS com EF Core para comandos e Dapper para queries", porque o foco em produtividade nas escritas (EF + LINQ reduz código boilerplate) e em consciência de SQL nas leituras (Dapper força o desenvolvedor a escrever e revisar a query) equilibra os dois objetivos.

- **Comandos** (`IProdutoCommandRepository`, `IPedidoCommandRepository`): EF Core com `DbContext` rastreado. Aproveitam change tracking, migrations e validações de concorrência.
- **Queries** (`IProdutoQueryRepository`, `IPedidoQueryRepository`): Dapper com SQL explícito. O desenvolvedor é responsável pela query, o que aumenta a consciência sobre índices e custo de execução.

### Consequências

* Positivo, porque queries de leitura são explícitas e auditáveis — nenhum desenvolvedor pode inadvertidamente gerar um N+1 sem perceber.
* Positivo, porque operações de escrita continuam concisas com LINQ e change tracking do EF Core.
* Negativo, porque o projeto mantém dois mecanismos de acesso a dados, aumentando a superfície de configuração e o número de dependências.
* Negativo, porque mudanças no schema precisam ser refletidas tanto nas migrations do EF Core quanto nas queries SQL do Dapper.

---

_Formato baseado no template [MADR — Markdown Architectural Decision Records](https://github.com/adr/madr/blob/develop/template/adr-template-minimal.md)._
