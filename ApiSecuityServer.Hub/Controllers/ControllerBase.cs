using Microsoft.AspNetCore.Mvc;

namespace ApiSecuityServer.Controllers;

[ApiController]
[Route("api/[Controller]")]
public abstract class ControllerBase : Controller
{
    
}