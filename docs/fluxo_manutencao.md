# Fluxo de Manutenção

## Parte 1: Classificação de Tickets (Taxonomia de Swanson)

### Ticket 1: "Sistema retorna erro 500 ao cadastrar evento com data no passado"
**Classificação:** Corretiva
**Justificativa:** Corrige defeito existente — validação deveria retornar 400, não 500.

### Ticket 2: "Migrar banco de dados de PostgreSQL 14 para PostgreSQL 16"
**Classificação:** Adaptativa
**Justificativa:** Adapta sistema ao novo ambiente sem alterar funcionalidades.

### Ticket 3: "Adicionar campo 'categoria' na tabela eventos (Show, Festival, Teatro)"
**Classificação:** Perfectiva
**Justificativa:** Evolui funcionalidade existente melhorando capacidade de classificação.

### Ticket 4: "Endpoint de listagem de eventos demora 8 segundos com 10 mil registros"
**Classificação:** Perfectiva
**Justificativa:** Melhora atributo de qualidade (performance) sem corrigir defeito funcional.

### Ticket 5: "API deve suportar autenticação via JWT conforme nova regulamentação"
**Classificação:** Adaptativa
**Justificativa:** Adapta sistema a novo requisito externo (compliance regulatório).

### Ticket 6: "Adicionar índice B-tree na coluna eventoid da tabela reservas"
**Classificação:** Preventiva
**Justificativa:** Previne degradação de performance futura antes que problema se manifeste.

### Ticket 7: "Corrigir cálculo de desconto que permite valores negativos"
**Classificação:** Corretiva
**Justificativa:** Corrige defeito na regra de negócio de aplicação de cupons.

### Ticket 8: "Implementar soft delete em vez de exclusão física de usuários"
**Classificação:** Perfectiva
**Justificativa:** Melhora rastreabilidade e auditoria sem corrigir falha.

### Ticket 9: "Atualizar Dapper de 2.1.72 para 2.1.85 por correção de CVE"
**Classificação:** Preventiva
**Justificativa:** Previne exploração de vulnerabilidade conhecida antes de ser atacada.

### Ticket 10: "Sistema deve rodar em containers Docker para deploy no Kubernetes"
**Classificação:** Adaptativa
**Justificativa:** Adapta sistema à nova plataforma de infraestrutura.

### Ticket 11: "Substituir validação manual de CPF por biblioteca FluentValidation"
**Classificação:** Perfectiva
**Justificativa:** Melhora qualidade do código e manutenibilidade sem corrigir defeito.

### Ticket 12: "Adicionar health check endpoint para monitoramento do Railway"
**Classificação:** Preventiva
**Justificativa:** Adiciona mecanismo de detecção precoce de falhas antes do impacto ao usuário.

---

## Parte 2: Pipeline de Liberação Segura

### Contexto do Ticket de Correção
**Ticket:** "Corrigir validação de capacidade do evento que permite venda além do limite"

**Descrição:** A função `ContarReservasPorEvento` retorna count >= capacidade, mas validação usa `>` em vez de `>=`, permitindo venda de 1 ingresso a mais.

---

### 1. Análise de Impacto

**Componentes Afetados:**
- `ReservaService.ComprarIngresso()` — lógica de validação
- `ReservaRepository.ContarReservasPorEvento()` — query de contagem
- Tabela `reservas` — possíveis inconsistências históricas

**Dependências:**
- Frontend confia no backend para bloquear compras
- Relatórios de ocupação podem estar incorretos
- SLA de "ingressos esgotados" foi violado

**Risco de Regressão:**
- Alto: Lógica de validação é crítica para regra de negócio central
- Mudança envolve operador lógico em condição de guarda

**Análise de Dados:**
```sql
-- Verificar eventos com overselling
SELECT eventoid, COUNT(*) as vendidos, e.capacidadetotal
FROM reservas r
JOIN eventos e ON r.eventoid = e.id
GROUP BY eventoid, e.capacidadetotal
HAVING COUNT(*) > e.capacidadetotal;
```

---

### 2. Teste como Instrumento Cirúrgico

**Teste de Regressão:**
```csharp
[Fact]
public async Task ComprarIngresso_QuandoCapacidadeExataEsgotada_DeveBloquear()
{
    // Arrange
    var evento = new Evento { CapacidadeTotal = 100 };
    reservaRepo.ContarReservasPorEvento(1).Returns(100); // Exato

    // Act
    var (sucesso, mensagem, _) = await service.ComprarIngresso(dto);

    // Assert
    Assert.False(sucesso);
    Assert.Contains("esgotados", mensagem);
}
```

**Teste de Borda:**
```csharp
[Fact]
public async Task ComprarIngresso_QuandoRestaumaVaga_DevePermitir()
{
    // Arrange
    reservaRepo.ContarReservasPorEvento(1).Returns(99); // 1 vaga livre

    // Act
    var (sucesso, _, _) = await service.ComprarIngresso(dto);

    // Assert
    Assert.True(sucesso);
}
```

---

### 3. Feature Toggle

**Implementação:**
```csharp
public async Task<(bool, string, int?)> ComprarIngresso(ComprarIngressoDto dto)
{
    var reservasExistentes = await _reservaRepo.ContarReservasPorEvento(dto.EventoId);
    
    // Feature Toggle: nova validação
    var usarValidacaoCorrigida = _configuration.GetValue<bool>("FeatureToggles:ValidacaoCapacidadeCorrigida");
    
    bool capacidadeEsgotada = usarValidacaoCorrigida 
        ? reservasExistentes >= evento.CapacidadeTotal  // NOVA LÓGICA
        : reservasExistentes > evento.CapacidadeTotal;  // ANTIGA LÓGICA
    
    if (capacidadeEsgotada)
    {
        return (false, "Ingressos esgotados", null);
    }
    
    // ... resto do código
}
```

**Configuração:**
```json
{
  "FeatureToggles": {
    "ValidacaoCapacidadeCorrigida": false
  }
}
```

**Controle:**
- Em staging: `true` (nova lógica ativada)
- Em produção: `false` inicialmente, ativa após validação em canário

---

### 4. Estratégia de Release e Regressão

**Fase 1: Canary Deployment (5% do tráfego)**
- Deploy com toggle ativado para 5% dos usuários
- Monitorar métricas:
  - Taxa de rejeição de compras (esperado: aumento de ~0.1%)
  - Tempo de resposta do endpoint (esperado: sem alteração)
  - Erros 500 (esperado: zero)
- Duração: 2 horas

**Fase 2: Rollout Gradual**
- 25% → 50% → 100% em incrementos de 1 hora
- Se erro rate > 0.5% em qualquer fase: ROLLBACK automático

**Fase 3: Cleanup**
- Após 7 dias com toggle em 100% e zero incidentes:
  - Remover código antigo
  - Remover toggle
  - Commit: "Remove feature toggle for capacity validation fix"

**Plano de Rollback:**
```bash
# Rollback instantâneo via configuração (sem redeploy)
curl -X POST https://api.codeticket.com/admin/toggles \
  -d '{"ValidacaoCapacidadeCorrigida": false}'

# Rollback completo (última versão estável)
railway rollback --to v2.3.1
```

**Validação Pós-Deploy:**
```bash
# Smoke test automatizado
curl -X POST https://api.codeticket.com/api/ingressos/comprar \
  -d '{"usuarioCpf":"00000000000","eventoId":999}' \
  | jq '.mensagem' | grep -q "esgotados"
```
