using System.IO;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Spectre.Console;
using Xunit;

namespace AutoSpectre.SourceGeneration.Tests;

public class LegacyCancellationCompatibilityTests : AutoSpectreGeneratorTestsBase
{
    public LegacyCancellationCompatibilityTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NamedGeneratedInterface_ExistingImplementationWithOnlyLegacyMethodCompiles(bool asynchronous)
    {
        var step = asynchronous
            ? "[TaskStep] public Task Work() => Task.CompletedTask;"
            : """[TextPrompt] public string Name { get; set; } = "";""";
        var implementation = asynchronous
            ? "public Task<TestForm> PromptAsync(TestForm form) => Task.FromResult(form);"
            : "public TestForm Prompt(TestForm form) => form;";

        var output = GetOutput(Form(step) + $$"""

                                              public sealed class HandwrittenFactory : ITestFormSpectreFactory
                                              {
                                                  {{implementation}}
                                              }

                                              public static class Consumer
                                              {
                                                  public static Task<TestForm> Prompt(TestForm form, CancellationToken token)
                                                      => ((ICancellableSpectreFactory<TestForm>)new HandwrittenFactory()).PromptAsync(form, token);
                                              }
                                              """).ShouldCompileWithoutErrors();

        output.GeneratorDiagnostics.Should().NotContain(x => x.Severity == DiagnosticSeverity.Error);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void ReferencedLegacyChild_CancellableParentCompilesWithLegacyFallback(bool asynchronous, bool collection)
    {
        var references = GetReferences();
        var legacyChild = CreateLegacyChildReference(asynchronous, references);
        var member = collection
            ? "[TextPrompt] public List<Legacy.Child> Children { get; set; } = new();"
            : "[TextPrompt] public Legacy.Child Child { get; set; } = new();";
        var compilation = CSharpCompilation.Create(
            "LegacyParent_" + Guid.NewGuid().ToString("N"),
            new[] {CSharpSyntaxTree.ParseText(Form(member), cancellationToken: TestContext.Current.CancellationToken)},
            references.Append(legacyChild),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        CSharpGeneratorDriver.Create(new IncrementAutoSpectreGenerator())
            .RunGeneratorsAndUpdateCompilation(compilation, out var generatedCompilation, out var diagnostics, TestContext.Current.CancellationToken);

        diagnostics.Should().NotContain(x => x.Severity == DiagnosticSeverity.Error);
        var output = new TestOutput(compilation.GetDiagnostics(TestContext.Current.CancellationToken), diagnostics, generatedCompilation)
            .ShouldCompileWithoutErrors();
        var cancellableMethod = output.OutputCompilation.SyntaxTrees
            .SelectMany(x => x.GetRoot().DescendantNodes())
            .OfType<ClassDeclarationSyntax>().Single(x => x.Identifier.ValueText == "TestFormSpectreFactory")
            .Members.OfType<MethodDeclarationSyntax>()
            .Single(x => x.Identifier.ValueText == "PromptAsync" && x.ParameterList.Parameters.Count == 2);
        var fallback = cancellableMethod.DescendantNodes().OfType<IfStatementSyntax>()
            .Single(x => x.Condition.ToString().Contains("ICancellableSpectreFactory<Legacy.Child>"));

        var childArgument = collection ? "newItem" : "item";
        fallback.Statement.ToString().Should().Contain($"PromptAsync({childArgument}, cancellationToken)");
        fallback.Else.Should().NotBeNull();
        fallback.Else!.Statement.ToString().Should()
            .Contain(asynchronous ? $".PromptAsync({childArgument})" : $".Prompt({childArgument})")
            .And.NotContain("cancellationToken");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void TokenHelpers_StaticAndExtensionCallsCompile(bool asynchronous, bool requiresConstructorArgument)
    {
        var constructor = requiresConstructorArgument ? "public TestForm(string name) { Name = name; }" : "";
        var step = asynchronous ? "[TaskStep] public Task Work() => Task.CompletedTask;" : "";
        var staticArguments = requiresConstructorArgument ? "form, token" : "token";

        GetOutput(Form($$"""
                         {{constructor}}
                         [TextPrompt] public string Name { get; set; } = "";
                         {{step}}
                         """) + $$"""

                                  public static class Consumer
                                  {
                                      public static Task<TestForm> ViaStatic(TestForm form, CancellationToken token)
                                          => SpectreFactory.PromptAsync_Test_TestForm({{staticArguments}});

                                      public static Task<TestForm> ViaExtension(TestForm form, CancellationToken token)
                                          => form.PromptAsync(token);
                                  }
                                  """).ShouldCompileWithoutErrors();
    }

    private static MetadataReference CreateLegacyChildReference(bool asynchronous, List<MetadataReference> references)
    {
        var contract = asynchronous ? "IAsyncSpectreFactory<Child>" : "ISpectreFactory<Child>";
        var step = asynchronous
            ? "[TaskStep] public Task Work() => Task.CompletedTask;"
            : """[TextPrompt] public string Name { get; set; } = "";""";
        var implementation = asynchronous
            ? "public Task<Child> PromptAsync(Child form) => Task.FromResult(form);"
            : "public Child Prompt(Child form) => form;";
        var source = $$"""
                       using System.Threading.Tasks;
                       using AutoSpectre;

                       namespace Legacy;

                       [AutoSpectreForm]
                       public class Child
                       {
                           {{step}}
                       }

                       public interface IChildSpectreFactory : {{contract}}
                       {
                       }

                       public class ChildSpectreFactory : IChildSpectreFactory
                       {
                           {{implementation}}
                       }
                       """;
        // Deliberately do not run the generator: this models an assembly produced before token overloads existed.
        var compilation = CSharpCompilation.Create(
            "LegacyChild_" + Guid.NewGuid().ToString("N"),
            new[] {CSharpSyntaxTree.ParseText(source)}, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var stream = new MemoryStream();
        var result = compilation.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
        result.Success.Should().BeTrue(string.Join(Environment.NewLine, result.Diagnostics));
        return MetadataReference.CreateFromImage(stream.ToArray());
    }

    private static List<MetadataReference> GetReferences()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(x => !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location))
            .Select(x => x.Location)
            .Concat(new[]
            {
                typeof(TextPromptAttribute).Assembly.Location,
                typeof(IAnsiConsole).Assembly.Location,
                typeof(Style).Assembly.Location
            })
            .Distinct()
            .Select(x => (MetadataReference) MetadataReference.CreateFromFile(x))
            .ToList();
    }

    private static string Form(string members) => $$"""
                                                    using System.Collections.Generic;
                                                    using System.Threading;
                                                    using System.Threading.Tasks;
                                                    using AutoSpectre;

                                                    namespace Test;

                                                    [AutoSpectreForm]
                                                    public class TestForm
                                                    {
                                                        {{members}}
                                                    }
                                                    """;
}
