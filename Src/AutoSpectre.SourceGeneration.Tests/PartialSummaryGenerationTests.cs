using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Spectre.Console;
using Xunit;

namespace AutoSpectre.SourceGeneration.Tests;

public class PartialSummaryGenerationTests
{
    private readonly ITestOutputHelper _helper;

    public PartialSummaryGenerationTests(ITestOutputHelper helper)
    {
        _helper = helper;
    }

    [Fact]
    public void PartialClassWithPartialProperty_GeneratesSummaryForProperty()
    {
        const string source = """
                              using AutoSpectre;

                              namespace Test;

                              [AutoSpectreForm]
                              public partial class PartialForm
                              {
                                  [TextPrompt(Title = "Enter your name")]
                                  public partial string Name { get; set; }
                              }
                              """;

        var partialSummary = RunGenerator(source);

        partialSummary.Should().NotBeNull(
            "a *PartialSummaries.g.cs file must be emitted when the form is a partial class with a partial property");

        var text = partialSummary!.ToString();
        text.Should().Contain("partial class PartialForm");
        text.Should().Contain("Name");
        text.Should().Contain("<summary>");
    }

    [Fact]
    public void PartialClassWithPartialProperty_RemovesInitializer()
    {
        const string source = """
                              using AutoSpectre;

                              namespace Test;

                              [AutoSpectreForm]
                              public partial class PartialForm
                              {
                                  [TextPrompt(Title = "Enter your name")]
                                  public partial string Name { get; set; } = "DefaultName";
                              }
                              """;

        var partialSummary = RunGenerator(source);

        partialSummary.Should().NotBeNull();

        var text = partialSummary!.ToString();
        text.Should().Contain("partial class PartialForm");
        text.Should().Contain("Name");
        text.Should().NotContain("\"DefaultName\"",
            "the initializer must be stripped from the generated partial property declaration");
        text.Should().NotMatchRegex(@"Name\s*\{[^}]*\}\s*=",
            "the partial property must not carry over a '= ...' initializer");
    }

    [Fact]
    public void PartialClassWithPartialTaskStepMethod_GeneratesSummaryAndPartialMethodDeclaration()
    {
        const string source = """
                              using AutoSpectre;
                              using System.Threading.Tasks;

                              namespace Test;

                              [AutoSpectreForm]
                              public partial class PartialForm
                              {
                                  [TaskStep]
                                  public partial Task DoWork();
                              }
                              """;

        var partialSummary = RunGenerator(source);

        partialSummary.Should().NotBeNull(
            "a *PartialSummaries.g.cs file must be emitted when the form is a partial class with a partial TaskStep method");

        var text = partialSummary!.ToString();
        text.Should().Contain("partial class PartialForm");
        text.Should().Contain("DoWork");
        text.Should().Contain("partial");
        text.Should().Contain("<summary>");
        text.Should().MatchRegex(@"partial\s+Task\s+DoWork\s*\(\s*\)\s*;",
            "the emitted partial method must be a declaration only — body stripped, semicolon terminator");
    }

    private SyntaxTree? RunGenerator(string source)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);

        var references = new List<MetadataReference>();
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            {
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }

        references.Add(MetadataReference.CreateFromFile(typeof(TextPromptAttribute).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(typeof(IAnsiConsole).Assembly.Location));

        var compilation = CSharpCompilation.Create(
            "foo",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        foreach (var diag in compilation.GetDiagnostics())
        {
            _helper.WriteLine(diag.ToString());
        }

        var driver = CSharpGeneratorDriver.Create([new IncrementAutoSpectreGenerator().AsSourceGenerator()], parseOptions:parseOptions);
            

        driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generateDiagnostics);

        foreach (var diag in generateDiagnostics)
        {
            _helper.WriteLine(diag.ToString());
        }

        generateDiagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error);

        return outputCompilation.SyntaxTrees
            .FirstOrDefault(t => t.FilePath.EndsWith("PartialSummaries.g.cs", System.StringComparison.Ordinal));
    }
}
