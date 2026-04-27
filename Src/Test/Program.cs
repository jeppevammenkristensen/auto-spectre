using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoSpectre;
using Spectre.Console;
using Test.Enums;


namespace Test
{
    public class Program
    {
        public static async Task Main()
        {
           
            
           // await result.PromptAsync(new SClass("Jeppe Roi Kristensen"));
        }
    }

    [AutoSpectreForm()]
    public partial class OtherClass
    {
        public bool NameCondition => false;

        [TextPrompt(Title = "Speek friend", ChoicesSource = nameof(SelectSource))] public partial string Name { get; set; }
        
        public string[] SelectSource => new[] {"First", "Second"};
        
        [SelectPrompt(SearchEnabled = true, SearchPlaceholderText = "Search for it")]
        public partial string Select { get; set; }

        [TaskStep(Condition = nameof(NameCondition), NegateCondition = true)]
        public partial void TaskStep()
        {
            
        }
    }

    [AutoSpectreForm()]
    public partial class SClass
    {
        private readonly string _firstName;

        public SClass(string firstName)
        {
            _firstName = firstName;
        }

        [TextPrompt(EditableDefaultValue = true, DefaultValueSource = nameof(SomeValueDefaultValueSource))]
        public partial string SomeValue { get; set; }

        public string SomeValueDefaultValueSource() => "Angry";
        
        /// <summary>
        /// This is a data <see cref="DateDefault"/>
        /// </summary>
        [TextPrompt(Title = "Enter date", DefaultValueSource = nameof(DateDefault))]
        public DateOnly Date { get; set; }
        
        [TextPrompt]
        public bool Abort { get; set; }
        
        [SelectPrompt(Source = nameof(SomePromptSource))]
        public string SomePrompt { get; set; }
        
        public readonly string SomePromptDefaultValue = "Ulrik";
        
         public List<string?> SomePromptSource => ["Jeppe", "Roo", "Ulrik"];
         public static string SomePromptCancelResult => "[empty]";        
         
         public string[] OtherPromptSource => new[] {"Jeppe", "Roo", "Ulrik"};
         
         [SelectPrompt(Source = nameof(OtherPromptSource))]
         public partial List<string> OtherPrompt { get; set; }

         public string OtherPromptDefaultValue() => "Jeppe";
         


        

        [Break(Condition = nameof(Abort))]
        public partial void AbortMethod(IAnsiConsole console)
        {
            console.MarkupLine("Aborting");
        }

        public DateOnly DateDefault() => DateOnly.FromDateTime(DateTime.Now);

        [SelectPrompt(Title = "Select \"name\"")]
        public partial string Name { get; set; } 
        
        [TextPrompt]
        public SomeEnum SomeEnum { get; set; } = SomeEnum.First;

        [TaskStep(StatusText = "Processing the bad boy", UseStatus = false, SpinnerStyle = "yellow bold")]
        public partial async Task Hello()
        {
            await Task.Delay(5000);
        }
        
        public IEnumerable<string> NameSource()  {
            yield return "First";
            yield return "Second";
        }

        [AutoSpectreForm]
        public class InnerTest
        {
            [SelectPrompt(Source = nameof(OthersSource))]
            public List<OtherClass> Others { get; set; } = new();
            
            public IEnumerable<OtherClass> OthersSource()
            {
                yield return new OtherClass()
                {
                    Name = "Jeppe"
                };
            }
            
            [TextPrompt]
            public string Name { get; set; }
            
            [TextPrompt(Title = "Select the dump method")]
            public Dumpy DumpMethod { get; set; }

            public class OtherClass
            {
                public string Name { get; set; }

                public override string ToString()
                {
                    return $"Name";
                }
            }
            
            
            public enum Dumpy
            {
                SpectreDump,
                Dumpify
            }
        }
        
        
        [TextPrompt(DefaultValueSource = nameof(FirstNameDefault))] public partial string FirstName { get; set; }
        public const string FirstNameDefault = "Jeppe";
    }
    
    
    
    
}