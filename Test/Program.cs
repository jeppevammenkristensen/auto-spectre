using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using AutoSpectre;
using Spectre.Console;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var forms = new ValidateClassSpectreFactory();
            var other = forms.Get();
            int i = 0;
        }
    }

 

   
    public enum SomeEnum
    {
        Red,
        Green,
        Refactor
    }

    public class ValidateClassSpectreFactory
    {
        public ValidateClass Get(ValidateClass destination = null)
        {
            destination ??= new Test.ValidateClass();
            // Prompt for values for destination.Name
            {
                List<string> items = new List<string>();
                bool continuePrompting = true;
                do
                {
                    bool valid = false;
                    while (!valid)
                    {
                        var item = AnsiConsole.Prompt(new TextPrompt<string>("Enter [green]Name[/]"));
                        var validationResult = destination.ValidateName(items, item);
                        if (validationResult is { } error)
                        {
                            AnsiConsole.MarkupLine($"[red]{error}[/]");
                            valid = false;
                        }
                        else
                        {
                            valid = true;
                            items.Add(item);
                        }
                    }

                    continuePrompting = AnsiConsole.Confirm("Add more items?");
                }
                while (continuePrompting);
                string[] result = items.ToArray();
                destination.Name = result;
            }

            return destination;
        }
    }

    public class ValidateClass
    {
        [Ask(Validator=nameof(ValidateName))]
        public string[] Name { get; set;}

        public string? ValidateName(List<string> items,string name)
        {
            if (items.Contains(name))
                return $"{name} exists";

            return null;
        }
    }


    //[AutoSpectreForm]
    //public class Name
    //{
    //    [Ask(Validator = nameof(FirstNameValidator))]
    //    public string[] Names { get; set; }

    //    public string? FirstNameValidator(string firstName)
    //    {
    //        if (Names?.Contains(firstName) == true)
    //        {
    //            return $"{firstName} has allready been added";
    //        }

    //        return null;
    //    }

    //    public string? FirstNameValidator(IReadOnlyList<string> collection, string name)
    //    {

    //    }

    //    [Ask]
    //    public string LastName { get; set; }

    //}

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

        //[Ask] 
        //public Name Owner { get; set; } = new Name(); 

        //[Ask]
        //public IReadOnlyList<Name> Investors { get; set; } = new List<Name>();

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
