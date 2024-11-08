namespace ApiSecuityServer.Model;

/// <summary>
/// 
/// </summary>
/// <param name="ClientId">客户端id'</param>
/// <param name="ClientName">客户端名称</param>
/// <param name="CpuName">cpu名称</param>
/// <param name="Platform">平台类型</param>
/// <param name="ProductId">产品id</param>
/// <param name="OsVersion">系统版本</param>
public sealed record ClientRegisterRequest(string ClientId, string ClientName, string CpuName, int Platform, string ProductId, string OsVersion);

