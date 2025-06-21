package main

import (
	"context"
	"log"
	"os"
	"os/signal"
	"syscall"

	"github.com/danielfonsecacastro/OrderOrchestrator/workers/stock-validation/internal/broker"
	"github.com/danielfonsecacastro/OrderOrchestrator/workers/stock-validation/internal/config"
	"github.com/danielfonsecacastro/OrderOrchestrator/workers/stock-validation/internal/repository"
	"github.com/danielfonsecacastro/OrderOrchestrator/workers/stock-validation/internal/service"
	"go.mongodb.org/mongo-driver/mongo"
	"go.mongodb.org/mongo-driver/mongo/options"
)

func main() {
	cfg := config.Load()

	// MongoDB
	ctx, cancel := context.WithTimeout(context.Background(), cfg.Timeout)
	defer cancel()
	client, err := mongo.Connect(ctx, options.Client().ApplyURI(cfg.MongoURI))
	if err != nil {
		log.Fatalf("erro conectar Mongo: %v", err)
	}
	repo := repository.NewStockRepo(client.Database(cfg.MongoDB))

	// RabbitMQ
	rb, err := broker.NewRabbit(cfg.RabbitURL, cfg.Exchange, cfg.QueueName, cfg.ConsumerTag)
	if err != nil {
		log.Fatalf("erro conectar RabbitMQ: %v", err)
	}

	// Service
	validator := service.NewValidator(repo, rb)

	// Consumo
	errs, err := rb.Consume(validator.HandleMessage)
	if err != nil {
		log.Fatalf("erro iniciar consumidor: %v", err)
	}
	log.Println("StockValidationWorker rodando...")

	// Graceful shutdown
	sigs := make(chan os.Signal, 1)
	signal.Notify(sigs, syscall.SIGINT, syscall.SIGTERM)
	select {
	case e := <-errs:
		log.Fatalf("erro no worker: %v", e)
	case s := <-sigs:
		log.Printf("recebido sinal %s, finalizando...", s)
	}
}
