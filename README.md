[![Nuget](https://img.shields.io/nuget/v/AutoSpectre.SourceGeneration?style=flat-square)](https://www.nuget.org/packages/AutoSpectre.SourceGeneration)

# Auto Spectre
Source generator project to generate classes that can be used in a console to prompt for values using Spectre.Console

## Short Guide
Decorate a class with the AutoSpectreForm attribute and then decorate the properties (must be settable) with AskAttribute. 

For example:

```csharp
[AutoSpectreForm]
   public class Someclass
   {
       [Ask(Title = "Enter first name")]
       public string? FirstName { get; set; }

       [Ask]
       public bool LeftHanded { get; set; }

       [Ask]
       public bool Age { get; set; }

       [Ask(AskType = AskType.Selection, SelectionSource = nameof(Items))]
       public string Item { get; set; }

       public List<string> Items { get; } = new List<string>() { "Alpha", "Bravo", "Charlie" };

   }
```

Behind the scenes this will generate an interface factory and implentation using `Spectre.Console` to prompt for the values. 

**Example output:**
```csharp
 public interface ISomeclassSpectreFactory
 {
     Someclass Get(Someclass destination = null);
 }

 public class SomeclassSpectreFactory : ISomeclassSpectreFactory
 {
     public Someclass Get(Someclass destination = null)
     {
         destination ??= new Test.Someclass();
         destination.FirstName = AnsiConsole.Ask<>("Enter first name ");
         destination.LeftHanded = AnsiConsole.Confirm("Enter [green]LeftHanded [/]  ");
         destination.Age = AnsiConsole.Confirm("Enter [green]Age [/]  ");
         destination.Item = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Enter [green]Item [/]  ").PageSize(10).AddChoices(destination.Items.ToArray());
         return destination;
     }
 }

```
