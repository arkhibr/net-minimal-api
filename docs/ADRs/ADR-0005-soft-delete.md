---
status: accepted
date: 2024-04-01
deciders: [Marco Mendes]
---

# Soft Delete para Deleção Lógica de Produtos

## Contexto e Problema

Ao deletar um produto, existe a necessidade de manter o histórico de existência do registro para fins de auditoria — por exemplo, pedidos históricos que referenciam um produto não devem quebrar, e o time de auditoria deve poder consultar o que existia no passado. Como implementar deleção sem perda de dados?

## Opções Consideradas

* Hard delete (remoção física do registro)
* Soft delete com coluna `Ativo` (deleção lógica)
* Tabela de arquivo separada (mover para `Produtos_Historico`)
* Temporal tables (SQL Server / SQLite não suportado)

## Decisão

Opção escolhida: "Soft delete com coluna `Ativo`", porque é a abordagem mais simples que atende ao requisito de auditoria sem complexidade adicional de infraestrutura ou migração de dados.

`DELETE /produtos/{id}` executa `produto.Desativar()` que seta `Ativo = false`. Todos os endpoints de consulta filtram `Ativo = 1` na query — um produto inativo retorna 404 em todos os endpoints, inclusive GET por ID.

A coluna `Ativo` possui índice no banco para não penalizar performance das queries de leitura.

### Consequências

* Positivo, porque registros deletados são preservados para auditoria e podem ser reativados.
* Positivo, porque pedidos históricos que referenciam o produto não quebram.
* Negativo, porque todas as queries precisam incluir o filtro `WHERE Ativo = 1` — um desenvolvedor que esquecer o filtro vai expor produtos inativos.
* Negativo, porque o banco cresce indefinidamente com registros inativos. Sem política de purge, isso pode se tornar um problema em produção.

---

_Formato baseado no template [MADR — Markdown Architectural Decision Records](https://github.com/adr/madr/blob/develop/template/adr-template-minimal.md)._
