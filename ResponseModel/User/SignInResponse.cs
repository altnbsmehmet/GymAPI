using System.Security.Principal;

public class SignInResponse : ResponseBase
{
    public string Token { get; set; }
    public string Role { get; set; }
}