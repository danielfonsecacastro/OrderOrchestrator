# OrderOrchestrator

Showcase de backend **event-driven** com arquitetura **agnóstica a linguagens**, com implementação inicial em **C#**, linguagem na qual atuo há mais de 10 anos, e possibilidade de usar Go, em que trabalho há dois anos, além de **Python** e outras tecnologias.

**Infraestrutura de microsserviços:** RabbitMQ para orquestração de eventos, MongoDB como banco NoSQL flexível, Docker para empacotar cada serviço em containers isolados e Kubernetes (K8s) para orquestração, deploy e escalabilidade automatizada.

**Observabilidade completa:** OpenTelemetry, Prometheus, Grafana e Elastic Stack (Elasticsearch, Logstash & Kibana)

📺 Assista à introdução:\
[![Watch the video](https://img.youtube.com/vi/sPPxSDkEOEE/hqdefault.jpg)](https://youtu.be/sPPxSDkEOEE)

## Como rodar o projeto?

Antes de tudo, tenha instalado em sua máquina:

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)  
- [Docker Compose (Opcional)](https://docs.docker.com/compose/install/)

**1. Clone o repositório**
   ```bash
   git clone https://github.com/danielfonsecacastro/OrderOrchestrator.git
   cd OrderOrchestrator
   ```
**2. Suba todos os serviços**
```bash
docker-compose up --build -d
```
- `--build` força a reconstrução das imagens
- `-d` opcional executa em modo “detached” (em background)

**3. Acompanhe os logs em tempo real**

```bash
docker-compose logs -f
```
**4. Para parar e remover containers**
```bash
docker-compose down
```
---

## URLs dos Serviços

Acesse os serviços nos seguintes endereços após o ambiente estar de pé:

| Serviço             | URL                                             | Usuário / Senha        |
|---------------------|-------------------------------------------------|------------------------|
| **Order API**       | [http://localhost:8080](http://localhost:8080)  |                        |
| **RabbitMQ**        | [http://localhost:15672](http://localhost:15672)| guest / guest          |
| **Prometheus**      | [http://localhost:9090](http://localhost:9090)  |                        |
| **Grafana**         | [http://localhost:3000](http://localhost:3000)  | admin / admin          |
| **Kibana**          | [http://localhost:5601](http://localhost:5601)  |                        |

---

## Observabilidade
- **Grafana** já está pré-configurado em `grafana/provisioning` para provisionar o datasource do Prometheus e os dashboards necessários.  
- **Prometheus** coleta métricas dos serviços via OpenTelemetry.  
- **Elastic Stack** (Elasticsearch, Logstash & Kibana) processa e visualiza logs estruturados:  
  - Logs estruturados enviados pelo Serilog chegam ao Logstash  
  - Logstash indexa no Elasticsearch  
  - Kibana exibe painéis de análise de logs  
- **Vídeo de Observabilidade**: veja como tudo está integrado ▶️ 
[![Watch the video](https://img.youtube.com/vi/R0KGun1jSvo/hqdefault.jpg)](https://youtu.be/R0KGun1jSvo)


## Estrutura de Pastas
```md
OrderOrchestrator
├── .gitignore
├── README.md
├── docs/       # Documentação do projeto
├── elk/        # Arquivos de config para o Elastic Stack
├── grafana/    # Arquivos para provisionar data sources e gráficos para o Grafana.
├── mongo-init/ # Seed para o MongoDB
├── prometheus/ # Definições dos sources de métricas para o Prometheus 
└── src/
    ├── docker-compose.yml    # Compose para orquestrar todos os serviços em dev local
    ├── order-api/            # Projeto ASP.NET Core Web API: expõe endpoints REST e publica eventos
    ├── workers/
        ├── stock-validation/ # Worker que consome OrderCreated e valida estoque
        ├── billing/          # Worker que consome OrderValidated e gera faturas (InvoiceIssued)
        ├── notification/     # Worker que consome InvoiceIssued e envia notificações ao cliente

````
## Contribuições

Sinta-se à vontade para abrir issues, pull requests ou sugerir melhorias!
