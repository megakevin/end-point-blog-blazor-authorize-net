namespace BlazorAuthorizeNet.WebApi.Models;

public record Order(
    decimal Total,
    IList<OrderItem> Items,
    Address BillingAddress,
    string PaymentMethodNonceValue,
    string PaymentMethodNonceDescriptor
);
