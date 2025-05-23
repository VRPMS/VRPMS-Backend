namespace VRPMS.Common.Exceptions;

public class BusinessRuleResponse
{
    public BusinessRuleResponse(BusinessException businessException)
    {
        Type = businessException.GetType().Name;
        Status = businessException.StatusCode ?? default;
        Message = businessException.Message;
    }

    public string Type { get; set; }

    public int Status { get; set; }

    public string Message { get; set; }
}
