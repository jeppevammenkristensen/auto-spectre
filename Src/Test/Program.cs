using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoSpectre;
using Spectre.Console.Rendering;
using Test.OneDirectory;


namespace Test
{
    public class Program
    {
        public static async Task Main()
        {
            IRenderable? renderable = null;
            var spectrePromptAsync = await new Example().SpectrePromptAsync();
            spectrePromptAsync.Dump();

        }
    }

     [AutoSpectreForm]    
public class Example
{
    [TextPrompt(Title = "Enter data from subclass")]
    public List<Subclass2> Subclass { get; set; }

    //public string DatesyConverter(DateTime date) => date.ToLongDateString(); 

    public IEnumerable<DateTime> DatesySource()
    {
        yield return DateTime.Now;
        yield return DateTime.Now.AddDays(1);
        yield return DateTime.Now.AddDays(2);
        yield return DateTime.Now.AddDays(4);
    }

    public static readonly string[] NameChoices = new[] { "Andreas", "Emilie", "Alberte" };

    [TaskStep(UseStatus = true, StatusText = "Loading....",SpinnerType = SpinnerKnownTypes.Balloon)]
    public async Task SomeOperation()
    {
        await Task.Delay(3000);
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