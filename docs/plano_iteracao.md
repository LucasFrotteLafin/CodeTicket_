# Plano de Iteração — Sprint 3

## Objetivo da Iteração:
Implementar sistema de compra de ingressos com validações robustas de negócio, garantindo proteção contra overselling e bloqueio de cambistas por CPF, além de adicionar endpoint de consulta de ingressos com JOIN entre múltiplas tabelas.

---

## Escopo (Backlog Selecionado):

### US-001: Compra de Ingresso
**Como** usuário cadastrado  
**Quero** comprar ingresso para um evento  
**Para** garantir minha participação

**Critérios de Aceite:**
- Sistema valida se usuário existe antes de processar compra
- Sistema bloqueia compra se evento já passou
- Sistema impede venda além da capacidade total do evento
- Sistema bloqueia múltiplas compras do mesmo CPF para mesmo evento

### US-002: Aplicação de Cupom de Desconto
**Como** usuário  
**Quero** aplicar cupom de desconto na compra  
**Para** obter preço reduzido

**Critérios de Aceite:**
- Sistema valida se cupom existe
- Sistema aplica desconto apenas se valor do ingresso atingir mínimo
- Sistema garante que valor final não seja negativo

### US-003: Consulta de Meus Ingressos
**Como** usuário  
**Quero** visualizar todos os meus ingressos comprados  
**Para** confirmar minhas reservas

**Critérios de Aceite:**
- Sistema retorna ingressos com JOIN entre reservas, usuários e eventos
- Resposta inclui nome do evento, data, local e valor pago
- Sistema calcula desconto aplicado quando houver cupom

---

## Entregáveis (Evidências):

1. **Código-Fonte:**
   - `ReservaService.cs` com 8 validações de negócio
   - `ReservaRepository.cs` com query JOIN entre 3 tabelas
   - `ComprarIngressoDto.cs` e `IngressoDetalheDto.cs`
   - 2 novos endpoints em `Program.cs`

2. **Testes Automatizados:**
   - 10 testes unitários em `ReservaTests.cs` seguindo padrão AAA
   - Nomenclatura padrão `Metodo_Cenario_ResultadoEsperado`
   - Cobertura de cenários de erro e sucesso

3. **Documentação:**
   - ADR sobre escolha do Dapper
   - Registro de 10 dívidas técnicas com priorização
   - Análise de arquitetura com 3 padrões e 5 violações

4. **Deploy:**
   - API atualizada no Railway com novos endpoints
   - Migration manual do banco aplicada
   - Swagger atualizado com documentação

---

## Risco Principal do Ciclo:

**Risco:** Race condition em compras simultâneas do último ingresso disponível.

**Probabilidade:** Média  
**Impacto:** Alto (overselling pode gerar reclamações e reembolsos)

**Mitigação Planejada:**
- Implementar lock pessimista com `SELECT FOR UPDATE` no PostgreSQL
- Adicionar constraint UNIQUE na tabela reservas (usuariocpf, eventoid)
- Teste de carga com JMeter simulando 100 requisições simultâneas

**Critério de Ativação do Plano B:**
Se testes de carga identificarem vendas duplicadas, pausar deploy e implementar transação distribuída com isolation level SERIALIZABLE.

---

## Definição de Pronto (DoD):

Uma história de usuário está PRONTA quando:

- [ ] Código implementado segue padrão de camadas (Repository → Service → Controller)
- [ ] Todas as queries SQL são parametrizadas (zero concatenação de strings)
- [ ] Pelo menos 3 testes unitários cobrem a funcionalidade (cenários positivo, negativo e borda)
- [ ] Testes seguem padrão AAA com comentários `// Arrange`, `// Act`, `// Assert`
- [ ] Nomenclatura de testes segue padrão `Metodo_Cenario_Resultado`
- [ ] Code review aprovado por pelo menos 1 membro do time
- [ ] Branch mergeada na `main` sem conflitos
- [ ] Deploy realizado no Railway com sucesso (status code 200 no health check)
- [ ] Endpoint testado manualmente via Swagger/Postman
- [ ] Documentação atualizada no README.md (se aplicável)
- [ ] Zero warnings de compilação ou alertas de segurança do SonarQube

---

## Quadro Visual (Kanban)

```
┌─────────────┬──────────────────┬─────────────┬────────────┐
│  Backlog    │ Em Desenvolvimento│ Code Review │ Concluído  │
│             │   (WIP máximo: 2)│             │            │
├─────────────┼──────────────────┼─────────────┼────────────┤
│ US-004      │ US-001 (Lucas)   │ US-003      │ US-002     │
│ US-005      │ US-006 (Ana L.)  │             │ DT-001     │
│ US-007      │                  │             │ ADR-001    │
│             │                  │             │            │
└─────────────┴──────────────────┴─────────────┴────────────┘
```

### Regras do Quadro:

1. **Limite de WIP: 2 tarefas** — Máximo de 2 cards simultaneamente em "Em Desenvolvimento"
2. **Bloqueio Visual:** Cards com impedimento recebem tag vermelha `[BLOQUEADO]`
3. **Pull System:** Desenvolvedor só puxa nova tarefa após mover anterior para Code Review
4. **Daily Sync:** Quadro atualizado diariamente às 9h via stand-up de 15 minutos
5. **Métrica de Fluxo:** Calcular Lead Time (Backlog → Concluído) ao fim da sprint

### Justificativa do WIP = 2:

Equipe possui 5 integrantes, mas definimos WIP conservador (< tamanho do time) porque:
- 40% do tempo é dedicado a atividades acadêmicas paralelas
- Code review requer pelo menos 2 reviewers, gerando gargalo
- Tarefas técnicas (ADRs, dívidas) não entram no WIP de desenvolvimento
