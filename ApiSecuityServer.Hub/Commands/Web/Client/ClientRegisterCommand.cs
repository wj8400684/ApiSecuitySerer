using ApiSecuityServer.Data.Entity;
using ApiSecuityServer.Hub.Application.Abstractions.Messaging;
using ApiSecuityServer.Model;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ApiSecuityServer.Hub.Commands.Web.Client;

internal readonly record struct ClientRegisterCommand(
    HttpContext HttpContext,
    string Serial,
    string OSName,
    string OSVersion,
    string? Product,
    string? Vendor,
    string? Processor,
    string UUID,
    string? Guid,
    long Memory) : ICommand;

internal sealed class ClientRegisterCommandValidation : AbstractValidator<ClientRegisterCommand>
{
    public ClientRegisterCommandValidation()
    {
        RuleFor(x => x.OSVersion).NotEmpty();
        RuleFor(x => x.OSName).NotEmpty();
        RuleFor(x => x.UUID).NotEmpty();
    }
}

internal sealed class ClientRegisterCommandHandler(IUnitOfWork unitOfWork, ILogger<ClientRegisterCommandHandler> logger)
    : ICommandHandler<ClientRegisterCommand>
{
    public async Task<ApiResponse> Handle(ClientRegisterCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<ClientEntity>();
        var address = request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4()?.ToString();

        var entity = new ClientEntity
        {
            Id = request.UUID,
            Guid = request.Guid,
            Serial = request.Serial,
            OsName = request.OSName,
            OsVersion = request.OSVersion,
            Processor = request.Processor,
            Product = request.Product,
            Memory = request.Memory,
            Vendor = request.Vendor,
            IpAddress = address,
        };

        try
        {
            repository.Add(entity);
            await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        }
        catch (DbUpdateException) //已经注册
        {
            logger.LogInformation("已经注册这个id：{0}", request.UUID);
        }
        catch (Exception e)
        {
            logger.LogError(e, "注册发生异常");
            return ApiResponse.Error("注册失败，请重试");
        }

        return ApiResponse.Success();
    }
}