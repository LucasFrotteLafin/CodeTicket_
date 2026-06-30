# Operação e Confiabilidade

## Matriz de Riscos

| Risco | Probabilidade | Impacto | Estratégia | Gatilho | Ação Planejada |
|-------|---------------|---------|------------|---------|----------------|
| Esgotamento de conexões do banco PostgreSQL | Médio | Alto | Mitigar | Pool de conexões atinge 80% da capacidade (>40 conexões ativas de 50 max) por mais de 5 minutos | Implementar connection pooling com Npgsql configurando `MaxPoolSize=50` e timeout de 30s. Adicionar log de warning quando >35 conexões. |
| Indisponibilidade do Railway (plataforma de hospedagem) | Baixo | Alto | Transferir | Status page do Railway reporta incidente crítico (status != operational) por mais de 10 minutos | Ativar plano de failover migrando tráfego para instância backup no Render via atualização de DNS no Netlify (TTL de 60s). |
| Ataque de DDoS em endpoint público `/api/eventos` | Baixo | Médio | Aceitar | Taxa de requisições excede 1000 req/min de IPs únicos distintos por mais de 3 minutos consecutivos | Aceitar risco no MVP. Em produção: implementar Cloudflare com rate limiting de 100 req/min por IP e challenge CAPTCHA após 3 violações. |
| Overselling de ingressos por race condition | Médio | Alto | Mitigar | Duas ou mais reservas criadas para mesmo evento quando `COUNT(reservas) = capacidadetotal - 1` | Implementar constraint UNIQUE (usuariocpf, eventoid) no banco. Adicionar lock pessimista com `SELECT FOR UPDATE` na query de validação. Teste de carga obrigatório antes de cada deploy. |
| Exposição de credenciais em logs de produção | Baixo | Crítico | Evitar | Palavra-chave sensível (`password`, `secret`, `token`) detectada em logs do Railway por análise automatizada | Configurar Serilog com filtro `DestructuringPolicy` bloqueando propriedades sensíveis. Implementar rotação automática de secrets via Railway CLI a cada 90 dias. Alertar equipe via Slack se regex `/password\s*[:=]/i` for encontrado. |

---

## Métricas Operacionais (Fichas de Definição)

### Métrica 1: Lead Time for Changes (DORA)

**Nome da Métrica:** Lead Time for Changes

**O que Mede:** Tempo decorrido entre commit no Git e deploy bem-sucedido em produção (disponibilidade para usuários finais).

**Fórmula:**
```
Lead Time (horas) = Timestamp do Deploy em Produção - Timestamp do Commit
```

**Fonte de Dados:**
- Commits: GitHub API (`GET /repos/:owner/:repo/commits`)
- Deploys: Railway webhooks enviando evento `deployment.success` com timestamp
- Agregação: Planilha Google Sheets atualizada via Zapier

**Frequência de Coleta:** Após cada deploy (evento-driven)

**Limites de Saúde:**
- 🟢 Excelente: < 1 hora (elite performers)
- 🟡 Aceitável: 1-24 horas (high performers)
- 🔴 Crítico: > 24 horas (low performers)

**Ação se Violado:**
Se Lead Time > 24h por 3 deploys consecutivos:
1. Reunião de retrospectiva emergencial com toda equipe
2. Análise de gargalos: revisar pipeline de CI/CD do GitHub Actions
3. Implementar deploy automatizado (eliminar steps manuais)
4. Considerar feature flags para reduzir tamanho de mudanças

---

### Métrica 2: Change Failure Rate (Qualidade)

**Nome da Métrica:** Change Failure Rate (Taxa de Falha em Mudanças)

**O que Mede:** Porcentagem de deploys que resultaram em falha, rollback ou hotfix dentro de 24 horas.

**Fórmula:**
```
CFR (%) = (Deploys com Falha / Total de Deploys) × 100

Onde "Deploys com Falha" inclui:
- Rollback manual executado
- Erro 500 com taxa > 5% nas primeiras 2 horas pós-deploy
- Hotfix crítico aplicado em < 24h
```

**Fonte de Dados:**
- Deploys totais: Railway deployment logs
- Rollbacks: Railway CLI history (`railway rollback --list`)
- Erros 500: Logs estruturados do Serilog filtrados por `StatusCode >= 500`
- Hotfixes: Commits tagueados com `[HOTFIX]` no título

**Frequência de Coleta:** Semanal (toda segunda-feira às 9h)

**Limites de Saúde:**
- 🟢 Excelente: < 5% (objetivo DORA elite)
- 🟡 Aceitável: 5-15% (high performers)
- 🔴 Crítico: > 15% (indica processo de QA inadequado)

**Ação se Violado:**
Se CFR > 15% por 2 semanas consecutivas:
1. Pausar novas features (Feature Freeze parcial)
2. Aumentar cobertura de testes: meta de 80% em lógica de negócio
3. Implementar smoke tests automatizados pós-deploy
4. Adicionar code review obrigatório por 2 desenvolvedores seniores
5. Realizar análise de causa raiz (5 Whys) de cada falha

---

## SLO (Service Level Objective)

### SLO: Disponibilidade do Endpoint de Compra de Ingressos

**Rota Crítica:** `POST /api/ingressos/comprar`

**SLI (Indicador):** Disponibilidade medida como porcentagem de requisições bem-sucedidas (status code 2xx ou 4xx esperado) sobre o total de requisições.

**Fórmula de Coleta:**
```
Availability (%) = (Requisições Bem-Sucedidas / Total de Requisições) × 100

Onde:
- Bem-Sucedidas = Status codes 200, 400, 404 (comportamento esperado)
- Falhas = Status codes 500, 502, 503, 504, timeout > 30s
```

**Fonte do Dado:**
- Logs estruturados do ASP.NET Core Middleware
- Agregação via query no Seq/Serilog:
  ```sql
  SELECT 
    COUNT(CASE WHEN StatusCode < 500 THEN 1 END) * 100.0 / COUNT(*) AS availability
  FROM Logs
  WHERE Path = '/api/ingressos/comprar'
    AND Timestamp >= NOW() - INTERVAL '30 days'
  ```

**Janela de Medição:** 30 dias (rolling window)

**Alvo (SLO):** 99.5%

**Orçamento de Erro (Error Budget):**
```
Error Budget = 100% - 99.5% = 0.5%

Em 30 dias (43.200 minutos):
Tempo de indisponibilidade permitido = 216 minutos (~3.6 horas)
```

---

## Error Budget Policy

### Nível 1: Error Budget Saudável (>50% restante)
**Condição:** Consumimos menos de 50% do orçamento de erro no período (disponibilidade > 99.75%)

**Ações:**
- ✅ Liberar novas features normalmente
- ✅ Experimentos e refatorações permitidos
- ✅ Deploys diários sem restrições
- ✅ Foco em inovação e velocidade

---

### Nível 2: Error Budget em Alerta (10-50% restante)
**Condição:** Consumimos entre 50-90% do orçamento (disponibilidade entre 99.5% e 99.75%)

**Ações:**
- ⚠️ Reduzir frequência de deploys para 1 por dia (apenas durante horário comercial)
- ⚠️ Code review obrigatório por 2 pessoas para mudanças em código crítico
- ⚠️ Pausar refatorações grandes que não corrigem bugs
- ⚠️ Priorizar correção de bugs conhecidos sobre features
- ⚠️ Aumentar cobertura de smoke tests pós-deploy
- ⚠️ Reunião semanal de revisão de incidentes

---

### Nível 3: Error Budget Esgotado (<10% restante)
**Condição:** Consumimos mais de 90% do orçamento (disponibilidade < 99.55%)

**Ações:**
- 🛑 **Feature Freeze**: Congelamento total de novas funcionalidades
- 🛑 **Zero novas funcionalidades** até orçamento ser restaurado
- 🛑 Apenas correções críticas (P0/P1) podem ser deployadas
- 🛑 Rollback automático se erro rate > 1% nas primeiras 10 minutos
- 🛑 Todos os deploys exigem aprovação do tech lead
- 🛑 Post-mortem obrigatório para cada incidente
- 🛑 Investimento em:
  - Testes de carga e caos (chaos engineering)
  - Melhorias de observabilidade (logs, métricas, tracing)
  - Automação de rollback
  - Revisão de arquitetura de componentes instáveis

**Critério de Saída do Nível 3:**
Sistema permanece em Feature Freeze até:
1. Disponibilidade voltar para > 99.6% por 7 dias consecutivos
2. Todas as causas raiz dos incidentes recentes estarem documentadas e mitigadas
3. Cobertura de testes aumentar em pelo menos 15% em áreas afetadas

**Comunicação:**
- Status de Error Budget publicado semanalmente no Slack (#engineering)
- Dashboard público em tempo real (Grafana) mostrando SLI atual vs SLO
