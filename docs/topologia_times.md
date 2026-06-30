# Topologia de Times (Team Topologies)

## Contexto do Projeto CodeTicket

O CodeTicket é um sistema de venda de ingressos desenvolvido como projeto acadêmico por uma equipe de 5 estudantes. Apesar da escala reduzida, aplicamos os conceitos de Team Topologies para estruturar responsabilidades e fluxos de interação.

---

## Mapeamento dos 4 Tipos de Time

### 1. Stream-Aligned Team (Time Alinhado ao Fluxo de Valor)

**Definição:** Time responsável por entregar valor end-to-end para o usuário final, possuindo ownership completo de uma funcionalidade ou jornada.

**Aplicação no CodeTicket:**

**Time:** Equipe Principal de Features (5 integrantes)

**Responsabilidades:**
- Implementar histórias de usuário do backlog (compra de ingressos, cadastro de eventos, cupons)
- Manter endpoints da API e frontend Blazor
- Garantir deploy contínuo no Railway/Netlify
- Responder a incidentes de produção

**Habilidades T-shaped:**
- Cada membro domina pelo menos 1 área (backend, frontend, banco de dados)
- Todos conseguem fazer code review e deployar sistema
- Rotação de papéis a cada sprint (dev, QA, DevOps)

**Fluxo de Valor:**
```
Requisito do Usuário → Desenvolvimento → Testes → Deploy → Monitoramento
```

**Interações:**
- **Colaboração** com Platform Team (quando precisa configurar novo banco de dados)
- **Facilitação** de Enabling Team (quando aprende novas práticas de segurança)

---

### 2. Platform Team (Time de Plataforma)

**Definição:** Time que fornece serviços internos de infraestrutura como produto, reduzindo carga cognitiva dos times stream-aligned.

**Aplicação no CodeTicket:**

**Time:** Plataformas Externas (Railway, Netlify, GitHub)

**Serviços Fornecidos:**
- Hospedagem gerenciada da API (Railway)
- Hospedagem estática do frontend (Netlify)
- CI/CD via GitHub Actions
- Banco de dados PostgreSQL gerenciado

**Por que usamos plataformas prontas:**
No contexto acadêmico, não temos tempo/recursos para manter infraestrutura própria. Terceirizamos complexidade para plataformas PaaS.

**Self-service:**
- Desenvolvedores fazem deploy via `git push` (Railway auto-deploy)
- Logs acessíveis via dashboard do Railway (sem precisar de SRE)
- Configurações via arquivo `railway.json` no repositório

**Analogia Interna:**
Se o projeto escalasse, criaríamos time de 2 pessoas responsáveis por:
- Manter scripts de migração de banco de dados
- Configurar pipelines de CI/CD
- Prover templates de projetos novos (scaffolding)

---

### 3. Enabling Team (Time Habilitador)

**Definição:** Time especialista que atua temporariamente com stream-aligned teams para elevar capacidades técnicas e remover impedimentos.

**Aplicação no CodeTicket:**

**Time:** Professor + Comunidade (Stack Overflow, GitHub Issues)

**Missões de Habilitação:**

**Exemplo 1: Adoção de Dapper**
- Problema: Equipe não sabia como implementar queries parametrizadas com Dapper
- Ação: Professor orientou em 2 sessões sobre padrão Repository e injeção de dependência
- Resultado: Equipe autônoma para criar novos repositórios sem supervisão

**Exemplo 2: Implementação de Testes AAA**
- Problema: Testes estavam sem estrutura clara
- Ação: Enabling team forneceu template de teste com comentários `// Arrange`, `// Act`, `// Assert`
- Resultado: Equipe padronizou todos os 22 testes em 1 sprint

**Modelo de Interação:**
- Enabling team NÃO faz o trabalho pelo stream-aligned team
- Atua como **consultor temporário** (2-4 semanas máximo)
- Após transferência de conhecimento, se afasta para permitir autonomia

**Ferramentas de Habilitação:**
- Documentação interna (ADRs, tutoriais)
- Pair programming com membro sênior
- Workshops práticos (ex: "Como fazer code review de segurança")

---

### 4. Complicated-Subsystem Team (Time de Subsistema Complexo)

**Definição:** Time especializado em componente técnico que exige conhecimento profundo e não pode ser simplificado (ex: algoritmos de ML, processamento de pagamento).

**Aplicação no CodeTicket:**

**Subsistema Identificado:** Integração com Gateway de Pagamento (futuro)

**Por que é "Complicated":**
- Requer conhecimento especializado de PCI-DSS compliance
- Lida com criptografia de dados sensíveis de cartão
- Integra com múltiplos provedores (Stripe, PagSeguro, Mercado Pago)
- Falha pode gerar perda financeira direta

**Modelo de Interação:**
```
Stream-Aligned Team (Features)
        ↓ (consome API)
Complicated-Subsystem Team (Pagamentos)
        ↓ (abstrai complexidade)
    Gateway Externo (Stripe)
```

**Interface Fornecida:**
```csharp
public interface IPagamentoService
{
    Task<ResultadoPagamento> ProcessarPagamento(
        decimal valor, 
        string tokenCartao, 
        string cpf
    );
}
```

Stream-aligned team **não precisa saber** sobre:
- Tokenização de cartão
- Webhooks de confirmação
- Retry com backoff exponencial
- Conciliação bancária

**Quando NÃO criar este tipo de time:**
Se a complexidade pode ser terceirizada (ex: usar SDK pronto do Stripe), manter no stream-aligned team. Complicated-subsystem só se justifica quando há necessidade de lógica customizada crítica.

---

## Fluxo de Interação entre Times

```
┌──────────────────────────────────────────────────────┐
│  Stream-Aligned Team (CodeTicket Core)              │
│  - Implementa features                               │
│  - Owner do produto completo                         │
└──────────────────────────────────────────────────────┘
         │                   │                  │
         │                   │                  │
    (consome)          (aprende com)       (consulta)
         │                   │                  │
         ↓                   ↓                  ↓
┌────────────────┐  ┌────────────────┐  ┌──────────────┐
│ Platform Team  │  │ Enabling Team  │  │ Complicated  │
│ (Railway)      │  │ (Professor)    │  │ Subsystem    │
│                │  │                │  │ (Pagamentos) │
│ - Auto-deploy  │  │ - Mentoria     │  │ - PCI-DSS    │
│ - Logs         │  │ - Padrões      │  │ - Crypto     │
│ - Banco PG     │  │ - Code Review  │  │ - Webhooks   │
└────────────────┘  └────────────────┘  └──────────────┘
```

---

## Benefícios da Aplicação de Team Topologies

### Para o Projeto Acadêmico:
1. **Clareza de Responsabilidades:** Cada membro sabe que o foco é entregar features (stream-aligned), não manter infraestrutura
2. **Redução de Carga Cognitiva:** Usar Railway/Netlify (platform) libera time para focar em lógica de negócio
3. **Aprendizado Estruturado:** Modelo enabling permite que professor atue como mentor sem criar dependência

### Para Transição para Produção Real:
1. **Escalabilidade Organizacional:** Se sistema crescer, separar time de pagamentos (complicated-subsystem) sem reescrever código
2. **Autonomia:** Stream-aligned team pode evoluir sistema sem esperar aprovação de outros times
3. **Fluxo Contínuo:** Minimizar handoffs e esperas através de self-service (platform) e documentação (enabling)

---

## Métricas de Saúde dos Times

| Métrica | Stream-Aligned | Platform | Enabling | Complicated |
|---------|----------------|----------|----------|-------------|
| Lead Time for Changes | < 24h | N/A | N/A | < 48h |
| Deploy Frequency | Diário | Semanal (upgrades) | N/A | Quinzenal |
| Time to Restore Service | < 1h | < 2h | N/A | < 30min (crítico) |
| Change Failure Rate | < 10% | < 5% | N/A | < 2% (alta criticidade) |
| Cognitive Load Score | Médio (6/10) | Baixo (2/10) | Baixo (3/10) | Alto (8/10) |

**Cognitive Load Score:** Avaliação subjetiva de 0-10 sobre complexidade percebida pelo time. Acima de 7 indica necessidade de simplificação ou split do time.
