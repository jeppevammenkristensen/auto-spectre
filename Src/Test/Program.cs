using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using AutoSpectre;
using AutoSpectre.Extensions;
using Spectre.Console;

namespace Test
{
    public class Program
    {
        public static async Task Main()
        {
            var spectrePrompt = await new Example().SpectrePromptAsync();
        }
    }
    
    [AutoSpectreForm(Culture = "da-DK")]
public class Example
{
    [TextPrompt(ChoicesSource= nameof(NameChoices), ChoicesStyle = "red on yellow", ChoicesInvalidText = "Must be one of the names")]
    public string Names { get; set; } = null!;

    public static readonly string[] NameChoices = new[] { "Andreas", "Emilie", "Alberte" };

    [TaskStep(UseStatus = true, StatusText = "Loading....",SpinnerType = SpinnerKnownTypes.Balloon)]
    public async Task SomeOperation()
    {
        await Task.Delay(10000);
    }

    //  public static string DefaultName  = "Jeppe";
    // public static IEnumerable<string> NameSource 
    // {
    //     get
    //     {
    //         yield return "32";
    //         yield return "332";
    //         yield return "433";
    //         yield return "305";
    //     }
    //     
    // }

}
}