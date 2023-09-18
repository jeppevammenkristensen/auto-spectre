using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests;

public class DefaultValueGeneratorTests : AutoSpectreGeneratorTestsBase
{
    public DefaultValueGeneratorTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public void DefaultValueCanPointToSelf()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(DefaultValueSource=nameof(Property))]
                         public string Property {get;set;} = "Jeppe"
                    }
                    """).OutputShouldContain(".DefaultValue(destination.Property)");
    }
    
    [Theory]
    [InlineData("public string DefaultValueSource {get;set}", ".DefaultValue(destination.DefaultValueSource)")]
    [InlineData("public static string DefaultValueSource {get;set}", ".DefaultValue(TestForm.DefaultValueSource)")]
    [InlineData("public static string DefaultValueSource() => string.Empty;", ".DefaultValue(TestForm.DefaultValueSource())")]
    [InlineData("public string DefaultValueSource() => string.Empty;", ".DefaultValue(destination.DefaultValueSource())")]
    public void DefaultValueValidSource(string source, string expected)
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(DefaultValueSource=nameof(DefaultValueSource))]
                         public string Property {get;set;} = "Jeppe"
                         
                         {{ source }}
                    }
                    """).OutputShouldContain(expected);
    }
    
    [Theory]
    [InlineData("private string DefaultValueSource {get;set}")]
    [InlineData("private static string DefaultValueSource {get;set}")]
    [InlineData("private static string DefaultValueSource() => string.Empty;")]
    [InlineData("public int DefaultValueSource() => string.Empty;")]
    public void DefaultValueIsInvalidReturnsDiagnostic(string source)
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(DefaultValueSource=nameof(DefaultValueSource))]
                         public string Property {get;set;} = "Jeppe"
                         
                         {{ source }}
                    }
                    """).ShouldHaveSourceGeneratorDiagnostic(DiagnosticIds.Id0025_DefaultValueSource_NotFound);
    }
    
    [Theory]
    [InlineData("public string DefaultValueSource {get;set}", ".DefaultValue(destination.DefaultValueSource)")]
    [InlineData("public static string DefaultValueSource {get;set}", ".DefaultValue(TestForm.DefaultValueSource)")]
    [InlineData("public static string DefaultValueSource() => string.Empty;", ".DefaultValue(TestForm.DefaultValueSource())")]
    [InlineData("public string DefaultValueSource() => string.Empty;", ".DefaultValue(destination.DefaultValueSource())")]
    public void DefaultValueValidSourceTypeEnumerable(string source, string expected)
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(DefaultValueSource=nameof(DefaultValueSource))]
                         public List<string> Property {get;set;} = "Jeppe"
                         
                         {{ source }}
                    }
                    """).OutputShouldContain(expected);
    }
}