using ApiSecuityServer.Dtos;
using FastEndpoints;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace ApiSecuityServer.Endpoints;

public sealed class FileUploadChunkEndpoint(ILogger<FileUploadChunkEndpoint> logger)
    : Endpoint<FileUploadChunkRequest>
{
    public override void Configure()
    {
        AllowAnonymous();
        AllowFileUploads();
        Post("api/file/upload/chunk");
    }

    public override async Task HandleAsync(FileUploadChunkRequest req, CancellationToken ct)
    {
        var contextType = MediaTypeHeaderValue.Parse(HttpContext.Request.ContentType);

        if (contextType.Boundary.Value != "Multipart")
        {
            await SendOkAsync("Invalid content type", ct);
            return;
        }

        var boundary = GetBoundary();

        if (string.IsNullOrWhiteSpace(boundary))
        {
            await SendOkAsync("Invalid content type", ct);
            return;
        }

        var reader = new MultipartReader(boundary, HttpContext.Request.Body);

        while (true)
        {
            var section = await reader.ReadNextSectionAsync(ct);

            if (section == null)
                break;
        }
    }

    private string? GetBoundary()
    {
        var mediaTypeHeaderContentType = MediaTypeHeaderValue.Parse(HttpContext.Request.ContentType);

        return HeaderUtilities.RemoveQuotes(mediaTypeHeaderContentType.Boundary).Value;
    }
}