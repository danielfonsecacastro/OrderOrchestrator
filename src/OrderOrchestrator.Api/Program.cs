using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapPost("/orders", (Order request) =>
{
    return Results.Accepted();
})
.WithParameterValidation()
.Accepts<Order>("application/json")
.ProducesValidationProblem();

app.Run();

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
