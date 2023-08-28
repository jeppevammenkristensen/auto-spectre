// See https://aka.ms/new-console-template for more information

using System.Text;
using Autospectre.Examples.Examples;
using Spectre.Console;
var examplesSpectreFactory = new ExamplesSpectreFactory();
while (true)
{
    // Hack to ensure that Spinners work as expected
    System.Console.OutputEncoding = Encoding.UTF8;
    System.Console.InputEncoding = Encoding.UTF8;
    string error = "Angry";
    AnsiConsole.MarkupLineInterpolated($"[red]{error}[/]");
    var examples = examplesSpectreFactory.Get();
    
    await examples.SelectExample.Run();
}
