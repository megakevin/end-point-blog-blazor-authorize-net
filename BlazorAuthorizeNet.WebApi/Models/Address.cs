namespace BlazorAuthorizeNet.WebApi.Models;

public record Address(
    string FirstName,
    string LastName,
    string StreetOne,
    string StreetTwo,
    string City,
    string ZipCode,
    string CountryCode,
    string StateCode
);
