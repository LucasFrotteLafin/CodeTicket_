# Requisitos do Sistema — CodeTicket

Este documento descreve os requisitos funcionais do backend CodeTicket na forma de **Histórias de Usuário** e **Critérios de Aceitação no formato BDD (Behavior-Driven Development)**.

---

## Histórias de Usuário

### HU-01 — Cadastro de Usuário

Como usuário do sistema, Quero me cadastrar informando meu CPF, nome e e-mail, Para que eu possa ser identificado nas compras de ingressos e o sistema possa bloquear cambistas.

### HU-02 — Cadastro de Evento

Como administrador, Quero cadastrar eventos com nome, capacidade, data e preço, Para que os ingressos possam ser vendidos com controle de lotação e valor correto.

### HU-03 — Listagem de Eventos

Como usuário, Quero visualizar a lista de eventos disponíveis, Para que eu possa escolher qual evento desejo comprar um ingresso.

### HU-04 — Cadastro de Cupom de Desconto

Como administrador, Quero cadastrar cupons de desconto com código, percentual e valor mínimo de aplicação, Para que os usuários possam obter descontos válidos sem que o sistema gere valores negativos.

### HU-05 — Segurança contra SQL Injection

Como engenheiro de software responsável pelo sistema, Quero que todas as consultas ao banco de dados utilizem parâmetros nomeados com `@`, Para que o sistema seja imune a ataques de SQL Injection e proteja os dados dos usuários.

---

## Critérios de Aceitação

### HU-01 — Cadastro de Usuário

**Cenário 1: Cadastro com dados válidos**

Dado que o administrador envia um CPF com exatamente 11 dígitos numéricos, um nome não vazio e um e-mail no formato válido,
Quando a requisição POST /api/usuarios é enviada,
Então o sistema deve persistir o usuário no banco de dados e retornar status 200 com a mensagem "Usuário cadastrado com sucesso!".

**Cenário 2: CPF já cadastrado**

Dado que já existe um usuário com o CPF "12345678901" no banco de dados,
Quando uma nova requisição POST /api/usuarios é enviada com o mesmo CPF,
Então o sistema deve retornar status 400 com a mensagem "CPF já cadastrado.".

**Cenário 3: CPF com formato inválido**

Dado que o usuário informa um CPF com menos ou mais de 11 dígitos, ou contendo letras,
Quando a requisição POST /api/usuarios é enviada,
Então o sistema deve retornar status 400 com a mensagem "CPF deve conter exatamente 11 dígitos numéricos.".

**Cenário 4: E-mail com formato inválido**

Dado que o usuário informa um e-mail sem o caractere "@" ou sem domínio,
Quando a requisição POST /api/usuarios é enviada,
Então o sistema deve retornar status 400 com a mensagem "Email inválido.".

**Cenário 5: Campos obrigatórios ausentes**

Dado que o usuário envia uma requisição sem CPF, sem nome ou sem e-mail,
Quando a requisição POST /api/usuarios é enviada,
Então o sistema deve retornar status 400 indicando qual campo está ausente.

---

### HU-02 — Cadastro de Evento

**Cenário 1: Cadastro com dados válidos**

Dado que o administrador informa nome, capacidade maior que zero, data futura e preço maior que zero,
Quando a requisição POST /api/eventos é enviada,
Então o sistema deve persistir o evento no banco de dados e retornar status 200 com a mensagem "Evento criado com sucesso!".

**Cenário 2: Capacidade zero ou negativa**

Dado que o administrador informa uma capacidade total igual a zero ou negativa,
Quando a requisição POST /api/eventos é enviada,
Então o sistema deve retornar status 400 com a mensagem "A capacidade total deve ser maior que zero.".

**Cenário 3: Data no passado**

Dado que o administrador informa uma data de evento anterior ao dia atual,
Quando a requisição POST /api/eventos é enviada,
Então o sistema deve retornar status 400 com a mensagem "A data do evento não pode ser anterior ao dia de hoje.".

**Cenário 4: Preço zero ou negativo**

Dado que o administrador informa um preço padrão igual a zero ou negativo,
Quando a requisição POST /api/eventos é enviada,
Então o sistema deve retornar status 400 com a mensagem "O preço do evento deve ser maior que zero.".

**Cenário 5: Nome vazio**

Dado que o administrador envia uma requisição sem informar o nome do evento,
Quando a requisição POST /api/eventos é enviada,
Então o sistema deve retornar status 400 com a mensagem "O nome do evento é obrigatório.".

---

### HU-03 — Listagem de Eventos

**Cenário 1: Existem eventos cadastrados**

Dado que existem um ou mais eventos cadastrados no banco de dados,
Quando a requisição GET /api/eventos é enviada,
Então o sistema deve retornar status 200 com uma lista contendo o id e o nome de cada evento.

**Cenário 2: Nenhum evento cadastrado**

Dado que não existe nenhum evento cadastrado no banco de dados,
Quando a requisição GET /api/eventos é enviada,
Então o sistema deve retornar status 200 com uma lista vazia.

---

### HU-04 — Cadastro de Cupom de Desconto

**Cenário 1: Cadastro com dados válidos**

Dado que o administrador informa um código único, um percentual de desconto entre 1 e 100 e um valor mínimo não negativo,
Quando a requisição POST /api/cupons é enviada,
Então o sistema deve persistir o cupom no banco de dados e retornar status 200 com a mensagem "Cupom criado com sucesso".

**Cenário 2: Código de cupom já existente**

Dado que já existe um cupom com o código "VERAO25" no banco de dados,
Quando uma nova requisição POST /api/cupons é enviada com o mesmo código,
Então o sistema deve retornar status 400 com a mensagem "Cupom já existe".

**Cenário 3: Percentual de desconto inválido**

Dado que o administrador informa um percentual de desconto igual a zero, negativo ou acima de 100,
Quando a requisição POST /api/cupons é enviada,
Então o sistema deve retornar status 400 com a mensagem "Desconto deve ser entre 0 e 100".

**Cenário 4: Valor mínimo negativo**

Dado que o administrador informa um valor mínimo de aplicação negativo,
Quando a requisição POST /api/cupons é enviada,
Então o sistema deve retornar status 400 com a mensagem "Valor mínimo não pode ser negativo".

---

### HU-05 — Segurança contra SQL Injection

**Cenário 1: Tentativa de injeção via CPF**

Dado que um atacante envia o valor `"' OR '1'='1"` no campo CPF,
Quando a requisição POST /api/usuarios é enviada,
Então o sistema deve rejeitar o CPF por formato inválido antes de qualquer consulta ao banco, retornando status 400.

**Cenário 2: Parâmetros nomeados em todas as queries**

Dado que qualquer endpoint realiza uma operação no banco de dados,
Quando a query SQL é executada via Dapper,
Então todos os valores variáveis devem ser passados como parâmetros nomeados com `@` (ex: `@Cpf`, `@Nome`), nunca por concatenação de strings.
