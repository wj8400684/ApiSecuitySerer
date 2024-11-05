using ApiSecuityServer.Dtos;
using ApiSecuityServer.Hub;
using FastEndpoints;

namespace ApiSecuityServer.Endpoints;

public sealed class FileDeleteEndEndpoint(FileManger fileManger) : Endpoint<FileDeleteRequest, ApiResponse>
{
    public override void Configure()
    {
        AllowAnonymous();
        Get("api/file/delete");
    }

    public override async Task<ApiResponse> ExecuteAsync(FileDeleteRequest req, CancellationToken ct)
    {
        await Task.CompletedTask;

        if (string.IsNullOrWhiteSpace(req.FileId))
            return ApiResponse.Error("参数错误");

        fileManger.Delete(req.FileId);

        return ApiResponse.Success();
    }
}