# CodeTicket

Sistema de venda de ingressos **CodeTicket**, construído com **.NET 8 Minimal API**, **Dapper**, **PostgreSQL** e **Blazor WebAssembly**.

O sistema é rigoroso na aplicação das regras de negócio — garantindo que nenhum ingresso seja vendido além da capacidade, que cambistas sejam bloqueados por CPF e que cupons de desconto não gerem valores negativos.

---

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
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
│   ├── arquitetura.md          # Decisões de arquitetura e diagrama de camadas
│   ├── analise_arquitetura.md  # Análise de padrões e violações arquiteturais
│   ├── registro_divida_tecnica.md
│   ├── fluxo_manutencao.md
│   ├── plano_iteracao.md
│   ├── operacao.md
│   ├── seguranca_ciclo.md
│   ├── topologia_times.md
│   └── adrs/
│       └── 001-escolha-do-micro-orm.md
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

## Executar o Backend

```bash
cd src/backend
dotnet run
```

| Ambiente | URL |
|----------|-----|
| API | `http://localhost:5007` |
| Swagger | `http://localhost:5007` (abre automaticamente) |

---

## Executar o Frontend

```bash
cd src/frontend
dotnet run
```

| Ambiente | URL |
|----------|-----|
| Frontend | `http://localhost:5000` |

> O frontend aponta automaticamente para `http://localhost:5007`. Certifique-se de que o backend está rodando antes de abrir o frontend.

---

## Executar os Testes

```bash
cd tests/TicketPrime.Tests
dotnet test
```

Resultado esperado:

```
Aprovado! – Com falha: 0, Aprovado: 22, Ignorado: 0, Total: 22
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

| Status | Descrição |
|--------|-----------|
| 200 | Usuário cadastrado com sucesso |
| 400 | CPF já cadastrado, CPF inválido ou e-mail inválido |

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

| Status | Descrição |
|--------|-----------|
| 200 | Evento criado com sucesso |
| 400 | Nome vazio, capacidade zero, data no passado ou preço inválido |

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
Cadastra um novo cupom de desconto.

**Body:**
```json
{
  "codigo": "VERAO25",
  "porcentagemDesconto": 25.00,
  "valorMinimoRegra": 150.00
}
```

| Status | Descrição |
|--------|-----------|
| 200 | Cupom criado com sucesso |
| 400 | Código já existe, desconto fora do intervalo (0–100) ou valor mínimo negativo |

---

### POST /api/ingressos/comprar
Compra um ingresso para um evento com múltiplas validações de negócio.

**Body:**
```json
{
  "usuarioCpf": "12345678901",
  "eventoId": 1,
  "cupomCodigo": "VERAO25"
}
```

**Validações aplicadas:**
- Verifica se usuário existe
- Verifica se evento existe
- Bloqueia compra para eventos no passado
- Impede venda além da capacidade (overselling)
- Bloqueia cambistas (1 ingresso por CPF por evento)
- Valida existência do cupom
- Verifica valor mínimo para usar o cupom
- Garante que o valor final não seja negativo

| Status | Descrição |
|--------|-----------|
| 200 | Ingresso comprado com sucesso + valor pago |
| 400 | Erro em qualquer validação (mensagem específica) |

---

### GET /api/ingressos/meus/{cpf}
Lista todos os ingressos comprados por um usuário (com JOIN entre tabelas).

**Parâmetros:**
- `cpf`: CPF do usuário (11 dígitos)

**Resposta:**
```json
{
  "mensagem": "2 ingresso(s) encontrado(s).",
  "ingressos": [
    {
      "reservaId": 1,
      "usuarioNome": "Lucas Frotte",
      "usuarioCpf": "12345678901",
      "eventoNome": "Festival de Verão 2025",
      "dataEvento": "2025-12-20T20:00:00",
      "localEvento": "Praia de Copacabana",
      "precoPadrao": 250.00,
      "cupomUtilizado": "VERAO25",
      "descontoAplicado": 62.50,
      "valorFinalPago": 187.50
    }
  ]
}
```

| Status | Descrição |
|--------|-----------|
| 200 | Lista de ingressos do usuário |
| 400 | Usuário não encontrado ou CPF inválido |

---

## Tecnologias Utilizadas

| Tecnologia | Versão | Função |
|------------|--------|--------|
| .NET | 8.0 | Plataforma da API e Frontend |
| ASP.NET Core Minimal API | 8.0 | Framework HTTP |
| Blazor WebAssembly | 8.0 | Frontend |
| MudBlazor | 7.x | Componentes de UI |
| Dapper | 2.1.72 | Micro-ORM para acesso ao banco |
| Npgsql | 8.0.5 | Driver PostgreSQL para .NET |
| Swashbuckle (Swagger) | 6.9.0 | Documentação interativa da API |
| xUnit | 2.5.3 | Framework de testes |
| NSubstitute | 5.1.0 | Mocking para testes unitários |
| PostgreSQL | 14+ | Banco de dados relacional |

---

## Colaboradores

| Aluno | Matrícula |
|-------|-----------|
| Lucas Frotte Lafin | 06010493 |
| Ana Luiza Maciel Mattos | 06009322 |
| Pedro Nogueira Teodosio | 06010196 |
| Alexandre dos Santos | 06010479 |
| Gabriel Duarte de Oliveira | 06010804 |
