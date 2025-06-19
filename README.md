# OrderOrchestrator
WIP

PT: Showcase de backend .NET C# event-driven com microserviços, RabbitMQ, MongoDB e Docker.\
EN: .NET C# event-driven backend showcase featuring microservices, RabbitMQ, MongoDB, and Docker.

## Como rodar o projeto

Certifique-se de ter o [Docker](https://www.docker.com/products/docker-desktop/) instalado.

Para subir todos os serviços, execute:

```sh
docker-compose up --build
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

- **Grafana** já está pré-configurado para se conectar ao Prometheus.
- **Prometheus** coleta métricas dos serviços.
- Acesse dashboards e métricas via URLs acima.

---

## Observações

- Ao encerrar, use `docker-compose down` para parar e remover os containers.
- Para persistir dados do MongoDB ou Grafana, volumes Docker estão configurados.
- O arquivo de configuração do Prometheus deve estar presente em `prometheus/prometheus.yml` conforme mostrado no `docker-compose.yml`.

---

## Contribuições

Sinta-se à vontade para abrir issues, pull requests ou sugerir melhorias!
