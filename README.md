# Desafio Técnico — GrupoDirecional

API REST em .NET 9 para gerenciamento de clientes, apartamentos, reservas e vendas de imóveis, com autenticação JWT.

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

## Documentação interativa (Scalar UI)

Acesse `http://localhost:8080/scalar/v1` para testar todos os endpoints via interface gráfica (disponível no ambiente Development).

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

| Método | Rota                          | Descrição                         | Auth |
|--------|-------------------------------|-----------------------------------|------|
| POST   | /api/auth/login               | Gerar token JWT                   | Não  |
| GET    | /api/clientes                 | Listar clientes                   | Sim  |
| POST   | /api/clientes                 | Cadastrar cliente                 | Sim  |
| PUT    | /api/clientes/{id}            | Atualizar cliente                 | Sim  |
| DELETE | /api/clientes/{id}            | Remover cliente                   | Sim  |
| GET    | /api/apartamentos             | Listar apartamentos               | Sim  |
| POST   | /api/apartamentos             | Cadastrar apartamento             | Sim  |
| PUT    | /api/apartamentos/{id}        | Atualizar apartamento             | Sim  |
| DELETE | /api/apartamentos/{id}        | Remover apartamento               | Sim  |
| GET    | /api/reservas                 | Listar reservas                   | Sim  |
| POST   | /api/reservas                 | Criar reserva                     | Sim  |
| POST   | /api/reservas/{id}/confirm    | Confirmar reserva (gera venda)    | Sim  |
| POST   | /api/reservas/{id}/cancel     | Cancelar reserva                  | Sim  |
| DELETE | /api/reservas/{id}            | Remover reserva                   | Sim  |
| GET    | /api/vendas                   | Listar vendas                     | Sim  |
| POST   | /api/vendas                   | Registrar venda direta            | Sim  |
| PUT    | /api/vendas/{id}              | Atualizar venda                   | Sim  |
| DELETE | /api/vendas/{id}              | Remover venda *(comentado — ver nota no controller)* | Sim  |

---

## Fluxo completo do cenário (exemplos curl)

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
curl -s -X POST http://localhost:8080/api/reservas/$RESERVA_ID/confirm \
  -H "Authorization: Bearer $TOKEN"

# O sistema automaticamente:
# - Cria uma Venda associada ao cliente e apartamento
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

```bash
dotnet test Tests/Tests.csproj --filter "Category!=Integration"
```

### Testes de integração (requer docker-compose em execução)

```bash
# PowerShell
$env:SA_PASSWORD = 'Grup0D!reCional2026'
$env:TEST_DB_SA_PASSWORD = $env:SA_PASSWORD
$env:TEST_DB_PORT = '14333'

dotnet test Tests/Tests.csproj --filter "Category=Integration"
```

---

## Arquitetura e decisões técnicas

### Estrutura em camadas

```
DesafioTecnico/
├── Api/            Controllers, DTOs, AutoMapper profile, Middleware
├── Domain/         Entidades (com comportamento) e Enums — sem dependências externas
└── Infrastructure/
    ├── Data/       AppDbContext, EF Configurations, SeedData, Migrations
    ├── Repositories/ Padrão Repository + Interfaces
    ├── Security/   JwtTokenGenerator
    └── Services/   Lógica de orquestração + Interfaces
```

### Decisões técnicas

- **Repository + Service pattern**: separa persistência de regras de negócio, facilita testes unitários com InMemory e mocks.
- **DTOs + AutoMapper**: entidades de domínio não são expostas diretamente; mapeamentos centralizados no `AutoMapperProfile`.
- **JWT + BCrypt**: autenticação stateless com tokens assinados; senhas armazenadas com hash BCrypt (work factor configurável).
- **Transação em `VendaService`**: a criação de venda e a atualização de status do apartamento ocorrem em uma única transação de banco de dados, garantindo consistência.
- **Migrations via job separado no Compose**: o serviço `migrations` aplica o `database update` antes da API subir, seguindo a prática de não aplicar migrations em runtime de produção.
- **Seed automático no startup**: usuário `admin` e dados de demonstração são inseridos na primeira inicialização, com guards idempotentes (`if (!context.X.Any())`).
- **Testes com EF InMemory**: testes unitários de serviço não precisam de banco real, rodando em memória para máxima velocidade.
- **Testcontainers (opcional)**: testes de integração podem subir um SQL Server real via Testcontainers ou reutilizar o container do docker-compose.

### Segurança

- Todos os endpoints (exceto `/api/auth/login`) requerem token JWT válido via `[Authorize]`.
- Segredos (SA_PASSWORD, JWT Key) são lidos de variáveis de ambiente; nunca hardcoded no código versionado.
- CPF validado por formato `NNN.NNN.NNN-NN` no DTO de entrada.
- Email e CPF com índice UNIQUE no banco, evitando duplicatas.
