package config

import (
	"os"
	"time"
)

type Config struct {
	RabbitURL   string
	Exchange    string
	MongoURI    string
	MongoDB     string
	QueueName   string
	ConsumerTag string
	Timeout     time.Duration
}

func Load() Config {
	return Config{
		RabbitURL:   os.Getenv("RABBITMQ_URL"),
		Exchange:    os.Getenv("EXCHANGE"),
		MongoURI:    os.Getenv("MONGO_URI"),
		MongoDB:     "ordersdb",
		QueueName:   "orders.created",
		ConsumerTag: "stock-validator",
		Timeout:     10 * time.Second,
	}
}
