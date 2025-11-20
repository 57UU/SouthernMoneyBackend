using Database;


public class RegisterRequest
{
    public string Name { get; set; }
    public string Password { get; set; }
}
public class LoginByPasswordRequest
{
    public string Name { get; set; }
    public string Password { get; set; }
}
public class LoginByTokenRequest
{
    public string Token { get; set; }
}