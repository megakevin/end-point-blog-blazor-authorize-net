namespace BlazorAuthorizeNet.WebApi.Models;

public record PaymentPayload(
    string PaymentMethodNonceValue,
    string PaymentMethodNonceDescriptor
);
