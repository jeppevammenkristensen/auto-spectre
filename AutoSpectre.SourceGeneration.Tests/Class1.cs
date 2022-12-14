using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests
{
    
    public class IncrementAutoSpectreGeneratorTests
    {
        private readonly ITestOutputHelper _helper;

        public IncrementAutoSpectreGeneratorTests(ITestOutputHelper _helper)
        {
            this._helper = _helper;
        }

        [Fact]
        public void Assert_Correct_Outputted()
        {
            GetGeneratedOutput("""
                using AutoSpectre;


                namespace Test  
                {
                    [AutoSpectreForm]
                    public class TestForm 
                    {
                        [Ask]
                        public string Name {get;set;}  
                        
                        [Ask(Title = "Custom title")] 
                        public bool BoolTest {get;set;}

                        [Ask(AskType = AskType.Selection]
                        public string NoSource {get;set;}

                        [Ask(AskType = AskType.Selection, SelectionSource = nameof("Sources")]
                        public int Source {get;set;}

                        public int[] Sources {get;} = new [] {12,24,36};
                    }
                }
                """).Should().Be("""
using Spectre.Console;
using System;
using System.Linq;

namespace Test
{
    public interface ITestFormSpectreFactory
    {
        TestForm Get(TestForm destination = null);
    }

    public class TestFormSpectreFactory : ITestFormSpectreFactory
    {
        public TestForm Get(TestForm destination = null)
        {
            destination ??= new Test.TestForm();
            destination.Name = AnsiConsole.Ask<string>("Enter [green]Name [/] ");
            destination.BoolTest = AnsiConsole.Confirm("Custom title");
            destination.Source = AnsiConsole.Prompt(new SelectionPrompt<int>().Title("Enter [green]Source [/]  ").PageSize(10).AddChoices(destination..ToArray()));
            return destination;
        }
    }
}
""");

            
        }

        private string GetGeneratedOutput(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);

            var references = new List<MetadataReference>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (!assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
                {
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
            }

            references.Add(MetadataReference.CreateFromFile(typeof(AskAttribute).Assembly.Location));

            var compilation = CSharpCompilation.Create("foo", new SyntaxTree[] { syntaxTree }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            // TODO: Uncomment this line if you want to fail tests when the injected program isn't valid _before_ running generators
            // var compileDiagnostics = compilation.GetDiagnostics();
            // Assert.False(compileDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + compileDiagnostics.FirstOrDefault()?.GetMessage());

            var generator = new IncrementAutoSpectreGenerator();

            var driver = CSharpGeneratorDriver.Create(generator);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generateDiagnostics);
            generateDiagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error, "Failed: " + generateDiagnostics.FirstOrDefault()?.GetMessage());

            string output = outputCompilation.SyntaxTrees.Last().ToString();

            _helper.WriteLine(output);

            return output;
        }
    }
}