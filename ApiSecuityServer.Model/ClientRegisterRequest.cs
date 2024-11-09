namespace ApiSecuityServer.Model;

public sealed record ClientRegisterRequest(
    string Serial,
    string OSName,
    string OSVersion,
    string? Product,
    string? Vendor,
    string? Processor,
    string UUID,
    string? Guid,
    long Memory);

