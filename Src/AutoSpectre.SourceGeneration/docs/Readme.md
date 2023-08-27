# AutoSpectre Source Generation

This allows you to decorate a class with the `AutoSpectreForm` attribute and decorate properties on that class with:

* `TextPromptAttribute`
* `SelectPromptAttribute`

and void or Task methods with

* `TaskStepAttribute`

and behind the scenes a Form (SpectreFactory) will be generated to request input using `Spectre.Console`

## Example

### Code

```csharp
[AutoSpectreForm]
public class Example
{
    [TextPrompt(Title = "Add item")] public int[] IntItems { get; set; } = Array.Empty<int>();

    [TextPrompt(Title = "Enter first name", DefaultValueStyle = "bold")]
    public string? FirstName { get; set; } = "John Doe"; // Default value in prompt

    [TextPrompt(PromptStyle = "green bold")] 
    public bool LeftHanded { get; set; }

    [TextPrompt(Title = "Choose your [red]value[/]" )]
    public SomeEnum Other { get; set; }
    
    [TextPrompt(Secret = true, Mask = '*')]
    public string? Password { get; set; }

    [TextPrompt] public OtherAutoSpectreFormClass ChildForm { get; set; } = new();

    [TextPrompt]
    public IReadOnlyList<OtherAutoSpectreFormClass> Investors { get; set; } = new List<OtherAutoSpectreFormClass>();

    [TaskStep(UseStatus = true, StatusText = "This will take a while", SpinnerType = SpinnerKnownTypes.Christmas, SpinnerStyle = "green on yellow")]
    public void DoSomething(IAnsiConsole console)
    {
        console.Write(new FigletText("A figlet text is needed"));
    }

    [SelectPrompt(WrapAround = true, PageSize = 3,
        MoreChoicesText = "Press down to see more choices", HighlightStyle = "purple")]
    //[SelectPrompt(Source = nameof(ItemSource))]
    public string Item { get; set; } = string.Empty;

    public List<string> ItemSource { get; } = new() { "Alpha", "Bravo", "Charlie" };

    [SelectPrompt(InstructionsText = "Check the special items you want to select")]
    //[SelectPrompt(Converter = nameof(SpecialProjectionConverter))]
    public List<int> SpecialProjection { get; set; } = new();

    public string SpecialProjectionConverter(int source) => $"Number {source}";
    public List<int> SpecialProjectionSource { get; set; } = new() { 1, 2, 3, 4 };

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
```

### Output

```csharp
/// <summary>
/// Helps create and fill <see cref = "Example"/> with values
/// </summary>
public interface IExampleSpectreFactory
{
    Example Get(Example destination = null);
}

/// <summary>
/// Helps create and fill <see cref = "Example"/> with values
/// </summary>
public class ExampleSpectreFactory : IExampleSpectreFactory
{
    public Example Get(Example destination = null)
    {
        IOtherAutoSpectreFormClassSpectreFactory OtherAutoSpectreFormClassSpectreFactory = new OtherAutoSpectreFormClassSpectreFactory();
        destination ??= new ConsoleApp1.Example();
        // Prompt for values for destination.IntItems
        {
            List<int> items = new List<int>();
            bool continuePrompting = true;
            do
            {
                var item = AnsiConsole.Prompt(new TextPrompt<int>("Add item"));
                items.Add(item);
                continuePrompting = AnsiConsole.Confirm("Add more items?");
            }
            while (continuePrompting);
            int[] result = items.ToArray();
            destination.IntItems = result;
        }

        destination.FirstName = AnsiConsole.Prompt(new TextPrompt<string?>("Enter first name").AllowEmpty().DefaultValue("John Doe").DefaultValueStyle("bold"));
        destination.LeftHanded = AnsiConsole.Confirm("Enter [green]LeftHanded[/]");
        destination.Other = AnsiConsole.Prompt(new SelectionPrompt<SomeEnum>().Title("Choose your [red]value[/]").PageSize(10).AddChoices(Enum.GetValues<SomeEnum>()));
        destination.Password = AnsiConsole.Prompt(new TextPrompt<string?>("Enter [green]Password[/]").AllowEmpty().Secret('*'));
        {
            AnsiConsole.MarkupLine("Enter [green]ChildForm[/]");
            var item = OtherAutoSpectreFormClassSpectreFactory.Get();
            destination.ChildForm = item;
        }

        // Prompt for values for destination.Investors
        {
            List<OtherAutoSpectreFormClass> items = new List<OtherAutoSpectreFormClass>();
            bool continuePrompting = true;
            do
            {
                {
                    AnsiConsole.MarkupLine("Enter [green]Investors[/]");
                    var newItem = OtherAutoSpectreFormClassSpectreFactory.Get();
                    items.Add(newItem);
                }

                continuePrompting = AnsiConsole.Confirm("Add more items?");
            }
            while (continuePrompting);
            System.Collections.Generic.IReadOnlyList<ConsoleApp1.OtherAutoSpectreFormClass> result = items;
            destination.Investors = result;
        }

        AnsiConsole.MarkupLine("Calling method [green]DoSomething[/]");
        AnsiConsole.Status().SpinnerStyle("green on yellow").Spinner(Spinner.Known.Christmas).Start("This will take a while", ctx =>
        {
            destination.DoSomething(AnsiConsole.Console);
        });
        destination.Item = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Enter [green]Item[/]").PageSize(3).WrapAround(true).MoreChoicesText("Press down to see more choices").HighlightStyle("purple").AddChoices(destination.ItemSource.ToArray()));
        destination.SpecialProjection = AnsiConsole.Prompt(new MultiSelectionPrompt<int>().Title("Enter [green]SpecialProjection[/]").UseConverter(destination.SpecialProjectionConverter).PageSize(10).InstructionsText("Check the special items you want to select").AddChoices(destination.SpecialProjectionSource.ToArray()));
        destination.EnterYear = AnsiConsole.Prompt(new TextPrompt<int>("Enter [green]EnterYear[/]").Validate(ctx =>
        {
            var result = destination.EnterYearValidator(ctx);
            return result == null ? ValidationResult.Success() : ValidationResult.Error(result);
        }));
        // Prompt for values for destination.Names
        {
            List<string> items = new List<string>();
            bool continuePrompting = true;
            do
            {
                bool valid = false;
                while (!valid)
                {
                    var item = AnsiConsole.Prompt(new TextPrompt<string>("Enter [green]Names[/]"));
                    var validationResult = destination.NamesValidator(items, item);
                    if (validationResult is { } error)
                    {
                        AnsiConsole.MarkupLine($"[red]{error}[/]");
                        valid = false;
                    }
                    else
                    {
                        valid = true;
                        items.Add(item);
                    }
                }

                continuePrompting = AnsiConsole.Confirm("Add more items?");
            }
            while (continuePrompting);
            System.Collections.Generic.HashSet<string> result = new System.Collections.Generic.HashSet<string>(items);
            destination.Names = result;
        }

        destination.AddExistingName = AnsiConsole.Confirm("Enter [green]AddExistingName[/]");
        if (destination.AddExistingName == true)
        {
            destination.ExistingName = AnsiConsole.Prompt(new TextPrompt<string?>("Enter [green]ExistingName[/]").AllowEmpty());
        }

        if (destination.AddExistingName == false)
        {
            destination.NewName = AnsiConsole.Prompt(new TextPrompt<string?>("Enter [green]NewName[/]").AllowEmpty());
        }

        return destination;
    }
}
```

## Credits

[Fear icons created by Smashicons - Flaticon](https://www.flaticon.com/free-icons/fear "fear icons")