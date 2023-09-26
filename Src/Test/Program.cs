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
    [SelectPrompt(Source = nameof(NameSource))]
    public string Name { get; set; } = null!;

    public static string DefaultName  = "Jeppe";
   public static IEnumerable<int> NameSource 
   {
       get
       {
           yield return 32;
           yield return 332;
           yield return 433;
           yield return 305;
       }
       
   }
   
}
}