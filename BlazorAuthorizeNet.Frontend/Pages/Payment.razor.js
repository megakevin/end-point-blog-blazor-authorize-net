// Sets up the page for interaction with Accept.js.
export function initializeAuthNetAcceptJs(dotNet, authNetEnvironment, authNetLoginId, authNetClientKey) {
    loadScript(authNetEnvironment);
    setUpSubmit(authNetLoginId, authNetClientKey, dotNet);
}

// Loads the Accept.js frontend library.
function loadScript(authNetEnvironment) {
    // Accept.js offers two different versions of the library, one for production and one for testing (i.e. sandbox)
    // So, we need to load one or the other depending on the given "authNetEnvironment".
    const acceptJsUrl = authNetEnvironment == "Production" ?
        "https://js.authorize.net/v1/Accept.js" :
        "https://jstest.authorize.net/v1/Accept.js";

    console.log("Loading Authorize.Net Accept.js from: ", acceptJsUrl);

    // Manually create a <script> element and attach it to the current DOM.
    var script = document.createElement('script');
    script.src = acceptJsUrl;
    document.body.appendChild(script);
}

// Wires up the onclick event handler that submits the credit card information to Authorize.NET via Accept.js, captures
// the returned tokenized card, and sends it to the Blazor component.
function setUpSubmit(authNetLoginId, authNetClientKey, dotNet) {
    const submitButton = getSubmitButton();
    submitButton.addEventListener("click", () => doSubmit(authNetLoginId, authNetClientKey, dotNet));
}

function doSubmit(authNetLoginId, authNetClientKey, dotNet) {
    disableSubmitButton();
    clearErrors();

    // Send the credit card details to Authorize.NET, and...
    sendPaymentDataToAuthNet(
        authNetLoginId,
        authNetClientKey,
        // ...when successful, use the DotNetObjectReference given by Blazor to call the SubmitOrder on the
        // Payment.razor component. Sending it the tokenized credit card.
        response => {
            dotNet.invokeMethod(
                "SubmitOrder",
                response.opaqueData.dataValue,
                response.opaqueData.dataDescriptor
            );
        }
    );
}

// This function constructs the payload that Accept.js expects containing the credit card information and sends it.
function sendPaymentDataToAuthNet(authNetLoginId, authNetClientKey, onDone) {
    var authData = {
        clientKey: authNetClientKey,
        apiLoginID: authNetLoginId
    };

    var cardData = {
        cardNumber: document.getElementById("cardNumber").value?.replace(/\s+/g, ''),
        month: document.getElementById("expMonth").value,
        year: document.getElementById("expYear").value,
        cardCode: document.getElementById("cardCode").value
    };

    // In addition to credit cards, Accept.js also supports charging a bank account. This commented out code
    // demonstrates how to do that.
    /* var bankData = {
        accountNumber: document.getElementById('accountNumber').value,
        routingNumber: document.getElementById('routingNumber').value,
        nameOnAccount: document.getElementById('nameOnAccount').value,
        accountType: document.getElementById('accountType').value
    }; */

    var data = {
        authData,
        cardData
        /* bankData: */
    };

    console.log("Request to Authorize.Net: ", data);
    Accept.dispatchData(data, response => handleAuthNetResponse(response, onDone));
}

function handleAuthNetResponse(response, onDone) {
    console.log("Response from Authorize.Net: ", response);
    if (response.messages.resultCode === "Error") {
        console.log("Error from Authorize.Net: ", response);

        displayErrors(response.messages.message);
    } else {
        onDone(response);
    }

    enableSubmitButton();
}

// This function populates the "AuthNetErrors" <ul> with items containing descriptions of the errors returned by
// Auhtorize.NET.
function displayErrors(errors) {
    const errorList = document.getElementById("AuthNetErrors");

    errors.forEach(error => {
        const li = document.createElement("li");
        li.textContent = getErrorMessage(error.code);
        li.className = "validation-message";
        errorList.appendChild(li);
    });
}

// These are some of the most common errors that we should expect from Authorize.NET.
// Learn more at https://developer.authorize.net/api/reference/features/acceptjs.html#Appendix_Error_Codes
function getErrorMessage(errorCode) {
    const map = {
        "E_WC_04": "Please provide card number, expiration month, year and CVV.",
        "E_WC_05": "Please provide valid card number.",
        "E_WC_06": "Please provide valid expiration month.",
        "E_WC_07": "Please provide valid expiration year.",
        "E_WC_08": "Please provide a future expiration date.",
        "E_WC_15": "Please provide valid CVV.",
        "E_WC_20": "Please provide valid card number."
    };

    return map[errorCode] || "We couldn't process your card at this time. Please try again.";
}

function getSubmitButton() {
    return document.getElementById("SubmitOrderButton");
}

function enableSubmitButton() {
    const submitButton = getSubmitButton();
    submitButton.disabled = false;
}

function disableSubmitButton() {
    const submitButton = getSubmitButton();
    submitButton.disabled = true;
}

function clearErrors() {
    const errorList = document.getElementById("AuthNetErrors");
    errorList.innerHTML = "";
}
