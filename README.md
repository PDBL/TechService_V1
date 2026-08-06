TECHSERVICE API — VERSÃO 1
GUIA RÁPIDO PARA EXTRAIR, INSTALAR, EXECUTAR E VALIDAR

OBJETIVO
Executar a API ASP.NET Core ligada ao MySQL remoto com:
- GET /
- GET /api/clientes
- Listagem apenas de clientes com status = 1
- Resposta em JSON
- Swagger em /swagger

============================================================
1. CRIAR A PASTA DE TRABALHO
============================================================

No Windows, crie:

C:\Projetos\TechService_V1

Pelo PowerShell:

mkdir C:\Projetos\TechService_V1

============================================================
2. COLOCAR O ZIP NA PASTA
============================================================

Copie:

TechService_V1.zip

para:

C:\Projetos\TechService_V1

============================================================
3. EXTRAIR O ZIP
============================================================

1. Clique com o botão direito em TechService_V1.zip.
2. Escolha "Extrair Tudo...".
3. Confirme a pasta C:\Projetos\TechService_V1.
4. Clique em "Extrair".

Depois deve existir:

C:\Projetos\TechService_V1\TechService.Api

============================================================
4. ABRIR NO VISUAL STUDIO CODE
============================================================

1. Abra o Visual Studio Code.
2. Selecione File > Open Folder.
3. Abra:

C:\Projetos\TechService_V1\TechService.Api

Confirme que aparecem:

Data\MySqlConnectionFactory.cs
Models\Cliente.cs
Program.cs
TechService.Api.csproj
appsettings.json
Properties

============================================================
5. ABRIR O TERMINAL DO VS CODE
============================================================

No VS Code:

Terminal > New Terminal

Atalho:

Ctrl + Shift + `

O terminal deve estar em:

C:\Projetos\TechService_V1\TechService.Api

============================================================
6. VERIFICAR O .NET
============================================================

Execute:

dotnet --version

Deve aparecer uma versão 9.x ou compatível com net9.0.

============================================================
7. RESTAURAR AS DEPENDÊNCIAS
============================================================

Execute:

dotnet restore

Este comando instala os pacotes:

- MySqlConnector
- Swashbuckle.AspNetCore

Resultado esperado:

Restore succeeded.

============================================================
8. COMPILAR
============================================================

Execute:

dotnet build

Resultado esperado:

Build succeeded.
0 Error(s)

============================================================
9. EXECUTAR A API
============================================================

Execute:

dotnet run

Resultado esperado:

Now listening on: http://localhost:5000
Application started.

Não feche o terminal enquanto estiver a testar.

============================================================
10. VALIDAR O ENDPOINT INICIAL
============================================================

Abra:

http://localhost:5000

Deve aparecer um JSON indicando:

- versão V1
- ligação ao MySQL
- endpoint GET /api/clientes

============================================================
11. VALIDAR A LISTAGEM DE CLIENTES
============================================================

Abra:

http://localhost:5000/api/clientes

Resultado esperado:

- Código HTTP 200
- Array JSON
- Apenas clientes com status = 1
- Campos reais da tabela clientes

Exemplo:

[
  {
    "idCliente": 1,
    "nome": "Cliente Exemplo",
    "email": "cliente@email.com",
    "telefone": "912345678",
    "status": 1,
    "createdAt": "2026-01-01T10:00:00",
    "updatedAt": null,
    "deletedAt": null
  }
]

============================================================
12. VALIDAR NO SWAGGER
============================================================

Abra:

http://localhost:5000/swagger

No Swagger:

1. Abra GET /api/clientes.
2. Clique em "Try it out".
3. Clique em "Execute".
4. Confirme o código 200.
5. Confirme a lista JSON.

Também pode testar GET /.

============================================================
13. PARAR A API
============================================================

No terminal, pressione:

Ctrl + C

============================================================
ERROS MAIS COMUNS
============================================================

ERRO: dotnet não é reconhecido
SOLUÇÃO: instalar/reparar o .NET SDK e reabrir o VS Code.

ERRO: Access denied for user
SOLUÇÃO: confirmar utilizador e palavra-passe em appsettings.json.

ERRO: Unable to connect / timeout
SOLUÇÃO: confirmar Internet, host aulaslab.com e porta 3306.

ERRO: Table 'techservice.clientes' doesn't exist
SOLUÇÃO: confirmar que Database=techservice e que a tabela clientes existe.

ERRO: endereço já está em uso
SOLUÇÃO: parar outra API com Ctrl+C ou executar:

dotnet run --urls=http://localhost:5001

Depois usar:

http://localhost:5001/swagger

============================================================
CHECKLIST FINAL
============================================================

[ ] O ZIP foi extraído.
[ ] A pasta TechService.Api foi aberta no VS Code.
[ ] dotnet --version funcionou.
[ ] dotnet restore terminou sem erros.
[ ] dotnet build terminou sem erros.
[ ] dotnet run iniciou a API.
[ ] GET / respondeu.
[ ] GET /api/clientes devolveu 200 OK.
[ ] A resposta contém apenas clientes ativos.
[ ] O Swagger abriu e executou GET /api/clientes.

RESULTADO
A Versão 1 está concluída quando GET /api/clientes consulta a base techservice no servidor aulaslab.com e devolve os clientes ativos em JSON.
