using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests;

public class ValidatorTests : AutoSpectreGeneratorTestsBase
{
    public ValidatorTests(ITestOutputHelper helper) : base(helper)
    {
    }
    
    [Theory]
    [InlineData("""public string? ValidateName(string name) { return name == "Jumping jack flash"; }""", "form")]
    [InlineData("""public static string? ValidateName(string name) { return name == "Jumping jack flash"; }""", "Test.ValidateClass")]
    public void PropertyWithValidatorDefinedThatIsValidReturnsExpected(string validValidator, string access)
    {
        GetGeneratedOutput($$"""
using AutoSpectre;
using System.Collections.Generic;

namespace Test;

[AutoSpectreForm]
public class ValidateClass
{
    [TextPrompt(Validator=nameof(ValidateName))]
    public string Name { get; set;}

    {{ validValidator}}
}
""").Should().Contain($@".Validate(ctx =>
            {{
                var result = {access}.ValidateName(ctx);
                return result == null ? ValidationResult.Success() : ValidationResult.Error(result);
            }})");
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
    [TextPrompt(Validator = nameof(ValidateAges))]
    public int[] Ages { get; set; } = Array.Empty<int>();

    public string? ValidateAges(List<int> items, int item)
    {
        if (ValidateAge(item) is { } error)
            return error;
        
        if (items?.Contains(item) == true)
        {
            return $"{item} already added";
        }

        return null;
    }
    
    public string? ValidateAge(int age)
    {
        return age >= 18 ? null : "Age must be at least 18";
    }
}
""").Should().Contain(@"var validationResult = form.ValidateAges(items, item);
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
    public void PropertyPointingToEnumerableOfOtherClassDecoratedWithAutoSpectreFromReturnsExpected()
    {
        GetGeneratedOutput("""
using AutoSpectre;
using System.Collections.Generic;

namespace Test;

[AutoSpectreForm]
public class Inner 
{
    [TextPrompt]
    public string Title {get;set;}
}

[AutoSpectreForm]
public class Example
{
    [TextPrompt()]
    public Inner[] InnerItems { get; set; } = Array.Empty<Inner>();

    public string? InnerItemsValidator(List<Inner> items, Inner item)
    {
        return null;
    }   
}
""").Should().Contain("var validationResult = form.InnerItemsValidator(items, newItem);");
    }
    
    [Fact]
    public void PropertyPointingToOtherClassDecoratedWithAutoSpectreFromReturnsExpected()
    {
        GetGeneratedOutput("""
using AutoSpectre;
using System.Collections.Generic;

namespace Test;

[AutoSpectreForm]
public class Inner 
{
    [TextPrompt]
    public string Title {get;set;}
}
[AutoSpectreForm]
public class Example
{
    [TextPrompt()]
    public Inner InnerItem { get; set; }

    public string? InnerItemValidator(Inner item)
    {
        return null;
    }   
}
""").Should().Contain("form.InnerItemValidator(item)");
    }
    
    [Theory]
    [InlineData("public string? InnerItemValidator(Inner[] items, Inner item){ return null }", "form.InnerItemValidator(items, newItem)")]
    [InlineData("public static string? InnerItemValidator(Inner[] items, Inner item){ return null }", "Example.InnerItemValidator(items, newItem)")]
    public void PropertyPointingToListWithOtherClassDecoratedWithAutoSpectreFromReturnsExpectedOutput(string validator, string expectedOutput)
    {
        GetOutput($$"""
using AutoSpectre;
using System.Collections.Generic;

namespace Test;

[AutoSpectreForm]
public class Inner 
{
    [TextPrompt]
    public string Title {get;set;}
}
[AutoSpectreForm]
public class Example
{
    [TextPrompt()]
    public Inner[] InnerItem { get; set; }

    {{ validator }}
}
""").OutputShouldContain(expectedOutput);
    }

    
    
    [Theory]
    [InlineData(false, "form")]
    [InlineData(true, "Test.Example")]
    public void PropertyWithEnumerableResultValidatorByConventionReturnsExpected(bool isStatic, string access)
    {
        GetGeneratedOutput($$"""
using AutoSpectre;
using System.Collections.Generic;

namespace Test;

[AutoSpectreForm]
public class Example
{
    [TextPrompt()]
    public int[] Ages { get; set; } = Array.Empty<int>();

    public {{ (isStatic ? "static" : "") }} string? AgesValidator(List<int> items, int item)
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
""").Should().Contain($$"""
                      var validationResult = {{ access }}.AgesValidator(items, item);
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
                      """);
    }
}