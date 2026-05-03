using AutoSpectre;
using Spectre.Console;


var someForm = new SomeForm();


[AutoSpectreForm]
public partial class SomeForm
{
    [SelectPrompt(Title = "Enter your name")]
    public partial string[] Name {get;set;}
    
    public string[] NameSource => ["Jeppe", "Peter","Ulrik"];
    public List<string> NameDefault = ["Jeppe","Peter"];
}
