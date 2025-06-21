## 1. Visão Geral  
Este repositório contém um **showcase de backend .NET C#** voltado ao estudo e prática de arquiteturas avançadas de software. O objetivo não é atender um problema de negócio real, mas sim exercitar padrões e tecnologias que se aplicam a cenários de alta escala, resiliência e evolutibilidade.

## 2. Propósito  
- **Aprender e demonstrar** como projetar sistemas event-driven e orientados a domínios.  
- **Experimentar** microsserviços, filas, NoSQL, programação assíncrona, containers e k8s, etc.  
- **Construir um portfólio** que mostre domínio de práticas de engenharia de software modernas.

## 3. Escopo  
- **Recepção de pedidos** (API REST que publica eventos).  
- **Validação de estoque** (worker que consome eventos, faz lógica de negócio e publica novos eventos).  
- **Emissão de cobrança** (worker que gera faturas, persiste em NoSQL e aciona webhook).  
- **Notificação ao cliente** (worker que envia e-mail/SMS a partir de eventos).  
- **Observabilidade** (logs estruturados, métricas, dashboard).  
- **Infraestrutura** provisionada via Terraform e orquestrada em containers Docker (local) e Kubernetes (prod).

## 4. Funcionalidades Principais  
1. **Publicação de eventos**  
   - `OrderCreated` ao criar pedido via API  
2. **Processamento assíncrono**  
   - `StockValidationWorker` (valida e publica `OrderValidated` ou `OrderRejected`)  
   - `BillingWorker` (gera `InvoiceIssued`)  
   - `NotificationWorker` (envia `NotificationSent`)  
3. **Persistência NoSQL**  
   - Armazenamento de pedidos, faturas e log de notificações em MongoDB  
4. **Webhooks**  
   - Chamada HTTP para sistemas externos após emissão de fatura  
5. **Observabilidade**  
   - Logs com Serilog + Elasticsearch (opcional)  
   - Métricas Prometheus + dashboard Grafana  
6. **Infraestrutura como Código**  
   - Módulos Terraform para provisioning de cluster, fila, banco e storage  
7. **Containerização**  
   - Dockerfiles para cada microserviço  
   - Docker Compose para ambiente de desenvolvimento

## 5. Casos de Uso & Eventos

| Caso de Uso                       | Evento Disparado    | Descrição Rápida                                            |
|-----------------------------------|---------------------|-------------------------------------------------------------|
| Criar pedido                      | `OrderCreated`      | API publica novo pedido na fila                            |
| Validar estoque                   | `OrderValidated`    | Worker consome `OrderCreated`, verifica estoque disponível  |
| Rejeitar pedido                   | `OrderRejected`     | Se sem estoque, publica evento de rejeição                  |
| Gerar fatura                      | `InvoiceIssued`     | Worker consome `OrderValidated`, cria e armazena fatura     |
| Enviar notificação ao cliente     | `NotificationSent`  | Worker consome `InvoiceIssued` e dispara e-mail/SMS         |
| Retry de mensagens falhas         | —                   | Mecanismo de retry e dead-letter para mensagens não processadas |

## 6. Tech Stack

- **Runtime & Linguagem**: .NET 9 , Go 1.24.4  
- **Mensageria**: RabbitMQ (ou Azure Service Bus)  
- **Banco NoSQL**: MongoDB  
- **Containerização**: Docker, Docker Compose, Kubernetes
- **IaC**: Terraform (módulos organizados por recurso)  
- **CI/CD**: GitHub Actions (CI build/test, CD deploy)  
- **Observabilidade**:  
  - **Logging**: Serilog (+ sink para Elasticsearch ou arquivo)  
  - **Métricas**: OpenTelemetry → Prometheus → Grafana dashboard  
- **Testes**: xUnit + Moq (unitários), WebApplicationFactory (integração)

## 7. Estrutura de Pastas
```md
src
├── API/                  # Projeto ASP.NET Core Web API: expõe endpoints REST e publica eventos
├── Workers/
│   ├── StockValidation   # Worker (BackgroundService) que consome OrderCreated e valida estoque
│   ├── Billing           # Worker que consome OrderValidated e gera faturas (InvoiceIssued)
│   ├── Notification      # Worker que consome InvoiceIssued e envia notificações ao cliente
├── Domain/               # Camada de Domínio: entidades, Value Objects e interfaces de repositório
├── Infrastructure/       # Implementações de infraestrutura:
│   ├── MongoDB           #   - MongoDB (NoSQL)
│   ├── RabbitMQ          #   - RabbitMQ (mensageria)
│   ├── WebhookPublisher
│   ├── Observability     #   - Observabilidade (OpenTelemetry)
├── tests/                # Testes unitários (xUnit + Moq) e de integração (WebApplicationFactory)
├── infra/                # IaC com Terraform: módulos para rede, mensageria, banco e compute
├── docs/                 # Documentação do projeto:
│   ├── PROJECT-SPEC.md
│   └── ARCHITECTURE.md
docker-compose.yml       # Compose para orquestrar todos os serviços em dev local
.github/workflows/       # Pipelines de CI (build, test, coverage) e CD (deploy)
README.md                # Visão geral, setup rápido e instruções de uso
```

## 8. Critérios de Sucesso  
- 🎯 **Funcionamento completo** em ambiente local via Docker Compose.  
- ✅ **Todos** os testes unitários e de integração passam.  
- 🚀 **Deploy automático** para cluster Kubernetes (ou Azure App Service).  
- 📊 **Dashboard** exibindo métricas de processamento e latência.  
- 📄 **Documentação clara**: cada componente explicado, diagramas visíveis e exemplos de payload.

## 9. Próximos Passos  
1. **Preencher** este arquivo com detalhes adicionais conforme o projeto evolui.  
2. **Desenhar** e exportar o diagrama em `docs/ARCHITECTURE.md`.  
3. **Iniciar** a modelagem de domínio em `/src/Domain`.  