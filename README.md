# ProdutosAPI — .NET 10 Minimal API

Projeto educacional em .NET 10 que implementa uma API REST completa demonstrando, lado a lado, princípios e padrões arquiteturais distintos aplicados ao mesmo stack tecnológico. Cada módulo resolve o mesmo tipo de problema de uma forma diferente, tornando a comparação direta o ponto central do aprendizado.

---

## Princípios e padrões implementados

### Clean Architecture (módulo Catálogo)

O Catálogo é organizado em quatro camadas com responsabilidades bem definidas e separadas em sub-projetos independentes:

- **Domain** — entidades, value objects e interfaces de repositório. Nenhuma dependência de infraestrutura.
- - **Application** — serviços de aplicação que orquestram casos de uso, DTOs e validadores com FluentValidation.
  - - **Infrastructure** — repositórios concretos com EF Core, implementando as interfaces definidas no Domain.
    - - **API** — endpoints Minimal API, sem lógica de negócio.
     
      - A regra de dependência é sempre de fora para dentro: a camada de API depende de Application, que depende de Domain. Domain não depende de nenhuma outra camada.
     
      - ### Vertical Slice Architecture (módulo Pedidos)
     
      - O módulo de Pedidos abandona a organização por camada técnica e adota a organização por caso de uso. Cada operação (criar pedido, adicionar item, cancelar) é uma pasta autocontida com seu próprio command, validador, handler e endpoint. Alterar o comportamento de uma operação não exige tocar em nenhuma outra pasta.
     
      - ### Domínio Rico vs. Modelo Anêmico
     
      - Os dois estilos coexistem intencionalmente no projeto para comparação direta:
     
      - **Modelo anêmico** (Atributo e Mídia no Catálogo): entidades são contêineres de dados com apenas propriedades `get/set`. Toda lógica vive nos serviços de aplicação. Adequado para CRUD sem regras de negócio.
     
      - **Domínio rico** (Produto, Categoria, Variante no Catálogo e aggregate Pedido): entidades encapsulam suas próprias invariantes em métodos de domínio. `Pedido.AddItem()` verifica se o pedido está aberto e se há estoque suficiente antes de adicionar o item. `Categoria` gera seu próprio slug e valida hierarquia. Regras de negócio vivem no lugar onde o estado é mantido.
     
      - ### Value Objects
     
      - `SKU` em Variante e `PrecoProduto` e `EstoqueProduto` são value objects — tipos imutáveis sem identidade própria que encapsulam invariantes (`SKU` valida regex `^[A-Z0-9\-]+$`, `Preco` impede valor negativo). Eliminam validações espalhadas e tornam o tipo inválido impossível de representar.
     
      - ### Result Pattern (sem exceções para erros de negócio)
     
      - O módulo de Pedidos usa `Result<T>` em vez de exceções para erros esperados. Métodos de domínio retornam `Result<Pedido>.Fail("Pedido não está aberto")` em vez de lançar `InvalidOperationException`. O handler sempre verifica `IsSuccess` antes de acessar `.Value`. O fluxo de controle fica linear e legível, sem blocos `try/catch` para erros previsíveis.
     
      - ### CQRS leve — segregação de repositórios Query/Command
     
      - No Catálogo, queries e commands usam interfaces separadas (`IProdutoQueryRepository` e `IProdutoCommandRepository`). Queries retornam DTOs diretamente do banco, sem passar pela camada de domínio. Commands operam sobre entidades. Isso elimina mapeamentos desnecessários em leitura sem a complexidade de event sourcing.
     
      - ### Auto-descoberta de endpoints via reflexão
     
      - Todos os endpoints implementam a interface `IEndpoint`. O `Program.cs` varre o assembly em tempo de inicialização e registra automaticamente todas as implementações. Adicionar um novo endpoint não exige nenhum registro manual — basta criar a classe.
     
      - ### Idempotência
     
      - Um middleware global intercepta requisições `POST`, `PUT` e `PATCH` com o header `Idempotency-Key`. Se a chave já foi vista, a resposta cacheada é devolvida imediatamente. Se a mesma chave chegar com um payload diferente, retorna `409 Conflict`. O módulo Pix demonstra isso de forma didática com exemplos de fluxos financeiros.
     
      - ### Rate Limiting com três algoritmos distintos
     
      - Três políticas com algoritmos diferentes são aplicadas no Catálogo, cada uma adequada ao seu contexto:
     
      - - **Fixed Window** (`leitura`) — 60 requisições por janela de 60s. Para leituras com tráfego alto e previsível.
        - - **Sliding Window** (`escrita`) — 20 requisições por minuto em 6 segmentos de 10s. Distribui melhor rajadas curtas do que janela fixa.
          - - **Token Bucket** (`criacao-produto`) — 5 tokens por minuto, repostos continuamente. Controle mais granular para operações de maior custo.
           
            - Todas retornam `429 Too Many Requests` com o header `Retry-After`.
           
            - ### Pipeline de resiliência com Polly
           
            - Os módulos ClientDemo demonstram retry, circuit breaker e timeout compostos via `Microsoft.Extensions.Http.Resilience` (Polly v8). O pipeline do Catálogo.ClientDemo tem quatro camadas: timeout por tentativa → retry com backoff exponencial e jitter → circuit breaker → timeout global. O Pix.ClientDemo usa `AddStandardResilienceHandler`, que configura o mesmo conjunto automaticamente.
           
            - ### Integração externa com mTLS e OAuth2 (módulo Pix)
           
            - O módulo Pix inclui um servidor mock que simula a API Pix do Banco Central com autenticação mútua TLS (mTLS) e OAuth2 (client credentials). O cliente tipado demonstra a cadeia completa: obtenção de token, injeção de `X-Correlation-Id` e `Idempotency-Key` via handlers encadeados, e consumo com resiliência. Tudo sem dependência de ambiente externo.
           
            - ### Tratamento de erro padronizado (RFC 7807 Problem Details)
           
            - Um middleware global captura exceções não tratadas e retorna respostas no formato `application/problem+json`, com `status`, `title`, `detail` e `type`. Endpoints de validação retornam `422 Unprocessable Entity` com o mesmo formato.
           
            - ### Soft Delete
           
            - `DELETE /produtos/{id}` seta `Ativo = false` em vez de remover o registro. O repositório aplica o filtro automaticamente em todas as queries — um produto inativo retorna `404` em todos os endpoints, sem nenhuma lógica adicional no endpoint.
           
            - ### Estratégia de testes em camadas
           
            - 150 testes cobrem três níveis distintos:
           
            - - **Unitários de domínio** — testam entidades e agregados diretamente, sem infraestrutura. `PedidoTests` testa `Pedido.AddItem()` com pedido cancelado sem instanciar banco ou HTTP.
              - - **Unitários de serviço** — testam serviços de aplicação com repositório mockado.
                - - **Integração HTTP** — sobem a aplicação real com `WebApplicationFactory` e banco em memória, fazem requisições HTTP e verificam status codes, headers e payload.
                  - - **Rate limiting isolado** — `RateLimitingApiFactory` substitui as políticas por limites baixos (2–3 req/janela) para testar comportamento de throttling sem depender de timing real.
                   
                    - ---

                    ## Módulos

                    | Módulo | Padrão principal | Rota base |
                    |---|---|---|
                    | Catálogo | Clean Architecture híbrida | `/api/v1/catalogo/*` |
                    | Pedidos | Vertical Slice + Domínio Rico | `/api/v1/pedidos/*` |
                    | Pix | Mock Server + HTTP Client resiliente | `/pix/v1/*` |

                    ---

                    ## Início rápido

                    **Pré-requisito:** [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

                    ```bash
                    git clone https://github.com/arkhibr/net-minimal-api.git
                    cd net-minimal-api
                    dotnet run --project src/Catalogo/Catalogo.API
                    # Swagger: http://localhost:5001/swagger

                    dotnet test ProdutosAPI.slnx -v minimal
                    ```

                    Credenciais para JWT: `admin@example.com` / `senha123`

                    ---

                    ## Documentação

                    | Arquivo | Conteúdo |
                    |---|---|
                    | `docs/01-ARQUITETURA.md` | Visão estrutural, fluxos de requisição e comparativo CA vs VSA |
                    | `docs/02-CATALOGO.md` | Clean Architecture, domínio rico, rate limiting, resiliência |
                    | `docs/03-PEDIDOS.md` | Vertical Slice, Result pattern, domínio rico, auto-discovery |
                    | `docs/04-PIX.md` | mTLS, OAuth2, idempotência, pipeline de HttpClient |
                    | `docs/05-TESTES.md` | Estratégia de testes, factories e isolamento de rate limiting |
                    | `docs/ADRs/` | 15 decisões arquiteturais no formato MADR 3.x |
