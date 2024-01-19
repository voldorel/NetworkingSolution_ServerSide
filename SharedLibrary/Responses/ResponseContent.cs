namespace SharedLibrary.Responses;

public enum ResponseStatus
{
    Success=0,
    Fail=1,
}
public class ResponseContent<T>
{
    public int Status { get; set; }
    public T Value { get; set; }

    public ResponseContent(ResponseStatus responseStatus,T value)
    {
        Status = (int)responseStatus;
        this.Value = value;
    }
}