using Microsoft.AspNetCore.Mvc;
using BlazorAuthorizeNet.WebApi.Models;
using BlazorAuthorizeNet.WebApi.Payments;

namespace BlazorAuthorizeNet.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentGateway _paymentGateway;

    public PaymentsController(IPaymentGateway paymentGateway)
    {
        _paymentGateway = paymentGateway;
    }

    // POST: Payments
    [HttpPost]
    public ActionResult PostPayment([FromBody] PaymentPayload payload)
    {
        // To keep things simple, this endpoint expects the values obtained from Authorize.NET's Accept.js that
        // represent the tokenized credit card, encapsulated in PaymentPayload.
        //
        // For the details of the actual "product" that was "purchased", we simulate an order by creating a hardcoded
        // object here. Of course, for real world applications, the generation of the "Order" object would be much more
        // complex.
        Order order = new(
            Total: 100.00m,
            Items:
            [
                new OrderItem(1, "Product A", 1, 50.00m),
                new OrderItem(2, "Product B", 1, 50.00m)
            ],
            BillingAddress: new Address(
                FirstName: "John",
                LastName: "Doe",
                StreetOne: "123 Main St",
                StreetTwo: "Suite A",
                City: "New York",
                ZipCode: "12345",
                StateCode: "CA",
                CountryCode: "US"
            ),
            // Importantly, we include the payment token values here.
            PaymentMethodNonceValue: payload.PaymentMethodNonceValue,
            PaymentMethodNonceDescriptor: payload.PaymentMethodNonceDescriptor
        );

        // Then we simply submit the payment via our payment gateway object and return a response based on the result
        // we get from Authorize.NET.
        var result = _paymentGateway.CreatePaymentTransaction(order);

        if (result.IsSuccess) return Ok(result);
        else return BadRequest(result);
    }
}
