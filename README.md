# CodeTicket

Backend do sistema de venda de ingressos **CodeTicket**, construído com **.NET 10 Minimal API**, **Dapper** e **PostgreSQL**.

O sistema foi projetado para ser rápido, seguro contra SQL Injection e rigoroso na aplicação das regras de negócio — garantindo que nenhum ingresso seja vendido além da capacidade, que cambistas sejam bloqueados por CPF e que cupons de desconto não gerem valores negativos.

---

## Hospedagem

| Serviço | Plataforma | URL |
|---------|------------|-----|
| Backend (API) | Railway | https://codeticket-production.up.railway.app/ |
| Frontend (Blazor) | Netlify | https://appcodeticket.netlify.app/ |
| Banco de Dados | Railway PostgreSQL | gerenciado pelo Railway |

---

## Pré-requisitos

Antes de rodar o projeto, certifique-se de ter instalado:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- [Git](https://git-scm.com/)

---

## Estrutura do Repositório

```
/
├── README.md
├── CodeTicket.slnx
├── db/
│   └── script.sql              # Script de criação das tabelas
├── docs/
│   ├── requisitos.md           # Histórias de usuário e critérios BDD
│   └── arquitetura.md          # Decisões de arquitetura e diagrama de camadas
├── src/
│   ├── backend/                # Projeto da Minimal API
│   │   ├── Data/
│   │   ├── DTOs/
│   │   ├── Models/
│   │   ├── Repositories/
│   │   ├── Services/
│   │   └── Program.cs
│   └── frontend/               # Projeto Blazor WebAssembly
│       ├── Layout/
│       ├── Models/
│       ├── Pages/
│       ├── Services/
│       └── Program.cs
└── tests/
    └── TicketPrime.Tests/      # Projeto de testes xUnit
```

---

## Configuração do Banco de Dados

### 1. Criar o banco

```bash
psql -U postgres -c "CREATE DATABASE codeticket;"
```

### 2. Executar o script de criação das tabelas

```bash
psql -U postgres -d codeticket -f db/script.sql
```

### 3. Configurar a connection string

Edite o arquivo `src/backend/appsettings.Development.json` substituindo `SUA_SENHA_AQUI` pela sua senha do PostgreSQL:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=codeticket;Username=postgres;Password=SUA_SENHA_AQUI"
  }
}
```

---

## Executar a API

```bash
cd src/backend
dotnet run
```

A API estará disponível em:

| Ambiente | URL |
|----------|-----|
| HTTP    | `http://localhost:5007` |
| Swagger | `http://localhost:5007` (abre automaticamente) |

---

## Executar os Testes

```bash
cd tests/TicketPrime.Tests
dotnet test
```

Resultado esperado:

```
Aprovado! – Com falha: 0, Aprovado: 12, Ignorado: 0, Total: 12
```

---

## Endpoints da API

### POST /api/usuarios
Cadastra um novo usuário. Retorna `400` se o CPF já existir ou for inválido.

**Body:**
```json
{
  "cpf": "12345678901",
  "nome": "Lucas Frotte",
  "email": "lucas@email.com"
}
```

**Respostas:**
| Status | Descrição |
|--------|-----------|
| 200    | Usuário cadastrado com sucesso |
| 400    | CPF já cadastrado, CPF inválido ou e-mail inválido |

---

### POST /api/eventos
Cadastra um novo evento. Retorna `400` se os dados forem inválidos.

**Body:**
```json
{
  "nome": "Festival de Verão 2025",
  "capacidadeTotal": 5000,
  "dataEvento": "2025-12-20T20:00:00",
  "precoPadrao": 250.00
}
```

**Respostas:**
| Status | Descrição |
|--------|-----------|
| 200    | Evento criado com sucesso |
| 400    | Nome vazio, capacidade zero, data no passado ou preço inválido |

---

### GET /api/eventos
Lista todos os eventos cadastrados.

**Resposta:**
```json
[
  { "id": 1, "nome": "Festival de Verão 2025" },
  { "id": 2, "nome": "Show de Rock Nacional" }
]
```

---

### POST /api/cupons
Cadastra um novo cupom de desconto. Retorna `400` se o código já existir ou os valores forem inválidos.

**Body:**
```json
{
  "codigo": "VERAO25",
  "porcentagemDesconto": 25.00,
  "valorMinimoRegra": 150.00
}
```

**Respostas:**
| Status | Descrição |
|--------|-----------|
| 200    | Cupom criado com sucesso |
| 400    | Código já existe, desconto fora do intervalo (0–100) ou valor mínimo negativo |

---

## Tecnologias Utilizadas

| Tecnologia | Versão | Função |
|------------|--------|--------|
| .NET | 10.0 | Plataforma da API |
| ASP.NET Core Minimal API | 10.0 | Framework HTTP |
| Dapper | 2.1.72 | Micro-ORM para acesso ao banco |
| Npgsql | 8.0.5 | Driver PostgreSQL para .NET |
| Swashbuckle (Swagger) | 6.9.0 | Documentação interativa da API |
| xUnit | 2.5.3 | Framework de testes |
| NSubstitute | 5.1.0 | Mocking para testes unitários |
| PostgreSQL | 14+ | Banco de dados relacional |
