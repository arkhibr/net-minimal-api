# 📚 Guia Definitivo: Melhores Práticas para Criação de APIs REST

Este é um guia conceitual desenvolvido **para todos os níveis**. Não vamos assumir que você já saiba arquitetura de software complexa. Aqui, explicaremos o *porquê* e o *como* das principais regras do desenvolvimento moderno de APIs.

---

## Índice
1. [O que é uma API REST?](#1-o-que-é-uma-api-rest)
2. [A Anatomia de uma URL e Design de Endpoints](#2-a-anatomia-de-uma-url)
3. [Os Verbos HTTP e a Idempotência (MUITO IMPORTANTE)](#3-os-verbos-http-e-a-idempotência)
4. [Segurança e JWT (JSON Web Tokens)](#4-segurança-e-jwt)
5. [Tratamento de Erros e Códigos HTTP](#5-tratamento-de-erros)
6. [Evolução e Versionamento](#6-evolução-e-versionamento)
7. [Performance e Resiliência](#7-performance-e-resiliência)

---

## 1. O que é uma API REST?

API significa *Application Programming Interface* (Interface de Programação de Aplicação). Resumindo de forma bem prática, é como se fosse um **"garçom"** em um restaurante.
- **O Cliente:** É você (o aplicativo do celular, ou um site). 
- **O Garçom (API):** É quem recebe o seu pedido.
- **A Cozinha:** É o Servidor / Banco de Dados onde a mágica acontece.

O garçom anota seu pedido e leva pra cozinha. A cozinha prepara, devolve para o garçom e ele entrega a você (o *Response*). 

**Mas o que é REST?**
REST é um "estilo" ou conjunto de regras e acordos sobre como esse garçom deve se comportar. Um garçom que segue o padrão REST tenta atender a premissas básicas, sendo a principal delas o modelo *Stateless* (O servidor não memoriza transações anteriores; se você pedir "mais um copo", ele precisa saber "um copo de que?").

---

## 2. A Anatomia de uma URL e Design de Endpoints

Endpoints devem representar recursos, como pastas no seu computador.
A grande regra do REST é: **Use Substantivos no plural e evite verbos na URL**.

- ✅ ERRADO: `/pegar-produtos-ativos`
- ✅ ERRADO: `/criarProduto`
- 🟢 CORRETO: `/produtos`

**Como fazemos ações se não usamos verbos na URL?**
Nós delegamos o "Verbo" para o próprio protocolo (o protocolo HTTP). É o Verbo HTTP da requisição que dirá o que queremos fazer naquela pasta `/produtos`.

### Paginação é inegociável
Nunca devolva o banco de dados inteiro. Sempre exija `page` e `pageSize`.
- 🟢 Correto: `/produtos?page=1&pageSize=20`

---

## 3. Os Verbos HTTP e a Idempotência

O método como o Cliente (aplicativo) chama o Servidor (API) muda de acordo com o Verbo da operação:

- **GET**: "Quero **ler** informações de `/produtos`."
- **POST**: "Quero **criar** uma informação nova em `/produtos`."
- **PUT**: "Quero **substituir/atualizar TUDO** no item `/produtos/123`."
- **PATCH**: "Quero **modificar uma coisinha pequena** no item `/produtos/123`."
- **DELETE**: "Quero **limpar/apagar** o item `/produtos/123`."

### 🤯 O que raios é Idempotência?
Na matemática e na programação, **Idempotência é a propriedade de uma operação poder ser repetida várias vezes de forma idêntica e o resultado final no servidor não se alterar a partir da 2ª vez**.

**Exemplo da vida real (Controle Remoto da TV):**
- O botão "Aumentar Volume (+)" **NÃO É** idempotente. Se você apertar ele 5 vezes, o volume vai aumentar 5 vezes (efeito colaterais continuam acumulando).
- O botão "Colocar no Canal 4" **É** idempotente. Você pode apertá-lo uma vez, ou apertá-lo 500 vezes sem parar... o resultado final será exatamente o mesmo: a TV estará no Canal 4.

**Na API:**
- **GET**: É Idempotente. Ler algo 100 vezes não altera o banco.
- **PUT** e **DELETE**: São Idempotentes! Deletar um item ou substituir um item na íntegra dez vazes resultará no item estando modificado ou excluído da mesma maneira.
- **POST (Criação): NÃO É IDEMPOTENTE!** 
  Se você apertar "Comprar" em um site, e a sua internet piscar, seu celular tentará enviar a requisição novamente. Se o servidor não tiver mecanismos de defesa (Idempotency Key), **o sistema gerará duas compras e você será cobrado duas vezes**!

Nesta API (Projeto Exemplo), implementamos uma defesa real através de um **IdempotencyMiddleware**: 
Quando fazemos um POST, se passarmos um cabeçalho chamado `Idempotency-Key: xyz-123`, a API o grava temporariamente num *Cache*, de forma que se a mesma requisição chegar batendo duplicada por lag de rede, ela rejeita o processamento repetido e protege o banco!

---

## 4. Segurança e JWT

Nenhum sistema moderno vive sem proteção, e a forma número #1 de protegermos APIs hoje é usando JWT.

### O que é o JWT (JSON Web Tokens)?
Imagine o JWT como uma **pulseira VIP** numa balada (sua API).
Quando você está "do lado de fora", o segurança não te conhece e não te deixa pegar bebidas nos endpoints privados. 
1. Você precisa "falar com o gerente" usando Endpoint de Login (`/api/auth/login`).
2. Se você der as senhas e usuários certos, o gerente gera uma Pulseira pra você (o Token JWT) com a data de validade impressa (Ex: Expira em 2 horas).
3. Essa pulseira contém as "Claims" (A cor da pulseira, se você tem permissões "Admin" ou não).
4. O servidor "assina/carimba" essa pulseira com uma "Chave Secreta" usando Criptografia Criptográfica, para que você não consiga imprimir falsamente na sua casa usando sulfite!
5. Para qualquer próxima requisição, você precisa mandar o header `Authorization: Bearer <Sua_Pulseira>`. A API confere o carimbo mágico secreto dela.

Neste repositório, implementamos de fato um `AuthEndpoints.cs` e configuramos o `[Authorize]` em endpoints críticos! Experimente o Swagger!

---

## 5. Tratamento de Erros

Não responda problemas do cliente com a tela azul da morte. Use Códigos de Status (Status Codes) e mensagens descritivas.

- **Família dos 200 (Sucesso)**
  - `200 OK`: Tudo certo.
  - `201 Created`: O POST conseguiu criar a entidade e salvar no banco.
  - `204 No Content`: O DELETE deu certo, e eu não tenho nada de texto pra te devolver pra ler. Apenas ocorreu sucesso.

- **Família dos 400 (O erro foi seu - Do Aplicativo / Cliente)**
  - `400 Bad Request`: Você tentou salvar, ou mandou algo ilegível/quebrado.
  - `401 Unauthorized`: Você não mandou o Token JWT (Pulseira Vip).
  - `403 Forbidden`: Você tem a pulseira JWT, mas ela é comum, e você tentou entrar na Área VIP que precisa de nível Admin.
  - `404 Not Found`: Você tentou abrir o Produto com ID que não existe.
  - `422 Unprocessable Entity`: O arquivo chegou bem, mas a regra de negócio (ex: preço negativo) falhou a importação ou validação!

- **Família dos 500 (O erro foi nosso - Do Servidor)**
  - `500 Internal Server Error`: Ocorreu uma exceção brutal, nosso código caiu, ou o banco derreteu. É problema nosso, tente novamente depois.

---

## 6. Evolução e Versionamento

Seu Mobile App leva tempo pra atualizar e ser aprovado pela loja do Google/Apple. Se você mudar a regra do seu servidor e quebrar como ele funciona de repente, todos os aplicativos antigos no celular dos clientes vão **craxar simultaneamente!**

Para isso usamos *Versioning*. Exemplo de Url Path (A mais usada):
- Sua API antiga vive em `/api/v1/produtos`.
- Quando você redesenha ela (para ter uma quebra grave) você não deleta a primeira. Você cria a `/api/v2/produtos`. 
As duas continuam online ao mesmo tempo, permitindo as pessoas baixarem o App Novo as poucos para gradualmente migrarem de versão!

---

## 7. Performance e Resiliência

Para finalizar, APIs boas são paranoicas e precavidas.
1. **Caching**: Se uma lista de "Categorias de Produtos" nunca muda, por favor, pare de perguntar ao banco de dados mil vezes por segundo. Mantenha em memória (Cache).
2. **CORS:** Browsers (Navegadores Google Chrome/Edge) são muito restritos. Por padrão eles não deixam um script rodando do domínio `site-dos-carros.com` fazer uma chamada escondida para a `site-do-banco.com`. Essa trava só é liberta se configurarmos a Política CORS na API dizendo quem é amigo.
3. **Validação**: Aplicamos a biblioteca (FluentValidation) para garantir que ninguém cadastre produtos contendo strings HTML maliciosas ou números de preço negativos. A API não deve confiar cegamente no front-end. Nunca!
