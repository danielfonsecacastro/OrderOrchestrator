FROM golang:1.24-alpine AS build
WORKDIR /app
COPY go.mod go.sum ./
RUN go mod download

COPY . .
RUN go build -o stock-validator cmd/main.go

FROM alpine:latest
WORKDIR /app
COPY --from=build /app/stock-validator .
ENTRYPOINT ["./stock-validator"]
