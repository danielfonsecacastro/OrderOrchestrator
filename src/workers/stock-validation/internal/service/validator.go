package service

import (
	"context"
	"encoding/json"
	"fmt"
	"time"

	"github.com/danielfonsecacastro/OrderOrchestrator/workers/stock-validation/internal/broker"
	"github.com/danielfonsecacastro/OrderOrchestrator/workers/stock-validation/internal/events"
	"github.com/danielfonsecacastro/OrderOrchestrator/workers/stock-validation/internal/repository"
)

type Validator struct {
	Repo   *repository.StockRepo
	Broker *broker.Rabbit
}

func NewValidator(repo *repository.StockRepo, broker *broker.Rabbit) *Validator {
	return &Validator{Repo: repo, Broker: broker}
}

func (v *Validator) HandleMessage(body []byte) error {
	var evt events.OrderCreated
	if err := json.Unmarshal(body, &evt); err != nil {
		return err
	}

	has, sku, err := v.Repo.HasStock(context.Background(), evt.Items)
	if err != nil {
		return err
	}

	if has {
		return v.Broker.Publish("OrderValidated", events.OrderValidated{
			OrderId:     evt.OrderId,
			ValidatedAt: time.Now().UTC(),
		})
	}

	return v.Broker.Publish("OrderRejected", events.OrderRejected{
		OrderId:      evt.OrderId,
		RejectedAt:   time.Now().UTC(),
		RejectReason: fmt.Sprintf("SKU %s sem estoque", sku),
	})
}
