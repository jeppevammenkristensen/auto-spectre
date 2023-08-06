[![Nuget](https://img.shields.io/nuget/v/AutoSpectre.SourceGeneration?style=flat-square)](https://www.nuget.org/packages/AutoSpectre.SourceGeneration)

# Auto Spectre

Source generator project to generate classes that can be used in a console to prompt for values using Spectre.Console

## Short Guide

Decorate a class with the AutoSpectreForm attribute and then decorate the properties (must be settable) with AskAttribute.

### Example input

```csharp
 [AutoSpectreForm]
public class Example
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
    public ChildFormClass ChildForm { get; set; } = new (); 

    [Ask]
    public IReadOnlyList<ChildFormClass> Investors { get; set; } = new List<ChildFormClass>();

    
    [Ask(AskType = AskType.Selection)]
    //[Ask(AskType = AskType.Selection, SelectionSource = nameof(ItemSource))]
    public string Item { get; set; } = string.Empty;
    public List<string> ItemSource { get; } = new List<string>() { "Alpha", "Bravo", "Charlie" };

    [Ask(AskType = AskType.Selection)]
    //[Ask(AskType = AskType.Selection, Converter = nameof(SpecialProjectionConverter))]
    public List<int> SpecialProjection { get; set; } = new();

    public string SpecialProjectionConverter(int source) => $"Number {source}";
    public List<int> SpecialProjectionSource { get; set; } = new(){1,2,3,4};
    
    [Ask]
    // [Ask(Validator = nameof(EnterYearValidator))]
    public int EnterYear { get; set; }

    public string? EnterYearValidator(int year)
    {
        return year <= DateTime.Now.Year ? null : "Year cannot be larger than current year";
    }

    [Ask] public HashSet<string> Names { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public string? NamesValidator(List<string> items, string newItem)
    {
        if (newItem == "Foobar")
            return "Cannot be Foobar";

        if (items.Contains(newItem))
            return $"{newItem} has already been added";

        return null;
    }
    
    [Ask]
    public bool AddExistingName { get; set; }
    
    [Ask(Condition = nameof(AddExistingName))]
    public string? ExistingName { get; set; }
    
    [Ask(Condition = nameof(AddExistingName), NegateCondition = true)]
    public string? NewName { get; set; }
    
}
```

Behind the scenes this will generate an interface factory and implentation using `Spectre.Console` to prompt for the values.

### Example output

```csharp
 public interface IExampleSpectreFactory
{
    Example Get(Example destination = null);
}

public class ExampleSpectreFactory : IExampleSpectreFactory
{
    public Example Get(Example destination = null)
    {
        IChildFormClassSpectreFactory ChildFormClassSpectreFactory = new ChildFormClassSpectreFactory();
        destination ??= new Test.Example();
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
            AnsiConsole.MarkupLine("Enter [green]ChildForm[/]");
            var item = ChildFormClassSpectreFactory.Get();
            destination.ChildForm = item;
        }

        // Prompt for values for destination.Investors
        {
            List<ChildFormClass> items = new List<ChildFormClass>();
            bool continuePrompting = true;
            do
            {
                {
                    AnsiConsole.MarkupLine("Enter [green]Investors[/]");
                    var newItem = ChildFormClassSpectreFactory.Get();
                    items.Add(newItem);
                }

                continuePrompting = AnsiConsole.Confirm("Add more items?");
            }
            while (continuePrompting);
            System.Collections.Generic.IReadOnlyList<Test.ChildFormClass> result = items;
            destination.Investors = result;
        }

        destination.Item = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Enter [green]Item[/]").PageSize(10).AddChoices(destination.ItemSource.ToArray()));
        destination.SpecialProjection = AnsiConsole.Prompt(new MultiSelectionPrompt<int>().Title("Enter [green]SpecialProjection[/]").UseConverter(destination.SpecialProjectionConverter).PageSize(10).AddChoices(destination.SpecialProjectionSource.ToArray()));
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

### How to call

```csharp
IExampleSpectreFactory formFactory = new ExampleSpectreFactory();
            
// The form object is initialized in the Get method
var form = formFactory.Get();

// We pass a pre initialized form object to the get method
var preinitializedForm = new Example();
preinitializedForm = formFactory.Get(preinitializedForm); 
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

![Alt text](/doc/multi-select.png?raw=true)

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

## Converter

You add a converter method that will transform a given class to a string. This can be used when the AskType is Selection to give a string representation of a class. Currently the converter
must be a method on the class with the [AutoSpectreForm] attribute on and it must be at least public or internal. The method should take the given type as input parameter and return a string.

### Example

#### Code

```csharp
public record Person(string FirstName, string LastName);

[AutoSpectreForm]
public class ConverterForms
{
    [Ask(Title = "Select person", SelectionSource = nameof(GetPersons), AskType = AskType.Selection, Converter = nameof(PersonToString))]
    public Person? Person { get; set; }

    [Ask(Title = "Select persons", SelectionSource = nameof(GetPersons), AskType = AskType.Selection, Converter = nameof(PersonToString))] 
    public List<Person> Persons { get; set; } = new List<Person>();
    
    public string PersonToString(Person person) => $"{person.FirstName} {person.LastName}";
    
    public IEnumerable<Person> GetPersons()
    {
        yield return new Person("John", "Doe");
        yield return new Person("Jane", "Doe");
        yield return new Person("John", "Smith");
        yield return new Person("Jane", "Smith");
    }
}
```

#### Generated

```csharp
public class ConverterFormsSpectreFactory : IConverterFormsSpectreFactory
{
    public ConverterForms Get(ConverterForms destination = null)
    {
        destination ??= new ConsoleApp1.ConverterForms();
        destination.Person = AnsiConsole.Prompt(new SelectionPrompt<ConsoleApp1.Person?>().Title("Select person").UseConverter(destination.PersonToString).PageSize(10).AddChoices(destination.GetPersons().ToArray()));
        destination.Persons = AnsiConsole.Prompt(new MultiSelectionPrompt<ConsoleApp1.Person>().Title("Select persons").UseConverter(destination.PersonToString).PageSize(10).AddChoices(destination.GetPersons().ToArray()));
        return destination;
    }
}
```

## Validation

You can define validation by using the `Validator` property on the `AskAttribute` or by following the {PropertyName}Validator convention. Validation will only take effect on AskType normal.

The method being pointed to should return a nullable string. If validation is succesfull `null` should be returned, otherwise the validation error text message should be returned.

Based on the return type there are two types of parameters needed

* Single property. It's expected that the method has one parameter that is the same as the property
* Enumerable property. It's expected that the first parameter is an IEnumerable of the property type and the second parameter is the type

### Example

```csharp
[Ask(Validator = nameof(ValidateAge))]
public int Age { get; set; }

[Ask()]
public int[] Ages { get; set; } = Array.Empty<int>();

public string? AgesValidator(List<int> items, int item)
{
    if (ValidateAge(item) is { } error)
        return error;
    
    if (items?.Contains(item) == true)
    {
        return $"{item} allready added";
    }

    return null;
}

public string? ValidateAge(int age)
{
    return age >= 18 ? null : "Age must be at least 18";
}
```

### Generated

```csharp
destination.Age = AnsiConsole.Prompt(new TextPrompt<int>("Enter [green]Age[/]").Validate(
ctx =>
{
    var result = destination.ValidateAge(ctx);
    return result == null ? ValidationResult.Success() : ValidationResult.Error(result);
}));
// Prompt for values for destination.Ages
{
    List<int> items = new List<int>();
    bool continuePrompting = true;
    do
    {
        bool valid = false;
        while (!valid)
        {
            var item = AnsiConsole.Prompt(new TextPrompt<int>("Enter [green]Ages[/]"));
            var validationResult = destination.AgesValidator(items, item);
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
    int[] result = items.ToArray();
    destination.Ages = result;
}
```

## Conditions

Using the `Condition` you can instruct whether prompting should occur for a given property. The condition should point to either a public bool property or a public method with no parameters returning bool. You can negate this by using the `NegateCondition` property.

### Example

```csharp
[AutoSpectreForm]
public class ConditionSampleForm
{
    [Ask]
    public bool AskFriendlyCondition { get; set; }
    
    [Ask(Title ="Please sir what is your name?")]
    public string AskFriendly { get; set; }
    
    [Ask(Title = "What is your #!^$ name", Condition = nameof(AskFriendlyCondition), NegateCondition = true)]
    public string AskHostile { get; set; }
}
```

### Generated

```csharp
destination.AskFriendlyCondition = AnsiConsole.Confirm("Enter [green]AskFriendlyCondition[/]");
if (destination.AskFriendlyCondition == true)
{
    destination.AskFriendly = AnsiConsole.Prompt(new TextPrompt<string>("Please sir what is your name?"));
}

if (destination.AskFriendlyCondition == false)
{
    destination.AskHostile = AnsiConsole.Prompt(new TextPrompt<string>("What is your #!^$ name"));
}
```

## Conventions

The following conventions come into play

### SelectionSource

You can leave out the SelectionSource in the Ask Attribute if you have a property or method that is named {NameOfProperty}Source and have the correct structure ( No input parameters and returns an enumerable of the type of the given property)

#### Example

```csharp
[Ask(AskType = AskType.Selection)]
public string Name { get; set; }

public IEnumerable<string> NameSource()
{
    yield return "John Doe";
    yield return "Jane Doe";
    yield return "John Smith";
}
```

### Converter

You can also leave out the `Converter` in the `AskAttribute` if you have a method with the name {NameOfProperty}Converter and the correct structure (One parameter of the same type as the property and returning a string)

#### Example

```csharp
[Ask(AskType = AskType.Selection)]
public FullName Name { get; set; }

public string NameConverter(FullName name) => $"{name.FirstName} {name.LastName}";
```

### Validation

It's possible to leave out the `Validator` in the `AskAttribute` if you have a method with the name {NameOfProperty}Validator and the correct structure (see the part about Validation)

#### Example

```csharp
[Ask]
public int Age {get;set;}

public string? AgeValidator(int age)
{
    return age >= 18 ? null : "Age must be at least 18";
}
```

### Condition

It's possible to leave out the `Condition` in the `AskAttribute` if you have a method with no parameters returning bool or a property returning bool that matches {NameOfProperty}Condition. If you want to negate this you will still need to provide the `NegateCondition` manually.

```csharp
[Ask]
public string FirstName { get; set; }

public bool FirstNameCondition => true;

[Ask(NegateCondition = true)]
public string NegatePrompt { get; set; }

public bool NegatePromptCondition => false;
```