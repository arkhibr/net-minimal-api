# Idempotência via Middleware com Idempotency-Key

## Context and Problem Statement

Em operações de escrita (POST, PUT, PATCH), clientes podem reenviar a mesma requisição por timeout, instabilidade de rede ou retry automático. Sem proteção, isso resulta em duplicação de recursos (ex: dois pedidos criados a partir do mesmo clique). Como garantir que requisições repetidas com o mesmo payload retornem o mesmo resultado sem efeitos colaterais?

## Considered Options

* Idempotência na lógica de negócio de cada handler
* Idempotência via middleware centralizado com header `Idempotency-Key`
* API Gateway com deduplicação por hash de payload
* Sem idempotência explícita (aceitar duplicidade)

## Decision Outcome

Chosen option: "Idempotência via middleware centralizado com header `Idempotency-Key`", porque implementar uma única vez no middleware garante proteção uniforme para todos os endpoints de escrita sem que cada handler precise reimplementar a lógica.

O middleware intercepta requisições POST, PUT e PATCH. Se o header `Idempotency-Key` estiver presente e a chave já existir no cache, retorna a resposta armazenada sem executar o handler. Apenas respostas 2xx são cacheadas. O TTL é de 24 horas.

**Limitação conhecida:** A implementação usa `IMemoryCache`, que é volátil — não sobrevive a restarts do servidor. Em um ambiente com múltiplas instâncias ou alta disponibilidade, um cliente que retenta após um restart pode processar a requisição duas vezes. Em produção, o cache deveria ser substituído por Redis ou uma tabela de idempotência persistida em banco de dados.

### Consequences

* Good, porque a proteção contra duplicidade é aplicada automaticamente a todos os endpoints de escrita sem mudança nos handlers.
* Good, porque o contrato é simples: o cliente envia um `Idempotency-Key` UUID e pode retentar com segurança.
* Bad, porque o cache em memória não é durável — restarts do servidor invalidam todas as chaves armazenadas.
* Bad, porque em deployment multi-instância (load balancer), cada instância tem seu próprio cache, quebrando a garantia de idempotência.
