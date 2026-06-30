# Segurança no Ciclo de Vida

## Threat Model: Endpoint de Compra de Ingressos

### Rota Analisada: `POST /api/ingressos/comprar`

**Ativos Protegidos:**
- Dados financeiros (valores pagos, descontos aplicados)
- Integridade de capacidade dos eventos (evitar overselling)
- Dados pessoais (CPF do comprador)
- Disponibilidade do serviço (prevenção de DoS)

**Vetor de Ataque Provável:**

**1. SQL Injection via Campos Não Validados**
- Atacante envia CPF malicioso: `"12345'; DROP TABLE reservas; --"`
- Se sistema concatenar string em vez de parametrizar, banco é comprometido

**2. Race Condition (TOCTOU)**
- Dois atacantes executam requisição simultânea para último ingresso
- Ambos passam na validação de capacidade antes do INSERT
- Sistema vende ingresso duplicado (overselling)

**3. Bypass de Validação de Cupom**
- Atacante manipula campo `cupomCodigo` com código inexistente mas bem formado
- Se validação tiver falha lógica, desconto pode ser aplicado indevidamente
- Cenário crítico: desconto de 100% gerando ingresso gratuito

**Falha Arquitetural Potencial:**

**Ausência de Rate Limiting**
- Endpoint público sem throttling permite:
  - Bot automatizado tentando todas combinações de CPF
  - Enumeração de eventos válidos via força bruta
  - DDoS esgotando pool de conexões do banco

**Controle de Engenharia (Mitigação):**

1. **Queries Parametrizadas Obrigatórias**
   ```csharp
   // ✅ CORRETO: Dapper com @Parametro
   var sql = "SELECT * FROM usuarios WHERE cpf = @Cpf";
   await conn.QueryAsync(sql, new { Cpf = dto.UsuarioCpf });
   
   // ❌ PROIBIDO: Concatenação
   var sql = $"SELECT * FROM usuarios WHERE cpf = '{dto.UsuarioCpf}'";
   ```

2. **Transação com Lock Pessimista**
   ```csharp
   using var transaction = conn.BeginTransaction();
   
   // Lock exclusivo na linha do evento durante contagem
   var sql = @"SELECT capacidadetotal FROM eventos 
               WHERE id = @EventoId FOR UPDATE";
   
   var capacidade = await conn.ExecuteScalarAsync<int>(sql, 
       new { EventoId }, transaction);
   
   // Contagem agora é atômica — nenhuma outra transação pode inserir
   var count = await conn.ExecuteScalarAsync<int>(
       "SELECT COUNT(*) FROM reservas WHERE eventoid = @EventoId",
       new { EventoId }, transaction);
   
   if (count >= capacidade)
       throw new InvalidOperationException("Esgotado");
   
   // INSERT e commit são atômicos
   await conn.ExecuteAsync("INSERT INTO reservas...", dto, transaction);
   transaction.Commit();
   ```

3. **Validação em Múltiplas Camadas**
   - **Camada DTO**: Validação de formato com FluentValidation
   - **Camada Service**: Validação de regras de negócio (8 validações)
   - **Camada Banco**: Constraints UNIQUE e CHECK

4. **Rate Limiting por IP**
   ```csharp
   builder.Services.AddRateLimiter(options =>
   {
       options.AddFixedWindowLimiter("compra", opt =>
       {
           opt.Window = TimeSpan.FromMinutes(1);
           opt.PermitLimit = 10; // Máx 10 compras/minuto por IP
           opt.QueueLimit = 0;
       });
   });
   
   app.MapPost("/api/ingressos/comprar", ...)
      .RequireRateLimiting("compra");
   ```

---

## 3 Gates de Segurança

### Gate 1: Análise Estática de Código (SAST) — Pre-Commit

**Quando:** Antes de cada commit ser pushado para o repositório

**Ferramentas:**
- **Security Code Scan** (NuGet Analyzer para .NET)
- **SonarQube Community** (análise contínua)

**Verificações Obrigatórias:**
- ❌ Bloquear: Credenciais hardcoded (regex: `password\s*=\s*["\']`)
- ❌ Bloquear: Concatenação de SQL (`$"INSERT INTO {variavel}"`)
- ❌ Bloquear: Deserialização insegura sem validação de tipo
- ⚠️ Warning: Algoritmos de hash fracos (MD5, SHA1)
- ⚠️ Warning: Ausência de `[ValidateAntiForgeryToken]` em POSTs

**Critério de Passagem:**
- Zero bloqueios críticos (severity: Critical ou High)
- Máximo de 3 warnings de segurança (devem ser justificados no PR)

**Ação em Falha:**
```bash
# Pre-commit hook (.git/hooks/pre-commit)
dotnet build /p:TreatWarningsAsErrors=true /p:WarningsAsErrors=CS8019,CA3147
if [ $? -ne 0 ]; then
  echo "❌ SAST falhou. Corrija vulnerabilidades antes de commitar."
  exit 1
fi
```

---

### Gate 2: Análise de Dependências (SCA) — Pull Request

**Quando:** Ao abrir Pull Request para branch `main`

**Ferramentas:**
- **GitHub Dependabot** (alertas automáticos de CVEs)
- **OWASP Dependency-Check** (scan de bibliotecas NuGet)

**Verificações Obrigatórias:**
- ❌ Bloquear: Dependências com CVE crítico (CVSS score > 9.0)
- ❌ Bloquear: Bibliotecas deprecated sem manutenção há > 2 anos
- ⚠️ Warning: Versões outdated com CVE médio (CVSS 4.0-8.9)

**Exemplo de Vulnerabilidade Bloqueada:**
```
🚨 Dapper 2.0.30 — CVE-2024-XXXX (CVSS 9.8)
Descrição: SQL Injection via parâmetros dinâmicos não sanitizados
Ação: Atualizar para Dapper >= 2.1.72
```

**Critério de Passagem:**
- Zero dependências com CVE crítico
- Todas as bibliotecas atualizadas para versão estável mais recente (N-1)

**Ação em Falha:**
```yaml
# GitHub Actions (.github/workflows/security.yml)
- name: OWASP Dependency Check
  run: |
    dotnet restore
    dependency-check --project CodeTicket --scan . --failOnCVSS 7
```

---

### Gate 3: Testes de Segurança Dinâmicos (DAST) — Pre-Deploy

**Quando:** Antes de deploy em produção (após deploy em staging)

**Ferramentas:**
- **OWASP ZAP** (Zed Attack Proxy) em modo automatizado
- **Postman Security Tests** (injeção de payloads maliciosos)

**Verificações Obrigatórias:**
- ❌ Bloquear: SQL Injection detectado em qualquer endpoint
- ❌ Bloquear: XSS refletido ou stored
- ❌ Bloquear: Exposição de stack traces em erro 500
- ❌ Bloquear: Secrets vazados em response headers
- ⚠️ Warning: Ausência de HTTPS Strict Transport Security (HSTS)
- ⚠️ Warning: Cookies sem flag `HttpOnly` e `Secure`

**Cenários de Teste:**
```bash
# Script de smoke test de segurança
curl -X POST https://staging.codeticket.com/api/ingressos/comprar \
  -H "Content-Type: application/json" \
  -d '{"usuarioCpf":"12345'\'' OR 1=1--","eventoId":1}' \
  | grep -q "inválido" && echo "✅ SQL Injection bloqueado" || exit 1

curl -X POST https://staging.codeticket.com/api/ingressos/comprar \
  -H "Content-Type: application/json" \
  -d '{"usuarioCpf":"<script>alert(1)</script>","eventoId":1}' \
  | grep -qv "<script>" && echo "✅ XSS sanitizado" || exit 1
```

**Critério de Passagem:**
- Zero vulnerabilidades críticas (OWASP Top 10)
- Score OWASP ZAP Risk Level: <= Medium

**Ação em Falha:**
```bash
# Pipeline de deploy (railway.yml)
echo "🔍 Executando DAST com OWASP ZAP..."
docker run -t owasp/zap2docker-stable zap-baseline.py \
  -t https://staging.codeticket.com \
  -r zap-report.html \
  -c zap-config.yaml

if grep -q "FAIL-NEW: 0" zap-report.html; then
  echo "✅ DAST passou"
else
  echo "❌ DAST falhou. Deploy bloqueado."
  exit 1
fi
```

---

## Resumo da Estratégia de Defesa em Profundidade

```
┌─────────────────────────────────────────────────────────┐
│  Gate 1: SAST (Pre-Commit)                              │
│  → Bloqueia código inseguro antes de entrar no repo     │
├─────────────────────────────────────────────────────────┤
│  Gate 2: SCA (Pull Request)                             │
│  → Bloqueia bibliotecas vulneráveis antes de mergear    │
├─────────────────────────────────────────────────────────┤
│  Gate 3: DAST (Pre-Deploy)                              │
│  → Bloqueia vulnerabilidades em runtime antes de produção│
└─────────────────────────────────────────────────────────┘
```

**Falhas Detectadas em Cada Camada:**
- Gate 1: Hardcoded password, SQL string concatenation
- Gate 2: Dapper 2.0.30 com CVE crítico
- Gate 3: Endpoint retornando stack trace completo em erro 500
