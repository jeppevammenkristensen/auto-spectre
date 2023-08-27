using AutoSpectre;
using Spectre.Console;

namespace Autospectre.Examples.Examples;

public class LoginExample : IExample
{
    public async Task Run()
    {
        var login = new LoginForm(new Authenticator());
        var factory = new LoginFormSpectreFactory();
        await factory.GetAsync(login);

        if (login.Authenticated)
        {
            AnsiConsole.MarkupLine("[green]Success[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Authentication failed [/]");
        }
    }
}

[AutoSpectreForm]
public class LoginForm
{
    private readonly Authenticator _authenticator;

    public LoginForm(Authenticator authenticator)
    {
        _authenticator = authenticator;
    }
    
    [TextPrompt]
    public string Username { get; set; }
    
     [TextPrompt(Secret = true)]
     public string Password { get; set; }

     [TaskStep(UseStatus = true, StatusText = "Authenticating...")]
     public async Task Authenticate(IAnsiConsole console)
     {
         var authenticated =await _authenticator.Authenticate(Username, Password);
         Authenticated = authenticated;
     }

     public bool Authenticated { get; set; }
}

public class Authenticator
{
    public async Task<bool> Authenticate(string userName, string password)
    {
        await Task.Delay(2000);
        if (userName == "speekfriend" && password == "mellon")
            return true;

        return false;
    }
}