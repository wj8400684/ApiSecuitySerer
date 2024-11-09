using ApiSecuityServer.Data.Entity;
using ApiSecuityServer.Hub.Application.Abstractions.Messaging;
using ApiSecuityServer.Model;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

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

internal sealed class ClientRegisterCommandHandler(IUnitOfWork unitOfWork, ILogger<ClientRegisterCommandHandler> logger)
    : ICommandHandler<ClientRegisterCommand>
{
    public async Task<ApiResponse> Handle(ClientRegisterCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<ClientEntity>();

        var entity = new ClientEntity
        {
            Id = request.ClientId,
            Mac = "",
            CpuName = "",
            SystemCaption = "Client Register",
            SystemName = "Client Register",
            SystemVersion = "1.0",
            SystemUser = "System",
            SerialNumber = "",
            IpAddress = "",
            DiskId = "",
        };

        try
        {
            repository.Add(entity);
            await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        }
        catch (DbUpdateException) //已经注册
        {
            logger.LogInformation("已经注册这个id：{0}", request.ClientId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "注册发生异常");
            return ApiResponse.Error("注册失败，请重试");
        }

        return ApiResponse.Success();
    }
}