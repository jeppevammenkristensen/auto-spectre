using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace AutoSpectre.SourceGeneration.Tests;

/// <summary>
/// Covers selection source resolution and diagnostics for missing and non-public members.
/// </summary>
public class SelectionSourceDiagnosticsTests : AutoSpectreGeneratorTestsBase
{
    public SelectionSourceDiagnosticsTests(ITestOutputHelper helper) : base(helper)
    {
    }

    /// <summary>
    /// Supplies visibility, member kind, static/instance, naming, and prompt cardinality combinations.
    /// </summary>
    public static IEnumerable<object[]> SourceCases()
    {
        foreach (var visibility in new[] { "public", "private", "protected", "internal" })
        foreach (var member in new[] { "field", "property", "method" })
        foreach (var isStatic in new[] { false, true })
        foreach (var explicitSource in new[] { false, true })
        foreach (var multiSelect in new[] { false, true })
        {
            yield return new object[] { visibility, member, isStatic, explicitSource, multiSelect };
        }
    }

    /// <summary>
    /// Checks that public sources generate compilable prompts and non-public sources report both
    /// the prompt error and a warning located at the candidate declaration.
    /// </summary>
    /// <param name="visibility">The accessibility of the source member.</param>
    /// <param name="member">The kind of source member.</param>
    /// <param name="isStatic">Whether the source is static.</param>
    /// <param name="explicitSource">Whether the attribute names the source explicitly.</param>
    /// <param name="multiSelect">Whether the prompt property is a collection.</param>
    [Theory]
    [MemberData(nameof(SourceCases))]
    public void SourceAccessibilityProducesExpectedDiagnostics(
        string visibility, string member, bool isStatic, bool explicitSource, bool multiSelect)
    {
        var sourceName = explicitSource ? "Choices" : "PickSource";
        var modifier = isStatic ? "static " : "";
        var declaration = member switch
        {
            "field" => $"{visibility} {modifier}string[] {sourceName} = new[] {{ \"a\", \"b\" }};",
            "property" => $"{visibility} {modifier}string[] {sourceName} => new[] {{ \"a\", \"b\" }};",
            _ => $"{visibility} {modifier}string[] {sourceName}() => new[] {{ \"a\", \"b\" }};"
        };
        var output = GetOutput(CreateForm(explicitSource, multiSelect, declaration));

        if (visibility == "public")
        {
            output.GeneratorDiagnostics.Should().BeEmpty();
            var receiver = isStatic ? "TestForm" : "form";
            var invocation = member == "method" ? "()" : "";
            output.OutputShouldContain($"{receiver}.{sourceName}{invocation}")
                .ShouldCompileWithoutErrors();
            return;
        }

        output.CompileDiagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);
        output.GeneratorDiagnostics.Should().HaveCount(2);
        var error = output.GeneratorDiagnostics.Should()
            .ContainSingle(d => d.Severity == DiagnosticSeverity.Error).Which;
        error.Id.Should().Be(explicitSource
            ? DiagnosticIds.Id0005_SelectionSourceNotFound
            : DiagnosticIds.Id0026_NoSelectionSource);
        error.GetMessage().Should().Be(explicitSource
            ? "A source matching Choices was found but it must be public"
            : "A selection source was found matching the signature but it must be public");
        LocatedText(error).Should().Be("Pick");

        var warning = output.GeneratorDiagnostics.Should()
            .ContainSingle(d => d.Severity == DiagnosticSeverity.Warning).Which;
        warning.Id.Should().Be(DiagnosticIds.Id0026_NoSelectionSource);
        warning.GetMessage().Should().Be(explicitSource
            ? "This could be a candidate as source for Pick. But it must be public"
            : "This could be a candidate as source for Pick if you adjust to internal or public");
        LocatedText(warning).Should().Be(sourceName);
    }

    /// <summary>
    /// Supplies missing or incorrectly shaped members for both source naming modes and prompt kinds.
    /// </summary>
    public static IEnumerable<object[]> InvalidSourceCases()
    {
        foreach (var declaration in new[]
                 {
                     "",
                     "private bool SOURCE() => true;",
                     "private int[] SOURCE => new[] { 1 };",
                     "private string[] SOURCE(int value) => new[] { \"a\" };",
                     "public string[] SOURCE(int value) => new[] { \"a\" };"
                 })
        foreach (var explicitSource in new[] { false, true })
        foreach (var multiSelect in new[] { false, true })
        {
            yield return new object[] { declaration, explicitSource, multiSelect };
        }
    }

    /// <summary>
    /// Ensures that a name match alone does not produce a non-public candidate warning.
    /// </summary>
    /// <param name="declaration">A missing or invalid source declaration with a name placeholder.</param>
    /// <param name="explicitSource">Whether the attribute specifies the source name.</param>
    /// <param name="multiSelect">Whether the prompt property is a collection.</param>
    [Theory]
    [MemberData(nameof(InvalidSourceCases))]
    public void MissingOrInvalidSourceReportsOnlyOriginalError(
        string declaration, bool explicitSource, bool multiSelect)
    {
        var output = GetOutput(CreateForm(explicitSource, multiSelect,
            declaration.Replace("SOURCE", explicitSource ? "Choices" : "PickSource")));

        output.CompileDiagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);
        var diagnostic = output.GeneratorDiagnostics.Should().ContainSingle().Which;
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        diagnostic.Id.Should().Be(explicitSource
            ? DiagnosticIds.Id0005_SelectionSourceNotFound
            : DiagnosticIds.Id0026_NoSelectionSource);
        diagnostic.GetMessage().Should().Be(explicitSource
            ? "The selection source Choices was not found on type"
            : "No selection source was set. You can fix this by setting the source or following the name convention for a valid source");
        LocatedText(diagnostic).Should().Be("Pick");
    }

    /// <summary>
    /// Ensures an inherited non-public candidate does not trigger a warning when a public source exists.
    /// </summary>
    /// <param name="explicitSource">Whether the attribute specifies the source name.</param>
    /// <param name="multiSelect">Whether the prompt property is a collection.</param>
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void PublicSourceIsPreferredOverNonPublicCandidate(bool explicitSource, bool multiSelect)
    {
        var sourceName = explicitSource ? "Choices" : "PickSource";
        var source = CreateForm(explicitSource, multiSelect,
                $"public new string[] {sourceName}() => new[] {{ \"public\" }};")
            .Replace("public class TestForm", "public class TestForm : BaseForm");
        source += $$"""

            public class BaseForm
            {
                protected string[] {{sourceName}}() => new[] { "protected" };
            }
            """;
        var output = GetOutput(source);

        output.GeneratorDiagnostics.Should().BeEmpty();
        output.OutputShouldContain($"form.{sourceName}()").ShouldCompileWithoutErrors();
    }

    /// <summary>
    /// Builds a form with independently selectable naming and collection semantics.
    /// </summary>
    /// <param name="explicitSource">Whether to name the Choices member in the attribute.</param>
    /// <param name="multiSelect">Whether Pick holds multiple values.</param>
    /// <param name="declaration">The source members to include in the form.</param>
    /// <returns>The consumer source supplied to the generator.</returns>
    private static string CreateForm(bool explicitSource, bool multiSelect, string declaration)
    {
        var attribute = explicitSource ? "[SelectPrompt(Source = \"Choices\")]" : "[SelectPrompt]";
        var propertyType = multiSelect ? "string[]" : "string";
        var initialValue = multiSelect ? "new string[0]" : "\"\"";
        return $$"""
            using AutoSpectre;

            namespace Test;

            [AutoSpectreForm]
            public class TestForm
            {
                {{attribute}}
                public {{propertyType}} Pick { get; set; } = {{initialValue}};

                {{declaration}}
            }
            """;
    }

    private static string LocatedText(Diagnostic diagnostic) =>
        diagnostic.Location.SourceTree!.GetText().ToString(diagnostic.Location.SourceSpan);
}
