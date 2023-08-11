﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using AutoSpectre;
using Spectre.Console;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var conditionSampleFormSpectreFactory = new ConditionSampleFormSpectreFactory();
            var conditionSampleForm = conditionSampleFormSpectreFactory.Get();
        }
    }

    [AutoSpectreForm]
    public class ConditionSampleForm
    {
        [TextPrompt] public bool AskFriendlyCondition { get; set; } = true;

        [TextPrompt(Title = "Please sir what is your name?", DefaultValueStyle = "yellow slowblink")]
        public string AskFriendly { get; set; } = "Sir Jeppelot";
        
        [TextPrompt(Title = "What is your #!^$ name", Condition = nameof(AskFriendlyCondition), NegateCondition = true)]
        public string AskHostile { get; set; }
    }

    [AutoSpectreForm]
    public class ConditionConvention
    {
        [TextPrompt]
        public string FirstName { get; set; }

        public bool FirstNameCondition => true;
        
        [TextPrompt(NegateCondition = true)]
        public string NegatePrompt { get; set; }

        public bool NegatePromptCondition => false;
    }
    
    [AutoSpectreForm]
    public class Example
    {
        [TextPrompt(Title = "Add item")] 
        public int[] IntItems { get; set; } = Array.Empty<int>();

        [TextPrompt(Title = "Enter first name")]
        public string? FirstName { get; set; }

        [TextPrompt]
        public bool LeftHanded { get; set; }

        [TextPrompt(Title = "Choose your [red]value[/]")]
        public SomeEnum Other { get; set; }

        [TextPrompt] 
        public ChildFormClass ChildForm { get; set; } = new (); 

        [TextPrompt]
        public IReadOnlyList<ChildFormClass> Investors { get; set; } = new List<ChildFormClass>();

        
        [SelectPrompt()]
        //[Ask(AskType = AskType.Selection, SelectionSource = nameof(ItemSource))]
        public string Item { get; set; } = string.Empty;
        public List<string> ItemSource { get; } = new List<string>() { "Alpha", "Bravo", "Charlie" };

        [SelectPrompt()]
        //[Ask(AskType = AskType.Selection, Converter = nameof(SpecialProjectionConverter))]
        public List<int> SpecialProjection { get; set; } = new();

        public string SpecialProjectionConverter(int source) => $"Number {source}";
        public List<int> SpecialProjectionSource { get; set; } = new(){1,2,3,4};
        
        [TextPrompt]
        // [Ask(Validator = nameof(EnterYearValidator))]
        public int EnterYear { get; set; }

        public string? EnterYearValidator(int year)
        {
            return year <= DateTime.Now.Year ? null : "Year cannot be larger than current year";
        }

        [SelectPrompt] public HashSet<string> Names { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public string? NamesValidator(List<string> items, string newItem)
        {
            if (newItem == "Foobar")
                return "Cannot be Foobar";

            if (items.Contains(newItem))
                return $"{newItem} has already been added";

            return null;
        }
        
        [TextPrompt]
        public bool AddExistingName { get; set; }
        
        [TextPrompt(Condition = nameof(AddExistingName))]
        public string? ExistingName { get; set; }
        
        [TextPrompt(Condition = nameof(AddExistingName), NegateCondition = true)]
        public string? NewName { get; set; }
        
    }

    [AutoSpectreForm]
    public class LoginForm
    {
        [TextPrompt(Title = "Enter username")]
        public string Username { get; set; } = string.Empty;

        [TextPrompt(Title = "Enter password", Secret = true)]
        public string Password { get; set; } = string.Empty;
    }

    [AutoSpectreForm]
    public class ChildFormClass
    {
        [TextPrompt]
        public string Name { get; set; }
    }

    [AutoSpectreForm]
    public class MainForm
    {
        [TextPrompt]
        public TestClass Main { get; set; }

        public string? MainValidator(TestClass main)
        {
            return string.Empty; 
        }
    }

    [AutoSpectreForm]
    public class TestClass
    {
        [TextPrompt]
        public string Name { get; set; }
    }
   
    public enum SomeEnum
    {
        Red,
        Green,
        Refactor
    }

    public class ValidateClass
    {
        [TextPrompt(Validator=nameof(ValidateName))]
        public string[] Name { get; set;}

        public string? ValidateName(List<string> items,string name)
        {
            if (items.Contains(name))
                return $"{name} exists";

            return null;
        }
    }
    
    [AutoSpectreForm]
    public class ConverterForm
    {
        [SelectPrompt(Title = "[green]Select person[/]", Source = nameof(Persons), Converter = nameof(Converter))]
        public Person? Person { get; set; }

        [SelectPrompt(Title = "[green]Select persons[/]", Source = nameof(Persons), Converter = nameof(Converter))]
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
        [TextPrompt(Title = "Add item")] 
        public int[] IntItems { get; set; } = Array.Empty<int>();

        [TextPrompt(Title = "Enter first name")]
        public string? FirstName { get; set; }

        [TextPrompt]
        public bool LeftHanded { get; set; }

        [SelectPrompt(Source = "Persons", Converter = nameof(PersonConvert))]
        public Person? Person { get; set; }
        
        public string PersonConvert(Person person) => $"{person.FirstName} {person.LastName}";

        [TextPrompt(Title = "Choose your [red]value[/]")]
        public SomeEnum Other { get; set; }

        [SelectPrompt(Source = nameof(Items))]
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
        [TextPrompt]
        public HashSet<string> Items { get; set; }
    }


    public record Person(string FirstName, string LastName)
    {

    }



    [AutoSpectreForm]
    public class CollectionSample
    {

        [SelectPrompt(Source = nameof(Items), Title = "Select multiple items")]
        public IReadOnlyList<string> Multiselect { get; set; }

        [SelectPrompt(Source = nameof(Items))]
        public string[] ArrayMultiSelect { get; set; } = Array.Empty<string>();

        [SelectPrompt(Source = nameof(Items))]
        public ImmutableArray<string> ArrayMultiSelectMultiple { get; set; }

        [SelectPrompt(Source = nameof(Numbers))]
        public List<int> ListNumbers { get; set; } = new List<int>();

        public int[] Numbers { get; } = new[] {1, 2, 3};
       
        public List<string> Items { get; } = new List<string>() { "Alpha", "Bravo", "Charlie" };

    }




}
