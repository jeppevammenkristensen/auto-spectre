using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using AutoSpectre;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Enumerable.Range(0, 1).ToArray();

            ISomeclassSpectreFactory factory = new SomeclassSpectreFactory();
            
            var item = new Someclass();
            factory.Get(item);
            
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
