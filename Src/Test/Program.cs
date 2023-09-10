using System.Globalization;
using System.Threading.Tasks;
using AutoSpectre;
using AutoSpectre.Extensions;
using Spectre.Console;

namespace Test
{
    public class Program
    {
        public static async Task Main()
        {
            ExampleSpectreFactory factory = new();
            factory.Get(new Example());
                
        }
    }


    [AutoSpectreForm(Culture = "da-DK")]
    public class Example
    {
        [TextPrompt(Title = "Vælg et tal")] public double RandomNumber { get; set; }
        //     
        // [TextPrompt(Title = "Add item")] public int[] IntItems { get; set; } = Array.Empty<int>();
        //
        // [TextPrompt(Title = "Enter first name", DefaultValueStyle = "bold")]
        // public string? FirstName { get; set; } = "John Doe"; // Default value in prompt
        //
        // [TextPrompt(PromptStyle = "green bold")] 
        // public bool LeftHanded { get; set; }
        //
        // [TextPrompt(Title = "Choose your [red]value[/]" )]
        // public SomeEnum Other { get; set; }
        //
        // [TextPrompt(Secret = true, Mask = '*')]
        // public string? Password { get; set; }
        //
        // [TextPrompt(TypeInitializer = nameof(Init))] public OtherAutoSpectreFormClass ChildForm { get; set; }
        //
        // public OtherAutoSpectreFormClass Init()
        // {
        //     return new OtherAutoSpectreFormClass(47);
        // }
        //
        // [TextPrompt(TypeInitializer = nameof(Init))]
        // public IReadOnlyList<OtherAutoSpectreFormClass> Investors { get; set; } = new List<OtherAutoSpectreFormClass>();
        //
        // [TaskStep(UseStatus = true, StatusText = "This will take a while", SpinnerType = SpinnerKnownTypes.Christmas, SpinnerStyle = "green on yellow")]
        // public void DoSomething(IAnsiConsole console)
        // {
        //     console.Write(new FigletText("A figlet text is needed"));
        // }
        //
        // [SelectPrompt(WrapAround = true, PageSize = 3,
        //     MoreChoicesText = "Press down to see more choices", HighlightStyle = "purple")]
        // //[SelectPrompt(Source = nameof(ItemSource))]
        // public string Item { get; set; } = string.Empty;
        //
        // public List<string> ItemSource { get; } = new() { "Alpha", "Bravo", "Charlie" };
        //
        // [SelectPrompt(InstructionsText = "Check the special items you want to select")]
        // //[SelectPrompt(Converter = nameof(SpecialProjectionConverter))]
        // public List<int> SpecialProjection { get; set; } = new();
        //
        // public string SpecialProjectionConverter(int source) => $"Number {source}";
        // public List<int> SpecialProjectionSource { get; set; } = new() { 1, 2, 3, 4 };
        //
        // [TextPrompt]
        // // [TextPrompt(Validator = nameof(EnterYearValidator))]
        // public int EnterYear { get; set; }
        //
        // public string? EnterYearValidator(int year)
        // {
        //     return year <= DateTime.Now.Year ? null : "Year cannot be larger than current year";
        // }
        //
        // [TextPrompt] public HashSet<string> Names { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        //
        // public string? NamesValidator(List<string> items, string newItem)
        // {
        //     if (newItem == "Foobar")
        //         return "Cannot be Foobar";
        //
        //     if (items.Contains(newItem))
        //         return $"{newItem} has already been added";
        //
        //     return null;
        // }
        //
        // [TextPrompt] public bool AddExistingName { get; set; }
        //
        // [TextPrompt(Condition = nameof(AddExistingName))]
        // public string? ExistingName { get; set; }
        //
        // [TextPrompt(Condition = nameof(AddExistingName), NegateCondition = true)]
        // public string? NewName { get; set; }
    }
}