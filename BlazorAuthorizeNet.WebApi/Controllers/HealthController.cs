using Microsoft.AspNetCore.Mvc;

namespace BlazorAuthorizeNet.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    // GET: Health
    [HttpGet]
    public ActionResult GetHealth() => Ok("Healthy");
}
