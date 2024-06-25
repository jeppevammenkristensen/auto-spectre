using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoSpectre;



namespace Test
{
    public class Program
    {
        public static async Task Main()
        {
            SClass sClass = new SClass().SpectrePrompt();
            sClass.SpectreDump();
        }
    }

    [AutoSpectreForm(DisableDump = false)]
    public class SClass
    {
        [SelectPrompt(Title = "Select \"name\"")]
        public string Name { get; set; } = string.Empty;
        
        public IEnumerable<string> NameSource()  {
            yield return "First";
            yield return "Second";
        }

        [TextPrompt(DefaultValueSource = nameof(FirstNameDefault))] public string FirstName { get; set; } = null!;
        public const string FirstNameDefault = "Jeppe";
    }
}