namespace VRPMS.Common.Exceptions;

public class BusinessRuleResponse
{
    public BusinessRuleResponse(Exception exception)
    {
        Type = exception.GetType().Name;
        Message = exception.Message;
    }

    public string Type { get; set; }

    public int Status { get; set; } = default;

    public string Message { get; set; }
}
