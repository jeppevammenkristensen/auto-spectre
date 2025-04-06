using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoSpectre;
using AutoSpectre.Extensions;
using Dumpify;
using Test.Enums;


namespace Test
{
    public class Program
    {
        public static async Task Main()
        { 
            InnerTestSpectreFactory factory = new InnerTestSpectreFactory();
            var innerTest = factory.Prompt();
            
        }
    }

    [AutoSpectreForm(DisableDump = false)]
    public class SClass
    {
        [SelectPrompt(Title = "Select \"name\"")]
        public string Name { get; set; } = string.Empty;
        
        [TextPrompt]
        public SomeEnum SomeEnum { get; set; } = SomeEnum.First;
        
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