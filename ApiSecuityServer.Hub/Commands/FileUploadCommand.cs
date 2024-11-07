using ApiSecuityServer.Dtos;
using ApiSecuityServer.Hubs;
using MediatR;
using Microsoft.AspNetCore.SignalR;
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

internal sealed class FileUploadCommandHandler(
    FileManger fileManger,
    IHubContext<ClientHub, IClientApi> context,
    ILogger<FileUploadCommandHandler> logger)
    : IRequestHandler<FileUploadCommand, ApiResponse<FileUpdateResultModel>>
{
    private readonly IHubClients<IClientApi> _hubClients = context.Clients;

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
        var file = new FileTransferStream("", request.FileName, request.Total, null, fileManger, reader);

        file.Start();
        file.Add();

        while (true)
        {
            var section = await reader.ReadNextSectionAsync(cancellationToken);

            if (section == null)
                break;

            await file.WriterAsync(section.Body, cancellationToken);
        }
        
        file.ChannelStream1!.Writer.Complete();

        return new FileUpdateResultModel(file.Id);
    }

    private static string? GetBoundary(HttpContext httpContent)
    {
        var mediaTypeHeaderContentType = MediaTypeHeaderValue.Parse(httpContent.Request.ContentType);

        return HeaderUtilities.RemoveQuotes(mediaTypeHeaderContentType.Boundary).Value;
    }
}