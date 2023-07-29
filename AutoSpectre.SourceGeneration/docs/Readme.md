# AutoSpectre Source Generation

This allows you to decorate a class with the `AutoSpectreForm` attribute and properties on that class with `AskAttribute` and behind the scenes a Form will be generated to request input using `Spectre.Console`

## Example

### Code

```csharp
 [AutoSpectreForm]
 public class Someclass
 {
     [Ask(AskType = AskType.Normal, Title = "Add item")] 
     public int[] IntItems { get; set; } = Array.Empty<int>();

     [Ask(Title = "Enter first name")]
     public string? FirstName { get; set; }

     [Ask]
     public bool LeftHanded { get; set; }

     [Ask(Title = "Choose your [red]value[/]")]
     public SomeEnum Other { get; set; }

     [Ask] 
     public Name Owner { get; set; } = new Name(); 

     [Ask]
     public IReadOnlyList<Name> Investors { get; set; } = new List<Name>();

     [Ask(AskType = AskType.Selection, SelectionSource = nameof(Items))]
     public string Item { get; set; } = string.Empty;

     public List<string> Items { get; } = new List<string>() { "Alpha", "Bravo", "Charlie" };
 }
```

### Output

```csharp
 public interface ISomeclassSpectreFactory
 {
     Someclass Get(Someclass destination = null);
 }

 public class SomeclassSpectreFactory : ISomeclassSpectreFactory
 {
     public Someclass Get(Someclass destination = null)
     {
         INameSpectreFactory NameSpectreFactory = new NameSpectreFactory();
         destination ??= new Test.Someclass();
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

         destination.FirstName = AnsiConsole.Prompt(new TextPrompt<string?>("Enter first name").AllowEmpty());
         destination.LeftHanded = AnsiConsole.Confirm("Enter [green]LeftHanded[/]");
         destination.Other = AnsiConsole.Prompt(new SelectionPrompt<SomeEnum>().Title("Choose your [red]value[/]").PageSize(10).AddChoices(Enum.GetValues<SomeEnum>()));
         {
             AnsiConsole.MarkupLine("Enter [green]Owner[/]");
             var item = NameSpectreFactory.Get();
             destination.Owner = item;
         }

         // Prompt for values for destination.Investors
         {
             List<Name> items = new List<Name>();
             bool continuePrompting = true;
             do
             {
                 {
                     AnsiConsole.MarkupLine("Enter [green]Investors[/]");
                     var newItem = NameSpectreFactory.Get();
                     items.Add(newItem);
                 }

                 continuePrompting = AnsiConsole.Confirm("Add more items?");
             }
             while (continuePrompting);
             System.Collections.Generic.IReadOnlyList<Test.Name> result = items;
             destination.Investors = result;
         }

         destination.Item = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Enter [green]Item[/]").PageSize(10).AddChoices(destination.Items.ToArray()));
         return destination;
     }
 }
```

## Credits

[Fear icons created by Smashicons - Flaticon](https://www.flaticon.com/free-icons/fear "fear icons")