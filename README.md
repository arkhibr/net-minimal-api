# ProdutosAPI — .NET 10 Minimal API

> Projeto educacional em .NET 10 Minimal API demonstrando três bounded contexts com padrões arquiteturais distintos coexistindo no mesmo repositório.
>
> ---
>
> ## O que é este projeto?
>
> **ProdutosAPI** é um laboratório de arquitetura de software construído sobre .NET 10 Minimal API. O objetivo não é apresentar *a* arquitetura correta, mas mostrar como diferentes abordagens resolvem o mesmo problema — uma API REST com persistência, validação, autenticação e testes — com diferentes graus de estrutura e separação de responsabilidades.
>
> Três bounded contexts coexistem intencionalmente no mesmo repositório, cada um adotando um padrão distinto, permitindo **comparação direta e estudo lado a lado**.
>
> ---
>
> ## Os três bounded contexts
>
> ### 🗂 Catálogo — Clean Architecture híbrida
> Organizado em sub-projetos (`Domain / Application / Infrastructure / API`), o contexto de Catálogo demonstra entidades com domínio rico, value objects, repositórios abstraídos por interfaces e rate limiting por política de rota. Contém 5 recursos: Produto, Categoria, Variante, Atributo e Mídia.
>
> ### 📦 Pedidos — Vertical Slice + Domínio Rico
> Organizado por caso de uso (cada operação é uma pasta isolada), o contexto de Pedidos demonstra um agregado rico com regras de negócio encapsuladas, o padrão `Result<T>` em substituição a exceções e auto-descoberta de endpoints via reflection. Autenticação JWT obrigatória.
>
> ### 💳 Pix — Mock Server + HTTP Client com resiliência
> Simula a integração com a API Pix do Banco Central. Inclui um servidor mock que implementa mTLS e OAuth2, além de um cliente HTTP tipado com pipelines de resiliência (retry, circuit breaker via Polly / `Microsoft.Extensions.Http.Resilience`). Ideal para estudar integração com APIs externas de forma realista e segura.
>
> ---
>
> ## Por que estudar este projeto?
>
> - Você verá **Clean Architecture** e **Vertical Slice Architecture** aplicadas a problemas reais e comparáveis entre si.
> - - Você entenderá como **domínio rico** e **Result pattern** eliminam o uso de exceções para controle de fluxo.
>   - - Você aprenderá como integrar com APIs externas usando **mTLS, OAuth2 e resiliência** sem depender de ambientes externos.
>     - - Todas as decisões arquiteturais estão documentadas em **15 ADRs** no formato MADR 3.x, com contexto, alternativas consideradas e consequências.
>       - - O projeto conta com **150 testes automatizados** cobrindo unidade, integração e comportamentos de rate limiting.
>        
>         - ---
>
> ## Início rápido
>
> **Pré-requisito:** [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
>
> ```bash
> git clone https://github.com/arkhibr/net-minimal-api.git
> cd net-minimal-api
>
> # Executar o contexto de Catálogo
> dotnet run --project src/Catalogo/Catalogo.API
>
> # Swagger UI disponível em:
> # http://localhost:5001/swagger
>
> # Rodar todos os testes (150 no total)
> dotnet test ProdutosAPI.slnx -v minimal
> ```
>
> Credenciais para JWT no Swagger: `admin@example.com` / `senha123`
>
> ---
>
> ## Bounded Contexts
>
> | Contexto | Padrão | Rotas base | Destaques |
> |---|---|---|---|
> | Catálogo | Clean Architecture híbrida | `/api/v1/catalogo/*` | 5 recursos, rate limiting, soft delete |
> | Pedidos | Vertical Slice + Domínio Rico | `/api/v1/pedidos/*` | Agregado rico, Result pattern, JWT obrigatório |
> | Pix | Mock Server + HTTP Client | `/pix/v1/*` | mTLS, OAuth2, idempotência, resiliência |
>
> ---
>
> ## Tecnologias
>
> .NET 10 · EF Core 10 · SQLite · FluentValidation · Polly / Http.Resilience · JWT Bearer · xUnit · FluentAssertions · AutoMapper · Serilog · Swagger / OpenAPI
>
> ---
>
> ## Documentação
>
> | Arquivo | Conteúdo |
> |---|---|
> | `docs/00-VISAO-GERAL.md` | Visão geral e roteiros de aprendizado por nível |
> | `docs/01-ARQUITETURA.md` | Diagramas e decisões arquiteturais |
> | `docs/02-CATALOGO.md` | Clean Architecture híbrida, recursos e rate limiting |
> | `docs/03-PEDIDOS.md` | Vertical Slice, domínio rico, Result pattern |
> | `docs/04-PIX.md` | Mock Server, mTLS, OAuth2, cliente HTTP resiliente |
> | `docs/05-TESTES.md` | Estratégia de testes, factories e helpers |
> | `docs/ADRs/` | 15 ADRs no formato MADR 3.x |
>
> ---
>
> ## Estrutura de diretórios
>
> ```
> net-minimal-api/
> ├── src/
> │   ├── Catalogo/          # Clean Architecture híbrida
> │   ├── Pedidos/           # Vertical Slice + Domínio Rico
> │   ├── Pix/               # Mock Server + HTTP Client
> │   └── Shared/            # Componentes compartilhados
> └── tests/
>     ├── ProdutosAPI.Tests/ # 143 testes
>     └── Pix.MockServer.Tests/ # 7 testes
> ```
>
> ---
>
> > **Nenhum padrão é prescrito como "o correto"** — a coexistência intencional é o ponto central do aprendizado.
