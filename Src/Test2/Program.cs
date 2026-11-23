using AutoSpectre;

var cancellationToken = CancellationToken.None;

var someForm = new SomeForm();
await someForm.PromptAsync(cancellationToken);
int i = 0;


[AutoSpectreForm]
public partial class SomeForm
{
    public List<string> NameDefault = ["Jeppe", "Peter"];

    [SelectPrompt(Title = "Enter your name", Source = nameof(NameSource))]
    public partial string[] Name { get; set; } = [];

    public string[] NameSource => ["Jeppe", "Peter", "Ulrik"];

    public string NameConverter(string source)
    {
        return EscapeInterpolated($"[red bold]{source}[/]");
    }
}