using ApiSecuityServer.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ApiSecuityServer.Controllers;

[ApiController]
[Route("api/file")]
public sealed class FileController : Controller
{
    [HttpPost]
    [Route("upload")]
    public ValueTask<IActionResult> UploadFileAsync([FromQuery] FileChunkRequest request,
        CancellationToken cancellationToken)
    {
        
    }
}