package broker

import (
	"encoding/json"
	"log"

	"github.com/streadway/amqp"
)

// Conexão e canal compartilhados
type Rabbit struct {
	Conn    *amqp.Connection
	Channel *amqp.Channel
	Config  struct {
		URL      string
		Exchange string
		Queue    string
		Tag      string
	}
}

func NewRabbit(url, exchange, queue, tag string) (*Rabbit, error) {
	conn, err := amqp.Dial(url)
	if err != nil {
		return nil, err
	}
	ch, err := conn.Channel()
	if err != nil {
		return nil, err
	}
	// Declara Exchange
	if err := ch.ExchangeDeclare(exchange, "direct", true, false, false, false, nil); err != nil {
		return nil, err
	}
	// Declara Fila e bind
	_, err = ch.QueueDeclare(queue, true, false, false, false, nil)
	if err != nil {
		return nil, err
	}
	err = ch.QueueBind(queue, "OrderCreated", exchange, false, nil)
	if err != nil {
		return nil, err
	}
	return &Rabbit{Conn: conn, Channel: ch, Config: struct {
		URL      string
		Exchange string
		Queue    string
		Tag      string
	}{url, exchange, queue, tag}}, nil
}

// Consumidor: callback por mensagem
func (r *Rabbit) Consume(handle func(body []byte) error) (<-chan error, error) {
	deliveries, err := r.Channel.Consume(
		r.Config.Queue,
		r.Config.Tag,
		true, false, false, false, nil,
	)
	if err != nil {
		return nil, err
	}
	errs := make(chan error)
	go func() {
		for d := range deliveries {
			if err := handle(d.Body); err != nil {
				log.Printf("[ERROR] ao processar mensagem: %v", err)
				errs <- err
			}
		}
		close(errs)
	}()
	return errs, nil
}

// Publicador de evento
func (r *Rabbit) Publish(routingKey string, msg interface{}) error {
	body, err := json.Marshal(msg)
	if err != nil {
		return err
	}
	return r.Channel.Publish(r.Config.Exchange, routingKey, false, false,
		amqp.Publishing{ContentType: "application/json", Body: body})
}
