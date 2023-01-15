using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using AutoSpectre;
using Spectre.Console;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            IItems2SpectreFactory fact = new Items2SpectreFactory();
            fact.Get();
        }
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
        public string Item { get; set; } = string.Empty;

        public List<string> Items { get; } = new List<string>() { "Alpha", "Bravo", "Charlie" };

        [Ask(AskType = AskType.Normal)] public int[] IntItems { get; set; } = Array.Empty<int>();
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
