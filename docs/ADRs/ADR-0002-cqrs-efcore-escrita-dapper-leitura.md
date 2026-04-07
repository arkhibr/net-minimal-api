# CQRS: EF Core para Escrita e Dapper para Leitura

## Context and Problem Statement

O projeto utiliza um banco de dados relacional (SQLite em desenvolvimento) para persistência. Operações de escrita (inserção, atualização, remoção) tendem a ser simples e se beneficiam de um ORM com rastreamento de mudanças e geração automática de SQL. Operações de leitura, especialmente em telas de listagem com filtros, paginação e joins, tendem a gerar queries pesadas quando delegadas ao ORM sem consciência do SQL gerado. Como equilibrar produtividade de escrita com controle de performance em leitura?

## Considered Options

* EF Core para todas as operações (leitura e escrita)
* Dapper para todas as operações (leitura e escrita)
* CQRS com EF Core para comandos e Dapper para queries

## Decision Outcome

Chosen option: "CQRS com EF Core para comandos e Dapper para queries", porque o foco em produtividade nas escritas (EF + LINQ reduz código boilerplate) e em consciência de SQL nas leituras (Dapper força o desenvolvedor a escrever e revisar a query) equilibra os dois objetivos.

- **Comandos** (`IProdutoCommandRepository`, `IPedidoCommandRepository`): EF Core com `DbContext` rastreado. Aproveitam change tracking, migrations e validações de concorrência.
- **Queries** (`IProdutoQueryRepository`, `IPedidoQueryRepository`): Dapper com SQL explícito. O desenvolvedor é responsável pela query, o que aumenta a consciência sobre índices e custo de execução.

### Consequences

* Good, porque queries de leitura são explícitas e auditáveis — nenhum desenvolvedor pode inadvertidamente gerar um N+1 sem perceber.
* Good, porque operações de escrita continuam concisas com LINQ e change tracking do EF Core.
* Bad, porque o projeto mantém dois mecanismos de acesso a dados, aumentando a superfície de configuração e o número de dependências.
* Bad, porque mudanças no schema precisam ser refletidas tanto nas migrations do EF Core quanto nas queries SQL do Dapper.
