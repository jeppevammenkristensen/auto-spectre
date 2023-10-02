using AutoSpectre;
using Spectre.Console;

namespace Autospectre.Examples.Examples.GithubSample;

public class GithubExample : IExample
{
    public async Task Run()
    {
        IExampleSpectreFactory factory = new ExampleSpectreFactory();
        var example = factory.Get();
    }
}

[AutoSpectreForm(Culture = "da-DK")]
public class Example
{
    [TextPrompt(Title = "Add item")] public int[] IntItems { get; set; } = Array.Empty<int>();

    [TextPrompt(Title = "Enter first name", DefaultValueStyle = "bold",
        DefaultValueSource = nameof(FirstNameDefaultValue))]
    public string? FirstName { get; set; }

    public readonly string? FirstNameDefaultValue = "John Doe";


    [TextPrompt(PromptStyle = "green bold")]
    public bool LeftHanded { get; set; }

    [TextPrompt(Title = "Choose your [red]value[/]")]
    public SomeEnum Other { get; set; }

    [TextPrompt(Secret = true, Mask = '*')]
    public string? Password { get; set; }

    [TextPrompt(ChoicesStyle = "red on yellow",
        ChoicesInvalidText = "Must be one of the names")]
    public string Name { get; set; }

    public static readonly string[] NameChoices = new[] {"Kurt", "Krist", "David", "Pat"};

    [TextPrompt] public OtherAutoSpectreFormClass ChildForm { get; set; } = new();

    [TextPrompt]
    public IReadOnlyList<OtherAutoSpectreFormClass> Investors { get; set; } = new List<OtherAutoSpectreFormClass>();

    [TaskStep(UseStatus = true, StatusText = "This will take a while", SpinnerType = SpinnerKnownTypes.Christmas,
        SpinnerStyle = "green on yellow")]
    public void DoSomething(IAnsiConsole console)
    {
        console.Write(new FigletText("A figlet text is needed"));
    }

    [SelectPrompt(WrapAround = true, PageSize = 3,
        MoreChoicesText = "Press down to see more choices", HighlightStyle = "purple")]
    //[SelectPrompt(Source = nameof(ItemSource))]
    public string Item { get; set; } = string.Empty;

    public List<string> ItemSource { get; } = new() {"Alpha", "Bravo", "Charlie"};

    [SelectPrompt(InstructionsText = "Check the special items you want to select")]
    //[SelectPrompt(Converter = nameof(SpecialProjectionConverter))]
    public List<int> SpecialProjection { get; set; } = new();

    public string SpecialProjectionConverter(int source) => $"Number {source}";
    public List<int> SpecialProjectionSource { get; set; } = new() {1, 2, 3, 4};

    [TextPrompt]
    // [TextPrompt(Validator = nameof(EnterYearValidator))]
    public int EnterYear { get; set; }

    public string? EnterYearValidator(int year)
    {
        return year <= DateTime.Now.Year ? null : "Year cannot be larger than current year";
    }

    [TextPrompt] public HashSet<string> Names { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public string? NamesValidator(List<string> items, string newItem)
    {
        if (newItem == "Foobar")
            return "Cannot be Foobar";

        if (items.Contains(newItem))
            return $"{newItem} has already been added";

        return null;
    }

    [TextPrompt] public bool AddExistingName { get; set; }

    [TextPrompt(Condition = nameof(AddExistingName))]
    public string? ExistingName { get; set; }

    [TextPrompt(Condition = nameof(AddExistingName), NegateCondition = true)]
    public string? NewName { get; set; }
}

[AutoSpectreForm()]
public class OtherAutoSpectreFormClass
{
    [TextPrompt] public string? Name { get; set; }
}

public enum SomeEnum
{
    Foo,
    Bar,
    Baz
}