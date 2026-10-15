using AutoSpectre;


var cancellationToken = CancellationToken.None;

var someForm = new SomeForm();
await someForm.PromptAsync(cancellationToken);
int i = 0;



[AutoSpectreForm]
public partial class SomeForm
{
    [SelectPrompt(Title = "Enter your name")]
    public partial string[] Name { get; set; } = [];
    
    public string[] NameSource => ["Jeppe", "Peter","Ulrik"];
    public List<string> NameDefault = ["Jeppe","Peter"];
}
