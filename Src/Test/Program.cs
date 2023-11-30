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
            S s = new S().SpectrePrompt();
            s.Dump();
            // var spectrePrompt = new Subclass3().SpectrePrompt();
            // spectrePrompt.Dump();
        }
    }

    [AutoSpectreForm(DisableDump = false)]
    public class S
    {
        [TextPrompt(DefaultValueStyle = "yellow", ChoicesStyle = "green on black")]
        public bool Yay { get; set; }

        [TextPrompt(DefaultValueSource = nameof(FirstNameDefault))] public string FirstName { get; set; } = null!;
        public const string FirstNameDefault = "Jeppe";
    }
}