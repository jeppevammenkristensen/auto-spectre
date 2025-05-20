using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoSpectre;
using AutoSpectre.Extensions;
using Dumpify;
using Spectre.Console;
using Test.Enums;


namespace Test
{
    public class Program
    {
        public static async Task Main()
        {
            var result = SpectreFactory.GetSpectreFactory_Test_SClass();
            await result.PromptAsync(new SClass("Jeppe Roi Kristensen"));


        }
    }

    [AutoSpectreForm()]
    public class SClass
    {
        private readonly string _firstName;

        public SClass(string firstName)
        {
            _firstName = firstName;
        }

        [TextPrompt(Title = "Enter date", DefaultValueSource = nameof(DateDefault))]
        public DateOnly Date { get; set; }
        
        [TextPrompt]
        public bool Abort { get; set; }



        [Break(Condition = nameof(Abort))]
        public void AbortMethod(IAnsiConsole console)
        {
            console.MarkupLine("Aborting");
        }

        public DateOnly DateDefault() => DateOnly.FromDateTime(DateTime.Now);

        [SelectPrompt(Title = "Select \"name\"")]
        public string Name { get; set; } = string.Empty;
        
        [TextPrompt]
        public SomeEnum SomeEnum { get; set; } = SomeEnum.First;

        [TaskStep(StatusText = "Processing", UseStatus = true, SpinnerStyle = "yellow bold")]
        public async Task Hello()
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

        

        
        
        [TextPrompt(DefaultValueSource = nameof(FirstNameDefault))] public string FirstName { get; set; } = null!;
        public const string FirstNameDefault = "Jeppe";
    }
    
    
    
    
}