package events

import "time"

type OrderItem struct {
	ProductId string `json:"productId"`
	Quantity  int    `json:"quantity"`
}

type OrderCreated struct {
	OrderId    string      `json:"orderId"`
	CustomerId string      `json:"customerId"`
	Items      []OrderItem `json:"items"`
	CreatedAt  time.Time   `json:"createdAt"`
}

type OrderValidated struct {
	OrderId     string    `json:"orderId"`
	ValidatedAt time.Time `json:"validatedAt"`
}

type OrderRejected struct {
	OrderId      string    `json:"orderId"`
	RejectedAt   time.Time `json:"rejectedAt"`
	RejectReason string    `json:"reason"`
}
