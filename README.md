# Auto Spectre

 _[![AutoSpectre.SourceGeneration Nuget Version](https://img.shields.io/nuget/v/AutoSpectre.SourceGeneration?style=flat-square&label=NuGet%3A%20AutoSpectre.SourceGeneration)](https://www.nuget.org/packages/AutoSpectre.SourceGeneration)_ _[![AutoSpectre](https://img.shields.io/nuget/v/AutoSpectre?style=flat-square&label=NuGet%3A%20AutoSpectre)](https://www.nuget.org/packages/AutoSpectre)_ _[![AutoSpectre.Analyzer](https://img.shields.io/nuget/v/AutoSpectre.Analyzer?style=flat-square&label=NuGet%3A%20AutoSpectre.Analyzer)](https://www.nuget.org/packages/AutoSpectre.Analyzer)_

Source generator project to generate classes that can be used in a console to prompt for values using Spectre.Console

## Short Guide

Decorate a class with the AutoSpectreForm attribute and then decorate the properties (must be settable) with a TextPrompt or SelectPrompt attribute.

**NOTE** The `AskAttribute` has been marked as obsolete and split into `TextPromptAttribute` and `SelectPromptAttribute` to avoid having a lot of properties only relevant for one or another type of prompt.

### Example input

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

Behind the scenes this will generate an interface factory and implentation using `Spectre.Console` to prompt for the values.

### Example output

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

### How to call

```csharp
IExampleSpectreFactory formFactory = new ExampleSpectreFactory();
            
// The form object is initialized in the Get method
var form = formFactory.Get();

// We pass a pre initialized form object to the get method
var preinitializedForm = new Example();
preinitializedForm = formFactory.Get(preinitializedForm); 
```

## The form attribute

The class that you wan't populated, should be decorated with the `AutoSpectreForm` attribute. If it has at least one valid property or method decorated with an attribute the SpectreFactory is generated.

Since version 0.5.0 the class is allowed to not have an empty constructor. But in that case the Get/GetAsync method will change to be for instance. Requirering you to new up the class before calling the Get method

```csharp
public void Get(Example destination)
```

Compared to below where destination can be preinitialized or null (in which case the result will be newed up in the call)

```csharp
public Example Get(Example destination = null)
```

## The property attributes

These properties are applied to properties. The properties must be settable and public. The two attributes share the following Properties

* Title (can be used to overrule the default title)
* Condition (points to a property or method that returns bool to determine if the given property should be prompted for)
* NegateCondition negates the condition.

### TextPromptAttribute

This will in the end present some kind of input prompt to the user, that tries to prompt the use for the value of a given type. Per default it will try to convert a string to the given value. However if the type is bool it will produce a ConfirmationPrompt (y/n). If the type is an enum it will generate a selection prompt (but unlike using the SelectPrompt, you do not have to provide a source).

If the type is another class that has been decorated with the AutoSpectreForm attribute it will use that forms SpectreFactory to prompt for the values.

#### Default value

You can apply a defaultvalue like this:
`public string FirstName { get; set; } = "John Doe";`

The default value mechanism is not perfect in it's current state. But it can help you some of the way. If it's a value that we can't use as default value will be ignored. Also note that we will not look for default values set in the constructor.

Default value is only currently used for single items (not enumerables)

#### IEnumerable types

In the case of a TextPromptAttribute and an IEnumerable&lt;T&gt; value. The rules above will be applied to a single type, and after collecting input the user will be prompted if the want's to continue.

#### Other functionality

The textprompt allows you to

* add [validation](#validation), see more below
* add a password prompt by using the `Secret` and/or `Mask` property
* control styles with the `DefaultValueStyle`for the DefaultValue and `PromptStyle` for the prompt itself. See [styles](#styles)

### SelectPromptAttribute

The select prompt will adjust based on whether or not the property type is single or enumerable. If it's single select prompt will be used and other a multiselect prompt.

It's required that you define a source. Either by pointing the Source property to the name of the property or method that returns the sources to use or by convention (see [source convention](#source))

Here you can use the [converter](#converter) to control how the values are presented to the user.

Other interesting attribute properties are:

* PageSize (defaults to 10)
* WrapAround. If set to true we can cycle through the elements. If you reach the end you will go to the first element and vice versa
* MoreChoicesText. If set this will be displayed when you reach the end of the page size
* InstructionsText (only for multiselect). This is custom text to guide them to select mulitiple items
* HighlightStyle

## The method attribute

If you wan't to do something that doesn't fit prompting. Between some of the steps you can use the `TaskStepAttribute` to do that.

The filosophy for this method is that you can do 'custom' things.

* You can choose display whatever you may want to the user (for instance figlet text)
* You can do a lookup to a database or an api.
* You can do some processing of the data you currently collected
* If the current supplied property step attributes does not fit your needs you do some custom prompting to set a property values. *NOTE* If you use the Status bar, prompting is not allowed, so it would be important that UseStatus is set to false in that case.

### Requirements for the method

It can return either void or Task. If the return type is Task this will affect how the SpectreFactory is generated. As the Get call will be async instead and be named GetAsync.

It allows you to have `IAnsiConsole` as a parameter, and that will be injected into the method when it's called. This should make it easier to do testing if that is desired.

### Title

Unlike the property based step attributes we will not generate a default title if the Title is not set. If you input an IAnsiConsole it might be overkill to also add a Title. But there is nothing hindering you from doing so.

### Status bar

You can control whether or not you want a status bar(spinner) to run when the method is called. This is done by setting the `UseStatus` property to true and adding text to the `StatusText` property. Both are required. The choice was that it was more intuitive that the status will be displayed by having two properties instead of StatusText being optional. You can control the spinner type and style by using the `SpinnerType` and `SpinnerStyle` properties.

The spinner type is set by using the `SpinnerKnownTypes` this has been generated from the KnownTypes. Custom spinner types are not possible. Alternatively you can generate the status inside the method, and thereby allowing you all the flexibility you want.

## Collections strategy

When requesting input for a collection we will often work with a List&lt;T&gt; and converted it to the type of the property. Most cases should be covered. But here is a short list of some of the "conversions" behind the scenes.

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

    [SelectPrompt(Source = nameof(Items), Title = "Select multiple items")]
    public IReadOnlyList<string> Multiselect { get; set; }

    [SelectPrompt(Source = nameof(Items))]
    public string[] ArrayMultiSelect { get; set; } = Array.Empty<string>();

    [SelectPrompt(Source = nameof(Items))]
    public ImmutableArray<string> ArrayMultiSelectMultiple { get; set; }

    [SelectPrompt(Source = nameof(Numbers))]
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

You can add a converter method that will transform a given class to a string. This can be used when the attribute is SelectPrompt to give a string representation of a class. Currently the converter
must be a method on the class with the [AutoSpectreForm] attribute on and it must be at least public or internal. The method should take the given type as input parameter and return a string.

### Example

#### Code

```csharp
public record Person(string FirstName, string LastName);

[AutoSpectreForm]
public class ConverterForms
{
    [SelectPrompt(Title = "Select person", Source = nameof(GetPersons), Converter = nameof(PersonToString))]
    public Person? Person { get; set; }

    [SelectPrompt(Title = "Select persons", Source = nameof(GetPersons), Converter = nameof(PersonToString))] 
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

You can define validation by using the `Validator` property on the `TextPromptAttribute` or by following the {PropertyName}Validator convention.

The method being pointed to should return a nullable string. If validation is succesfull `null` should be returned, otherwise the validation error text message should be returned.

Based on the return type there are two types of parameters needed

* Single property. It's expected that the method has one parameter that is the same as the property
* Enumerable property. It's expected that the first parameter is an IEnumerable of the property type and the second parameter is the type

### Example

```csharp
[TextPrompt(Validator = nameof(ValidateAge))]
public int Age { get; set; }

[TextPrompt()]
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

## Styles

There are different properties to control the style. We won't go into detail here but we take a string as input and try to evaluate the style. If it can't be parsed an error will be outputted in the build log(since it's a source generator it will often first appear after a build).

Styles can be, in very rough terms, [colors](https://spectreconsole.net/appendix/colors) or [styles](https://spectreconsole.net/appendix/styles) and used in different combinations. Try them out, you will be "told" if it's not allowed by the AutoSpectre SourceGenerator. :)

* Red
* Green slowblink
* Red on white (red foreground on white background)
* italic blue on yellow

## Conditions

Using the `Condition` you can instruct whether prompting should occur for a given property. The condition should point to either a public bool property or a public method with no parameters returning bool. You can negate this by using the `NegateCondition` property.

### Example

```csharp
[AutoSpectreForm]
public class ConditionSampleForm
{
    [TextPrompt]
    public bool AskFriendlyCondition { get; set; }
    
    [TextPrompt(Title ="Please sir what is your name?")]
    public string AskFriendly { get; set; }
    
    [TextPrompt(Title = "What is your #!^$ name", Condition = nameof(AskFriendlyCondition), NegateCondition = true)]
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

### Source

You can leave out the Source in the SelectPromptAttribute if you have a property or method that is named {NameOfProperty}Source and have the correct structure ( No input parameters and returns an enumerable of the type of the given property)

#### Example

```csharp
[SelectPrompt]
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
[SelectPrompt(AskType = AskType.Selection)]
public FullName Name { get; set; }

public string NameConverter(FullName name) => $"{name.FirstName} {name.LastName}";
```

### Validation

It's possible to leave out the `Validator` in the `TextPromptAttribute` if you have a method with the name {NameOfProperty}Validator and the correct structure (see the part about Validation)

#### Example

```csharp
[TextPrompt]
public int Age {get;set;}

public string? AgeValidator(int age)
{
    return age >= 18 ? null : "Age must be at least 18";
}
```

### Condition

It's possible to leave out the `Condition` in a prompt attribute if you have a method with no parameters returning bool or a property returning bool that matches {NameOfProperty}Condition. If you want to negate this you will still need to provide the `NegateCondition` manually.

```csharp
[TextPrompt]
public string FirstName { get; set; }

public bool FirstNameCondition => true;

[TextPrompt(NegateCondition = true)]
public string NegatePrompt { get; set; }

public bool NegatePromptCondition => false;
```
