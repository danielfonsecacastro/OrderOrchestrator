using OrderOrchestrator.Domain.Interfaces;
using OrderOrchestrator.Infrastructure.Configurations;
using OrderOrchestrator.Infrastructure.MessageBus;
using Prometheus;
using RabbitMQ.Client.Exceptions;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.Network;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddScoped<IMessageBus, RabbitMqPublisher>();

var logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.TCPSink("tcp://logstash:5000", new JsonFormatter())
    .CreateLogger();

builder.Host.UseSerilog(logger);


var app = builder.Build();
app.MapMetrics();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/orders", async (Order request, IMessageBus messageBus, ILogger<Program> logger, HttpContext httpContext) =>
{
    string? correlationId = null;
    if (httpContext.Request.Headers.TryGetValue("X-Correlation-Id", out var values))
        correlationId = values;

    using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId ?? Guid.NewGuid().ToString() }))
    {
        try
        {
            logger.LogInformation("Received new order: {@Order}", request);
            await messageBus.Publish("order_queue", System.Text.Json.JsonSerializer.Serialize(new
            {
                CorrelationId = correlationId,
                Payload = request
            }));
            logger.LogInformation("Order published successfully to RabbitMQ.");


            return Results.Accepted();
        }
        catch (BrokerUnreachableException ex)
        {
            logger.LogError(ex, "RabbitMQ broker unreachable while publishing order: {@Order}", request);
            return Results.Problem(
                title: "Error publishing message",
                detail: ex.Message,
                statusCode: StatusCodes.Status502BadGateway);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while publishing order: {@Order}", request);
            return Results.Problem(
                title: "Unexpected error publishing message",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
})
.WithParameterValidation()
.Accepts<Order>("application/json")
.ProducesValidationProblem();

app.Run();
public partial class Program { }
internal record Address(
        [property: Required(ErrorMessage = "Street is required.")]
        string Street,

        [property: Required(ErrorMessage = "City is required.")]
        string City,

        [property: Required(ErrorMessage = "State is required.")]
        string State,

        [property: Required(ErrorMessage = "ZipCode is required.")]
        [property: RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "Invalid ZipCode format.")]
        string ZipCode,

        [property: Required(ErrorMessage = "Country is required.")]
        string Country
    );

internal record Customer(
    [property: Required(ErrorMessage = "CustomerId is required.")]
        string CustomerId,

    [property: Required(ErrorMessage = "Name is required.")]
        string Name,

    [property: Required(ErrorMessage = "Email is required.")]
        [property: EmailAddress(ErrorMessage = "Invalid email format.")]
        string Email,

    [property: Required(ErrorMessage = "BillingAddress is required.")]
        Address BillingAddress,

    [property: Required(ErrorMessage = "ShippingAddress is required.")]
        Address ShippingAddress
);

[JsonConverter(typeof(JsonStringEnumConverter))]
internal enum PaymentMethodType
{
    CreditCard,
    DebitCard,
    PayPal,
    BankTransfer,
    Cash
}

internal record PaymentDetails(
    [property: Required(ErrorMessage = "Method is required.")]
        PaymentMethodType? Method,

    [property: Required(ErrorMessage = "TransactionId is required.")]
        string TransactionId,

    [property: Required(ErrorMessage = "PaidAt is required.")]
        DateTime PaidAt
);

internal record OrderItem(
    [property: Required(ErrorMessage = "ProductId is required.")]
        string ProductId,

    [property: Required(ErrorMessage = "ProductName is required.")]
        string ProductName,

    [property: Range(0.01, double.MaxValue, ErrorMessage = "UnitPrice must be greater than zero.")]
        decimal UnitPrice,

    [property: Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        int Quantity
);

internal record Order(
    [property: Required(ErrorMessage = "OrderId is required.")]
        string OrderId,

    [property: Required(ErrorMessage = "Customer is required.")]
        Customer Customer,

    [property: Required(ErrorMessage = "Items are required.")]
        [property: MinLength(1, ErrorMessage = "At least one OrderItem is required.")]
        IReadOnlyList<OrderItem> Items,

    [property: Required(ErrorMessage = "ShippingAddress is required.")]
        Address ShippingAddress,

    [property: Required(ErrorMessage = "BillingAddress is required.")]
        Address BillingAddress,

    [property: Required(ErrorMessage = "Payment is required.")]
        PaymentDetails Payment,

    [property: Required(ErrorMessage = "OrderDate is required.")]
        DateTime OrderDate
)
{
    public decimal TotalAmount => Items.Sum(item => item.UnitPrice * item.Quantity);
}
