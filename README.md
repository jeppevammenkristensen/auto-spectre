[![Nuget](https://img.shields.io/nuget/v/AutoSpectre.SourceGeneration?style=flat-square)](https://www.nuget.org/packages/AutoSpectre.SourceGeneration)

# Auto Spectre
Source generator project to generate classes that can be used in a console to prompt for values using Spectre.Console

## Short Guide
Decorate a class with the AutoSpectreForm attribute and then decorate the properties (must be settable) with AskAttribute. 

### Example input

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

### Example output ###
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

### How to call ###

```csharp
ISomeClassSpectreFactory factory = new SomeClassSpectreFactory();
var item = factory.Get();
// or 
SomeClass someclass = new();
factory.Get(someclass);
```

## Collections

### Multiselect

if you use the AskType.Selection combined with a IEnumerable type the user will be presented with a multiselect. 

This property

```csharp
[Ask(AskType = AskType.Selection, SelectionSource = nameof(Items))]
public string[] ArrayMultiSelect { get; set; } = Array.Empty<string>();
```

Will become
```csharp
destination.ArrayMultiSelect = AnsiConsole.Prompt(new MultiSelectionPrompt<string>().Title("Enter [green]ArrayMultiSelect[/]").PageSize(10).AddChoices(destination.Items.ToArray())).ToArray();
```

With the prompt below

![Alt text](doc/multi-select.png?raw=true)

### Strategy

The multiselection prompt always returns List of given type. In the above example (`List<string>`) But AutoSpectre will attempt to adjust to the property type. 
* Array will result in a ToList()
* HashSet will be initalized with new HashSet<>
* Immutable collection types will append ToImmutable{Type}() 
* Interfaces like `IList<T>` `ICollection<T>` `IEnumerable<T>` `IReadOnlyCollection<T>` `IReadOnlyList<T>` have their values directly set as `List<T>` inherit directly from them

### Example

#### Code

```csharp
[AutoSpectreForm]
public class CollectionSample
{

    [Ask(AskType = AskType.Selection, SelectionSource = nameof(Items), Title = "Select multiple items")]
    public IReadOnlyList<string> Multiselect { get; set; }

    [Ask(AskType = AskType.Selection, SelectionSource = nameof(Items))]
    public string[] ArrayMultiSelect { get; set; } = Array.Empty<string>();

    [Ask(AskType = AskType.Selection, SelectionSource = nameof(Items))]
    public ImmutableArray<string> ArrayMultiSelectMultiple { get; set; }

    [Ask(AskType = AskType.Selection, SelectionSource = nameof(Numbers))]
    public List<int> ListNumbers { get; set; } = new List<int>();

    public int[] Numbers { get; } = new[] {1, 2, 3};
    
    public List<string> Items { get; } = new List<string>() { "Alpha", "Bravo", "Charlie" };

}
```

#### Generated
```csharp
public interface ICollectionSampleSpectreFactory
{
    CollectionSample Get(CollectionSample destination = null);
}

public class CollectionSampleSpectreFactory : ICollectionSampleSpectreFactory
{
    public CollectionSample Get(CollectionSample destination = null)
    {
        destination ??= new Test.CollectionSample();
        destination.Multiselect = AnsiConsole.Prompt(new MultiSelectionPrompt<string>().Title("Select multiple items").PageSize(10).AddChoices(destination.Items.ToArray()));
        destination.ArrayMultiSelect = AnsiConsole.Prompt(new MultiSelectionPrompt<string>().Title("Enter [green]ArrayMultiSelect[/]").PageSize(10).AddChoices(destination.Items.ToArray())).ToArray();
        destination.ArrayMultiSelectMultiple = AnsiConsole.Prompt(new MultiSelectionPrompt<string>().Title("Enter [green]ArrayMultiSelectMultiple[/]").PageSize(10).AddChoices(destination.Items.ToArray())).ToImmutableArray();
        destination.ListNumbers = AnsiConsole.Prompt(new MultiSelectionPrompt<int>().Title("Enter [green]ListNumbers[/]").PageSize(10).AddChoices(destination.Numbers.ToArray()));
        return destination;
    }
}
```


