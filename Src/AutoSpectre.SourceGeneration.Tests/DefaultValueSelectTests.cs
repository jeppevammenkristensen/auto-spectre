using Xunit;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests;

public class DefaultValueSelectTests : AutoSpectreGeneratorTestsBase
{
    public DefaultValueSelectTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Theory]
    [InlineData("public string TestDefaultValue => string.Empty;", "form.TestDefaultValue")]
    [InlineData("public string TestDefaultValue() => string.Empty;", "form.TestDefaultValue()")]
    [InlineData("public string TestDefaultValue = string.Empty;", "form.TestDefaultValue")]
    [InlineData("public readonly string TestDefaultValue = string.Empty;", "form.TestDefaultValue")]
    [InlineData("public static string TestDefaultValue => string.Empty;", "Test.TestForm.TestDefaultValue")]
    [InlineData("public static string TestDefaultValue() => string.Empty;", "Test.TestForm.TestDefaultValue()")]
    [InlineData("public static readonly string TestDefaultValue = string.Empty;", "Test.TestForm.TestDefaultValue")]
    public void DefaultValueAddedThroughConvention(string defaultSource, string call)
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [SelectPrompt]
                         public string Test {get;set;}

                         public List<string> TestSource => new();

                         {{defaultSource}}
                    }
                    """).OutputShouldContain($".DefaultValue({call})");
    }

    [Fact]
    public void DefaultValueAddedThroughExplicitName()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [SelectPrompt(DefaultValueSource = nameof(OverrideDefault))]
                         public string Test {get;set;}

                         public List<string> TestSource => new();

                         public string OverrideDefault => string.Empty;
                    }
                    """).OutputShouldContain(".DefaultValue(form.OverrideDefault)");
    }

    [Fact]
    public void DefaultValueAddedForEnumerableProperty()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [SelectPrompt]
                         public string[] Items {get;set;}

                         public List<string> ItemsSource => new();

                         public string ItemsDefaultValue => string.Empty;
                    }
                    """).OutputShouldContain(".DefaultValue(form.ItemsDefaultValue)");
    }

    [Fact]
    public void NoDefaultValueMatchByExplicitNameReturnsExpectedDiagnostic()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [SelectPrompt(DefaultValueSource = "NonExistent")]
                         public string Test {get;set;}

                         public List<string> TestSource => new();
                    }
                    """).ShouldHaveSourceGeneratorDiagnosticOnlyOnce(DiagnosticIds.Id0025_DefaultValueSource_NotFound);
    }

    [Fact]
    public void DefaultValueByConventionWithNoMatchDoesNotReportDiagnostic()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [SelectPrompt]
                         public string Test {get;set;}

                         public List<string> TestSource => new();
                    }
                    """)
            .HasNoSourceGeneratorDiagnosticWith(DiagnosticIds.Id0025_DefaultValueSource_NotFound);
    }
    
    [Fact]
    public void DefaultValueByConventionWithMatchDoesNotReportDiagnostic()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [SelectPrompt]
                         public string[] Test {get;set;}

                         public List<string> TestSource => new();
                    }
                    """)
            .HasNoSourceGeneratorDiagnosticWith(DiagnosticIds.Id0025_DefaultValueSource_NotFound);
    }
}
