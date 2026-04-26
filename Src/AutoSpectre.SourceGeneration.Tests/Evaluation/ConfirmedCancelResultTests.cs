using System;
using System.Linq;
using System.Reflection;
using System.Text;
using AutoSpectre;
using AutoSpectre.SourceGeneration;
using AutoSpectre.SourceGeneration.BuildContexts;
using AutoSpectre.SourceGeneration.Evaluation;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace AutoSpectre.SourceGeneration.Tests.Evaluation;

[TestSubject(typeof(ConfirmedCancelResult))]
public class ConfirmedCancelResultTests
{
    [Fact]
    public void Constructor_InvalidEvaluation_Throws()
    {
        var act = () => new ConfirmedCancelResult("Source", SourceEvaluation.False);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WriteToSummary_WritesCancelResultLineWithSourceName()
    {
        var evaluation = BuildValidEvaluation();
        var cancelResult = new ConfirmedCancelResult("CancelSource", evaluation);
        var builder = new StringBuilder();

        cancelResult.WriteToSummary(new SummaryLineWriter(builder));

        builder.ToString().Should().Be("/// Cancel result source: CancelSource");
    }

    [Fact]
    public void WriteToSummary_OutputDoesNotDependOnEvaluationDetails()
    {
        var evaluation = BuildValidEvaluation();
        var cancelResult = new ConfirmedCancelResult("OtherSource", evaluation);
        var builder = new StringBuilder();

        cancelResult.WriteToSummary(new SummaryLineWriter(builder));

        builder.ToString().Should().Be("/// Cancel result source: OtherSource");
    }

    private static SourceEvaluation BuildValidEvaluation()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText("""
            namespace Test
            {
                public class TestClass
                {
                    public string CancelSource { get; set; }
                }
            }
            """);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            .Select(a => (MetadataReference)MetadataReference.CreateFromFile(a.Location))
            .ToList();

        var compilation = CSharpCompilation.Create(
            "cancelResultTests",
            [syntaxTree],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var classSyntax = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var classSymbol = semanticModel.GetDeclaredSymbol(classSyntax)!;
        var propertySymbol = classSymbol.GetMembers().OfType<IPropertySymbol>().First();

        return new SourceEvaluation(valid: true, SourceAccessType.PropertyOrField, propertySymbol, targetIsEnumerable: false);
    }
}
