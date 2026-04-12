namespace Vbodlaci.Web.Application.Common;

public sealed record ServiceResult(bool Succeeded, string Message)
{
    public static ServiceResult Success(string message) => new(true, message);

    public static ServiceResult Failure(string message) => new(false, message);
}
