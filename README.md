# Auto Spectre

 _[![AutoSpectre.SourceGeneration Nuget Version](https://img.shields.io/nuget/v/AutoSpectre.SourceGeneration?style=flat-square&label=NuGet%3A%20AutoSpectre.SourceGeneration)](https://www.nuget.org/packages/AutoSpectre.SourceGeneration)_ _[![AutoSpectre](https://img.shields.io/nuget/v/AutoSpectre?style=flat-square&label=NuGet%3A%20AutoSpectre)](https://www.nuget.org/packages/AutoSpectre)_ _[![AutoSpectre.Analyzer](https://img.shields.io/nuget/v/AutoSpectre.Analyzer?style=flat-square&label=NuGet%3A%20AutoSpectre.Analyzer)](https://www.nuget.org/packages/AutoSpectre.Analyzer)_

Source generator project to generate classes that can be used in a console to prompt for values using Spectre.Console

## Short Guide

Decorate a class with the AutoSpectreForm attribute and then decorate the properties (must be settable) with a TextPrompt or SelectPrompt attribute.

**NOTE** The `AskAttribute` that was marked as obsolete and split into `TextPromptAttribute` and `SelectPromptAttribute` to avoid having a lot of properties only relevant for one or another type of prompt, has been removed.

### Example input

```csharp
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

    [TextPrompt(ChoicesSource = nameof(NameChoices), ChoicesStyle = "red on yellow",
        ChoicesInvalidText = "Must be one of the names")]
    public string Name { get; set; } = null!;

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

    [SelectPrompt(Source = nameof(SearchStringSource), SearchEnabled = true, SearchPlaceholderText = "Placeholder \"text\"")]
    public string SearchString { get; set; }

    public string[] SearchStringSource() => new[] {"First", "Second"};
}
```

Behind the scenes this will generate an interface factory and implentation using `Spectre.Console` to prompt for the values.

### Example output

```csharp
/// <summary>
    /// Helps create and fill <see cref = "ExampleNamespace.Example"/> with values
    /// </summary>
    public interface IExampleSpectreFactory : ISpectreFactory<ExampleNamespace.Example>
    {
    }

    /// <summary>
    /// Helps create and fill <see cref = "ExampleNamespace.Example"/> with values
    /// </summary>
    public class ExampleSpectreFactory : IExampleSpectreFactory
    {
        public ExampleNamespace.Example Prompt(ExampleNamespace.Example form)
        {
            if (form == null)
                throw new ArgumentNullException(nameof(form));
            IOtherAutoSpectreFormClassSpectreFactory OtherAutoSpectreFormClassSpectreFactory = new OtherAutoSpectreFormClassSpectreFactory();
            var culture = new CultureInfo("da-DK");
            // Prompt for values for form.IntItems
            {
                List<int> items = new List<int>();
                bool continuePrompting = true;
                do
                {
                    var item = AnsiConsole.Prompt(new TextPrompt<int>("Add item").WithCulture(culture));
                    items.Add(item);
                    continuePrompting = AnsiConsole.Confirm("Add more items?");
                }
                while (continuePrompting);
                int[] result = items.ToArray();
                form.IntItems = result;
            }

            form.FirstName = AnsiConsole.Prompt(new TextPrompt<string?>("Enter first name").AllowEmpty().WithCulture(culture).DefaultValue(form.FirstNameDefaultValue).DefaultValueStyle("bold"));
            form.LeftHanded = AnsiConsole.Prompt(new ConfirmationPrompt("Enter [green]LeftHanded[/]"));
            form.Other = AnsiConsole.Prompt(new SelectionPrompt<ExampleNamespace.SomeEnum>().Title("Choose your [red]value[/]").PageSize(10).AddChoices(Enum.GetValues<ExampleNamespace.SomeEnum>()));
            form.Password = AnsiConsole.Prompt(new TextPrompt<string?>("Enter [green]Password[/]").AllowEmpty().WithCulture(culture).Secret('*'));
            form.Name = AnsiConsole.Prompt(new TextPrompt<string>("Enter [green]Name[/]").WithCulture(culture).AddChoices(ExampleNamespace.Example.NameChoices).InvalidChoiceMessage("Must be one of the names").ChoicesStyle("red on yellow"));
            {
                AnsiConsole.MarkupLine("Enter [green]ChildForm[/]");
                var item = new ExampleNamespace.OtherAutoSpectreFormClass();
                OtherAutoSpectreFormClassSpectreFactory.Promptitem);
                form.ChildForm = item;
            }

            // Prompt for values for form.Investors
            {
                List<OtherAutoSpectreFormClass> items = new List<OtherAutoSpectreFormClass>();
                bool continuePrompting = true;
                do
                {
                    {
                        AnsiConsole.MarkupLine("Enter [green]Investors[/]");
                        var newItem = new ExampleNamespace.OtherAutoSpectreFormClass();
                        OtherAutoSpectreFormClassSpectreFactory.PromptnewItem);
                        items.Add(newItem);
                    }

                    continuePrompting = AnsiConsole.Confirm("Add more items?");
                }
                while (continuePrompting);
                System.Collections.Generic.IReadOnlyList<ExampleNamespace.OtherAutoSpectreFormClass> result = items;
                form.Investors = result;
            }

            AnsiConsole.Status().SpinnerStyle("green on yellow").Spinner(Spinner.Known.Christmas).Start("This will take a while", ctx =>
            {
                form.DoSomething(AnsiConsole.Console);
            });
            form.Item = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Enter [green]Item[/]").PageSize(3).WrapAround(true).MoreChoicesText("Press down to see more choices").HighlightStyle("purple").AddChoices(form.ItemSource.ToArray()));
            form.SpecialProjection = AnsiConsole.Prompt(new MultiSelectionPrompt<int>().Title("Enter [green]SpecialProjection[/]").UseConverter(form.SpecialProjectionConverter).PageSize(10).InstructionsText("Check the special items you want to select").AddChoices(form.SpecialProjectionSource.ToArray()));
            form.EnterYear = AnsiConsole.Prompt(new TextPrompt<int>("Enter [green]EnterYear[/]").WithCulture(culture).Validate(ctx =>
            {
                var result = form.EnterYearValidator(ctx);
                return result == null ? ValidationResult.Success() : ValidationResult.Error(result);
            }));
            // Prompt for values for form.Names
            {
                List<string> items = new List<string>();
                bool continuePrompting = true;
                do
                {
                    bool valid = false;
                    while (!valid)
                    {
                        var item = AnsiConsole.Prompt(new TextPrompt<string>("Enter [green]Names[/]").WithCulture(culture));
                        var validationResult = form.NamesValidator(items, item);
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
                form.Names = result;
            }

            form.AddExistingName = AnsiConsole.Prompt(new ConfirmationPrompt("Enter [green]AddExistingName[/]"));
            if (form.AddExistingName == true)
            {
                form.ExistingName = AnsiConsole.Prompt(new TextPrompt<string?>("Enter [green]ExistingName[/]").AllowEmpty().WithCulture(culture));
            }

            if (form.AddExistingName == false)
            {
                form.NewName = AnsiConsole.Prompt(new TextPrompt<string?>("Enter [green]NewName[/]").AllowEmpty().WithCulture(culture));
            }

            form.SearchString = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Enter [green]SearchString[/]").PageSize(10).EnableSearch().SearchPlaceholderText("Placeholder \"text\"").AddChoices(form.SearchStringSource().ToArray()));
            return form;
        }
    }
```

### How to call

```csharp
// Initalize factory and form
var exampleSpectreFactory = new ExampleSpectreFactory();
var result = exampleSpectreFactory.Prompt(new Example());

// Initialize through SpectreFactory
ISpectreFactory<Example> exampleFactory = SpectreFactory.GetSpectreFactoryConsoleApp8_Example();
exampleSpectreFactory.Prompt(new Example());

// Prompt directly. Possible if the form has an empty constructor
var directPromptResult = SpectreFactory.Prompt_ConsoleApp8_Example();

// Through extension method
var promptThroughExtension = new Example().Prompt();

```

## The form attribute

The class that you wan't populated, should be decorated with the `AutoSpectreForm` attribute. If it has at least one valid property or method decorated with an attribute the SpectreFactory is generated.

### Culture

You can control the overall culture used by setting the culture property. This will be used for TextPrompts. If it's isn't set the CurrentUICulture will be used.

### ~~DisableDump~~ Removed since version 0.10.0

~~If you do want to generate the SpectreDump method, you can disable it using the DisableDump property.
Note that the SpectreDump will be removed in a future version~~

### Inheritance

This class can inherit from other classes that has the properties or methods decorated with attributes. It is not required that the baseclass has the AutoSpectreForm. The steps on the base class will be generated last

```csharp
public class BaseClass 
{
    [TextPrompt]
    public string BaseProperty { get;set;}
}

[AutoSpectreForm]
public class DerivedClass : BaseClass
{
    [TextPrompt]
    public string DerivedProperty { get;set;}
}
```

### Constructor

Since version 0.5.0 the class is allowed to not have an empty constructor. But in that case the Prompt/PromptAsync method will change to be for a passed in instance. Requirering you to new up the class before calling the Prompt method.

Since version 0.10.0 the passed in form must not be null.

~~If you have multiple constructors, you can decorate the constructor intended for initalization by using the `UsedConstructor`attribute~~

```csharp
public void Get(Example destination)
```

## The property attributes

These properties are applied to properties. The properties must be settable and public. The two attributes share the following Properties

* Title (can be used to overrule the default title)
* Condition (points to a property or method that returns bool to determine if the given property should be prompted for)
* NegateCondition negates the condition.

### A short note on sources

Some of the properties of the attributes are source properties. It can vary if they can point to fields, properties or methods. But a general rule is that the sources must be public and available on the `AutoSpectreForm` decorated class.

### TextPromptAttribute

This will in the end present some kind of input prompt to the user, that tries to prompt the use for the value of a given type. Per default it will try to convert a string to the given value. However if the type is bool it will produce a ConfirmationPrompt (y/n). If the type is an enum it will generate a selection prompt (but unlike using the SelectPrompt, you do not have to provide a source).

If the type is another class that has been decorated with the AutoSpectreForm attribute it will use that forms SpectreFactory to prompt for the values.

#### Default value

**Note** that the logic behind default value has changed. Earlier the default value was determined by the value set to a property like below:

`public string FirstName { get; set; } = "John Doe";`

The new approach from version 0.7.0 is that it can be set by using the `DefaultValueSource`. The source can be a method, property or field. And can be instance or static and must be public

The style can be controlled by setting the `DefaultValueStyle`

Default value is only currently used for single items (not enumerables)

#### IEnumerable types

In the case of a `TextPromptAttribute` and an `IEnumerable<T>` value. The rules above will be applied to a single type, and after collecting input the user will be prompted if the want's to continue.

#### Enums

If the property types is an enum, a SelectPrompt will be generates (much like when using the SelectPromptAttribute). The source will be generated based on the enum values. You can use EnableSearch and SearchPlaceholderText properties to control search text functionality.

#### Other functionality

The textprompt allows you to

* add [validation](#validation), see more below
* add a password prompt by using the `Secret` and/or `Mask` property
* control styles with the `DefaultValueStyle`for the DefaultValue and `PromptStyle` for the prompt itself. See [styles](#styles).
* support Choices (autocomplete) 

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
* EnableSearch ( and SearchPlaceholderText). This will enable search to limit displayed drop down values. The SearchPlaceholderText can control the search help text being displayed-

## The method attribute

If you wan't to do something that doesn't fit prompting. Between some of the steps you can use the `TaskStepAttribute` to do that.

The filosophy for this method is that you can do 'custom' things.

* You can choose display whatever you may want to the user (for instance figlet text)
* You can do a lookup to a database or an api.
* You can do some processing of the data you currently collected
* If the current supplied property step attributes does not fit your needs you do some custom prompting to set a property values. **NOTE** If you use the Status bar, prompting is not allowed, so it would be important that UseStatus is set to false in that case.

### Requirements for the method

It can return either void or Task. If the return type is Task this will affect how the SpectreFactory is generated. As the Prompt call will be async instead and be named PromptAsync.

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
    CollectionSample PromptCollectionSample destination = null);
}

public class CollectionSampleSpectreFactory : ICollectionSampleSpectreFactory
{
    public CollectionSample PromptCollectionSample destination = null)
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
    public ConverterForms PromptConverterForms destination)
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

The method pointed to must be public, but can be instance and static.

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

## Choices

You can define choices for a property by using the `ChoicesSource` property or by following the {PropertyName}Choices convention. The source can be a property, method (no parameters) or field that returns the singular type of the property.

The choices are autocomplete and will be displayed in the prompt. The style of how they are displayed can be controlled through the `ChoicesStyle` prpoerty. When the user types something that is not in the choices, an invalid text will be displayed (this can be overloaded with `ChoicesInvalidText`).

### Example

```csharp
[TextPrompt(
    ChoicesSource = nameof(NameChoices), 
    ChoicesStyle = "red on yellow",ChoicesInvalidText = "Must be one of the names")]
public string Name { get; set; } = null!;

public static readonly string[] NameChoices = new[] {"Kurt", "Krist", "David", "Pat"};
```

### Generated

```csharp
destination.Name = AnsiConsole.Prompt(
    new TextPrompt<string>("Enter [green]Name[/]")
    .WithCulture(culture)
    .AddChoices(Example.NameChoices)
    .ChoicesStyle("red on yellow")
    .InvalidChoiceMessage("Must be one of the names"));
```

## Type initialization

If you have a property type that is another form. For instance

```csharp
[AutoSpectreForm]
public class MainForm 
{
    [TextPrompt]
    public OtherForm ChildForm { get; set; }
}

[AutoSpectreForm]
public class OtherForm
{
    
    public OtherForm(string title)
    {

    }

    ...
}
```

If the constructor of the other type is not empty (no parameters), you will get an error. This can however be amended by using the `TypeInitalizer` and point it to a method that return the type you want to initalize. The method pointing to should be public or internal and should return the given type and not have any parameters.

```csharp
...

[TextPrompt(TypeInitializer = nameof(InitializeChildForm))]
public OtherForm ChildForm { get;set; }

public OtherForm InitializeChildForm() => new("Some title");

...
```

Like in other scenarios this can also be achieved by convention. This is done by naming the method Init{TypeName} and have the correct signature.

```csharp
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

## Factories

### ISpectreFactory and IAutoSpectreFactory

In the AutoSpectre library two general interfaces are exposed that the generated factories will implement

* `ISpectreFactory<T>` which exposes the Prompt method
* `IAutoSpectreFactory<T>` which exposes the PromptAsync method.

Both of the factories implements the `ISpectreFactory` which is a marker interface to identify factories.

These were introduced in version 0.10.0 and was a breaking change as previously the method name was Get/GetAsync which has now been changed to Prompt/PromptAsync. 

### SpectreFactory

A new static class has been created to help create factories and/or prompt for values. It will generate the following

* A method for creating the factory. For instance for SomeNamespace.Someclass the generated method will be called GetSpectreFactory_SomeNamespace_Someclass
* A method for prompting for the values that return an instance of the class filled will values will be created. The name will be Prompt_SomeNamespace_Someclass. If the class can be initalized without constructor it will have no parameters otherwise it will require an instance of the form class. (PromptAsync if async)
* An extension method will be added where Prompt can be called directly on an instance of a form class (PromptAsync if async)

**Note** Prior to version 0.10.0 extensions method SpectreDump was added. This one has been removed.