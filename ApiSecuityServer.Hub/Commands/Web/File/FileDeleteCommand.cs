using ApiSecuityServer.Model;
using MediatR;

namespace ApiSecuityServer.Hub.Commands.Web.File;

internal readonly record struct FileDeleteCommand(string FileId) : IRequest<ApiResponse>;

internal sealed class FileDeleteCommandHandler(FileManger fileManger, ILogger<FileDeleteCommandHandler> logger)
    : IRequestHandler<FileDeleteCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(FileDeleteCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("ɾ���ļ�{0}", request.FileId);

        var result = await fileManger.DeleteAsync(request.FileId);

        if (!result)
            return ApiResponse.Error("�ļ�������");

        return ApiResponse.Success();
    }
}