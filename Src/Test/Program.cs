﻿using System.Collections.Generic;
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
            // ExampleSpectreFactory factory = new();
            // factory.Get(new Example());
        }
    }


    [AutoSpectreForm(Culture = "da-DK")]
public class Example
{
    [TextPrompt(ChoicesSource = nameof(NameChoices), Title = "Vælg et andet tal",
        ChoicesInvalidText = "What are you doing")]
    public string Name { get; set; } = null!;

   public static IEnumerable<int> NameChoices 
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