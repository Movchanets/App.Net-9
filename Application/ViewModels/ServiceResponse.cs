namespace Application.ViewModels;

public class ServiceResponse
{
    public string Message { get; set; }
    public bool IsSuccess { get; set; }
    public object Payload { get; set; }
    
    public ServiceResponse(bool isSuccess, string message, object? payload = null )
    {
        Message = message;
        IsSuccess = isSuccess;
        Payload = payload;
    }
}