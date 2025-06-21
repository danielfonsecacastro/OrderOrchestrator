package repository

import (
	"context"
	"time"

	"go.mongodb.org/mongo-driver/bson"
	"go.mongodb.org/mongo-driver/mongo"

	"github.com/danielfonsecacastro/OrderOrchestrator/workers/stock-validation/internal/events"
)

type StockRepo struct {
	Coll *mongo.Collection
}

func NewStockRepo(db *mongo.Database) *StockRepo {
	return &StockRepo{Coll: db.Collection("stock")}
}

// HasStock determines whether all requested items have sufficient stock.
func (r *StockRepo) HasStock(ctx context.Context, items []events.OrderItem) (bool, string, error) {
	for _, it := range items {
		filter := bson.M{
			"sku":      it.ProductId,
			"quantity": bson.M{"$gte": it.Quantity},
		}
		ctx, cancel := context.WithTimeout(ctx, 5*time.Second)
		defer cancel()
		count, err := r.Coll.CountDocuments(ctx, filter)
		if err != nil {
			return false, "", err
		}
		if count == 0 {
			return false, it.ProductId, nil
		}
	}
	return true, "", nil
}
