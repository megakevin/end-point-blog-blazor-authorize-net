using BlazorAuthorizeNet.WebApi.Models;

namespace BlazorAuthorizeNet.WebApi.Payments;

public interface IPaymentGateway
{
    PaymentTransactionResult CreatePaymentTransaction(Order order);
}
