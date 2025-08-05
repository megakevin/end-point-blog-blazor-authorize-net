using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers.Bases;
using BlazorAuthorizeNet.WebApi.Models;

namespace BlazorAuthorizeNet.WebApi.Payments;

// Provives an interface for interacting with the Authorize.NET payment processor.
public class AuthorizeNetPaymentGateway : IPaymentGateway
{
    private readonly IConfiguration _configuration;

    public AuthorizeNetPaymentGateway(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Submits a payment transaction to Authorize.NET
    public PaymentTransactionResult CreatePaymentTransaction(Order order)
    {
        PrepareConnection();
        return AuthorizeNetCreateTransaction.Run(order);
    }

    // This method sets up the AuthorizeNet.Core client library's connection to Authorize.NET's Web API.
    // The values for RunEnvironment and MerchantAuthentication are constructed based on app settings.
    private void PrepareConnection()
    {
        ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment =
            _configuration["AuthNetEnvironment"] == "Production" ?
                AuthorizeNet.Environment.PRODUCTION : AuthorizeNet.Environment.SANDBOX;

        ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new()
        {
            name = _configuration["AuthNetLoginId"],
            ItemElementName = ItemChoiceType.transactionKey,
            Item = _configuration["AuthNetTransactionKey"],
        };
    }
}
