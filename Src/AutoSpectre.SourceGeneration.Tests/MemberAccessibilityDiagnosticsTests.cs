using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace AutoSpectre.SourceGeneration.Tests;

/// <summary>
/// Covers signature-first accessibility diagnostics for optional prompt members and nested-form initializers.
/// </summary>
public class MemberAccessibilityDiagnosticsTests : AutoSpectreGeneratorTestsBase
{
    private static readonly string[] Areas =
    {
        "Validator", "CollectionValidator", "Converter", "Choices", "DefaultValue",
        "SelectionDefaultValue", "CollectionDefaultValue", "CancelResult", "CollectionCancelResult",
        "Initializer", "Condition", "TaskCondition"
    };

    public MemberAccessibilityDiagnosticsTests(ITestOutputHelper helper) : base(helper)
    {
    }

    /// <summary>
    /// Supplies supported member shapes across non-public visibility, naming, and static/instance variants.
    /// </summary>
    /// <returns>The candidate combinations supported by each evaluator.</returns>
    public static IEnumerable<object[]> AccessibilityCases()
    {
        foreach (var area in Areas)
        foreach (var visibility in new[] {"private", "internal", "protected"})
        foreach (var explicitName in new[] {false, true})
        foreach (var isStatic in area is "Condition" or "TaskCondition" ? new[] {false} : new[] {false, true})
        foreach (var shape in Shapes(area))
            yield return new object[] {area, visibility, explicitName, isStatic, shape};
    }

    /// <summary>
    /// Supplies naming variants for missing candidates, invalid signatures, and overload selection.
    /// </summary>
    /// <returns>Each evaluator with conventional and explicit naming.</returns>
    public static IEnumerable<object[]> NamingCases()
    {
        foreach (var area in Areas)
        foreach (var explicitName in new[] {false, true})
            yield return new object[] {area, explicitName};
    }

    /// <summary>
    /// Checks that inaccessible signature matches warn at the candidate without generating an invalid access.
    /// </summary>
    /// <param name="area">The evaluator being exercised.</param>
    /// <param name="visibility">The candidate's non-public visibility.</param>
    /// <param name="explicitName">Whether the attribute explicitly names the candidate.</param>
    /// <param name="isStatic">Whether the candidate is static.</param>
    /// <param name="shape">The supported member shape.</param>
    [Theory]
    [MemberData(nameof(AccessibilityCases))]
    public void InaccessibleSignatureMatchWarnsAtCandidate(
        string area, string visibility, bool explicitName, bool isStatic, string shape)
    {
        var name = CandidateName(area, explicitName);
        var output = GetOutput(Source(area, explicitName, Member(area, name, visibility, isStatic, shape)));

        output.CompileDiagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);
        var warning = output.GeneratorDiagnostics.Should()
            .ContainSingle(d => d.Id == DiagnosticId(area) && d.Severity == DiagnosticSeverity.Warning).Which;
        warning.GetMessage().Should().Contain("must be public");
        LocatedText(warning).Should().Be(name);
        Generated(output).Should().NotContain($"form.{name}");
        Generated(output).Should().NotContain($"TestForm.{name}");

        if (IsDefaultValue(area) && explicitName)
        {
            var error = output.GeneratorDiagnostics.Should()
                .ContainSingle(d => d.Id == DiagnosticId(area) && d.Severity == DiagnosticSeverity.Error).Which;
            error.GetMessage().Should().Contain("must be public");
            LocatedText(error).Should().Be("Pick");
            output.GeneratorDiagnostics.Should().HaveCount(2);
        }
        else
        {
            output.GeneratorDiagnostics.Should().HaveCount(1);
            output.ShouldCompileWithoutErrors();
        }
    }

    /// <summary>
    /// Checks that a same-name member with the wrong signature never receives an accessibility suggestion.
    /// </summary>
    /// <param name="area">The evaluator being exercised.</param>
    /// <param name="explicitName">Whether the attribute explicitly names the invalid candidate.</param>
    [Theory]
    [MemberData(nameof(NamingCases))]
    public void WrongSignatureDoesNotProduceAccessibilityWarning(string area, bool explicitName)
    {
        var name = CandidateName(area, explicitName);
        var invalid = area switch
        {
            "Validator" or "CollectionValidator" or "Converter" => $"private string {name}(int value) => null;",
            "Condition" or "TaskCondition" => $"private bool {name}(int value) => true;",
            "Initializer" => $"private Child {name}(int value) => new Child();",
            _ => $"private int {name} => 42;"
        };
        var output = GetOutput(Source(area, explicitName, invalid));

        output.CompileDiagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);
        output.GeneratorDiagnostics.Should().NotContain(d =>
            d.Severity == DiagnosticSeverity.Warning &&
            d.Descriptor.Title.ToString() == "Matching member must be public");
        output.GeneratorDiagnostics.Should().NotContain(d => LocatedText(d) == name);
        Generated(output).Should().NotContain($"form.{name}");
    }

    /// <summary>
    /// Checks that absent conventional optional members stay silent and explicit missing defaults retain their error.
    /// </summary>
    /// <param name="area">The evaluator being exercised.</param>
    /// <param name="explicitName">Whether an absent member is explicitly requested.</param>
    [Theory]
    [MemberData(nameof(NamingCases))]
    public void MissingCandidateDoesNotProduceAccessibilityWarning(string area, bool explicitName)
    {
        var output = GetOutput(Source(area, explicitName, ""));

        output.GeneratorDiagnostics.Should().NotContain(d =>
            d.Descriptor.Title.ToString() == "Matching member must be public");
        if (!explicitName || area == "Initializer")
            output.GeneratorDiagnostics.Should().BeEmpty();
        else if (IsDefaultValue(area))
        {
            var error = output.GeneratorDiagnostics.Should().ContainSingle().Which;
            error.Id.Should().Be(DiagnosticIds.Id0025_DefaultValueSource_NotFound);
            error.Severity.Should().Be(DiagnosticSeverity.Error);
            LocatedText(error).Should().Be("Pick");
        }
        else
        {
            var warning = output.GeneratorDiagnostics.Should().ContainSingle().Which;
            warning.Severity.Should().Be(DiagnosticSeverity.Warning);
            warning.Id.Should().Be(area switch
            {
                "Validator" or "CollectionValidator" => DiagnosticIds.Id0009_ValidatorNameNotFound,
                "Converter" => "AutoSpectre_JJK0008",
                "Choices" => DiagnosticIds.Id0024_ChoiceCandidate_NotFound,
                "CancelResult" or "CollectionCancelResult" => DiagnosticIds.Id0028_CancelResult_NotFound,
                _ => DiagnosticIds.Id0011_ConditionNameNotFound
            });
            LocatedText(warning).Should().Be("Pick");
        }
    }

    /// <summary>
    /// Checks that a public valid overload wins over an earlier non-public wrong-signature overload.
    /// </summary>
    /// <param name="area">The evaluator being exercised.</param>
    /// <param name="explicitName">Whether the overload name is explicitly configured.</param>
    [Theory]
    [MemberData(nameof(NamingCases))]
    public void PublicOverloadIsPreferred(string area, bool explicitName)
    {
        var name = CandidateName(area, explicitName);
        var invalid = area switch
        {
            "Validator" or "CollectionValidator" or "Converter" => $"private string {name}(int value) => null;",
            "Condition" or "TaskCondition" => $"private bool {name}(int value) => true;",
            "Initializer" => $"private Child {name}(int value) => new Child();",
            "Choices" or "CollectionCancelResult" => $"private string[] {name}(int value) => new[] {{ \"a\" }};",
            _ => $"private string {name}(int value) => \"a\";"
        };
        var output = GetOutput(Source(area, explicitName,
            invalid + "\n" + Member(area, name, "public", false, "method")));

        output.GeneratorDiagnostics.Should().BeEmpty();
        Generated(output).Should().Contain($"form.{name}");
        output.ShouldCompileWithoutErrors();
    }

    /// <summary>
    /// Verifies that array-valued multi-select cancel sources are adapted to Spectre's list overload.
    /// </summary>
    /// <param name="shape">The source member shape.</param>
    /// <param name="isStatic">Whether to use a static source.</param>
    [Theory]
    [InlineData("method", false)]
    [InlineData("method", true)]
    [InlineData("property", false)]
    [InlineData("property", true)]
    [InlineData("field", false)]
    [InlineData("field", true)]
    public void ArrayCancelResultCompiles(string shape, bool isStatic)
    {
        var output = GetOutput(Source("CollectionCancelResult", false,
            Member("CollectionCancelResult", "PickCancelResult", "public", isStatic, shape)));

        output.GeneratorDiagnostics.Should().BeEmpty();
        var receiver = isStatic ? "Test.TestForm" : "form";
        var value = shape == "method"
            ? $"() => {receiver}.PickCancelResult().ToList()"
            : $"{receiver}.PickCancelResult.ToList()";
        Generated(output).Should().Contain($".AddCancelResult({value})");
        output.ShouldCompileWithoutErrors();
    }

    /// <summary>
    /// Checks that a public valid signature wins over an inaccessible inherited signature match.
    /// </summary>
    /// <param name="area">An evaluator that searches inherited members.</param>
    [Theory]
    [InlineData("Validator")]
    [InlineData("CollectionValidator")]
    [InlineData("Converter")]
    [InlineData("Initializer")]
    public void PublicMatchSuppressesInheritedCandidateWarning(string area)
    {
        var name = CandidateName(area, false);
        var source = Source(area, false, Member(area, name, "public new", false, "method"))
            .Replace("public class TestForm", "public class TestForm : BaseForm");
        source += $"\npublic class BaseForm {{ {Member(area, name, "protected", false, "method")} }}";
        var output = GetOutput(source);

        output.GeneratorDiagnostics.Should().BeEmpty();
        Generated(output).Should().Contain($"form.{name}");
        output.ShouldCompileWithoutErrors();
    }

    /// <summary>
    /// Checks that the initializer candidate warning does not replace the required-initializer error.
    /// </summary>
    /// <param name="visibility">The initializer's non-public visibility.</param>
    /// <param name="explicitName">Whether the initializer is explicitly named.</param>
    [Theory]
    [InlineData("private", false)]
    [InlineData("private", true)]
    [InlineData("internal", false)]
    [InlineData("internal", true)]
    [InlineData("protected", false)]
    [InlineData("protected", true)]
    public void RequiredInitializerRetainsError(string visibility, bool explicitName)
    {
        var name = CandidateName("Initializer", explicitName);
        var output = GetOutput(Source("Initializer", explicitName,
            $"{visibility} Child {name}() => new Child(42);", requiredInitializer: true));

        output.CompileDiagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);
        var warning = output.GeneratorDiagnostics.Should().ContainSingle(d =>
            d.Id == DiagnosticIds.Id0020_InitializerNeeded && d.Severity == DiagnosticSeverity.Warning).Which;
        warning.GetMessage().Should().Contain("must be public");
        LocatedText(warning).Should().Be(name);
        var error = output.GeneratorDiagnostics.Should().ContainSingle(d =>
            d.Id == DiagnosticIds.Id0020_InitializerNeeded && d.Severity == DiagnosticSeverity.Error).Which;
        LocatedText(error).Should().Be("Pick");
    }

    /// <summary>
    /// Checks that missing and wrong-signature required initializers retain their error without a candidate warning.
    /// </summary>
    /// <param name="explicitName">Whether the initializer is explicitly named.</param>
    /// <param name="wrongSignature">Whether a same-name but invalid method exists.</param>
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void RequiredInitializerWithoutSignatureMatchDoesNotSuggestAccessibility(
        bool explicitName, bool wrongSignature)
    {
        var name = CandidateName("Initializer", explicitName);
        var member = wrongSignature ? $"private Child {name}(int value) => new Child(value);" : "";
        var output = GetOutput(Source("Initializer", explicitName, member, requiredInitializer: true));

        output.CompileDiagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);
        output.GeneratorDiagnostics.Should().NotContain(d =>
            d.Descriptor.Title.ToString() == "Matching member must be public");
        output.GeneratorDiagnostics.Should().NotContain(d =>
            d.Id == DiagnosticIds.Id0020_InitializerNeeded && d.Severity == DiagnosticSeverity.Warning);
        var error = output.GeneratorDiagnostics.Should().ContainSingle(d =>
            d.Id == DiagnosticIds.Id0020_InitializerNeeded && d.Severity == DiagnosticSeverity.Error).Which;
        LocatedText(error).Should().Be("Pick");
    }

    private static bool IsDefaultValue(string area) => area.Contains("DefaultValue");

    private static string[] Shapes(string area) => area switch
    {
        "Validator" or "CollectionValidator" or "Converter" or "Initializer" => new[] {"method"},
        "Condition" or "TaskCondition" => new[] {"method", "property"},
        _ => new[] {"method", "property", "field"}
    };

    private static string CandidateName(string area, bool explicitName) => explicitName
        ? "Candidate"
        : area switch
        {
            "CollectionValidator" => "PickValidator",
            "SelectionDefaultValue" or "CollectionDefaultValue" => "PickDefaultValue",
            "CollectionCancelResult" => "PickCancelResult",
            "Initializer" => "InitChild",
            "TaskCondition" => "PickCondition",
            _ => "Pick" + area
        };

    private static string DiagnosticId(string area) => area switch
    {
        "Validator" or "CollectionValidator" => DiagnosticIds.Id0008_ValidatorNameMatchInvalid,
        "Converter" => "AutoSpectre_JJK0008",
        "Choices" => DiagnosticIds.Id0023_ChoiceCandidate_NotValid,
        "CancelResult" or "CollectionCancelResult" => DiagnosticIds.Id0029_CancelResult_NotValid,
        "Initializer" => DiagnosticIds.Id0020_InitializerNeeded,
        "Condition" or "TaskCondition" => DiagnosticIds.Id0010_ConditionNameMatchInvalid,
        _ => DiagnosticIds.Id0025_DefaultValueSource_NotFound
    };

    /// <summary>
    /// Creates a correctly typed candidate declaration for the selected evaluator and member shape.
    /// </summary>
    /// <param name="area">The evaluator's signature requirements.</param>
    /// <param name="name">The candidate identifier.</param>
    /// <param name="visibility">The declaration's accessibility modifiers.</param>
    /// <param name="isStatic">Whether to emit a static declaration.</param>
    /// <param name="shape">Whether to emit a method, property, or field.</param>
    /// <returns>A valid consumer member declaration.</returns>
    private static string Member(string area, string name, string visibility, bool isStatic, string shape)
    {
        var type = area switch
        {
            "Choices" or "CollectionCancelResult" => "string[]",
            "Condition" or "TaskCondition" => "bool",
            "Initializer" => "Child",
            _ => "string"
        };
        var value = type switch
        {
            "string[]" => "new[] { \"a\" }",
            "bool" => "true",
            "Child" => "new Child()",
            _ => "\"a\""
        };
        var parameters = area switch
        {
            "Validator" or "Converter" => "string value",
            "CollectionValidator" => "IEnumerable<string> values, string value",
            _ => ""
        };
        var prefix = $"{visibility} {(isStatic ? "static " : "")}{type} {name}";
        return shape switch
        {
            "method" => $"{prefix}({parameters}) => {value};",
            "property" => $"{prefix} => {value};",
            _ => $"{prefix} = {value};"
        };
    }

    /// <summary>
    /// Creates a form with a valid prompt and optional member configuration isolated to one evaluator.
    /// </summary>
    /// <param name="area">The evaluator being exercised.</param>
    /// <param name="explicitName">Whether to configure the member name explicitly.</param>
    /// <param name="members">The candidate declarations to include.</param>
    /// <param name="requiredInitializer">Whether the child form lacks a parameterless constructor.</param>
    /// <returns>Consumer source with a valid selection source and child form.</returns>
    private static string Source(string area, bool explicitName, string members, bool requiredInitializer = false)
    {
        var selection = area is "Converter" or "SelectionDefaultValue" or "CollectionDefaultValue"
            or "CancelResult" or "CollectionCancelResult";
        var setting = area switch
        {
            "CollectionValidator" => "Validator",
            "Choices" => "ChoicesSource",
            "DefaultValue" => "DefaultValueSource",
            "SelectionDefaultValue" or "CollectionDefaultValue" => "DefaultValueSource",
            "CollectionCancelResult" => "CancelResult",
            "Initializer" => "TypeInitializer",
            "TaskCondition" => "Condition",
            _ => area
        };
        var options = explicitName ? $"({setting} = \"Candidate\")" : "";
        var type = area.StartsWith("Collection") ? "string[]" : area == "Initializer" ? "Child" : "string";
        var initial = type == "string[]" ? "new string[0]" : type == "Child" ? "null" : "\"\"";
        var step = area == "TaskCondition"
            ? $"[TaskStep{options}] public void Pick() {{ }}"
            : $"[{(selection ? "SelectPrompt" : "TextPrompt")}{options}] public {type} Pick {{ get; set; }} = {initial};";
        return $$"""
                 using AutoSpectre;
                 using System.Collections.Generic;

                 namespace Test;

                 [AutoSpectreForm]
                 public class TestForm
                 {
                     {{step}}

                     public string[] PickSource => new[] { "a", "b" };
                     {{members}}
                 }

                 [AutoSpectreForm]
                 public class Child
                 {
                     {{(requiredInitializer ? "public Child(int value) { }" : "")}}
                     [TextPrompt]
                     public string Name { get; set; } = "";
                 }
                 """;
    }

    private static string Generated(TestOutput output) =>
        string.Join("\n", output.OutputCompilation.SyntaxTrees.Skip(1).Select(tree => tree.ToString()));

    private static string LocatedText(Diagnostic diagnostic) =>
        diagnostic.Location.SourceTree!.GetText().ToString(diagnostic.Location.SourceSpan);
}
