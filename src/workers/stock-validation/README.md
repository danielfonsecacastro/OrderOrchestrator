# StockValidationWorker

Worker em Go para validar estoque de pedidos em um sistema event-driven.  
Ele consome eventos `OrderCreated` do RabbitMQ, checa disponibilidade no MongoDB e publica `OrderValidated` ou `OrderRejected`.

---

## Pré-requisitos

- Go 1.24.4
