using AutoSpectre;
using Spectre.Console;


var someForm = new SomeForm();
MultiSelectionPrompt<string> fisto = new MultiSelectionPrompt<string>();


[AutoSpectreForm]
public class SomeForm
{
//     [SelectPrompt(Title = "Enter your name", DefaultValueSource = nameof(NameDefault))]
//     public string[] Name {get;set;}
//     
//     public string[] NameSource => ["Jeppe", "Peter","Ulrik"];
//     public List<string> NameDefault = ["Jeppe","Peter"];
// }
}