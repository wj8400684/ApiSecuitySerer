using ApiSecuityServer.Dtos;
using MediatR;

namespace ApiSecuityServer.Commands;

internal readonly record struct FileDeleteCommand(string FileId) : IRequest<ApiResponse>;

internal sealed class FileDeleteCommandHandler(FileManger fileManger, ILogger<FileDeleteCommandHandler> logger)
    : IRequestHandler<FileDeleteCommand, ApiResponse>
{
    public async Task<ApiResponse> Handle(FileDeleteCommand request, CancellationToken cancellationToken)
    {
        await fileManger.DeleteAsync(request.FileId);

        return ApiResponse.Success();
    }
}