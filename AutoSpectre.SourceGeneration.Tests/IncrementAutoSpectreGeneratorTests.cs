using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests
{
    public class AutoSpectreGeneratorTestsBase
    {
        protected ITestOutputHelper _helper;

        protected AutoSpectreGeneratorTestsBase(ITestOutputHelper helper)
        {
            _helper = helper;
        }

        protected string GetGeneratedOutput(string source)
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
            var compileDiagnostics = compilation.GetDiagnostics();

            foreach (var compileDiagnostic in compileDiagnostics)
            { 
                _helper.WriteLine(compileDiagnostic.ToString());
                
            }
            // Assert.False(compileDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), "Failed: " + compileDiagnostics.FirstOrDefault()?.GetMessage());

            var generator = new IncrementAutoSpectreGenerator();

            var driver = CSharpGeneratorDriver.Create(generator);
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generateDiagnostics);
            generateDiagnostics.Should().NotContain(d => d.Severity == DiagnosticSeverity.Error, "Failed: " + generateDiagnostics.FirstOrDefault()?.GetMessage());

            string output = outputCompilation.SyntaxTrees.Last().ToString();

            foreach (var diagnostic in generateDiagnostics)
            {
                _helper.WriteLine(diagnostic.ToString());
            }

            return output;
        }
    }

    public class IncrementAutoSpectreGeneratorTests : AutoSpectreGeneratorTestsBase
    {
        public IncrementAutoSpectreGeneratorTests(ITestOutputHelper helper) : base(helper)
        {
        }

        [Fact]
        public void ValidForm_WithArrayNormal_GeneratesExpected()
        {
            GetGeneratedOutput("""
                using AutoSpectre;
                using System.Collections.Generic;
                
                
                namespace Test  
                {
                    [AutoSpectreForm]
                    public class TestForm
                    {
                        [Ask()]
                        public int[]? MultiSelect {get;set;}

                        [Ask()]
                        public List<string> Items {get;set;}

                        [Ask()]
                        public HashSet<bool> BooleanValues {get;set;}
                    }                   
                }                    
                """).Should().Be("""
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;

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
            // Prompt for values for destination.MultiSelect
            {
                List<int> items = new List<int>();
                bool continuePrompting = true;
                do
                {
                    var item = AnsiConsole.Prompt(new TextPrompt<int>("Enter [green]MultiSelect[/]").AllowEmpty());
                    items.Add(item);
                    continuePrompting = AnsiConsole.Confirm("Add more items?");
                }
                while (continuePrompting);
                int[]? result = items.ToArray();
                destination.MultiSelect = result;
            }

            // Prompt for values for destination.Items
            {
                List<string> items = new List<string>();
                bool continuePrompting = true;
                do
                {
                    var item = AnsiConsole.Prompt(new TextPrompt<string>("Enter [green]Items[/]"));
                    items.Add(item);
                    continuePrompting = AnsiConsole.Confirm("Add more items?");
                }
                while (continuePrompting);
                System.Collections.Generic.List<string> result = items;
                destination.Items = result;
            }

            // Prompt for values for destination.BooleanValues
            {
                List<bool> items = new List<bool>();
                bool continuePrompting = true;
                do
                {
                    var item = AnsiConsole.Confirm("Enter [green]BooleanValues[/]");
                    items.Add(item);
                    continuePrompting = AnsiConsole.Confirm("Add more items?");
                }
                while (continuePrompting);
                System.Collections.Generic.HashSet<bool> result = new System.Collections.Generic.HashSet<bool>(items);
                destination.BooleanValues = result;
            }

            return destination;
        }
    }
}
""");
        }


        [Fact]
        public void SingleItemPropertyWithMatchedConverterShouldGenerateCorrectCode()
        {
            var output = GetGeneratedOutput("""
                using AutoSpectre;
                using System.Collections.Generic;               


                namespace Test  
                {
                    [AutoSpectreForm]
                    public class TestForm 
                    {
                        [Ask(AskType = AskType.Selection, Converter=nameof(OtherStringConverter), SelectionSource = nameof(ListOfOther))]
                        public OtherTest.OtherClass Other {get;set;}

                        public string OtherStringConverter(OtherTest.OtherClass other)
                        {
                            return other.ToString();
                        }

                       
                        public List<OtherTest.OtherClass> ListOfOther {get;set;} = new ();
                    }                   
                }

                namespace OtherTest 
                {                    
                    public class OtherClass
                    {

                    }
                }

                """).Should().Contain(".UseConverter(destination.OtherStringConverter)");

        }

        [Fact]
        public void CollectionPropertyOfOtherAutoformTypeGeneratesCorrectCode()
        {
            GetGeneratedOutput("""
using AutoSpectre;
using System.Collections.Generic;

namespace Test;

[AutoSpectreForm]
    public class Other
    {
        [Ask]
        public string Name { get; set; }
    }

    [AutoSpectreForm]
    public class ConverterForms
    {
        [Ask] public List<Other> Other { get; set; }
    }               

""").Should().Contain("IOtherSpectreFactory OtherSpectreFactory = new OtherSpectreFactory();").And
                .Contain("var newItem = OtherSpectreFactory.Get();");
        }
        
        [Fact]
        public void CollectionPropertyWithMatchedConverterShouldGenerateCorrectCode()
        {
            var output = GetGeneratedOutput("""
                using AutoSpectre;
                using System.Collections.Generic;               


                namespace Test  
                {
                    [AutoSpectreForm]
                    public class TestForm 
                    {
                        [Ask(AskType = AskType.Selection, Converter=nameof(OtherStringConverter), SelectionSource = nameof(ListOfOther))]
                        public List<OtherTest.OtherClass> Other {get;set;}

                        public string OtherStringConverter(OtherTest.OtherClass other)
                        {
                            return other.ToString();
                        }

                       
                        public List<OtherTest.OtherClass> ListOfOther {get;set;} = new ();
                    }                   
                }

                namespace OtherTest 
                {                    
                    public class OtherClass
                    {

                    }
                }

                """).Should().Contain(".UseConverter(destination.OtherStringConverter)").And.Contain("MultiSelectionPrompt<OtherTest.OtherClass>");

        }
        
        [Fact]
        public void CollectionPropertyWithMatchedConverterFromConventionShouldGenerateCorrectCode()
        {
            var output = GetGeneratedOutput("""
                using AutoSpectre;
                using System.Collections.Generic;

                namespace Test  
                {
                    [AutoSpectreForm]
                    public class TestForm 
                    {
                        [Ask(AskType = AskType.Selection)]
                        public List<OtherTest.OtherClass> Other {get;set;}

                        public string OtherConverter(OtherTest.OtherClass other)
                        {
                            return other.ToString();
                        }

                       
                        public List<OtherTest.OtherClass> OtherSource {get;set;} = new ();
                    }                   
                }

                namespace OtherTest 
                {                    
                    public class OtherClass
                    {

                    }
                }

                """).Should().Contain(".UseConverter(destination.OtherConverter)").And.Contain("MultiSelectionPrompt<OtherTest.OtherClass>");

        }


        [Fact]
        public void ValidForm_ReferencesOther_GeneratesExpected()
        {
            GetGeneratedOutput("""
                using AutoSpectre;
                using System.Collections.Generic;
                using System.Collections.Immutable;


                namespace Test  
                {
                    [AutoSpectreForm]
                    public class TestForm 
                    {
                        [Ask]
                        public OtherTest.OtherClass Other {get;set;}

                        [Ask]
                        public List<OtherTest.OtherClass> ListOfOther {get;set;} = new ();
                    }                   
                }

                namespace OtherTest 
                {
                    [AutoSpectreForm]
                    public class OtherClass
                    {
                        
                    }
                }
                """).Should().Be("""
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;
using OtherTest;

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
            IOtherClassSpectreFactory OtherClassSpectreFactory = new OtherClassSpectreFactory();
            destination ??= new Test.TestForm();
            {
                AnsiConsole.MarkupLine("Enter [green]Other[/]");
                var item = OtherClassSpectreFactory.Get();
                destination.Other = item;
            }

            // Prompt for values for destination.ListOfOther
            {
                List<OtherClass> items = new List<OtherClass>();
                bool continuePrompting = true;
                do
                {
                    {
                        AnsiConsole.MarkupLine("Enter [green]ListOfOther[/]");
                        var newItem = OtherClassSpectreFactory.Get();
                        items.Add(newItem);
                    }

                    continuePrompting = AnsiConsole.Confirm("Add more items?");
                }
                while (continuePrompting);
                System.Collections.Generic.List<OtherTest.OtherClass> result = items;
                destination.ListOfOther = result;
            }

            return destination;
        }
    }
}
""");
        }

        [Fact]
        public void Assert_Correct_Outputted()
        {
            GetGeneratedOutput("""
                using AutoSpectre;
                using System.Collections.Generic;
                using System.Collections.Immutable;


                namespace Test  
                {
                    [AutoSpectreForm]
                    public class TestForm 
                    {
                        [Ask]
                        public string Name {get;set;}  

                        [Ask(Title = "Custom title")] 
                        public bool BoolTest {get;set;}

                        [Ask(AskType = AskType.Selection)]
                        public string NoSource {get;set;}

                        [Ask(AskType = AskType.Selection, SelectionSource = nameof(Sources))]
                        public int Source {get;set;}

                        [Ask(AskType = AskType.Selection, SelectionSource = nameof(Sources))]
                        public List<int> MultiSelect {get;set;}

                        [Ask(AskType = AskType.Selection, SelectionSource = nameof(Sources))]
                        public IReadOnlyList<int> ReadOnlyList {get;set;}                        

                        [Ask(AskType = AskType.Selection, SelectionSource = nameof(Sources))]
                        public ImmutableList<int> ReadOnlyList2 {get;set;}

                        [Ask(AskType = AskType.Selection, SelectionSource = nameof(Sources))]
                        public HashSet<int> HashSet {get;set;}

                        public int[] Sources {get;} = new [] {12,24,36};
                    }
                }
                """).Should().Be("""
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;

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
            destination.Name = AnsiConsole.Prompt(new TextPrompt<string>("Enter [green]Name[/]"));
            destination.BoolTest = AnsiConsole.Confirm("Custom title");
            destination.Source = AnsiConsole.Prompt(new SelectionPrompt<int>().Title("Enter [green]Source[/]").PageSize(10).AddChoices(destination.Sources.ToArray()));
            destination.MultiSelect = AnsiConsole.Prompt(new MultiSelectionPrompt<int>().Title("Enter [green]MultiSelect[/]").PageSize(10).AddChoices(destination.Sources.ToArray()));
            destination.ReadOnlyList = AnsiConsole.Prompt(new MultiSelectionPrompt<int>().Title("Enter [green]ReadOnlyList[/]").PageSize(10).AddChoices(destination.Sources.ToArray()));
            destination.ReadOnlyList2 = AnsiConsole.Prompt(new MultiSelectionPrompt<int>().Title("Enter [green]ReadOnlyList2[/]").PageSize(10).AddChoices(destination.Sources.ToArray())).ToImmutableList();
            destination.HashSet = new System.Collections.Generic.HashSet<int>(AnsiConsole.Prompt(new MultiSelectionPrompt<int>().Title("Enter [green]HashSet[/]").PageSize(10).AddChoices(destination.Sources.ToArray())));
            return destination;
        }
    }
}
""");
        }

        [Fact]
        public void ValidForm_UseEnum_CorrectlyOutputted()
        {
            GetGeneratedOutput("""
                using AutoSpectre;
                using System.Collections.Generic;
                using System.Collections.Immutable;


                namespace Test  
                {
                    public enum TestEnum
                    {
                        Red,
                        Green,
                        Refactor
                    }

                    [AutoSpectreForm]
                    public class TestForm 
                    {
                        [Ask]
                        public TestEnum Name {get;set;}                          
                    }
                }
                """).Should().Be("""
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;

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
            destination.Name = AnsiConsole.Prompt(new SelectionPrompt<TestEnum>().Title("Enter [green]Name[/]").PageSize(10).AddChoices(Enum.GetValues<TestEnum>()));
            return destination;
        }
    }
}
""");
        }
    }
}