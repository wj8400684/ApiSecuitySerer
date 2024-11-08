namespace ApiSecuityServer.Model;

public class ApiResponse
{
    public bool SuccessFul { get; set; }

    public int? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }

    public static ApiResponse Success() => new ApiResponse { SuccessFul = true };

    public static ApiResponse Error(string errorMessage, int? errorCode = default) =>
        new() { ErrorMessage = errorMessage, ErrorCode = errorCode };

    public static ApiResponse<TResultModel> Fail<TResultModel>(string errorMessage) where TResultModel : class =>
        new() { SuccessFul = false, ErrorMessage = errorMessage };
}

public sealed class ApiResponse<TResultModel> where TResultModel : class
{
    public bool SuccessFul { get; set; }

    public int? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }

    public TResultModel? Content { get; set; }

    public ApiResponse()
    {
    }

    public ApiResponse(TResultModel value)
    {
        Content = value;
    }

    /// <summary>
    /// 隐式将T转化为ResponseResult<T>
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator ApiResponse<TResultModel>(TResultModel value)
    {
        return new ApiResponse<TResultModel>(value) { SuccessFul = true };
    }
}