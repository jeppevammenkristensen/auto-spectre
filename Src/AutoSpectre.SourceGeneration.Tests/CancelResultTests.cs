using Xunit;

namespace AutoSpectre.SourceGeneration.Tests;

public class CancelResultTests : AutoSpectreGeneratorTestsBase
{
    public CancelResultTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Theory]
    [InlineData("public string TestCancelResult => string.Empty;", "form.TestCancelResult")]
    [InlineData("public string TestCancelResult() => string.Empty;", "form.TestCancelResult")]
    [InlineData("public string TestCancelResult = string.Empty;", "form.TestCancelResult")]
    [InlineData("public readonly string TestCancelResult = string.Empty;", "form.TestCancelResult")]
    [InlineData("public static string TestCancelResult => string.Empty;", "Test.TestForm.TestCancelResult")]
    [InlineData("public static string TestCancelResult() => string.Empty;", "Test.TestForm.TestCancelResult")]
    [InlineData("public static readonly string TestCancelResult = string.Empty;", "Test.TestForm.TestCancelResult")]
    public void CancelResultAddedThroughConvention(string cancelSource, string call)
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

                                   {{cancelSource}}
                             }
                             """).OutputShouldContain($".AddCancelResult({call})");
    }

    [Theory]
    [InlineData("First")]
    [InlineData("Another")]
    [InlineData("SomePropertyName")]
    public void CancelResultConventionIsDerivedFromPropertyName(string propertyName)
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [SelectPrompt]
                         public string {{propertyName}} {get;set;}

                         public List<string> {{propertyName}}Source => new();

                         public string {{propertyName}}CancelResult => string.Empty;
                    }
                    """).OutputShouldContain($".AddCancelResult(form.{propertyName}CancelResult)");
    }

    [Fact]
    public void CancelResultAddedThroughExplicitName()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [SelectPrompt(CancelResult = nameof(OverrideCancel))]
                         public string Test {get;set;}

                         public List<string> TestSource => new();

                         public string OverrideCancel => string.Empty;
                    }
                    """).OutputShouldContain(".AddCancelResult(form.OverrideCancel)");
    }

    [Fact]
    public void CancelResultAddedForEnumerableProperty()
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

                         public IEnumerable<string> ItemsCancelResult => new string[0];
                    }
                    """).OutputShouldContain(".AddCancelResult(form.ItemsCancelResult)");
    }

    [Fact]
    public void NoCancelResultMatchByNameReturnsExpectedDiagnostic()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [SelectPrompt(CancelResult = "NonExistent")]
                         public string Test {get;set;}

                         public List<string> TestSource => new();
                    }
                    """).ShouldHaveSourceGeneratorDiagnosticOnlyOnce(DiagnosticIds.Id0028_CancelResult_NotFound);
    }

    [Theory]
    [InlineData("public int TestCancel => 42;")]
    [InlineData("public int TestCancel() => 42;")]
    public void CancelResultMatchByNameInvalidTypeReturnsExpectedDiagnostic(string invalidSource)
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [SelectPrompt(CancelResult = nameof(TestCancel))]
                         public string Test {get;set;}

                         public List<string> TestSource => new();

                         {{invalidSource}}
                    }
                    """).ShouldHaveSourceGeneratorDiagnosticOnlyOnce(DiagnosticIds.Id0029_CancelResult_NotValid);
    }

    [Fact]
    public void CancelResultByConventionWithNoMatchDoesNotReportDiagnostic()
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
            .HasNoSourceGeneratorDiagnosticWith(DiagnosticIds.Id0028_CancelResult_NotFound)
            .HasNoSourceGeneratorDiagnosticWith(DiagnosticIds.Id0029_CancelResult_NotValid);
    }
}
