using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

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
            .OutputShouldContain(".WithCulture(culture)");
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

    [Theory]
    [InlineData(null, "public", true)]
    [InlineData("da-DK", "public", true)]
    [InlineData(null, "internal", true)]
    [InlineData("da-DK", "internal", false)]
    public void PartialFormGeneratesPrivateCultureHelper(string? culture, string accessibility, bool hasPrompt)
    {
        var cultureArgument = culture is null ? string.Empty : $"(Culture = \"{culture}\")";
        var prompt = hasPrompt ? "[TextPrompt] public string Name { get; set; } = \"\";" : string.Empty;
        var output = GetOutput($$"""
                                 using AutoSpectre;
                                 using System.Globalization;

                                 namespace Test;

                                 [AutoSpectreForm{{cultureArgument}}]
                                 {{accessibility}} partial class TestForm
                                 {
                                     {{prompt}}
                                     public string CultureName() => GetCultureInfo().Name;
                                 }
                                 """);

        output.ShouldCompileWithoutErrors();
        var form = output.OutputCompilation.GetTypeByMetadataName("Test.TestForm")!;
        var helper = form.GetMembers("GetCultureInfo").OfType<IMethodSymbol>().Should().ContainSingle().Subject;
        helper.DeclaredAccessibility.Should().Be(Accessibility.Private);
        helper.ReturnType.ToDisplayString().Should().Be("System.Globalization.CultureInfo");
        helper.Parameters.Should().BeEmpty();
        var syntax = (MethodDeclarationSyntax) helper.DeclaringSyntaxReferences.Single()
            .GetSyntax(TestContext.Current.CancellationToken);
        syntax.ExpressionBody!.Expression.ToString().Should().Be(culture is null
            ? "CultureInfo.CurrentUICulture"
            : $"new CultureInfo(\"{culture}\")");
    }

    [Fact]
    public void NonPartialFormDoesNotGenerateCultureHelper()
    {
        var output = GetOutput("""
                               using AutoSpectre;
                               namespace Test;
                               [AutoSpectreForm]
                               public class TestForm
                               {
                                   [TextPrompt] public string Name { get; set; } = "";
                               }
                               """);

        output.ShouldCompileWithoutErrors();
        output.OutputCompilation.GetTypeByMetadataName("Test.TestForm")!
            .GetMembers("GetCultureInfo").Should().BeEmpty();
        output.OutputCompilation.GetTypeByMetadataName("Test.TestForm")!
            .GetMembers("EscapeInterpolated").Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("da-DK")]
    public void PartialFormGeneratesPrivateEscapeInterpolatedOverloads(string? culture)
    {
        var cultureArgument = culture is null ? string.Empty : $"(Culture = \"{culture}\")";
        var output = GetOutput($$"""
                                 using AutoSpectre;
                                 using System.Globalization;
                                 namespace Test;

                                 [AutoSpectreForm{{cultureArgument}}]
                                 public partial class TestForm
                                 {
                                     public string Format(decimal number) => EscapeInterpolated($"[bold]{number:N2}[/]");
                                     public string FormatWithProvider(decimal number) =>
                                         EscapeInterpolated(CultureInfo.InvariantCulture, $"{number:N2}");
                                 }
                                 """);

        output.ShouldCompileWithoutErrors();
        var helpers = output.OutputCompilation.GetTypeByMetadataName("Test.TestForm")!
            .GetMembers("EscapeInterpolated").OfType<IMethodSymbol>().ToArray();
        helpers.Should().HaveCount(2);
        helpers.Should().OnlyContain(helper => helper.DeclaredAccessibility == Accessibility.Private);
        helpers.Should().OnlyContain(helper => helper.ReturnType.SpecialType == SpecialType.System_String);
        var defaultHelper = helpers.Single(helper => helper.Parameters.Length == 1);
        defaultHelper.Parameters[0].Type.ToDisplayString().Should().Be("System.FormattableString");
        var defaultSyntax = (MethodDeclarationSyntax) defaultHelper.DeclaringSyntaxReferences.Single()
            .GetSyntax(TestContext.Current.CancellationToken);
        defaultSyntax.Body!.ToString().Should().Contain("EscapeInterpolated(GetCultureInfo(), formattableString)");

        var providerHelper = helpers.Single(helper => helper.Parameters.Length == 2);
        providerHelper.Parameters[0].Type.ToDisplayString().Should().Be("System.IFormatProvider");
        providerHelper.Parameters[1].Type.ToDisplayString().Should().Be("System.FormattableString");
        var providerSyntax = (MethodDeclarationSyntax) providerHelper.DeclaringSyntaxReferences.Single()
            .GetSyntax(TestContext.Current.CancellationToken);
        providerSyntax.Body!.ToString().Should().Contain("text.EscapeMarkup()")
            .And.Contain("IFormattable => arg")
            .And.Contain("arg.ToString()?.EscapeMarkup()")
            .And.Contain("string.Format(provider, value.Format, args)");
    }

    [Fact]
    public void BaseAndDerivedFormsHaveIndependentPrivateCultureHelpers()
    {
        var output = GetOutput("""
                               using AutoSpectre;
                               namespace Test;

                               [AutoSpectreForm(Culture = "da-DK")]
                               public partial class BaseForm
                               {
                                   [TextPrompt] public string Name { get; set; } = "";
                                   public string BaseCultureName() => GetCultureInfo().Name;
                               }

                               [AutoSpectreForm(Culture = "en-US")]
                               public partial class DerivedForm : BaseForm
                               {
                                   public string DerivedCultureName() => GetCultureInfo().Name;
                               }
                               """);

        output.ShouldCompileWithoutErrors();
        foreach (var (typeName, culture) in new[] {("BaseForm", "da-DK"), ("DerivedForm", "en-US")})
        {
            var form = output.OutputCompilation.GetTypeByMetadataName($"Test.{typeName}")!;
            var helper = form.GetMembers("GetCultureInfo").OfType<IMethodSymbol>().Should().ContainSingle().Subject;
            helper.DeclaredAccessibility.Should().Be(Accessibility.Private);
            var syntax = (MethodDeclarationSyntax) helper.DeclaringSyntaxReferences.Single()
                .GetSyntax(TestContext.Current.CancellationToken);
            syntax.ExpressionBody!.Expression.ToString().Should().Be($"new CultureInfo(\"{culture}\")");
        }
    }
}