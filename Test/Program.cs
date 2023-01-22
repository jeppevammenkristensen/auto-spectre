using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using AutoSpectre;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var someclass = new SomeclassSpectreFactory();
            var someclass1 = someclass.Get();
            int i = 0;
        }
    }

    public enum SomeEnum
    {
        Red,
        Green,
        Refactor
    }

    [AutoSpectreForm]
    public class Name
    {
        [Ask]
        public string FirstName { get; set; }

        [Ask]
        public string LastName { get; set; }

    }

    [AutoSpectreForm]
    public class Someclass
    {
        [Ask(AskType = AskType.Normal, Title = "Add item")] 
        public int[] IntItems { get; set; } = Array.Empty<int>();

        [Ask(Title = "Enter first name")]
        public string? FirstName { get; set; }

        [Ask]
        public bool LeftHanded { get; set; }

        [Ask(Title = "Choose your [red]value[/]")]
        public SomeEnum Other { get; set; }

        [Ask] 
        public Name Owner { get; set; } = new Name(); 

        [Ask]
        public IReadOnlyList<Name> Investors { get; set; } = new List<Name>();

        [Ask(AskType = AskType.Selection, SelectionSource = nameof(Items))]
        public string Item { get; set; } = string.Empty;

        public List<string> Items { get; } = new List<string>() { "Alpha", "Bravo", "Charlie" };
    }

    [AutoSpectreForm]
    public class Items2
    {
        [Ask]
        public HashSet<string> Items { get; set; }
    }
    

    [AutoSpectreForm]
    public class CollectionSample
    {

        [Ask(AskType = AskType.Selection, SelectionSource = nameof(Items), Title = "Select multiple items")]
        public IReadOnlyList<string> Multiselect { get; set; }

        [Ask(AskType = AskType.Selection, SelectionSource = nameof(Items))]
        public string[] ArrayMultiSelect { get; set; } = Array.Empty<string>();

        [Ask(AskType = AskType.Selection, SelectionSource = nameof(Items))]
        public ImmutableArray<string> ArrayMultiSelectMultiple { get; set; }

        [Ask(AskType = AskType.Selection, SelectionSource = nameof(Numbers))]
        public List<int> ListNumbers { get; set; } = new List<int>();

        public int[] Numbers { get; } = new[] {1, 2, 3};
       
        public List<string> Items { get; } = new List<string>() { "Alpha", "Bravo", "Charlie" };

    }




}
