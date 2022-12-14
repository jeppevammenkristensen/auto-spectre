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
    public class Someclass
    {
        [Ask(Title = "Middle name")]
        public string? MiddleName { get; set; }

        //[Ask()] public string LastName { get; set; } = null!;

        //[Ask()]
        //public int Age { get; set; }

        [Ask(AskType = AskType.Selection, SelectionSource = nameof(Items))]
        public string Item { get; set; }

        public List<string> Items { get; } = new List<string>() { "Alpha", "Bravo", "Charlie" };

        [Ask(AskType = AskType.Selection, SelectionSource = nameof(Ages))]
        public int Age { get; set; }

        public IEnumerable<int> Ages => new List<int>() {44, 39};

    }
}
