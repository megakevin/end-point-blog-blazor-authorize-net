# Blazor + Authorize.NET

This is a Blazor WebAssembly Standalone app with a form to capture credit card information and send it to Authorize.NET using their Accept.js frontend integration library. It submits the resulting tokenized credit card to a backend API to actually effectuate the payment.

This backend API, which is also included in this repo, is a simple ASP.NET Core Web API. It defines an endpoint for submitting payment transactions to Authorize.NET, given the token obtained via Accept.js.
