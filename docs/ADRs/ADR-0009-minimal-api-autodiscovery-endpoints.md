# Minimal API com Auto-Discovery de Endpoints via Reflexão

## Context and Problem Statement

O projeto usa ASP.NET Core Minimal APIs (sem controllers) para maior simplicidade e performance. Com a arquitetura Vertical Slice em Pedidos, cada slice define seu próprio endpoint. Como registrar esses endpoints sem um `Program.cs` que cresce linearmente a cada nova funcionalidade adicionada?

## Considered Options

* Registro manual de cada endpoint no `Program.cs`
* Controllers MVC tradicionais (atributo `[Route]`, `[HttpPost]`, etc.)
* Interface `IEndpoint` com auto-discovery por reflexão na inicialização

## Decision Outcome

Chosen option: "Interface `IEndpoint` com auto-discovery por reflexão", porque permite que cada slice defina e registre seu próprio endpoint de forma autossuficiente, sem que o `Program.cs` precise conhecer os slices existentes — alinhado com o princípio Open/Closed.

A interface `IEndpoint` (`src/Shared/Common/IEndpoint.cs`) define um único método `MapEndpoints(IEndpointRouteBuilder)`. Na inicialização, `AddEndpointsFromAssembly()` reflete sobre o assembly e registra no DI todas as implementações de `IEndpoint`. Em seguida, `MapRegisteredEndpoints()` chama `MapEndpoints()` em cada instância registrada.

Para adicionar um novo endpoint em Pedidos: criar uma classe que implementa `IEndpoint` — sem tocar em `Program.cs`.

### Consequences

* Good, porque adicionar um novo endpoint não requer modificação do `Program.cs` — a descoberta é automática.
* Good, porque mantém a coesão do Vertical Slice: toda a definição do slice (Command, Handler, Validator, Endpoint) fica no mesmo diretório.
* Bad, porque o comportamento "mágico" por reflexão pode ser opaco para desenvolvedores que não conhecem o mecanismo — um endpoint pode existir sem nenhuma referência explícita no ponto de entrada da aplicação.
* Bad, porque erros de configuração em um `IEndpoint` podem causar falhas silenciosas na inicialização se não houver logging adequado do processo de discovery.
