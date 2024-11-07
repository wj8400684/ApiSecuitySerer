using ApiSecuityServer.Dtos;
using MediatR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace ApiSecuityServer.Commands;

internal readonly record struct FileUploadCommand(
    HttpContext HttpContext,
    string FileName,
    int PartNumber,
    int Chunks,
    int Start,
    int End,
    int Total) : IRequest<ApiResponse<FileUpdateResultModel>>;

internal sealed class FileUploadCommandHandler(FileManger fileManger, ILogger<FileUploadCommandHandler> logger)
    : IRequestHandler<FileUploadCommand, ApiResponse<FileUpdateResultModel>>
{
    public async Task<ApiResponse<FileUpdateResultModel>> Handle(FileUploadCommand request,
        CancellationToken cancellationToken)
    {
        if (!MediaTypeHeaderValue.TryParse(request.HttpContext.Request.ContentType, out var mediaType))
            return ApiResponse.Fail<FileUpdateResultModel>(
                "ContentType不正确");

        if (string.IsNullOrWhiteSpace(mediaType.Boundary.Value))
            return ApiResponse.Fail<FileUpdateResultModel>(
                "ContentType不正确");

        var boundary = GetBoundary(request.HttpContext);

        if (string.IsNullOrWhiteSpace(boundary))
            return ApiResponse.Fail<FileUpdateResultModel>(
                "ContentType不正确");

        var reader = new MultipartReader(boundary, request.HttpContext.Request.Body);

        while (true)
        {
            var section = await reader.ReadNextSectionAsync(cancellationToken);

            if (section == null)
                break;
            
            
            section.Body.CopyToAsync()
        }

        return new FileUpdateResultModel("");
    }

    private static string? GetBoundary(HttpContext httpContent)
    {
        var mediaTypeHeaderContentType = MediaTypeHeaderValue.Parse(httpContent.Request.ContentType);

        return HeaderUtilities.RemoveQuotes(mediaTypeHeaderContentType.Boundary).Value;
    }
}