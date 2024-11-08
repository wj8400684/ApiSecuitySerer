using ApiSecuityServer.Hub.Application.Abstractions.Messaging;
using ApiSecuityServer.Model;
using FluentValidation;

namespace ApiSecuityServer.Hub.Commands.Web.Client;

internal readonly record struct ClientRegisterCommand(
    string ClientId,
    string ClientName,
    string CpuName,
    int Platform,
    string ProductId,
    string OsVersion) : ICommand;

internal sealed class ClientRegisterCommandValidation : AbstractValidator<ClientRegisterCommand>
{
    public ClientRegisterCommandValidation()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.ClientName).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.OsVersion).NotEmpty();
        RuleFor(x => x.CpuName).NotEmpty();
    }
}

internal sealed class ClientRegisterCommandHandler(ILogger<ClientRegisterCommandHandler> logger)
    : ICommandHandler<ClientRegisterCommand>
{
    public Task<ApiResponse> Handle(ClientRegisterCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}