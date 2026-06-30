# Análise de Arquitetura

## Parte 1: Identificação de Padrões Arquiteturais

### Cenário 1: Sistema de Streaming de Vídeo

**Padrão Provável:** Arquitetura de Microsserviços

**Trade-off:**
- **Positivo:** Permite escalar independentemente cada serviço (autenticação, catálogo, streaming, recomendação), otimizando recursos e custos para milhões de usuários simultâneos.
- **Negativo:** Aumenta a complexidade operacional com necessidade de orquestração de containers, gerenciamento de múltiplos deploys e monitoramento distribuído.

### Cenário 2: Sistema Bancário Tradicional

**Padrão Provável:** Arquitetura Monolítica em Camadas

**Trade-off:**
- **Positivo:** Garante consistência transacional ACID forte através de transações locais, essencial para operações financeiras críticas sem riscos de inconsistência.
- **Negativo:** Dificuldade para escalar horizontalmente apenas as funcionalidades mais demandadas, exigindo escalar toda a aplicação mesmo que apenas um módulo esteja sob alta carga.

### Cenário 3: Plataforma de E-commerce de Grande Porte

**Padrão Provável:** Event-Driven Architecture (EDA)

**Trade-off:**
- **Positivo:** Desacopla componentes através de eventos assíncronos, permitindo que pagamentos, estoque, notificações e entregas operem independentemente com alta resiliência.
- **Negativo:** Debugging e rastreamento de fluxos complexos torna-se desafiador, pois uma única operação de compra pode gerar dezenas de eventos em múltiplos sistemas.

---

## Parte 2: Análise de Violações Arquiteturais

### Código Fornecido para Análise

```csharp
public class PedidoController : ControllerBase
{
    [HttpPost]
    public IActionResult CriarPedido(PedidoDto dto)
    {
        var connectionString = "Server=prod;Database=vendas;User=sa;Password=123";
        using var conn = new SqlConnection(connectionString);
        conn.Open();
        
        var cmd = new SqlCommand($"SELECT * FROM Clientes WHERE Email = '{dto.Email}'", conn);
        var reader = cmd.ExecuteReader();
        
        if (!reader.HasRows)
            return BadRequest("Cliente não existe");
        
        reader.Close();
        
        var sql = $"INSERT INTO Pedidos VALUES ('{dto.ClienteId}', {dto.Valor})";
        new SqlCommand(sql, conn).ExecuteNonQuery();
        
        var smtp = new SmtpClient("smtp.gmail.com");
        smtp.Send("vendas@loja.com", dto.Email, "Pedido Criado", "Seu pedido foi registrado");
        
        return Ok("Pedido criado");
    }
}
```

### Violações Identificadas

#### Violação 1: Credenciais Hardcoded

**Problema:** Connection string com credenciais em texto plano no código-fonte.

**Evidência:** Linha com `Password=123` diretamente na variável `connectionString`.

**Impacto:** Risco crítico de segurança. Credenciais expostas em repositório Git podem ser exploradas por invasores, comprometendo toda a base de dados de produção.

**Ação Recomendada:** Mover configurações sensíveis para variáveis de ambiente ou Azure Key Vault, utilizando `IConfiguration` para injetar a connection string de forma segura.

#### Violação 2: SQL Injection

**Problema:** Concatenação direta de entrada do usuário em comandos SQL sem parametrização.

**Evidência:** `$"SELECT * FROM Clientes WHERE Email = '{dto.Email}'"` e `$"INSERT INTO Pedidos VALUES ('{dto.ClienteId}', {dto.Valor})"`.

**Impacto:** Vulnerabilidade crítica permitindo execução arbitrária de SQL. Atacante pode inserir `'; DROP TABLE Pedidos; --` para destruir dados.

**Ação Recomendada:** Substituir por queries parametrizadas usando `@Email` e `@ClienteId`, ou adotar Dapper/Entity Framework que protegem automaticamente contra injeção.

#### Violação 3: Violação de Single Responsibility Principle

**Problema:** Controller realizando acesso direto ao banco, validação de negócio e envio de e-mail simultaneamente.

**Evidência:** Instanciação de `SqlConnection`, `SqlCommand` e `SmtpClient` dentro do método do controller.

**Impacto:** Código impossível de testar unitariamente, alta complexidade ciclomática, violação de Separation of Concerns. Mudança em regra de negócio ou infraestrutura afeta mesma classe.

**Ação Recomendada:** Extrair lógica para camada de serviço (`PedidoService`) e repositório (`IPedidoRepository`). Controller deve apenas orquestrar chamadas.

#### Violação 4: Gerenciamento Manual de Recursos

**Problema:** Abertura manual de conexão sem garantia de fechamento em caso de exceção.

**Evidência:** `conn.Open()` seguido de múltiplas operações sem `try-finally` ou `using` adequado em todos os comandos.

**Impacto:** Connection leak em cenários de falha pode esgotar pool de conexões do banco, causando indisponibilidade do sistema sob carga.

**Ação Recomendada:** Utilizar padrão Repository com injeção de dependência de `DbConnectionFactory`, garantindo que frameworks gerenciem lifecycle das conexões.

#### Violação 5: Acoplamento com Infraestrutura SMTP

**Problema:** Instanciação direta de `SmtpClient` dentro de lógica de negócio crítica.

**Evidência:** `new SmtpClient("smtp.gmail.com")` hardcoded no fluxo de criação de pedido.

**Impacto:** Falha no envio de e-mail pode bloquear criação do pedido. Sistema fica acoplado ao provedor Gmail. Testes automatizados disparam e-mails reais.

**Ação Recomendada:** Criar interface `IEmailService`, implementar injeção de dependência e processar envio de forma assíncrona via fila (RabbitMQ/AWS SQS) para desacoplar operação crítica de I/O externo.
