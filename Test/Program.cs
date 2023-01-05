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
            Someclass cls = new();
            ISomeclassSpectreFactory factory = new SomeclassSpectreFactory();
            factory.Get(cls);
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


        [Ask(AskType = AskType.Selection, SelectionSource = "Items")]
        public string Item { get; set; }

        public List<string> Items { get; } = new List<string>() { "Alpha", "Bravo", "Charlie" };

    }

    
}
