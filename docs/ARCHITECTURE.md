# ARCHITECTURE.md

## 1. Visão Geral da Arquitetura

Este documento apresenta o desenho de alto nível e a descrição detalhada dos componentes e fluxos do projeto **OrderOrchestrator**, um showcase .NET C# para estudar arquiteturas avançadas, event-driven e distribuídas.

---

## 2. Diagrama de Componentes


---

## 3. Componentes Principais

| Componente                 | Descrição                                                                                                             |
| -------------------------- | --------------------------------------------------------------------------------------------------------------------- |
| **Api**                    | Projeto ASP.NET Core Web API que recebe requisições REST (`POST /orders`) e publica eventos.                          |
| **StockValidation Worker** | BackgroundService consumindo `OrderCreated`, valida estoque em MongoDB e publica `OrderValidated` ou `OrderRejected`. |
| **Billing Worker**         | BackgroundService consumindo `OrderValidated`, gera documento de fatura em MongoDB e publica `InvoiceIssued`.         |
| **Notification Worker**    | BackgroundService consumindo `InvoiceIssued`, envia e-mail/SMS ao cliente via SMTP ou API externa.                    |
| **Infrastructure**         | Biblioteca compartilhada para acesso a MongoDB, RabbitMQ, WebhookPublisher e integração OpenTelemetry.                |
| **Domain**                 | Camada de domínio contendo entidades, Value Objects e interfaces de repositório.                                      |
| **Infra/Terraform**        | Módulos Terraform para provisionamento de rede, mensageria, banco NoSQL, cluster Kubernetes/App Service e Storage.    |
| **Docker Compose**         | Arquivo `docker-compose.yml` para orquestrar containers em ambiente de desenvolvimento local.                         |
| **CI/CD**                  | Workflows GitHub Actions para build, testes, cobertura, criação de imagens Docker e deploy automatizado.              |
| **Observability**          | OpenTelemetry instrumentações + Prometheus para coleta de métricas + Grafana para dashboards + Serilog para logs.     |

---

## 4. Fluxo de Eventos (Event-Driven)

1. **Criação do Pedido**

   * Cliente faz `POST /orders` na API.
   * API publica evento `OrderCreated` em um **Exchange** do RabbitMQ.

2. **Validação de Estoque**

   * `StockValidation Worker` consome da **fila** ligada à `OrderCreated`.
   * Verifica disponibilidade em MongoDB.

     * Se disponível: publica `OrderValidated`.
     * Se indisponível: publica `OrderRejected`.

3. **Geração de Fatura**

   * `Billing Worker` consome `OrderValidated`.
   * Cria documento de fatura (`Invoice`) e persiste em MongoDB.
   * Publica evento `InvoiceIssued`.

4. **Notificação ao Cliente**

   * `Notification Worker` consome `InvoiceIssued`.
   * Envia mensagem ao cliente (e-mail/SMS).
   * Registra log de notificação no MongoDB.

5. **Retries e Dead-Letter**

   * Configuração de retry policy no RabbitMQ (ou Service Bus).
   * Mensagens falhas vão para dead-letter queue para análise manual.

---

## 5. Infraestrutura Local (Docker Compose)

```yaml
version: '3.8'
services:
  api:
    build: ./src/Api
    ports:
      - '5000:80'
    depends_on:
      - rabbitmq
      - mongo
  stock-validation:
    build: ./src/Workers/StockValidation
    depends_on:
      - rabbitmq
      - mongo
  billing:
    build: ./src/Workers/Billing
    depends_on:
      - rabbitmq
      - mongo
  notification:
    build: ./src/Workers/Notification
    depends_on:
      - rabbitmq
  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - '5672:5672'
      - '15672:15672'
  mongo:
    image: mongo:6
    ports:
      - '27017:27017'
  prometheus:
    image: prom/prometheus
    volumes:
      - ./infra/prometheus/prometheus.yml:/etc/prometheus/prometheus.yml
    ports:
      - '9090:9090'
  grafana:
    image: grafana/grafana
    ports:
      - '3000:3000'
```

---

## 6. Infraestrutura de Produção (Kubernetes + Terraform)

* **Terraform**: código em `infra/` organizando:

  * **network**: VPC/VNet, subnets, NSG
  * **messaging**: RabbitMQ (Helm chart) ou Azure Service Bus namespace
  * **database**: MongoDB Atlas ou Azure Cosmos DB
  * **compute**: AKS cluster + Helm charts para deployment dos microserviços
  * **storage**: blobs para anexos se necessário

* **Kubernetes**:

  * Namespaces separados: `dev`, `staging`, `prod`
  * Deployments e Services para cada microserviço
  * ConfigMaps e Secrets para configurações e credenciais
  * Horizontal Pod Autoscaler (HPA) baseado em métricas do Prometheus
  * Ingress Controller com TLS e regras de roteamento

---

## 7. Observabilidade Detalhada

* **Logging**: Serilog configurado para:

  * Console (local)
  * Elasticsearch (prod) com índice por serviço
  * Formato JSON estruturado

* **Métricas**:

  * Instrumentação OTel nos projetos (`AddOpenTelemetryTracing`, `AddOpenTelemetryMetrics`)
  * Exporters para Prometheus
  * Métricas principais:

    * Contador de mensagens processadas por worker
    * Histogram de latência de handlers
    * Gauge de mensagens em fila

* **Dashboard**:

  * Exemplo de painel em `infra/grafana/` com JSON das telas:

    * *Overview*: taxa de eventos por minuto
    * *Latency*: latência média de processamento
    * *Queue Depth*: tamanho das filas RabbitMQ

---

## 8. Segurança e Configuração

* **Autenticação/Autorização**: JWT Bearer Token na API, roles e policies
* **Segredos**: armazenados em Azure Key Vault ou Kubernetes Secrets
* **TLS**: habilitado no Ingress para comunicação segura
* **Políticas de Rede**: Kubernetes NetworkPolicies para isolar comunicação entre serviços

---

## 9. Diagramas Complementares

* `docs/diagrams/order-flow.drawio` → arquivo fonte do diagrama
* `docs/diagrams/k8s-architecture.drawio` → diagrama de implantação em Kubernetes

---

> Este documento serve como guia de referência para desenvolvedores e arquitetos, detalhando cada camada, fluxo e prática de implantação do projeto OrquestraPedidos.
