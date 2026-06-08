# Desafio Técnico — GrupoDirecional

API REST em .NET 9 para gerenciamento de clientes, apartamentos, reservas e vendas de imóveis, com autenticação JWT.

---

## Stack de tecnologias

| Camada | Tecnologia |
|---|---|
| Runtime | .NET 9 |
| Web framework | ASP.NET Core (minimal hosting) |
| ORM | Entity Framework Core 9 |
| Banco — produção | SQL Server 2019 (Docker) |
| Banco — desenvolvimento | SQLite (criado automaticamente, sem configuração) |
| Autenticação | JWT Bearer |
| Hash de senha | BCrypt.Net-Next |
| Mapeamento | AutoMapper 16 |
| Documentação interativa | Scalar (OpenAPI 3) |
| Testes unitários | xUnit · Moq · EF InMemory — 90 testes, sem dependências externas |
| Testes de integração | xUnit · WebApplicationFactory · SQL Server real — 4 testes end-to-end |

---

## Como rodar com Docker

### Pré-requisitos
- Docker e Docker Compose instalados

### Subir o ambiente

```bash
# 1. Copie o arquivo de variáveis de ambiente
cp .env.example .env   # Linux/macOS
Copy-Item .env.example .env   # PowerShell

# 2. Suba todos os serviços (banco, migrations e API)
docker compose up --build
```

A stack inicializa na seguinte ordem:
1. **sqlserver** — SQL Server 2019
2. **migrations** — aplica as migrations automaticamente
3. **api** — inicia em `http://localhost:8080`

A API já sobe com um usuário `admin` criado (seed automático).

> Para parar: `docker compose down`  
> Para parar e remover o volume do banco: `docker compose down -v`

---

## Credenciais iniciais

| Campo    | Valor      |
|----------|------------|
| Username | `admin`    |
| Password | `admin123` |

---

## Variáveis de ambiente

Todos os segredos são lidos de variáveis de ambiente — nenhum valor sensível está no código versionado. O arquivo `.env.example` na raiz serve de template; copie-o para `.env` antes de rodar o Docker Compose.

| Variável | Descrição | Default / Observação |
|---|---|---|
| `SA_PASSWORD` | Senha SA do SQL Server. Deve atender aos requisitos de complexidade do SQL Server (maiúsculas, números, símbolos). | Obrigatória |
| `DB_PORT` | Porta do host mapeada para o container do banco. | `14333` |
| `TEST_DB_PORT` | Porta usada pelos testes de integração. | `14333` |
| `TEST_DB_SA_PASSWORD` | Senha SA usada nos testes de integração. | Herda `SA_PASSWORD` |
| `JWT__KEY` | Chave secreta para assinar tokens JWT. **Mínimo 32 caracteres** (requisito técnico do HS256 — chaves menores causam erro na inicialização). | `appsettings.json` tem um valor de 44 chars para `Development`; **obrigatória em produção**. |
| `JWT__ISSUER` | Issuer declarado no token. | `DesafioTecnicoApi` |
| `JWT__AUDIENCE` | Audience declarada no token. | `DesafioTecnicoApiUsers` |
| `JWT__EXPIRY_MINUTES` | Tempo de vida do token em minutos. | `60` |
| `ASPNETCORE_ENVIRONMENT` | Define qual `appsettings.{env}.json` é carregado. `Development` habilita o Scalar UI. | `Production` no Docker; `Development` localmente. |
| `ConnectionStrings__DefaultConnection` | Connection string completa do banco. Se ausente em `Development`, a API usa SQLite automaticamente. | Montada pelo docker-compose a partir de `SA_PASSWORD`. |

---

## Documentação interativa (Scalar UI)

| Ambiente | URL |
|---|---|
| Local (`dotnet run`) | `http://localhost:5160/scalar/v1` |
| Docker Compose | `http://localhost:8080/scalar/v1` |

> Disponível apenas quando `ASPNETCORE_ENVIRONMENT=Development`. Localmente está sempre ativo; no Docker Compose está desabilitado por padrão (ambiente `Production`). Para habilitar no Docker, adicione `ASPNETCORE_ENVIRONMENT: Development` ao serviço `api` no `docker-compose.yml`.

### Como autenticar

1. Expanda o endpoint **POST /api/auth/login**
2. Clique em **Send** com o body:
```json
{
  "username": "admin",
  "password": "admin123"
}
```
3. Copie o valor do campo `token` da resposta
4. Clique no botão **Authorize** (cadeado) no topo da página
5. Cole o token no campo e confirme — todos os endpoints passam a enviar `Authorization: Bearer {token}` automaticamente

### Fluxo sugerido para testes

A ordem abaixo respeita as dependências entre entidades:

| Passo | Endpoint | Descrição |
|-------|----------|-----------|
| 1 | `POST /api/clientes` | Cadastrar um cliente |
| 2 | `POST /api/apartamentos` | Cadastrar um apartamento (status inicial: `Disponivel`) |
| 3 | `POST /api/reservas` | Reservar o apartamento para o cliente |
| 4a | `POST /api/reservas/{id}/confirm` | Confirmar a reserva → gera venda automaticamente, marca apartamento como `Vendido`; retorna `201 Created` com header `Location` apontando para a venda criada |
| 4b | `POST /api/reservas/{id}/cancel` | Ou cancelar → devolve apartamento para `Disponivel` |
| 5 | `POST /api/vendas` | Alternativa: venda direta sem reserva prévia |

---

## Estrutura das tabelas

### `Clientes`
| Coluna          | Tipo           | Restrições               |
|-----------------|----------------|--------------------------|
| Id              | uniqueidentifier | PK                     |
| Nome            | nvarchar(200)  | NOT NULL                 |
| Email           | nvarchar(200)  | NOT NULL, UNIQUE         |
| Cpf             | nvarchar(14)   | NOT NULL, UNIQUE         |
| DataNascimento  | datetime2      | NOT NULL                 |

### `Apartamentos`
| Coluna  | Tipo             | Restrições               |
|---------|------------------|--------------------------|
| Id      | uniqueidentifier | PK                       |
| Codigo  | nvarchar(50)     | NOT NULL, UNIQUE         |
| Bloco   | nvarchar(50)     |                          |
| Andar   | int              | NOT NULL                 |
| Area    | decimal(10,2)    |                          |
| Valor   | decimal(18,2)    |                          |
| Status  | int              | 0=Disponivel, 1=Reservado, 2=Vendido |

### `Reservas`
| Coluna         | Tipo             | Restrições          |
|----------------|------------------|---------------------|
| Id             | uniqueidentifier | PK                  |
| ClienteId      | uniqueidentifier | FK → Clientes       |
| ApartamentoId  | uniqueidentifier | FK → Apartamentos   |
| DataReserva    | datetime2        | NOT NULL            |
| Status         | int              | 0=Pendente, 1=Confirmada, 2=Cancelada |

### `Vendas`
| Coluna         | Tipo             | Restrições          |
|----------------|------------------|---------------------|
| Id             | uniqueidentifier | PK                  |
| ClienteId      | uniqueidentifier | FK → Clientes       |
| ApartamentoId  | uniqueidentifier | FK → Apartamentos   |
| DataVenda      | datetime2        | NOT NULL            |
| ValorPago      | decimal(18,2)    |                     |

---

## Endpoints

| Método | Rota                          | Descrição                                    | Auth |
|--------|-------------------------------|----------------------------------------------|------|
| POST   | /api/auth/login               | Gerar token JWT                              | Não  |
| GET    | /api/clientes                 | Listar clientes (paginado)                   | Sim  |
| GET    | /api/clientes/{id}            | Obter cliente por ID                         | Sim  |
| POST   | /api/clientes                 | Cadastrar cliente                            | Sim  |
| PUT    | /api/clientes/{id}            | Atualizar cliente                            | Sim  |
| DELETE | /api/clientes/{id}            | Remover cliente                              | Sim  |
| GET    | /api/apartamentos             | Listar apartamentos (paginado; `?status=Disponivel\|Reservado\|Vendido`) | Sim  |
| GET    | /api/apartamentos/{id}        | Obter apartamento por ID                     | Sim  |
| POST   | /api/apartamentos             | Cadastrar apartamento                        | Sim  |
| PUT    | /api/apartamentos/{id}        | Atualizar apartamento                        | Sim  |
| DELETE | /api/apartamentos/{id}        | Remover apartamento                          | Sim  |
| GET    | /api/reservas                 | Listar reservas (paginado)                   | Sim  |
| GET    | /api/reservas/{id}            | Obter reserva por ID                         | Sim  |
| POST   | /api/reservas                 | Criar reserva (→ apartamento Reservado)      | Sim  |
| POST   | /api/reservas/{id}/confirm    | Confirmar reserva (→ gera venda + Vendido); retorna `201 Created` com `Location` para a venda criada | Sim  |
| POST   | /api/reservas/{id}/cancel     | Cancelar reserva (→ apartamento Disponivel)  | Sim  |
| DELETE | /api/reservas/{id}            | Remover reserva                              | Sim  |
| GET    | /api/vendas                   | Listar vendas (paginado)                     | Sim  |
| GET    | /api/vendas/{id}              | Obter venda por ID                           | Sim  |
| POST   | /api/vendas                   | Registrar venda direta (→ apartamento Vendido)| Sim  |
| PUT    | /api/vendas/{id}              | Atualizar valor pago da venda                | Sim  |
| DELETE | /api/vendas/{id}              | Remover venda                                | Sim  |

---

## Formato das respostas

### Resposta paginada

Todos os endpoints `GET` de listagem retornam o envelope abaixo. `totalPages` é calculado automaticamente.

```json
{
  "items": [ "..." ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 47,
  "totalPages": 3
}
```

Parâmetros de query disponíveis em todos os endpoints de listagem:

| Parâmetro | Tipo | Default | Limite |
|---|---|---|---|
| `page` | `int` | `1` | Mínimo 1 |
| `pageSize` | `int` | `20` | 1 – 100 |

O endpoint `GET /api/apartamentos` aceita adicionalmente:

| Parâmetro | Valores aceitos | Exemplo |
|---|---|---|
| `status` | `Disponivel` · `Reservado` · `Vendido` | `?status=Disponivel` |

### Resposta de erro de negócio (400)

Erros originados de regras de domínio (ex.: reservar apartamento indisponível) retornam:

```json
{ "error": "Apartamento não está disponível para reserva." }
```

Erros inesperados (500) seguem o mesmo formato via `ExceptionMiddleware`.

### Resposta de erro de validação (400)

Quando o modelo enviado não passa nas anotações de validação dos DTOs, o ASP.NET Core retorna o formato padrão `ModelState`:

```json
{
  "errors": {
    "Valor": ["The field Valor must be between 0.01 and 1.7976931348623157E+308."]
  }
}
```

---

## Máquina de estados

### Apartamento

```
Disponivel ──[reservar]──► Reservado ──[confirmar reserva]──► Vendido
Disponivel ──[venda direta]──────────────────────────────────► Vendido
Reservado  ──[cancelar reserva]──► Disponivel
```

### Reserva

```
Pendente ──[confirm]──► Confirmada
Pendente ──[cancel] ──► Cancelada
```

---

## Como rodar localmente sem Docker

Para desenvolvimento rápido sem precisar do SQL Server, a API usa **SQLite automaticamente** quando detecta o ambiente `Development` sem connection string configurada.

```bash
# Restaurar dependências e executar
dotnet run --project DesafioTecnico
```

A API sobe em `http://localhost:5160`.  
O banco SQLite é criado como arquivo `desafio_dev.db` na primeira execução com seed automático do usuário `admin`. Para resetar os dados entre execuções, delete o arquivo e reinicie.

> **Nota:** SQLite não suporta todas as funcionalidades do SQL Server. Para validar comportamentos de produção, use o docker-compose.

---

## Fluxo completo do cenário (exemplos curl)

> **Pré-requisito:** Os exemplos usam [`jq`](https://jqlang.org) para extrair campos do JSON. Instale com `brew install jq` (macOS/Linux) ou `winget install stedolan.jq` (Windows).

> **Porta:** Os exemplos usam a porta `8080` (Docker Compose). Se estiver rodando localmente com `dotnet run`, substitua `http://localhost:8080` por `http://localhost:5160`.

### 1. Autenticação — gerar token JWT

```bash
TOKEN=$(curl -s -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' \
  | jq -r '.token')

echo "Token: $TOKEN"
```

Resposta:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600
}
```

### 2. Cadastrar cliente

```bash
CLIENTE_ID=$(curl -s -X POST http://localhost:8080/api/clientes \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "João da Silva",
    "email": "joao@exemplo.com",
    "cpf": "123.456.789-00",
    "dataNascimento": "1985-06-15T00:00:00Z"
  }' | jq -r '.id')

echo "Cliente ID: $CLIENTE_ID"
```

### 3. Verificar disponibilidade do apartamento

```bash
# Listar apartamentos disponíveis (resultado paginado)
curl -s "http://localhost:8080/api/apartamentos?status=Disponivel" \
  -H "Authorization: Bearer $TOKEN" \
  | jq '.items'

# Nota: o apartamento de demonstração A-101 persiste entre execuções.
# Se já estiver Vendido/Reservado, crie um novo via POST /api/apartamentos antes de continuar.

# Obter o ID do primeiro apartamento disponível
APT_ID=$(curl -s "http://localhost:8080/api/apartamentos?status=Disponivel" \
  -H "Authorization: Bearer $TOKEN" \
  | jq -r '.items[0].id')

echo "Apartamento ID: $APT_ID"
```

### 4. Criar reserva (garante o apartamento)

```bash
RESERVA_ID=$(curl -s -X POST http://localhost:8080/api/reservas \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"clienteId\": \"$CLIENTE_ID\",
    \"apartamentoId\": \"$APT_ID\"
  }" | jq -r '.id')

echo "Reserva ID: $RESERVA_ID"
# Status do apartamento agora: Reservado
```

### 5. Confirmar reserva → venda definitiva

```bash
curl -si -X POST http://localhost:8080/api/reservas/$RESERVA_ID/confirm \
  -H "Authorization: Bearer $TOKEN"

# Resposta: 201 Created
# Location: http://localhost:8080/api/Vendas/{venda-id}
#
# O sistema automaticamente:
# - Cria uma Venda e retorna sua URL no header Location
# - Atualiza o status do apartamento para "Vendido"
# - Atualiza o status da reserva para "Confirmada"
```

### Cancelar reserva (alternativa ao passo 5)

```bash
curl -s -X POST http://localhost:8080/api/reservas/$RESERVA_ID/cancel \
  -H "Authorization: Bearer $TOKEN"

# O apartamento volta ao status "Disponivel"
```

---

## Como usar o token JWT

Após o login, inclua o token em todas as requisições autenticadas no header:

```
Authorization: Bearer <seu-token-aqui>
```

O token expira em 60 minutos (configurável via `Jwt__ExpiryMinutes`). Após expirar, realize novo login.

---

## Como executar os testes

### Testes unitários (sem dependências externas)

90 testes de controllers e serviços usando EF Core InMemory. Não precisam de banco, Docker ou qualquer configuração adicional.

```bash
dotnet test Tests/Tests.csproj --filter "Category!=Integration"
```

### Testes de integração (requerem SQL Server)

4 testes que sobem a aplicação completa via `WebApplicationFactory` contra um SQL Server real:

| Teste | O que valida |
|---|---|
| `ConfirmarReserva` | Fluxo completo: cadastrar cliente → reservar → confirmar → verificar venda no banco |
| `CancelarReserva` | Login via JWT real → reservar → cancelar → verificar status no banco |
| `VenderDiretamente` | Venda direta sem reserva → apartamento marcado como Vendido |
| `CriarReserva_ApartamentoVendido` | Tentativa de reservar apartamento já vendido retorna 400 |

Há três formas de fornecer o banco:

---

#### Opção A — Docker Compose já em execução

Se o ambiente Docker Compose já estiver no ar (`docker compose up`), basta exportar as variáveis de ambiente e rodar os testes:

```powershell
# PowerShell
$env:SA_PASSWORD         = 'Grup0D!reCional2026'
$env:TEST_DB_SA_PASSWORD = $env:SA_PASSWORD
$env:TEST_DB_PORT        = '14333'

dotnet test Tests/Tests.csproj --filter "Category=Integration"
```

---

#### Opção B — SQL Server isolado via scripts (sem subir a API completa)

Para rodar apenas os testes sem precisar do docker-compose completo, use os scripts na pasta `scripts/`:

```powershell
# 1. Subir o container do SQL Server e aguardar ele estar pronto
.\scripts\docker-prep.ps1

# 2. Exportar a senha e rodar os testes
$env:SA_PASSWORD         = 'Your_password123'
$env:TEST_DB_SA_PASSWORD = $env:SA_PASSWORD
dotnet test Tests/Tests.csproj --filter "Category=Integration"

# 3. (Opcional) Limpar o container ao finalizar
.\scripts\cleanup-containers.ps1
```

> `docker-prep.ps1` aceita parâmetros: `-SaPassword`, `-HostPort` e `-TimeoutSeconds`. Sem argumentos usa os mesmos defaults das variáveis de ambiente.

---

#### Opção C — Testcontainers (container gerenciado automaticamente)

Se o Docker daemon estiver exposto via TCP na porta 2375, o próprio runner de testes sobe e derruba o container automaticamente:

```powershell
.\scripts\run-tests.ps1 -UseTestcontainers
```

> Requer que o Docker esteja configurado para aceitar conexões TCP (`tcp://localhost:2375`). Use `-UseTcpDaemon` se necessário. Em Windows, `-KillTestHost` encerra processos `testhost` que eventualmente fiquem pendurados.

---

Se nenhuma variável de ambiente estiver configurada e o Testcontainers não estiver ativo, os testes de integração são **pulados automaticamente** com uma mensagem explicativa — a suite unitária nunca é bloqueada por ausência de banco.

---

## Scripts utilitários (`scripts/`)

| Script | Finalidade |
|---|---|
| `docker-prep.ps1` | Baixa a imagem do SQL Server 2019, sobe o container `desafio-test-sql` na porta 14333 e aguarda a porta TCP estar acessível antes de retornar. |
| `cleanup-containers.ps1` | Para e remove o container `desafio-test-sql`. Aceita `-RemoveImage` para remover também a imagem local. |
| `run-tests.ps1` | Wrapper para `dotnet test` que configura variáveis de ambiente do Testcontainers, encerra processos `testhost` pendurados (comum no Windows) e expõe a opção `-UseTestcontainers` para compilar com o símbolo `TESTCONTAINERS`. |

---

## Arquitetura e decisões técnicas

### Estrutura em camadas

```
DesafioTecnico/
├── Api/            Controllers, DTOs, AutoMapper profile, Middleware
├── Application/
│   └── Services/   Lógica de orquestração (casos de uso) + Interfaces
├── Domain/         Entidades (com comportamento) e Enums — sem dependências externas
└── Infrastructure/
    ├── Data/       AppDbContext, EF Configurations, SeedData, Migrations
    ├── Repositories/ Padrão Repository + Interfaces
    └── Security/   JwtTokenGenerator
```

### Decisões técnicas

- **Repository + Service pattern**: separa persistência de regras de negócio, facilita testes unitários com InMemory e mocks.
- **DTOs + AutoMapper**: entidades de domínio não são expostas diretamente; mapeamentos centralizados no `AutoMapperProfile`.
- **JWT + BCrypt**: autenticação stateless com tokens assinados; senhas armazenadas com hash BCrypt (work factor configurável).
- **Transação em `VendaService`**: a criação de venda e a atualização de status do apartamento ocorrem em uma única transação de banco de dados, garantindo consistência.
- **Exclusão de vendas**: o endpoint `DELETE /api/vendas/{id}` foi incluído por requisito do desafio. Em produção, vendas são registros contábeis — a prática correta é marcar como estornadas (soft delete ou campo de status), nunca remover o registro do banco.
- **Concorrência em reservas (não implementado no desafio)**: o fluxo de reserva lê o status do apartamento e, em seguida, atualiza — sem locking. Em produção, duas requisições simultâneas poderiam reservar o mesmo apartamento. A solução correta é concorrência otimista via `RowVersion`/`ETag` no `Apartamento`, rejeitando a segunda operação com 409 Conflict.
- **Preço da venda congelado na reserva (não implementado no desafio)**: ao confirmar uma reserva, o `ValorPago` da venda é calculado com o preço atual do apartamento. Se o valor mudar entre a criação da reserva e sua confirmação, o cliente é cobrado um valor diferente do acordado. O correto seria registrar o valor na `Reserva` e transferi-lo para a `Venda` na confirmação.
- **Exclusão de clientes com histórico (não implementado no desafio)**: atualmente é possível excluir um cliente que possui vendas ou reservas. Em produção, essa operação deveria ser bloqueada com 409 Conflict enquanto existirem registros vinculados.
- **Validação do CPF**: o sistema valida o formato `NNN.NNN.NNN-NN`, mas não os dígitos verificadores do algoritmo da Receita Federal. Em produção, utilizaria uma biblioteca de validação de CPF.
- **Migrations via job separado no Compose**: o serviço `migrations` aplica o `database update` antes da API subir, seguindo a prática de não aplicar migrations em runtime de produção.
- **Seed automático no startup**: usuário `admin` e dados de demonstração são inseridos na primeira inicialização, com guards idempotentes (`if (!context.X.Any())`).
- **Testes unitários com EF InMemory**: testes de serviço não precisam de banco real, rodando em memória para máxima velocidade.
- **Testes de integração com WebApplicationFactory**: a aplicação sobe em modo `SqlIntegrationTests` — ambiente que seleciona SQL Server como provedor e suprime o SeedData de startup, permitindo que cada teste controle seus próprios dados. O `DbContextOptions<AppDbContext>` é substituído no `ConfigureServices` com a connection string de teste (abordagem obrigatória porque `builder.Configuration.GetConnectionString` em `Program.cs` resolve o valor *antes* de `builder.Build()`, tornando `ConfigureAppConfiguration` insuficiente para sobrescrever o `appsettings.json`). O `TestAuthHandler` é promovido a scheme padrão via `PostConfigure<AuthenticationOptions>` para sobrescrever o `JwtBearer` registrado pelo `Program.cs`.
- **Testcontainers (opcional)**: testes de integração também podem subir um SQL Server real via Testcontainers ou reutilizar o container do docker-compose.

### Segurança

- Todos os endpoints (exceto `/api/auth/login`) requerem token JWT válido via `[Authorize]`.
- Segredos (SA_PASSWORD, JWT Key) são lidos de variáveis de ambiente; nunca hardcoded no código versionado.
- CPF validado por formato `NNN.NNN.NNN-NN` no DTO de entrada.
- Email e CPF com índice UNIQUE no banco, evitando duplicatas.

---

## Padrões de Projeto

Cinco padrões consolidados foram aplicados. Cada um foi escolhido para resolver um problema concreto, não como exercício acadêmico.

### Repository

**Onde:** `Infrastructure/Repositories/`

**Por que foi escolhido:** Os serviços precisam buscar e persistir entidades sem depender diretamente do EF Core. Com interfaces (`IClienteRepository`, `IApartamentoRepository`, etc.), o serviço não sabe se está falando com SQL Server, SQLite ou um repositório em memória.

**O que se ganha:**
- Testes de serviço trocam o banco real por EF InMemory sem nenhuma alteração no código de produção.
- Uma eventual troca de ORM não afeta a camada de serviço.

---

### Unit of Work

**Onde:** `Infrastructure/Data/IUnitOfWork.cs` / `UnitOfWork.cs`

**Por que foi escolhido:** Antes da implementação, cada repositório chamava `SaveChangesAsync` individualmente. Na confirmação de uma reserva, isso produzia três escritas separadas no banco — uma falha no meio deixava o sistema em estado inconsistente (reserva confirmada, venda não criada, status do apartamento desatualizado).

O `UnitOfWork` expõe todos os repositórios como propriedades e oferece um único `CommitAsync`. O EF Core envolve todas as entidades rastreadas em uma única transação implícita, eliminando a necessidade de `BeginTransactionAsync` explícito nos serviços.

**O que se ganha:**
- Atomicidade: ou tudo persiste, ou nada persiste.
- Serviços mais limpos — um único ponto de persistência no final do fluxo.
- Repositórios focados apenas em rastrear mudanças, sem responsabilidade de commit.

---

### Service Layer

**Onde:** `Application/Services/`

**Por que foi escolhido:** Controllers devem apenas traduzir HTTP → objeto → resposta HTTP. A orquestração — buscar entidades, aplicar regras de domínio, persistir via Unit of Work — fica exclusivamente nos serviços, evitando *fat controllers*.

**O que se ganha:**
- Regras de negócio testáveis com mocks simples, sem subir o pipeline HTTP.
- Controllers intercambiáveis: o mesmo serviço poderia ser chamado por um consumer de fila ou um endpoint gRPC sem alteração alguma.

---

### Result Pattern

**Onde:** `Domain/Results/Result.cs` → consumido em `Domain/Entities/`, `Application/Services/` e `Api/Controllers/`

**Por que foi escolhido:** As entidades de domínio modelam máquinas de estado (apartamento Disponível → Reservado → Vendido). Quando uma transição é inválida — por exemplo, tentar reservar um apartamento já vendido — o código original lançava `InvalidOperationException`. Exceções como fluxo de controle são custosas, obscurecem o fluxo normal e forçam os controllers a usar `try/catch` para tratar situações *esperadas*.

Com o Result Pattern, métodos como `Reservar()`, `Confirmar()` e `Cancelar()` retornam `Result` ou `Result<T>`. O controller verifica `result.IsSuccess` e responde diretamente, sem tratamento de exceção.

```csharp
// Antes
try { await _service.ConfirmAsync(id, ct); return NoContent(); }
catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }

// Depois
var result = await _service.ConfirmAsync(id, ct);
return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
```

**O que se ganha:**
- Fluxo de erro explícito na assinatura do método — quem chama sabe que pode falhar.
- Sem custo de stack unwinding para regras de negócio previsíveis.
- Mensagens de erro tipadas, não strings dentro de exceções.

---

### Factory

**Onde:** `Domain/Factories/VendaFactory.cs`

**Por que foi escolhido:** `Venda` é criada de duas formas distintas com lógica diferente:
- **Venda direta** (`POST /vendas`): valor pago vem do comprador.
- **Venda por confirmação de reserva** (`POST /reservas/{id}/confirm`): valor pago é o valor atual do apartamento no momento da confirmação.

Sem o Factory, o object initializer com `Id = Guid.NewGuid()` e `DataVenda = DateTime.UtcNow` estava duplicado em `VendaService` e `ReservaService`, e a distinção entre os dois casos era implícita no código.

```csharp
// VendaFactory expõe dois métodos com nomes semânticos
var venda = VendaFactory.CriarVendaDireta(clienteId, apartamentoId, valorPago);
var venda = VendaFactory.CriarPorReserva(reserva, apartamento);
```

**O que se ganha:**
- Os dois caminhos de criação ficam documentados por nome, não enterrados em object initializers.
- Invariantes de criação (`Id`, `DataVenda`) garantidos em um único lugar — impossível criar uma `Venda` sem eles.
