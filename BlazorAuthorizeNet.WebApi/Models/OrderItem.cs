namespace BlazorAuthorizeNet.WebApi.Models;

public record OrderItem(
    int Id,
    string ProductName,
    int Quantity,
    decimal UnitPrice
);
