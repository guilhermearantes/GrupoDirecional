Testcontainers integration (opcional)

Este repositório suporta executar os testes de integração usando Testcontainers (.NET), que gerencia containers de forma programática.

Como usar (opcional):

1. Adicione o pacote DotNet.Testcontainers no projeto Tests (dotnet add Tests\Tests.csproj package DotNet.Testcontainers)
2. Existem duas formas suportadas para ativar Testcontainers nos testes:
   - Via MSBuild property (recomendada): dotnet test Tests\Tests.csproj -p:UseTestcontainers=true
   - Via símbolo de compilação: dotnet test Tests\Tests.csproj -p:DefineConstants=TESTCONTAINERS

Se preferir não usar Testcontainers, execute o docker-compose localizado no repositório (ou suba manualmente um SQL Server):

1. docker-compose up -d (levanta o SQL Server em localhost:14333)
2. dotnet test Tests\Tests.csproj

O teste de integração assume as credenciais SA_PASSWORD=Your_password123 e que a porta 14333 está mapeada para 1433 do container.

Observação: em alguns ambientes o Testcontainers pode falhar ao anexar streams do Docker (erro relacionado ao 'hijack chunked stream').
Se ocorrer, defina a variável de ambiente TESTCONTAINERS_RYUK_DISABLED=true antes de executar os testes e certifique-se de limpar containers manualmente quando necessário.

Comandos úteis (PowerShell)

1) Recomendada — expor o daemon TCP do Docker e executar com Testcontainers (Ryuk habilitado):

   # no Docker Desktop: Settings → General → Enable "Expose daemon on tcp://localhost:2375" e reinicie o Docker
	  $env:DOCKER_HOST = 'tcp://localhost:2375'
   dotnet test Tests\\Tests.csproj -p:UseTestcontainers=true

2) Alternativa — desativar Ryuk (rápido, limpar containers manualmente depois):

	  $env:TESTCONTAINERS_RYUK_DISABLED = 'true'
   dotnet test Tests\\Tests.csproj -p:UseTestcontainers=true

3) Limpar container criado manualmente (quando Ryuk estiver desabilitado):

   docker ps -a --filter "name=desafio-test-sql"
   docker rm -f <containerId>

Script útil

Um script PowerShell auxiliar foi incluído em scripts\\run-tests.ps1 para facilitar execução com as opções acima.
Exemplo:

   # usar daemon TCP (recomendado)
   .\\scripts\\run-tests.ps1 -UseTcpDaemon

   # ou desabilitar Ryuk (alternativa)
   .\\scripts\\run-tests.ps1 -DisableRyuk

Leia o topo do script para detalhes das opções.

Instruções rápidas

- Rodar apenas testes de integração (filtrar por Category):
  dotnet test Tests\\Tests.csproj --filter "Category=Integration" -p:DefineConstants=TESTCONTAINERS

- Pré-puxar a imagem do SQL Server para acelerar execuções locais:
  docker pull mcr.microsoft.com/mssql/server:2019-latest

- Subir o banco manualmente antes de rodar os testes (evita criação pelo Testcontainers):
  docker run -d -p 14333:1433 -e ACCEPT_EULA=Y -e SA_PASSWORD=Your_password123 --name desafio-test-sql mcr.microsoft.com/mssql/server:2019-latest

Esses passos reduzem bastante o tempo do fluxo de desenvolvimento local ao executar o teste E2E.

Script de preparação automática

Um script PowerShell auxiliar scripts\docker-prep.ps1 foi adicionado para automatizar o pull e o run do container:

  .\scripts\docker-prep.ps1 -SaPassword 'Your_password123' -HostPort 14333

Isso puxa a imagem, remove container existente com o mesmo nome e inicia o container, aguardando que a porta TCP esteja aberta.

Cleanup

Um script de limpeza foi adicionado em scripts\cleanup-containers.ps1. Use para remover o container e opcionalmente a imagem:

  .\scripts\cleanup-containers.ps1 -ContainerName 'desafio-test-sql' -RemoveImage

Ou apenas remover container:

  .\scripts\cleanup-containers.ps1 -ContainerName 'desafio-test-sql'
