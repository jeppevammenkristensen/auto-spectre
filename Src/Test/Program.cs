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
            ExampleSpectreFactory factory = new();
            factory.Get(new Example());
        }
    }
    
    [AutoSpectreForm(Culture = "da-DK")]
public class Example
{
    [TextPrompt]
    public string Name { get; set; } = null!;

    public static string? NameValidator(string name) => name == "Jeppe" ? "Stupid Name" : null;
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