using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using BlazorAuthorizeNet.WebApi.Models;

namespace BlazorAuthorizeNet.WebApi.Payments;

// Calls the Authorize.NET "Create an Accept Payment Transaction" API endpoint.
// https://developer.authorize.net/api/reference/index.html#accept-suite-create-an-accept-payment-transaction
internal static class AuthorizeNetCreateTransaction
{
    // This method accepts an Order object, which is nothing more than a DTO that contains all the information needed
    // to piece together a request payload for Authorize.NET's "Create an Accept Payment Transaction" API endpoint.
    // It constructs a new object that represents such payload (i.e. the createTransactionRequest), submits the request,
    // and handles the respose.
    //
    // It returns a PaymentTransactionResult which is, again, a DTO that captures important information from the
    // response from Authorize.NET so that the code and issued the request can act according to the response.
    internal static PaymentTransactionResult Run(Order order)
    {
        // 1. Construct the request payload.
        var request = new createTransactionRequest
        {
            transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),

                amount = order.Total,
                payment = BuildPayment(order),
                billTo = BuildBillingAddress(order),
                lineItems = BuildLineItems(order)
            }
        };

        // 2. Send the request.
        var controller = new createTransactionController(request);
        controller.Execute();

        var response = controller.GetApiResponse();

        // 3. Process the response.
        return BuildResult(response);
    }

    // As we've mentioned before, the way we're going to process payments is using Authorize.NET's Accept.js client side
    // library to tokenize a credit card. This is the method that constructs the part of the request that instructs
    // Authorize.NET to process the payment in that fashion. It uses an "opaqueDataType" which contains the "Nonce
    // descriptor" and "Nonce value" fields. These fields represent the tokenized credit card sent to the backend by the
    // frontend.
    private static paymentType BuildPayment(Order order) =>
        new()
        {
            Item = new opaqueDataType
            {
                dataDescriptor = order.PaymentMethodNonceDescriptor,
                dataValue = order.PaymentMethodNonceValue
            }
        };

    private static customerAddressType BuildBillingAddress(Order order) =>
        new()
        {
            firstName = order.BillingAddress.FirstName,
            lastName = order.BillingAddress.LastName,
            address = order.BillingAddress.StreetOne,
            city = order.BillingAddress.City,
            zip = order.BillingAddress.ZipCode,
            state = order.BillingAddress.StateCode,
            country = order.BillingAddress.CountryCode
        };

    private static lineItemType[] BuildLineItems(Order order) =>
        order.Items.Select(item => new lineItemType
        {
            itemId = item.Id.ToString(),
            name = item.ProductName,
            quantity = item.Quantity,
            unitPrice = item.UnitPrice
        }).ToArray();

    // This method constructs a PaymentTransactionResult based on the response from Authorize.NET.
    // It inspects the response in various ways to determine whether it was successful or erroneus and extracts useful
    // information from it.
    private static PaymentTransactionResult BuildResult(createTransactionResponse? response)
    {
        if (response == null)
        {
            return PaymentTransactionResult.Failure(
                errorMessage: "No response from gateway"
            );
        }

        if (response.messages.resultCode != messageTypeEnum.Ok)
        {
            if (response.transactionResponse != null && response.transactionResponse.errors != null)
            {
                return PaymentTransactionResult.Failure(
                    response.transactionResponse.errors[0].errorCode,
                    response.transactionResponse.errors[0].errorText
                );
            }
            else
            {
                return PaymentTransactionResult.Failure(
                    response.messages.message[0].code,
                    response.messages.message[0].text
                );
            }
        }

        if (response.transactionResponse.messages == null)
        {
            if (response.transactionResponse.errors != null)
            {
                return PaymentTransactionResult.Failure(
                    response.transactionResponse.errors[0].errorCode,
                    response.transactionResponse.errors[0].errorText
                );
            }
            else
            {
                return PaymentTransactionResult.Failure(
                    errorMessage: "Unexpected format for error response from gateway"
                );
            }
        }

        return PaymentTransactionResult.Success(
            response.transactionResponse.transId,
            response.transactionResponse.responseCode,
            response.transactionResponse.messages[0].code,
            response.transactionResponse.messages[0].description,
            response.transactionResponse.authCode
        );
    }
}
