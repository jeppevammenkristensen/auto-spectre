using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using AutoSpectre;
using Spectre.Console;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            bool? prompt = AnsiConsole.Prompt(new ConfirmationPrompt("Hello there"));
            var text = AnsiConsole.Prompt(
                new TextPrompt<string>("Custom title")
            );

            //Console.WriteLine(item.MiddleName);
        }
    }
    
    [AutoSpectreForm]
    public class TestForm 
    {
        [Ask]
        public string Name {get;set;}   
    }



    [AutoSpectreForm]
    public class Someclass
    {
        [Ask(Title = "Enter first name")]
        public string? FirstName { get; set; }
        
        [Ask]
        public bool LeftHanded { get; set; }

        [Ask]
        public bool Age { get; set; }


        [Ask(AskType = AskType.Selection, SelectionSource = nameof(Items))]
        public string Item { get; set; }

        public List<string> Items { get; } = new List<string>() { "Alpha", "Bravo", "Charlie" };

    }

    
}
