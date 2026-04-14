# Arquitetura do Sistema — CodeTicket

Este documento descreve as decisões de arquitetura, a organização em camadas, o modelo de dados e o fluxo de uma requisição dentro do backend CodeTicket.

---

## Visão Geral

O CodeTicket é um backend construído como uma **Minimal API em .NET 8**, seguindo uma arquitetura em camadas com separação clara de responsabilidades. O acesso ao banco de dados é feito exclusivamente via **Dapper** com parâmetros nomeados, eliminando qualquer risco de SQL Injection.

```
Cliente HTTP (Swagger / Frontend / Postman)
            │
            ▼
┌─────────────────────────────┐
│         Program.cs          │  ← Roteamento (MapPost / MapGet)
│     (Camada de Entrada)     │
└────────────┬────────────────┘
             │ injeta e chama
             ▼
┌─────────────────────────────┐
│         Services/           │  ← Regras de negócio e validações
│  UsuarioService             │
│  EventoService              │
│  CupomService               │
└────────────┬────────────────┘
             │ injeta e chama
             ▼
┌─────────────────────────────┐
│        Repositories/        │  ← Acesso ao banco via Dapper
│  UsuarioRepository          │
│  EventoRepository           │
│  CupomRepository            │
└────────────┬────────────────┘
             │ usa
             ▼
┌─────────────────────────────┐
│     DbConnectionFactory     │  ← Cria conexão NpgsqlConnection
└────────────┬────────────────┘
             │
             ▼
┌─────────────────────────────┐
│         PostgreSQL          │  ← Banco de dados relacional
└─────────────────────────────┘
```

---

## Estrutura de Pastas

```
CodeTicket/src/backend/
├── Data/
│   └── DbConnectionFactory.cs      # Fábrica de conexões com o banco
├── DTOs/
│   ├── CriarUsuarioDto.cs          # Dados de entrada para criação de usuário
│   ├── CriarEventoDto.cs           # Dados de entrada para criação de evento
│   ├── CriarCupomDto.cs            # Dados de entrada para criação de cupom
│   └── EventoListarDto.cs          # Dados de saída para listagem de eventos
├── Models/
│   ├── Usuario.cs                  # Entidade que representa a tabela Usuarios
│   ├── Evento.cs                   # Entidade que representa a tabela Eventos
│   └── Cupom.cs                    # Entidade que representa a tabela Cupons
├── Repositories/
│   ├── IUsuarioRepository.cs       # Contrato da interface de usuário
│   ├── IEventoRepository.cs        # Contrato da interface de evento
│   ├── ICupomRepository.cs         # Contrato da interface de cupom
│   ├── UsuarioRepository.cs        # Implementação com Dapper
│   ├── EventoRepository.cs         # Implementação com Dapper
│   └── CupomRepository.cs          # Implementação com Dapper
├── Services/
│   ├── UsuarioService.cs           # Validações e regras de negócio de usuário
│   ├── EventoService.cs            # Validações e regras de negócio de evento
│   └── CupomService.cs             # Validações e regras de negócio de cupom
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── appsettings.Development.json    # Connection string (não versionar em produção)
├── Program.cs                      # Ponto de entrada, DI e roteamento
└── CodeTicket.API.csproj
```

---

## Camadas e Responsabilidades

### Program.cs — Entrada e Roteamento
Ponto de entrada da aplicação. Responsável por:
- Registrar todos os serviços no contêiner de injeção de dependência
- Mapear as rotas HTTP com `app.MapPost` e `app.MapGet`
- Configurar o Swagger e o CORS
- Não contém nenhuma lógica de negócio

### Services — Regras de Negócio
Camada onde vivem todas as validações e decisões do sistema. Cada service:
- Recebe um DTO como entrada
- Valida os dados (campos obrigatórios, formatos, regras de negócio)
- Consulta o repositório para verificar duplicatas ou existência de registros
- Retorna uma tupla `(bool sucesso, string mensagem)` para o endpoint

Exemplos de regras aplicadas:
- CPF deve ter exatamente 11 dígitos numéricos
- E-mail deve ter formato válido (validado por Regex)
- Data do evento não pode ser no passado
- Capacidade e preço devem ser maiores que zero
- Percentual de desconto deve estar entre 1 e 100
- Valor mínimo do cupom não pode ser negativo

### Repositories — Acesso a Dados
Camada responsável exclusivamente por executar queries SQL via Dapper. Cada repositório:
- Recebe uma conexão criada pela `DbConnectionFactory`
- Executa queries parametrizadas com `@NomeDoParametro`
- Nunca contém lógica de negócio
- É injetado via interface (`IUsuarioRepository`, etc.) para permitir mock nos testes

### DTOs — Transferência de Dados
Objetos simples usados para receber dados da requisição HTTP (entrada) ou retornar dados ao cliente (saída). Separam o contrato da API dos modelos internos do banco.

### Models — Entidades do Banco
Representam as tabelas do banco de dados. São usados pelos repositórios para mapear os resultados das queries via Dapper.

---

## Modelo de Dados

```
┌──────────────────────────────────────────────────────────────────┐
│                           Usuarios                               │
├──────────────┬──────────────────────────────────────────────────┤
│ Cpf          │ VARCHAR(14)  PRIMARY KEY                          │
│ Nome         │ VARCHAR(100) NOT NULL                             │
│ Email        │ VARCHAR(100) NOT NULL                             │
└──────────────┴──────────────────────────────────────────────────┘
        │
        │ 1 : N
        ▼
┌──────────────────────────────────────────────────────────────────┐
│                            Reservas                              │
├──────────────┬──────────────────────────────────────────────────┤
│ Id           │ SERIAL       PRIMARY KEY                          │
│ UsuarioCpf   │ VARCHAR(14)  FK → Usuarios.Cpf                    │
│ EventoId     │ INTEGER      FK → Eventos.Id                      │
│ CupomUtilizado│ VARCHAR(50) FK → Cupons.Codigo  (NULLABLE)       │
│ ValorFinalPago│ DECIMAL(10,2) NOT NULL                           │
└──────────────┴──────────────────────────────────────────────────┘
        ▲                        ▲
        │ N : 1                  │ N : 1
        │                        │
┌───────────────────┐   ┌────────────────────────────────────────┐
│      Eventos      │   │               Cupons                   │
├───────────────────┤   ├────────────────────────────────────────┤
│ Id  SERIAL PK     │   │ Codigo           VARCHAR(50)  PK        │
│ Nome              │   │ PorcentagemDesconto DECIMAL(5,2)        │
│ CapacidadeTotal   │   │ ValorMinimoRegra    DECIMAL(10,2)       │
│ DataEvento        │   └────────────────────────────────────────┘
│ PrecoPadrao       │
└───────────────────┘
```

---

## Fluxo de uma Requisição

Exemplo: `POST /api/usuarios`

```
1. Cliente envia JSON com { cpf, nome, email }
        │
        ▼
2. Program.cs recebe a requisição e chama UsuarioService.CriarUsuario(dto)
        │
        ▼
3. UsuarioService valida:
   ├── CPF não vazio
   ├── CPF com 11 dígitos numéricos (Regex)
   ├── Nome não vazio
   ├── Email não vazio
   ├── Email com formato válido (Regex)
   └── Consulta UsuarioRepository.BuscarPorCpf(cpf)
            │
            ▼
       DbConnectionFactory cria NpgsqlConnection
            │
            ▼
       Dapper executa: SELECT * FROM Usuarios WHERE Cpf = @Cpf
            │
            ▼
       Retorna Usuario? (null se não existir)
        │
        ▼
4. Se CPF já existe → retorna (false, "CPF já cadastrado.")
   Se válido        → chama UsuarioRepository.Criar(usuario)
                              │
                              ▼
                         Dapper executa:
                         INSERT INTO Usuarios (Cpf, Nome, Email)
                         VALUES (@Cpf, @Nome, @Email)
        │
        ▼
5. UsuarioService retorna (true, "Usuário cadastrado com sucesso!")
        │
        ▼
6. Program.cs retorna Results.Ok(mensagem) → HTTP 200
   ou Results.BadRequest(mensagem)         → HTTP 400
```

---

## Decisões Técnicas

### Por que Minimal API e não Controllers?
A Minimal API do .NET 8 é mais enxuta, com menos boilerplate, ideal para APIs pequenas e focadas. O roteamento fica explícito no `Program.cs`, facilitando a leitura e auditoria das rotas.

### Por que Dapper e não Entity Framework?
O Dapper é um micro-ORM que executa SQL puro, dando controle total sobre as queries. Isso garante:
- Queries previsíveis e auditáveis
- Sem migrations automáticas que podem alterar o banco sem controle
- Parâmetros nomeados com `@` que eliminam SQL Injection por design

### Por que interfaces nos Repositories?
As interfaces (`IUsuarioRepository`, `IEventoRepository`, `ICupomRepository`) permitem que os testes unitários substituam as implementações reais por mocks (via NSubstitute), sem precisar de banco de dados real durante os testes.

### Por que NSubstitute nos testes?
NSubstitute permite criar implementações falsas das interfaces de repositório, controlando o que cada método retorna. Isso torna os testes unitários rápidos, isolados e sem dependência de infraestrutura externa.

---

## Segurança

Todas as queries SQL utilizam parâmetros nomeados com `@`. Nenhuma query usa concatenação (`+`) ou interpolação (`$"{}"`). Isso garante proteção total contra SQL Injection, conforme exigido pelo contrato do projeto.

Exemplos de queries seguras utilizadas:

```sql
SELECT * FROM Usuarios WHERE Cpf = @Cpf

INSERT INTO Usuarios (Cpf, Nome, Email) VALUES (@Cpf, @Nome, @Email)

INSERT INTO Eventos (Nome, CapacidadeTotal, DataEvento, PrecoPadrao)
VALUES (@Nome, @CapacidadeTotal, @DataEvento, @PrecoPadrao)

SELECT COUNT(1) FROM Cupons WHERE Codigo = @Codigo
```

---

## Injeção de Dependência

Todas as dependências são registradas no contêiner do ASP.NET Core em `Program.cs`:

| Tipo | Ciclo de Vida | Motivo |
|------|--------------|--------|
| `DbConnectionFactory` | Singleton | Criada uma vez, reutilizada para criar conexões |
| `IUsuarioRepository` → `UsuarioRepository` | Scoped | Uma instância por requisição HTTP |
| `IEventoRepository` → `EventoRepository` | Scoped | Uma instância por requisição HTTP |
| `ICupomRepository` → `CupomRepository` | Scoped | Uma instância por requisição HTTP |
| `UsuarioService` | Scoped | Depende do repositório, mesmo ciclo |
| `EventoService` | Scoped | Depende do repositório, mesmo ciclo |
| `CupomService` | Scoped | Depende do repositório, mesmo ciclo |
