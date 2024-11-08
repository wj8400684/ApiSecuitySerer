using ApiSecuityServer.Dtos;
using ApiSecuityServer.Hubs;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace ApiSecuityServer.Commands;

internal readonly record struct FileUploadCommand(
    HttpContext HttpContext,
    string ConnectionId,
    string FileName,
    int PartNumber,
    int Chunks,
    int Size,
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
        logger.LogInformation("用户: [{0}] 上传文件，名称{1} 大小{2}", request.ConnectionId, request.FileName, request.Total);

        if (!MediaTypeHeaderValue.TryParse(request.HttpContext.Request.ContentType, out var mediaType))
            return ApiResponse.Fail<FileUpdateResultModel>(
                "ContentType不正确");

        if (string.IsNullOrWhiteSpace(mediaType.Boundary.Value))
            return ApiResponse.Fail<FileUpdateResultModel>(
                "ContentType不正确");

        var boundary = mediaType.Boundary.Value!;

        if (string.IsNullOrWhiteSpace(boundary))
            return ApiResponse.Fail<FileUpdateResultModel>(
                "ContentType不正确");

        var reader = new MultipartReader(boundary, request.HttpContext.Request.Body);
        var file = new FileTransferStream(
            id: request.ConnectionId,
            fileName: request.FileName,
            fileSize: request.Total,
            bufferSize: request.Size,
            multipartReader: reader,
            partNumber: request.PartNumber,
            clientApi: _hubClients.Clients(request.ConnectionId));

        fileManger.AddFile(file);

        try
        {
            await file.PublishDownloadAsync();
        }
        catch (Exception e)
        {
            await fileManger.DeleteAsync(file.Id);
            return ApiResponse.Fail<FileUpdateResultModel>(e.Message);
        }

        try
        {
            await file.WriterAsync(cancellationToken);
        }
        catch (Exception e)
        {
            await fileManger.DeleteAsync(file.Id);
            return ApiResponse.Fail<FileUpdateResultModel>(e.Message);
        }
        finally
        {
            file.ReadComplete();
        }

        return new FileUpdateResultModel(file.Id);
    }

    private static string? GetBoundary(string contentType)
    {
        var mediaTypeHeaderContentType = MediaTypeHeaderValue.Parse(contentType);

        return HeaderUtilities.RemoveQuotes(mediaTypeHeaderContentType.Boundary).Value;
    }
}