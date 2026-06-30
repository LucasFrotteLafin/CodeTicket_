# ADR 001: Escolha do Dapper como Micro-ORM

**Status:** Aceito

## Contexto

O sistema CodeTicket precisa realizar operações de banco de dados com alta performance e controle fino sobre queries SQL, mantendo proteção contra SQL Injection. Avaliamos três alternativas:

1. **Entity Framework Core**: ORM completo com migrations, change tracking e LINQ.
2. **Dapper**: Micro-ORM leve que mapeia resultados SQL diretamente para objetos.
3. **ADO.NET puro**: Acesso manual ao banco sem abstrações.

Os requisitos principais são:
- Queries parametrizadas obrigatórias (segurança)
- Performance em consultas simples e complexas (JOINs)
- Baixa curva de aprendizado para equipe iniciante/intermediária
- Controle explícito sobre SQL gerado

## Decisão

Adotamos o **Dapper** como micro-ORM oficial do projeto.

Justificativa:
- Garante parametrização automática via sintaxe `@Parametro`, eliminando risco de SQL Injection
- Performance próxima ao ADO.NET puro (overhead mínimo de ~5%)
- Permite escrever SQL explícito, facilitando otimizações e compreensão por desenvolvedores juniores
- Integração nativa com PostgreSQL via Npgsql
- Comunidade ativa e documentação clara

## Consequências

### Prós:
- **Segurança**: Proteção automática contra SQL Injection através de queries parametrizadas
- **Performance**: Execução ~3x mais rápida que Entity Framework em selects simples
- **Transparência**: SQL escrito manualmente facilita debugging e code review
- **Leveza**: Biblioteca minimalista (50kb) sem dependências pesadas
- **Controle**: Desenvolvedores têm controle total sobre índices e planos de execução

### Contras:
- **Ausência de Migrations**: Mudanças de schema exigem scripts SQL manuais versionados
- **Sem Change Tracking**: Atualizações requerem queries UPDATE explícitas (não há `context.SaveChanges()`)
- **Repetição de código**: Mapeamento manual entre DTOs e comandos SQL aumenta boilerplate
- **Falta de validação em tempo de design**: Erros de sintaxe SQL só são detectados em runtime
- **Curva de aprendizado SQL**: Equipe precisa dominar SQL nativo ao invés de abstrações LINQ
