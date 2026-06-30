# Registro de Dívida Técnica

| ID da Dívida | Descrição Técnica | Freq. Alteração | Risco | Esforço | Decisão |
|--------------|-------------------|-----------------|-------|---------|---------|
| DT-001 | Ausência de tratamento de exceções customizado. Exceções de banco retornam stack trace completo ao usuário final, expondo estrutura interna. | Baixo | Alto | Médio | Prioridade 1 (Imediato) |
| DT-002 | Falta de logging estruturado. Impossível rastrear fluxo de requisições ou identificar causa raiz de falhas em produção. | Médio | Alto | Médio | Prioridade 1 (Imediato) |
| DT-003 | Validação de CPF implementada apenas na camada de serviço. Não há validação de formato na camada de DTO, permitindo dados inválidos entrarem no sistema. | Alto | Médio | Baixo | Prioridade 2 (Próxima Sprint) |
| DT-004 | Ausência de cache para listagem de eventos. Toda requisição bate no banco mesmo para dados que mudam raramente, gerando carga desnecessária. | Médio | Médio | Médio | Prioridade 2 (Próxima Sprint) |
| DT-005 | Testes unitários não cobrem cenários de concorrência. Possível race condition quando dois usuários compram o último ingresso simultaneamente. | Baixo | Alto | Alto | Prioridade 2 (Próxima Sprint) |
| DT-006 | Nomenclatura inconsistente entre banco (snake_case) e C# (PascalCase). Configuração `MatchNamesWithUnderscores` resolve parcialmente, mas dificulta manutenção. | Baixo | Baixo | Alto | Prioridade 3 (Aceitar/Ignorar) |
| DT-007 | Falta de paginação no endpoint de listagem de eventos. Sistema pode retornar milhares de registros, causando timeout ou consumo excessivo de memória. | Médio | Médio | Baixo | Prioridade 2 (Próxima Sprint) |
| DT-008 | Ausência de índices no banco de dados. Queries como "contar reservas por evento" fazem table scan completo, degradando performance com crescimento da base. | Alto | Alto | Baixo | Prioridade 1 (Imediato) |
| DT-009 | Connection strings em appsettings.Development.json. Embora separado de produção, ainda expõe credenciais locais no repositório Git. | Baixo | Médio | Baixo | Prioridade 3 (Aceitar/Ignorar) |
| DT-010 | Falta de rate limiting nos endpoints. Sistema vulnerável a ataques de DDoS ou bots fazendo requisições em massa. | Baixo | Médio | Médio | Prioridade 3 (Aceitar/Ignorar) |

---

## Análise das Prioridades

### Prioridade 1 (Imediato)
**DT-001, DT-002, DT-008**: Representam riscos críticos de segurança e performance que podem causar incidentes em produção. Devem ser resolvidas antes do próximo deploy.

### Prioridade 2 (Próxima Sprint)
**DT-003, DT-004, DT-005, DT-007**: Melhorias importantes que aumentam qualidade e confiabilidade, mas não bloqueiam operação atual. Devem ser endereçadas no próximo ciclo de desenvolvimento.

### Prioridade 3 (Aceitar/Ignorar)
**DT-006, DT-009, DT-010**: Dívidas de baixo impacto no contexto atual (MVP acadêmico). Snake_case é convenção PostgreSQL padrão, conexões locais não são expostas publicamente, e volume de tráfego é mínimo. Custobenefício de resolução é negativo neste momento.
