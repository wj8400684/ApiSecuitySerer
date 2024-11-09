namespace ApiSecuityServer.Model;

/// <summary>
/// 
/// </summary>
/// <param name="OSName"></param>
/// <param name="OSVersion"></param>
/// <param name="Product"></param>
/// <param name="Vendor"></param>
/// <param name="Processor"></param>
/// <param name="UUID"></param>
/// <param name="Guid"></param>
/// <param name="Memory"></param>
public sealed record ClientRegisterRequest(
    string OSName,
    string OSVersion,
    string? Product,
    string? Vendor,
    string? Processor,
    string UUID,
    string? Guid,
    long Memory);

