namespace VRPMS.Common.Exceptions;

public class BusinessException : Exception
{
    public BusinessException(
        string? message = null, int? statusCode = null)
        : base(message ?? string.Empty)
    {
        StatusCode = statusCode;
    }

    public int? StatusCode { get; }
}
