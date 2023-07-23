using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using AutoSpectre;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var converter = new ConverterFormSpectreFactory();
            var converterForm = converter.Get();
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
    public class ConverterForm
    {
        [Ask(Title = "[green]Select person[/]", AskType = AskType.Selection, SelectionSource = nameof(Persons), Converter = nameof(Converter))]
        public Person? Person { get; set; }

        [Ask(Title = "[green]Select persons[/]", AskType = AskType.Selection, SelectionSource = nameof(Persons), Converter = nameof(Converter))]
        public List<Person> SelectedPersons { get; set; } = new List<Person>();

        public string Converter(Person value) => $"{value.FirstName} {value.LastName}";

        public List<Person> Persons()
        {
            return new List<Person>()
            {
                new Person("John", "Doe"),
                new Person("Jane", "Doe"),
                new Person("John", "Smith"),
                new Person("Jane", "Smith"),
                new Person("John", "Jones")
            };
        }
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

        [Ask(AskType = AskType.Selection, SelectionSource = "Persons", Converter = nameof(PersonConvert))]
        public Person? Person { get; set; }

        

        public string PersonConvert(Person person) => $"{person.FirstName} {person.LastName}";

        [Ask(Title = "Choose your [red]value[/]")]
        public SomeEnum Other { get; set; }

        [Ask] 
        public Name Owner { get; set; } = new Name(); 

        [Ask]
        public IReadOnlyList<Name> Investors { get; set; } = new List<Name>();

        [Ask(AskType = AskType.Selection, SelectionSource = nameof(Items))]
        public string Item { get; set; } = string.Empty;

        public List<string> Items { get; } = new List<string>() { "Alpha", "Bravo", "Charlie" };

        public List<Person> Persons()
        {
            return new List<Person>()
            {
                new Person("John", "Doe"),
                new Person("Jane", "Doe"),
                new Person("John", "Smith"),
            };
        }
    }

    [AutoSpectreForm]
    public class Items2
    {
        [Ask]
        public HashSet<string> Items { get; set; }
    }


    public record Person(string FirstName, string LastName)
    {

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
