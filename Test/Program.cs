using System.Collections.Generic;
using System.Collections.ObjectModel;
using AutoSpectre;
using Spectre.Console;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var smClass = new SomeclassSpectreFactory();
            var someclass = smClass.Get();
            
            int i = 0;
        }
    }

    [AutoSpectreForm]
    public class TestForm
    {
        [Ask]
        public string Name { get; set; }
    }



    [AutoSpectreForm]
    public class Someclass
    {
        [Ask(Title = "Enter first name")]
        public string? FirstName { get; set; }

        [Ask]
        public bool LeftHanded { get; set; }

        [Ask]
        public double Age { get; set; }


        [Ask(AskType = AskType.Selection, SelectionSource = "Items")]
        public string Item { get; set; }

        public List<string> Items { get; } = new List<string>() { "Alpha", "Bravo", "Charlie" };

        [Ask(AskType = AskType.Selection, SelectionSource = nameof(Items), Title = "Select multiple items")]
        public IReadOnlyList<string> Multiselect { get; set; }

    }


}
