# Release Checklist Final — CodeTicket AV2

Este checklist documenta a conformidade do sistema CodeTicket com os critérios de qualidade e engenharia de software exigidos para a AV2.

---

## [x] 1. Fundamentos de Engenharia

**Critérios:**
- Sistema compila sem erros ou warnings críticos
- Arquitetura em camadas implementada (Models, DTOs, Repositories, Services, Controllers)
- Injeção de dependência configurada corretamente
- Separação de responsabilidades (SRP) respeitada

**Evidências:**
- ✅ `dotnet build` executa com 0 errors, 0 warnings
- ✅ Estrutura de pastas: `/Data`, `/DTOs`, `/Models`, `/Repositories`, `/Services`
- ✅ DI container configurado em `Program.cs` com `AddScoped`, `AddSingleton`
- ✅ Controllers não contêm lógica de negócio (delegam para Services)

**Arquivos Verificados:**
- `src/backend/Program.cs` — Configuração DI e endpoints
- `src/backend/Services/ReservaService.cs` — Lógica de negócio isolada
- `src/backend/Repositories/ReservaRepository.cs` — Acesso a dados parametrizado

---

## [x] 2. Produto Mínimo Viável

**Critérios:**
- Pelo menos 2 novos endpoints implementados além do CRUD básico
- 1 endpoint aplica múltiplas validações de negócio (≥3)
- 1 endpoint realiza JOIN entre tabelas
- Sistema deployado e acessível publicamente

**Evidências:**
- ✅ **Endpoint 1:** `POST /api/ingressos/comprar` — 8 validações de negócio
  - Valida existência de usuário
  - Valida existência de evento
  - Valida data do evento (não pode estar no passado)
  - Valida capacidade (overselling)
  - Bloqueia cambistas (1 ingresso por CPF)
  - Valida cupom
  - Valida valor mínimo do cupom
  - Previne valor negativo
- ✅ **Endpoint 2:** `GET /api/ingressos/meus/{cpf}` — JOIN entre 3 tabelas
  - `INNER JOIN` entre `reservas`, `usuarios` e `eventos`
  - Retorna dados combinados (nome do usuário + detalhes do evento + valor pago)
- ✅ Deploy realizado no Railway: https://codeticket-production.up.railway.app/

**Arquivos Verificados:**
- `src/backend/Services/ReservaService.cs` — Linhas 18-92 (validações)
- `src/backend/Repositories/ReservaRepository.cs` — Linhas 49-67 (query JOIN)
- `src/backend/Program.cs` — Linhas 71-95 (endpoints registrados)

---

## [x] 3. Evidência de Qualidade

**Critérios:**
- Pelo menos 10 testes unitários implementados
- Testes seguem padrão AAA com comentários `// Arrange`, `// Act`, `// Assert`
- Nomenclatura de testes no formato `Metodo_Cenario_ResultadoEsperado`
- Testes não contêm lógica condicional (if, for, while)
- `dotnet test` executa com 100% de sucesso

**Evidências:**
- ✅ **10 novos testes** implementados em `ReservaTests.cs`
- ✅ Todos os testes possuem comentários AAA explícitos
- ✅ Nomenclatura padronizada:
  - `ComprarIngresso_QuandoUsuarioNaoExiste_DeveRetornarErro`
  - `ComprarIngresso_QuandoIngressosEsgotados_DeveRetornarErro`
  - `ListarMeusIngressos_QuandoUsuarioExisteComIngressos_DeveRetornarLista`
- ✅ Nenhum teste contém `if`, `switch`, `for` ou `while`
- ✅ Execução: `dotnet test` → `Aprovado! – Com falha: 0, Aprovado: 22+, Ignorado: 0`

**Arquivos Verificados:**
- `tests/TicketPrime.Tests/ReservaTests.cs` — 10 testes com padrão AAA

---

## [x] 4. Decisões Documentadas

**Critérios:**
- Pelo menos 1 ADR (Architecture Decision Record) criado
- ADR contém seções `# Contexto`, `# Decisão`, `# Consequências`
- Campo `Status:` preenchido (Proposto/Aceito/Rejeitado/Obsoleto)
- Consequências divididas em `Prós:` e `Contras:`

**Evidências:**
- ✅ **ADR 001:** "Escolha do Dapper como Micro-ORM"
- ✅ Status: **Aceito**
- ✅ Seções completas:
  - Contexto: Necessidade de ORM leve com controle de SQL
  - Decisão: Adotar Dapper sobre Entity Framework e ADO.NET
  - Consequências:
    - **Prós:** Segurança, performance, transparência, leveza, controle
    - **Contras:** Ausência de migrations, sem change tracking, repetição de código

**Arquivos Verificados:**
- `docs/adrs/001-escolha-do-micro-orm.md`

---

## [x] 5. Evidência de Requisitos

**Critérios:**
- Análise de 3 padrões arquiteturais com trade-offs documentados
- Análise de violações arquiteturais com pelo menos 5 problemas identificados
- Cada violação possui `**Problema:**`, `**Evidência:**`, `**Impacto:**`, `**Ação Recomendada:**`
- Registro de dívidas técnicas com tabela contendo 6+ dívidas
- Colunas de priorização (Freq. Alteração, Risco, Esforço, Decisão) preenchidas

**Evidências:**
- ✅ **Análise de Padrões:** Microsserviços, Monolito em Camadas, Event-Driven
  - Cada padrão possui trade-off com Positivo/Negativo
- ✅ **5 Violações Arquiteturais Identificadas:**
  1. Credenciais hardcoded
  2. SQL Injection
  3. Violação de SRP
  4. Gerenciamento manual de recursos
  5. Acoplamento SMTP
- ✅ **10 Dívidas Técnicas Registradas:**
  - Classificadas por Freq. Alteração (Alto/Médio/Baixo)
  - Risco (Alto/Médio/Baixo)
  - Esforço (Alto/Médio/Baixo)
  - Decisão (Prioridade 1, 2 ou 3)

**Arquivos Verificados:**
- `docs/analise_arquitetura.md` — Padrões e violações
- `docs/registro_divida_tecnica.md` — Tabela com 10 dívidas

---

## [x] 6. Governança e Operação

**Critérios:**
- Plano de iteração com Objetivo, Escopo, Entregáveis, Risco, DoD
- Quadro visual com 4+ colunas e limite de WIP definido
- Matriz de riscos com 5+ riscos, colunas de Probabilidade/Impacto/Estratégia/Gatilho/Ação
- 2 métricas operacionais (1 de fluxo, 1 de qualidade) com 7 campos da ficha de definição
- 1 SLO com SLI, Fórmula, Fonte, Janela e Alvo
- Error Budget Policy com 3 níveis graduados (Nível 3 menciona Feature Freeze)

**Evidências:**
- ✅ **Plano de Iteração:** Sprint 3 completo com US-001, US-002, US-003
- ✅ **Quadro Kanban:** 4 colunas (Backlog, Em Desenvolvimento, Code Review, Concluído)
  - WIP máximo: **2 tarefas** (menor que tamanho do time = 5)
- ✅ **Matriz de Riscos:** 5 riscos mapeados
  - Gatilhos mensuráveis (ex: "Pool atinge 80% por 5min")
- ✅ **Métricas:**
  - Fluxo: Lead Time for Changes (DORA) — 7 campos completos
  - Qualidade: Change Failure Rate — 7 campos completos
- ✅ **SLO:** Disponibilidade de 99.5% em 30 dias para `POST /api/ingressos/comprar`
- ✅ **Error Budget Policy:**
  - Nível 1: >50% restante (liberar features)
  - Nível 2: 10-50% restante (alerta)
  - Nível 3: <10% restante (**Feature Freeze**, Zero novas funcionalidades)

**Arquivos Verificados:**
- `docs/plano_iteracao.md` — Planejamento e Kanban
- `docs/operacao.md` — Riscos, métricas, SLO e Error Budget

---

## [x] 7. Segurança e Ciclo de Vida

**Critérios:**
- Nenhuma credencial hardcoded em arquivos `.cs` da pasta `/src`
- Strings de conexão usam `IConfiguration` ou variáveis de ambiente
- Threat Model com Ativos, Vetor de Ataque, Falha Arquitetural, Controle de Mitigação
- 3 Gates de segurança (SAST, SCA, DAST) numerados e descritos
- Classificação de 12 tickets pela taxonomia de Swanson
- Pipeline de liberação segura com 4 passos documentados
- Topologia de times mapeando 4 tipos (Stream-aligned, Platform, Enabling, Complicated-Subsystem)

**Evidências:**
- ✅ **Código Seguro:**
  - Connection string usa `builder.Configuration` em `DbConnectionFactory.cs`
  - Porta do servidor usa `Environment.GetEnvironmentVariable("PORT")`
  - Zero ocorrências de `Password=`, `User Id=` com valor literal
- ✅ **Threat Model:** Endpoint `/api/ingressos/comprar` analisado
  - Ativos: Dados financeiros, capacidade, CPF
  - Vetores: SQL Injection, Race Condition, Bypass de Cupom
  - Mitigações: Queries parametrizadas, locks, rate limiting
- ✅ **3 Gates de Segurança:**
  - Gate 1: SAST (Security Code Scan) — Pre-Commit
  - Gate 2: SCA (OWASP Dependency-Check) — Pull Request
  - Gate 3: DAST (OWASP ZAP) — Pre-Deploy
- ✅ **12 Tickets Classificados:**
  - Corretiva: 2 tickets
  - Adaptativa: 3 tickets
  - Perfectiva: 4 tickets
  - Preventiva: 3 tickets
- ✅ **Pipeline de Liberação:** 4 passos implementados
  1. Análise de Impacto
  2. Teste como Instrumento Cirúrgico
  3. Feature Toggle
  4. Estratégia de Release e Regressão
- ✅ **Topologia de Times:** 4 tipos mapeados ao contexto CodeTicket

**Arquivos Verificados:**
- `src/backend/Data/DbConnectionFactory.cs` — Sem credenciais hardcoded
- `docs/seguranca_ciclo.md` — Threat model e gates
- `docs/fluxo_manutencao.md` — Classificação de tickets e pipeline
- `docs/topologia_times.md` — Mapeamento de times

---

## Conclusão

✅ **TODOS OS 7 CRITÉRIOS ATENDIDOS**

O sistema CodeTicket AV2 atende integralmente aos 20 itens da tabela de avaliação, demonstrando:
- Código funcional com regras de negócio complexas
- Qualidade assegurada por testes automatizados
- Decisões arquiteturais documentadas
- Práticas de segurança aplicadas
- Governança e operação estruturadas
- Maturidade em Engenharia de Software

**Equipe CodeTicket:**
- Lucas Frotte Lafin (06010493)
- Ana Luiza Maciel Mattos (06009322)
- Ana Carolina Tomas (06010096)
- Alexandre dos Santos (06010479)
- Gabriel Duarte de Oliveira (06010804)

**Data de Conclusão:** 2025

**Versão do Sistema:** 2.0.0 (AV2)

**Status:** PRONTO PARA AVALIAÇÃO
