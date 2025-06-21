using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OrderOrchestrator.Domain.Interfaces;
using System.Net;

namespace OrderOrchestrator.Api.Tests
{
    public class OrdersEndpointTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly Mock<IMessageBus> _messageBusMock;

        public OrdersEndpointTests(WebApplicationFactory<Program> factory)
        {
            _messageBusMock = new Mock<IMessageBus>();
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.Remove(services.Single(d => d.ServiceType == typeof(IMessageBus)));
                    services.AddSingleton(_messageBusMock.Object);
                });
            });
        }

        [Fact]
        public async Task ShouldReturnAccepted()
        {
            var client = _factory.CreateClient();

            var payload = new
            {
                orderId = "123e4567-e89b-12d3-a456-426614174000",
                customer = new
                {
                    customerId = "c1",
                    name = "Daniel Castro",
                    email = "daniel@example.com",
                    billingAddress = new
                    {
                        street = "123 Main St",
                        city = "São Paulo",
                        state = "SP",
                        zipCode = "01234-567",
                        country = "Brazil"
                    },
                    shippingAddress = new
                    {
                        street = "123 Main St",
                        city = "São Paulo",
                        state = "SP",
                        zipCode = "01234-567",
                        country = "Brazil"
                    }
                },
                items = new[]
                {
                    new
                    {
                        productId = "p1",
                        productName = "Produto Exemplo",
                        unitPrice = 99.90,
                        quantity = 2
                    },
                    new
                    {
                        productId = "p2",
                        productName = "Outro Produto",
                        unitPrice = 49.50,
                        quantity = 1
                    }
                },
                shippingAddress = new
                {
                    street = "123 Main St",
                    city = "São Paulo",
                    state = "SP",
                    zipCode = "01234-567",
                    country = "Brazil"
                },
                billingAddress = new
                {
                    street = "123 Main St",
                    city = "São Paulo",
                    state = "SP",
                    zipCode = "01234-567",
                    country = "Brazil"
                },
                payment = new
                {
                    method = "CreditCard",
                    transactionId = "txn_001",
                    paidAt = "2025-06-08T17:00:00Z"
                },
                orderDate = "2025-06-08T17:00:00Z"
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/orders", content);

            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        [Fact]
        public async Task ShouldReturnBadRequestWhenPayloadIsInvalid()
        {
            var client = _factory.CreateClient();

            var invalidPayload = new
            {
                orderId = "123e4567-e89b-12d3-a456-426614174000",
                customer = new
                {
                    customerId = "c1",
                    name = "Daniel Castro",
                    email = "daniel@example.com",
                    billingAddress = new
                    {
                        street = "123 Main St",
                        city = "São Paulo",
                        state = "SP",
                        zipCode = "01234-567",
                        country = "Brazil"
                    },
                    shippingAddress = new
                    {
                        street = "123 Main St",
                        city = "São Paulo",
                        state = "SP",
                        zipCode = "01234-567",
                        country = "Brazil"
                    }
                },
               
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(invalidPayload),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/orders", content);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
