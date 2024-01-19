namespace SharedLibrary.Responses;

public class RegisterResponse
{
    public Guid Token;

    public RegisterResponse(Guid token)
    {
        Token = token;
    }
}