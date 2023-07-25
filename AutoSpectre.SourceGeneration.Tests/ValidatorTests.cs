using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests;

public class ValidatorTests : AutoSpectreGeneratorTestsBase
{
    public ValidatorTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public void PropertyWithValidatorDefinedThatIsValidReturnsExpected()
    {
        GetGeneratedOutput("""
using AutoSpectre;
using System.Collections.Generic;

namespace Test;

[AutoSpectreForm]
public class ValidateClass
{
    [Ask(Validator=nameof(ValidateName))]
    public string Name { get; set;}

    public string? ValidateName(string name)
    {
        return name == "Jumping jack flash";
    }
}
""").Should().Contain(@".Validate(ctx =>
            {
                var result = destination.ValidateName(ctx);
                return result == null ? ValidationResult.Success() : ValidationResult.Error(result);
            })");
    }

    
    [Fact]
    public void PropertyWithEnumerableResultValidatorDefinedThatIsValidReturnsExpected()
    {
        GetGeneratedOutput("""
using AutoSpectre;
using System.Collections.Generic;

namespace Test;

[AutoSpectreForm]
public class Example
{
    // [Ask(Validator = nameof(ValidateAge) )]
    // public int Age { get; set; }

    [Ask(Validator = nameof(ValidateAges))]
    public int[] Ages { get; set; } = Array.Empty<int>();

    public string? ValidateAges(List<int> items, int item)
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
}
""").Should().Contain(@"var validationResult = destination.ValidateAges(items, item);
                        if (validationResult is { } error)
                        {
                            AnsiConsole.MarkupLine($""[red]{error}[/]"");
                            valid = false;
                        }
                        else
                        {
                            valid = true;
                            items.Add(item);
                        }");
    }
    
    [Fact]
    public void PropertyWithEnumerableResultValidatorByConventionReturnsExpected()
    {
        GetGeneratedOutput("""
using AutoSpectre;
using System.Collections.Generic;

namespace Test;

[AutoSpectreForm]
public class Example
{
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
}
""").Should().Contain(@"var validationResult = destination.AgesValidator(items, item);
                        if (validationResult is { } error)
                        {
                            AnsiConsole.MarkupLine($""[red]{error}[/]"");
                            valid = false;
                        }
                        else
                        {
                            valid = true;
                            items.Add(item);
                        }");
    }
}