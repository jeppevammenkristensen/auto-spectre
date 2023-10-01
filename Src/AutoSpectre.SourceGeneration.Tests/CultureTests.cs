using Xunit;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests;

public class CultureTests : AutoSpectreGeneratorTestsBase
{
    public CultureTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public void ClassDecoratedWithNoCultureShouldInitCultureCorrectly()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt]
                         public string SomeProperty {get;set;}
                    }
                    """).OutputShouldContain("var culture = CultureInfo.CurrentUICulture;")
            .OutputShouldContain(".WithCulture(culture)")
            .ShouldContainNamespace("AutoSpectre.Extensions");
    }
    
    [Fact]
    public void ClassDecoratedWithValidCultureShouldInitCultureCorrectly()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm(Culture = "da-DK")]
                    public class TestForm
                    {
                         [TextPrompt]
                         public string SomeProperty {get;set;}
                    }
                    """).OutputShouldContain("var culture = new CultureInfo(\"da-DK\")")
            .OutputShouldContain(".WithCulture(culture)")
            .ShouldContainNamespace("AutoSpectre.Extensions")
            .DumpGeneratedCode(_helper);
    }
    
    [Fact]
    public void ClassDecoratedWithInValidCultureShouldInitCultureCorrectly()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm(Culture = "chokolaaaaad")]
                    public class TestForm
                    {
                         [TextPrompt]
                         public string SomeProperty {get;set;}
                    }
                    """).ShouldHaveSourceGeneratorDiagnosticOnlyOnce(DiagnosticIds.Id0022_CannotParseCulture);
    }
}