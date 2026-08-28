# TaskManager API — Autenticação com Microsoft Entra ID

Este documento explica, de forma prática e reutilizável, como funciona a autenticação de uma API ASP.NET Core com **Microsoft Entra ID**, **JWT Bearer**, **Microsoft.Identity.Web**, **Scopes** e a configuração **Expose an API**.

O objetivo é servir como referência para este projeto e também para projetos futuros.

---

# 1. Visão geral

A API é protegida pelo Microsoft Entra ID.

O fluxo básico é:

```text
Cliente
  |
  | solicita autenticação
  v
Microsoft Entra ID
  |
  | emite Access Token JWT
  v
Cliente
  |
  | Authorization: Bearer <token>
  v
TaskManager.API
  |
  | valida o token
  v
Controller
```

Neste projeto, durante o desenvolvimento, o **Azure CLI** pode ser usado como cliente para obter um Access Token e testar a API pelo Swagger.

Em produção, o cliente pode ser, por exemplo:

- React + MSAL
- Angular + MSAL
- Aplicação mobile
- Outra API
- Postman
- Azure CLI

A API não precisa saber qual cliente gerou o token.

Ela apenas recebe e valida um JWT emitido pelo Microsoft Entra ID.

---

# 2. Conceitos principais

Antes da configuração, é importante separar alguns conceitos.

## App Registration

Um **App Registration** representa a identidade lógica de uma aplicação dentro do Microsoft Entra ID.

Exemplos:

```text
taskmanager-api
taskmanager-web
erp-api
finance-api
portal-react
```

Cada App Registration possui um identificador chamado:

```text
Application (client) ID
```

Exemplo:

```text
0ae744a0-1aea-44db-af8f-f61583721b2b
```

Apesar do nome conter `client`, esse ID também é utilizado para representar APIs.

---

# 3. O que significa "Expose an API"

No portal do Azure:

```text
Microsoft Entra ID
    |
    v
App registrations
    |
    v
taskmanager-api
    |
    v
Expose an API
```

**Expose an API não publica a API na internet.**

Ele também não hospeda a API.

A API continua rodando normalmente em algum endereço, por exemplo:

```text
https://localhost:7192
```

ou futuramente:

```text
https://taskmanager-api.azurewebsites.net
```

O objetivo de **Expose an API** é registrar no Microsoft Entra ID:

> "Esta aplicação representa uma API protegida e estas são as permissões que outros clientes podem solicitar para acessá-la."

Em termos de OAuth 2.0, a API passa a representar um:

```text
Resource Server
```

---

# 4. Application ID URI

Dentro de **Expose an API**, existe:

```text
Application ID URI
```

Exemplo deste projeto:

```text
api://0ae744a0-1aea-44db-af8f-f61583721b2b
```

Esse valor é a identidade lógica da API no Microsoft Entra ID.

Ele não precisa ser uma URL HTTP real.

Ou seja:

```text
URL real da API:
https://localhost:7192

Identidade OAuth da API:
api://0ae744a0-1aea-44db-af8f-f61583721b2b
```

São conceitos diferentes.

Mentalmente:

```text
TaskManager.API
|
+-- Endereço HTTP
|   `-- https://localhost:7192
|
`-- Identidade no Entra ID
    `-- api://0ae744a0-1aea-44db-af8f-f61583721b2b
```

---

# 5. Resource

Quando um cliente solicita um token, ele precisa informar para qual recurso deseja o token.

Neste projeto, o recurso é:

```text
api://0ae744a0-1aea-44db-af8f-f61583721b2b
```

Por isso este comando funciona:

```powershell
az account get-access-token `
  --resource api://0ae744a0-1aea-44db-af8f-f61583721b2b
```

Ele significa aproximadamente:

> "Microsoft Entra ID, quero um token para acessar a TaskManager API."

---

# 6. Scope

Um **Scope** é uma permissão delegada exposta pela API.

Neste projeto foi criado:

```text
access_as_user
```

O identificador completo é:

```text
api://0ae744a0-1aea-44db-af8f-f61583721b2b/access_as_user
```

Isso deve ser lido assim:

```text
api://0ae744a0-1aea-44db-af8f-f61583721b2b
|
`-- access_as_user
```

Ou:

```text
RESOURCE
|
`-- SCOPE
```

O resource identifica a API.

O scope identifica uma permissão dentro dessa API.

---

# 7. Resource e Scope NÃO são a mesma coisa

Este foi um ponto importante durante a configuração.

Isto está errado:

```powershell
az account get-access-token `
  --resource api://0ae744a0-1aea-44db-af8f-f61583721b2b/access_as_user
```

Porque `--resource` espera apenas o identificador da API.

O Entra tentaria procurar uma aplicação com o identificador:

```text
api://0ae744a0-1aea-44db-af8f-f61583721b2b/access_as_user
```

Mas essa aplicação não existe.

O correto é:

```powershell
az account get-access-token `
  --resource api://0ae744a0-1aea-44db-af8f-f61583721b2b
```

Ou, usando o modelo de scopes:

```powershell
az account get-access-token `
  --scope "api://0ae744a0-1aea-44db-af8f-f61583721b2b/.default"
```

---

# 8. Por que `access_as_user` é uma Delegated Permission

O scope:

```text
access_as_user
```

representa uma **permissão delegada**.

Isso significa que existem duas identidades envolvidas:

```text
Aplicação cliente
+
Usuário autenticado
```

Exemplo:

```text
Usuário
   |
   v
Azure CLI
   |
   v
Microsoft Entra ID
   |
   v
TaskManager.API
```

O Azure CLI acessa a API **em nome do usuário autenticado**.

Isso é diferente de um fluxo application-to-application sem usuário.

---

# 9. Authorized client applications

Dentro de:

```text
Expose an API
```

existe também:

```text
Authorized client applications
```

Neste projeto adicionamos o Microsoft Azure CLI.

Client ID oficial do Azure CLI:

```text
04b07795-8ddb-461a-bbee-02f9e1bf7b46
```

E autorizamos o scope:

```text
access_as_user
```

A configuração representa:

```text
TaskManager.API
|
+-- Scope
|   `-- access_as_user
|
`-- Authorized client applications
    |
    `-- Microsoft Azure CLI
        |
        `-- access_as_user
```

Isso significa:

> A TaskManager API pré-autoriza o Azure CLI a solicitar essa permissão.

---

# 10. O erro `consent_required`

Antes da autorização do Azure CLI, a tentativa retornou:

```text
AADSTS65001
consent_required
```

Isso significava:

```text
Azure CLI
    |
    | Quero acessar TaskManager.API
    v
Microsoft Entra ID
    |
    `-- ERRO
        O cliente ainda não possui consentimento/permissão
        para solicitar acesso a esse recurso.
```

Depois de adicionar o Azure CLI em:

```text
Authorized client applications
```

o fluxo passou a funcionar.

---

# 11. Por que `az login` normal não foi suficiente

Um login simples:

```powershell
az login
```

autentica o Azure CLI para trabalhar com Azure e seus recursos padrão.

Porém, neste projeto queríamos que o Azure CLI também solicitasse acesso a uma **API customizada**.

Por isso foi necessário fazer login informando explicitamente o scope da aplicação:

```powershell
az login `
  --tenant <TENANT_ID> `
  --scope "api://<API_CLIENT_ID>/.default"
```

Exemplo:

```powershell
az login `
  --tenant de88cf66-42b8-4b48-a979-71e6ea29a3ed `
  --scope "api://0ae744a0-1aea-44db-af8f-f61583721b2b/.default"
```

---

# 12. O que significa `.default`

`.default` é um scope especial da Microsoft Identity Platform.

Exemplo:

```text
api://0ae744a0-1aea-44db-af8f-f61583721b2b/.default
```

Ele não é um scope criado manualmente.

Ele significa aproximadamente:

> "Solicite as permissões previamente configuradas e consentidas para este recurso."

Portanto:

```text
/.default
```

não deve ser interpretado como:

```text
access_as_user/.default
```

O correto é anexar `.default` ao identificador da API:

```text
CORRETO

api://<API_ID>/.default
```

e não:

```text
ERRADO

api://<API_ID>/access_as_user/.default
```

---

# 13. Obtendo um Access Token pelo Azure CLI

Primeiro:

```powershell
az logout
```

Opcionalmente:

```powershell
az account clear
```

Depois:

```powershell
az login `
  --tenant <TENANT_ID> `
  --scope "api://<API_CLIENT_ID>/.default"
```

Exemplo:

```powershell
az login `
  --tenant de88cf66-42b8-4b48-a979-71e6ea29a3ed `
  --scope "api://0ae744a0-1aea-44db-af8f-f61583721b2b/.default"
```

Depois obtenha o token:

```powershell
az account get-access-token `
  --scope "api://<API_CLIENT_ID>/.default" `
  --query accessToken `
  -o tsv
```

Exemplo:

```powershell
az account get-access-token `
  --scope "api://0ae744a0-1aea-44db-af8f-f61583721b2b/.default" `
  --query accessToken `
  -o tsv
```

A saída será parecida com:

```text
eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIs...
```

Esse valor é o:

```text
Access Token
```

---

# 14. Testando o token no Swagger

Abra:

```text
https://localhost:<PORTA>/swagger
```

Clique:

```text
Authorize
```

Cole apenas:

```text
eyJ...
```

Não é necessário escrever:

```text
Bearer eyJ...
```

quando o Swagger estiver configurado com:

```csharp
Type = SecuritySchemeType.Http,
Scheme = "bearer"
```

O Swagger monta automaticamente o header:

```http
Authorization: Bearer eyJ...
```

---

# 15. Configuração de autenticação no ASP.NET Core

No `Program.cs`:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(
        builder.Configuration.GetSection("AzureAd"));
```

Essa configuração informa ao ASP.NET Core:

> "Esta é uma API protegida por tokens Bearer emitidos pelo Microsoft Entra ID."

Também é necessário:

```csharp
builder.Services.AddAuthorization();
```

E no pipeline:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

A ordem importa.

Primeiro:

```text
UseAuthentication()
```

descobre quem é o usuário.

Depois:

```text
UseAuthorization()
```

decide o que ele pode fazer.

---

# 16. Pipeline de autenticação

Quando chega uma requisição:

```http
GET /api/TodoTasks
Authorization: Bearer eyJ...
```

o fluxo é aproximadamente:

```text
HTTP Request
    |
    v
UseAuthentication()
    |
    v
JwtBearer
    |
    v
Microsoft.Identity.Web
    |
    +-- valida assinatura
    +-- valida issuer
    +-- valida audience
    +-- valida expiração
    |
    v
ClaimsPrincipal
    |
    v
UseAuthorization()
    |
    v
[Authorize]
    |
    v
Controller
```

---

# 17. `[Authorize]`

Um controller pode ser protegido com:

```csharp
using Microsoft.AspNetCore.Authorization;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TodoTasksController : ControllerBase
{
}
```

Ou apenas um endpoint:

```csharp
[Authorize]
[HttpGet]
public IActionResult Get()
{
    ...
}
```

Com `[Authorize]`:

```text
Sem token
    -> 401 Unauthorized

Token inválido
    -> 401 Unauthorized

Token válido
    -> endpoint pode continuar
```

---

# 18. `[Authorize]` não é igual a validar Scope

`[Authorize]` verifica se existe uma identidade autenticada válida.

Mas isso não significa necessariamente que o usuário possui uma determinada permissão.

Por exemplo:

```text
Token válido
```

não significa automaticamente:

```text
Possui access_as_user
```

Para validar scope com Microsoft.Identity.Web, pode-se usar:

```csharp
using Microsoft.Identity.Web;

[Authorize]
[RequiredScope("access_as_user")]
[HttpGet]
public IActionResult Get()
{
    ...
}
```

Fluxo:

```text
Token válido?
    |
    v
[Authorize]
    |
    v
Possui access_as_user?
    |
    v
[RequiredScope]
    |
    v
Controller
```

---

# 19. 401 vs 403

Essa diferença é importante.

## 401 Unauthorized

Significa normalmente:

```text
Não existe autenticação válida.
```

Exemplos:

- token ausente;
- token expirado;
- token inválido;
- assinatura inválida;
- token emitido para outra API.

## 403 Forbidden

Significa aproximadamente:

```text
A identidade é válida,
mas não possui permissão suficiente.
```

Exemplo:

```text
Token válido

mas

scope necessário ausente
```

---

# 20. JWT

O Access Token emitido pelo Entra ID normalmente é um JWT.

Ele contém vários claims.

Exemplo conceitual:

```json
{
  "aud": "0ae744a0-1aea-44db-af8f-f61583721b2b",
  "iss": "https://sts.windows.net/<tenant-id>/",
  "tid": "<tenant-id>",
  "oid": "<object-id-do-usuario>",
  "scp": "access_as_user",
  "exp": 1234567890
}
```

Claims importantes:

| Claim | Significado |
|---|---|
| `aud` | Audience — para qual API o token foi emitido |
| `iss` | Issuer — quem emitiu o token |
| `tid` | Tenant ID |
| `oid` | Object ID do usuário |
| `scp` | Scopes delegados concedidos |
| `exp` | Data/hora de expiração |

---

# 21. Audience (`aud`)

`aud` significa:

```text
Audience
```

Ele identifica para qual aplicação/recurso aquele token foi emitido.

Exemplo:

```json
{
  "aud": "0ae744a0-1aea-44db-af8f-f61583721b2b"
}
```

Isso significa:

> Este Access Token foi emitido para a TaskManager API.

Um token emitido para outro recurso, como Microsoft Graph, não deve ser usado para acessar esta API.

Exemplo:

```text
Token para Microsoft Graph
        |
        v
TaskManager.API
        |
        `-- rejeitado
```

---

# 22. Scope (`scp`) dentro do token

Um token delegado pode conter:

```json
{
  "scp": "access_as_user"
}
```

Isso informa quais permissões delegadas foram concedidas.

Uma API maior poderia trabalhar com scopes como:

```text
Tasks.Read
Tasks.Create
Tasks.Update
Tasks.Delete
```

Então um token poderia possuir:

```json
{
  "scp": "Tasks.Read Tasks.Update"
}
```

---

# 23. O Entra ID não é consultado em toda requisição

Um erro conceitual comum é imaginar:

```text
API
 |
 | "Esse token é válido?"
 v
Microsoft Entra ID
```

Isso não acontece a cada request normalmente.

O JWT possui assinatura criptográfica.

A API consegue validar essa assinatura utilizando as chaves públicas publicadas pelo Microsoft Entra ID.

Fluxo simplificado:

```text
Microsoft Entra ID
    |
    | assina JWT
    v
Cliente
    |
    | Authorization: Bearer <JWT>
    v
TaskManager.API
    |
    | valida assinatura localmente
    v
Controller
```

Isso torna o modelo escalável.

---

# 24. Access Token vs ID Token

Esses dois tokens possuem objetivos diferentes.

## ID Token

Responde principalmente:

```text
Quem é o usuário?
```

É utilizado pelo cliente durante autenticação.

## Access Token

Responde principalmente:

```text
Este cliente possui autorização para acessar este recurso?
```

É o token enviado para a API.

Portanto:

```text
React
 |
 | Access Token
 v
TaskManager.API
```

Nunca use um token apenas porque ele "parece ser JWT".

A API deve receber um **Access Token emitido especificamente para ela**.

---

# 25. Azure CLI como cliente de teste

O Azure CLI possui sua própria identidade no Microsoft Entra ID.

Client ID:

```text
04b07795-8ddb-461a-bbee-02f9e1bf7b46
```

No fluxo deste projeto:

```text
CLIENT

Microsoft Azure CLI
04b07795-...
        |
        | solicita acesso
        v
RESOURCE

TaskManager.API
0ae744a0-...
```

O Azure CLI não faz parte da API.

Ele é apenas um cliente utilizado para obter e testar tokens.

---

# 26. Como isso será no React

Durante desenvolvimento:

```text
Azure CLI
    |
    v
Entra ID
    |
    v
Access Token
    |
    v
Swagger
    |
    v
TaskManager.API
```

Quando existir um frontend React:

```text
React
  |
  v
MSAL
  |
  v
Microsoft Entra ID
  |
  v
Access Token
  |
  v
React
  |
  | Authorization: Bearer <token>
  v
TaskManager.API
```

A API continuará usando a mesma lógica de validação.

---

# 27. O backend não precisa saber quem obteve o token

A API pode receber tokens provenientes de:

```text
React
Postman
Azure CLI
Aplicação mobile
Outra aplicação
```

O backend se preocupa com:

```text
Token foi emitido por uma autoridade confiável?

Token é destinado à minha API?

Token está válido?

Token expirou?

Quais scopes possui?

Quem é o usuário?
```

Não com:

```text
"Foi o React que criou esse token?"
```

Aliás, o React não cria o JWT.

Quem emite o JWT é:

```text
Microsoft Entra ID
```

---

# 28. Scopes podem ser mais específicos

Para uma API simples:

```text
access_as_user
```

é suficiente.

Em uma aplicação maior, é possível expor scopes mais específicos:

```text
Tasks.Read

Tasks.Create

Tasks.Update

Tasks.Delete
```

Exemplo:

```text
TaskManager.API
|
+-- Tasks.Read
+-- Tasks.Create
+-- Tasks.Update
`-- Tasks.Delete
```

Um cliente poderia solicitar apenas:

```text
Tasks.Read
```

e não ter permissão para modificar dados.

---

# 29. Scope não é Role

Outro conceito importante:

```text
Scope != Role
```

Scopes normalmente representam:

```text
O que um cliente pode fazer em nome do usuário?
```

Exemplo:

```text
Tasks.Read
Tasks.Write
```

Roles normalmente representam:

```text
Qual papel o usuário/aplicação possui?
```

Exemplo:

```text
ADMIN
USER
MANAGER
```

É possível combinar ambos.

Exemplo:

```text
Scope:
Tasks.Write

Role:
ADMIN
```

E criar regras como:

```text
Usuário precisa estar autenticado
+
cliente precisa possuir Tasks.Write
+
usuário precisa possuir role ADMIN
```

---

# 30. Delegated Permission vs Application Permission

Existem dois grandes cenários.

## Delegated Permission

Existe usuário.

```text
Usuário
  |
  v
React / Azure CLI
  |
  v
Entra ID
  |
  v
API
```

O token representa:

```text
Aplicação + usuário
```

Scopes normalmente aparecem em:

```text
scp
```

---

## Application Permission

Não existe usuário interativo.

Exemplo:

```text
Worker Service
    |
    v
Entra ID
    |
    v
API
```

ou:

```text
API A
  |
  v
API B
```

Nesse cenário normalmente são utilizadas:

```text
App Roles / Application Permissions
```

e o token pode utilizar:

```text
roles
```

em vez de:

```text
scp
```

---

# 31. Configuração conceitual completa

A configuração atual pode ser visualizada assim:

```text
                    MICROSOFT ENTRA ID

        +--------------------------------------+
        |                                      |
        | taskmanager-api                      |
        |                                      |
        | Application ID                       |
        | 0ae744a0-...                         |
        |                                      |
        | Application ID URI                   |
        | api://0ae744a0-...                   |
        |                                      |
        | Exposed Scope                        |
        | access_as_user                       |
        |                                      |
        | Authorized Client                    |
        | Microsoft Azure CLI                  |
        | 04b07795-...                         |
        |                                      |
        +-------------------+------------------+
                            |
                            |
                            v
                   Access Token JWT
                            |
                            |
                            v
                    TaskManager.API
                            |
                    Microsoft.Identity.Web
                            |
                            v
                       [Authorize]
                            |
                            v
                 TodoTasksController
```

---

# 32. Configuração recomendada no `appsettings.json`

Exemplo:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "<TENANT_ID>",
    "ClientId": "<API_CLIENT_ID>"
  }
}
```

Exemplo conceitual:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "de88cf66-42b8-4b48-a979-71e6ea29a3ed",
    "ClientId": "0ae744a0-1aea-44db-af8f-f61583721b2b"
  }
}
```

Não coloque secrets diretamente em arquivos versionados.

Para secrets, use ferramentas apropriadas, como:

- User Secrets em desenvolvimento;
- Azure Key Vault;
- variáveis de ambiente;
- Managed Identity quando aplicável.

---

# 33. Configuração mínima no `Program.cs`

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(
        builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

---

# 34. Protegendo o Controller

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TodoTasksController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok();
    }
}
```

---

# 35. Protegendo por Scope

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TodoTasksController : ControllerBase
{
    [RequiredScope("access_as_user")]
    [HttpGet]
    public IActionResult Get()
    {
        return Ok();
    }
}
```

---

# 36. Checklist para futuros projetos

Quando criar uma nova API protegida por Microsoft Entra ID:

```text
[ ] Criar App Registration da API

[ ] Anotar:
    - Tenant ID
    - Application (client) ID

[ ] Abrir "Expose an API"

[ ] Criar Application ID URI
    api://<CLIENT_ID>

[ ] Criar um ou mais scopes

[ ] Configurar clientes autorizados quando necessário

[ ] Configurar AzureAd no appsettings

[ ] Instalar Microsoft.Identity.Web

[ ] Configurar AddAuthentication()

[ ] Configurar AddMicrosoftIdentityWebApi()

[ ] Configurar AddAuthorization()

[ ] Adicionar UseAuthentication()

[ ] Adicionar UseAuthorization()

[ ] Proteger endpoints com [Authorize]

[ ] Quando necessário, validar scopes

[ ] Obter Access Token para a API

[ ] Verificar o claim aud

[ ] Verificar scopes/roles

[ ] Testar 401

[ ] Testar 403

[ ] Testar acesso autorizado
```

---

# 37. Troubleshooting

## Erro AADSTS500011

Exemplo:

```text
The resource principal named api://... was not found
```

Verifique:

```text
Application ID URI
```

e confirme que está usando:

```text
api://<API_CLIENT_ID>
```

Não passe um scope dentro de `--resource`.

---

## Erro AADSTS65001 / consent_required

Exemplo:

```text
The user or administrator has not consented
```

Verifique:

```text
Expose an API
    |
    v
Authorized client applications
```

Também pode ser necessário executar login interativo solicitando explicitamente o recurso:

```powershell
az login `
  --tenant <TENANT_ID> `
  --scope "api://<API_CLIENT_ID>/.default"
```

---

## API retorna 401

Verifique:

```text
Token existe?

Token expirou?

aud corresponde à API?

Tenant está correto?

Issuer está correto?

Authorization header está correto?
```

Header esperado:

```http
Authorization: Bearer <ACCESS_TOKEN>
```

---

## API retorna 403

Provavelmente a autenticação funcionou, mas faltou autorização.

Verifique:

```text
scp

roles

policies

RequiredScope
```

---

# 38. Modelo mental final

Guarde estas definições.

## App Registration

```text
Identidade de uma aplicação no Microsoft Entra ID.
```

## Expose an API

```text
Registra que essa aplicação representa uma API protegida
e define quais permissões clientes podem solicitar.
```

## Application ID URI

```text
Identificador lógico da API no sistema OAuth.
```

Exemplo:

```text
api://0ae744a0-...
```

## Resource

```text
A API que o cliente deseja acessar.
```

## Scope

```text
Uma permissão delegada disponibilizada pela API.
```

Exemplo:

```text
access_as_user
```

## Client

```text
Aplicação que solicita um token para acessar a API.
```

Exemplos:

```text
React
Azure CLI
Postman
Mobile App
```

## Access Token

```text
Credencial assinada pelo Microsoft Entra ID que permite
ao cliente acessar determinado recurso.
```

## `[Authorize]`

```text
Exige uma identidade autenticada válida.
```

## `[RequiredScope]`

```text
Exige uma permissão específica dentro do Access Token.
```

---

# 39. Resumo em uma frase

O fluxo inteiro pode ser resumido assim:

```text
O cliente autentica no Microsoft Entra ID,
solicita permissão para acessar uma API,
recebe um Access Token destinado a essa API
e envia esse token no header Authorization.
O ASP.NET Core valida o token antes de permitir
que a requisição chegue ao endpoint protegido.
```

---

# 40. Fluxo completo

```text
                    Usuário
                       |
                       v
                 Aplicação Cliente
              (Azure CLI / React)
                       |
                       | solicita:
                       |
                       | resource:
                       | api://<API_ID>
                       |
                       | scope:
                       | access_as_user
                       |
                       v
              Microsoft Entra ID
                       |
                       | valida:
                       |
                       +-- cliente
                       +-- usuário
                       +-- consentimento
                       +-- permissões
                       |
                       v
                 Access Token
                     JWT
                       |
                       v
                 Aplicação Cliente
                       |
                       | Authorization:
                       | Bearer <token>
                       |
                       v
                 TaskManager.API
                       |
                       v
              UseAuthentication()
                       |
                       v
             Microsoft.Identity.Web
                       |
                       +-- assinatura
                       +-- issuer
                       +-- audience
                       +-- expiração
                       |
                       v
               ClaimsPrincipal
                       |
                       v
              UseAuthorization()
                       |
                       v
                  [Authorize]
                       |
                       v
             [RequiredScope(...)]
                       |
                       v
                   Controller
```

---

## Observação de segurança

Os seguintes valores normalmente **não são secrets**:

```text
Tenant ID
Application (client) ID
Application ID URI
Scope name
```

Porém, nunca versione:

```text
Client Secret
Senha
Certificate private key
Access Token
Refresh Token
```

Use:

```text
.env local
User Secrets
Azure Key Vault
Variáveis de ambiente
Managed Identity
```

quando aplicável.

---

## Referência rápida de comandos

Login na API:

```powershell
az logout

az account clear

az login `
  --tenant <TENANT_ID> `
  --scope "api://<API_CLIENT_ID>/.default"
```

Obter Access Token:

```powershell
az account get-access-token `
  --scope "api://<API_CLIENT_ID>/.default" `
  --query accessToken `
  -o tsv
```

Depois:

```text
Swagger
    |
    v
Authorize
    |
    v
Cole o token eyJ...
```

---

# Conclusão

A separação mais importante é:

```text
App Registration
    =
identidade

Expose an API
    =
permissões que minha API disponibiliza

Application ID URI
    =
identidade OAuth da API

Scope
    =
o que um cliente pode solicitar

Access Token
    =
prova assinada das permissões concedidas

ASP.NET Core + Microsoft.Identity.Web
    =
validação do token

[Authorize] / [RequiredScope]
    =
autorização de acesso aos endpoints
```

Se esse modelo mental estiver claro, a mesma lógica pode ser reutilizada em praticamente qualquer API ASP.NET Core protegida por Microsoft Entra ID.
