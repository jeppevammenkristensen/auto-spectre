using System.Collections.Immutable;
using System.Reflection;
using AutoSpectre.SourceGeneration;
using AutoSpectre.SourceGeneration.BuildContexts;
using AutoSpectre.SourceGeneration.Evaluation;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Xunit.Sdk;

namespace AutoSpectre.SourceGeneration.Tests;

[TestSubject(typeof(DumpMethodBuilder))]
public class DumpMethodBuilderTest
{
    [Fact]
    public void GenerateDumpMethod_ValidGeneratesExpectedStructure()
    {
        var symbol = RoslynTestUtil.CreateNamedTypeSymbol(@"public class Someclass {}");
        var methodBuilder = new DumpMethodBuilder(symbol, new List<IStepContext>(), new SingleFormEvaluationContext());

        var method = methodBuilder.GenerateDumpMethods();
        method.AsMethodShould().BePublic().And.HaveName("GenerateTable")
            .And.HaveBodyThat
            .ContainsLineWithExactly("var table = new Table();")
            .And.ContainsLineWithExactly("""table.AddColumn(new TableColumn("Name"));""")
            .And.ContainsLineWithExactly("""table.AddColumn(new TableColumn("Name"));""")
            .And.ContainsLineWithExactly("return table");
        
        
    }

    [Fact]
    public void GenerateDumpMethod_WithConfirmAnnotatedProperty()
    {
        var symbol = RoslynTestUtil.CreateNamedTypeSymbol(@"public class Someclass {}");
        var methodBuilder = new DumpMethodBuilder(symbol,
            new() { GenerateConfirmPropertyContext(false) }, new SingleFormEvaluationContext());

        var method = methodBuilder
            .GenerateDumpMethods();
        method.AsMethodShould()
            .HaveBodyThat.ContainsLineWithExactly($"""table.AddRow(new Markup("First"), new Markup(source.First.ToString()))""");
    }
    
    [Fact]
    public void GenerateDumpMethod_WithConfirmNullableAnnotatedProperty()
    {
        var symbol = RoslynTestUtil.CreateNamedTypeSymbol(@"public class Someclass {}");
        var methodBuilder = new DumpMethodBuilder(symbol,
            new() { GenerateConfirmPropertyContext(true) }, new SingleFormEvaluationContext());
        var method = methodBuilder
            .GenerateDumpMethods();
        method.AsMethodShould()
            .HaveBodyThat.ContainsLineWithExactly($"""table.AddRow(new Markup("First"), new Markup(source.First?.ToString()))""");
    }
    
    [Fact]
    public void GenerateDumpMethod_WithTextPromptAnnotatedProperty()
    {
        var symbol = RoslynTestUtil.CreateNamedTypeSymbol(@"public class Someclass {}");
        var methodBuilder = new DumpMethodBuilder(symbol,
            new() { GenerateTextPromptBuildContext("TextPrompt", "Text prompt title", symbol, false) }, new SingleFormEvaluationContext());

        var method = methodBuilder
            .GenerateDumpMethods();
        method.AsMethodShould()
            .HaveBodyThat.ContainsLineWithExactly($"""table.AddRow(new Markup("TextPrompt"), new Markup(source.TextPrompt.ToString()))""");
    }

    [Fact]
    public void GenerateDumpMethod_WithTextPromptNullableAnnotatedProperty()
    {
        var symbol = RoslynTestUtil.CreateNamedTypeSymbol(@"public class Someclass {}");
        var methodBuilder = new DumpMethodBuilder(symbol,
            new() { GenerateTextPromptBuildContext("TextPrompt", "Text prompt title", symbol, true) }, new SingleFormEvaluationContext());

        var method = methodBuilder
            .GenerateDumpMethods();
        method.AsMethodShould()
            .HaveBodyThat.ContainsLineWithExactly($"""table.AddRow(new Markup("TextPrompt"), new Markup(source.TextPrompt?.ToString()))""");
    }
    
    
    
    [Fact]
    public void GenerateDumpMethod_WithEnumAnnotatedProperty()
    {
        var symbol = RoslynTestUtil.CreateNamedTypeSymbol(@"public class Someclass {}");
        var methodBuilder = new DumpMethodBuilder(symbol,
            new() { GenerateEnumPropertyContext("EnumProperty", "SomeEnum", "EnumTitle", false) }, new SingleFormEvaluationContext());

        var method = methodBuilder
            .GenerateDumpMethods();
        method.AsMethodShould()
            .HaveBodyThat.ContainsLineWithExactly($"""table.AddRow(new Markup("EnumProperty"), new Markup(source.EnumProperty.ToString()))""");
    }
    
    [Fact]
    public void GenerateDumpMethod_WithEnumNullableAnnotatedProperty()
    {
        var symbol = RoslynTestUtil.CreateNamedTypeSymbol(@"public class Someclass {}");
        var methodBuilder = new DumpMethodBuilder(symbol,
            new() { GenerateEnumPropertyContext("EnumProperty", "SomeEnum?", "EnumTitle", true) }, new SingleFormEvaluationContext());

        var method = methodBuilder
            .GenerateDumpMethods();
        method.AsMethodShould()
            .HaveBodyThat.ContainsLineWithExactly($"""table.AddRow(new Markup("EnumProperty"), new Markup(source.EnumProperty?.ToString()))""");
    }
    
    [Fact]
    public void GenerateDumpMethod_WithSelectPromptAnnotatedProperty()
    {
        var symbol = RoslynTestUtil.CreateNamedTypeSymbol(@"public class Someclass {}");
        var methodBuilder = new DumpMethodBuilder(symbol,
            new() { GenerateSelectPromptBuildContext("SelectPrompt", "Select prompt", symbol, false)}, new SingleFormEvaluationContext());

        var method = methodBuilder
            .GenerateDumpMethods();
        method.AsMethodShould()
            .HaveBodyThat.ContainsLineWithExactly($"""table.AddRow(new Markup("SelectPrompt"), new Markup(source.SelectPrompt.ToString()))""");
    }
    
    [Fact]
    public void GenerateDumpMethod_WithSelectPromptNullableAnnotatedProperty()
    {
        var symbol = RoslynTestUtil.CreateNamedTypeSymbol(@"public class Someclass {}");
        var methodBuilder = new DumpMethodBuilder(symbol,
            new() { GenerateSelectPromptBuildContext("SelectPrompt", "Select prompt", symbol, true)}, new SingleFormEvaluationContext());

        var method = methodBuilder
            .GenerateDumpMethods();
        method.AsMethodShould()
            .HaveBodyThat.ContainsLineWithExactly($"""table.AddRow(new Markup("SelectPrompt"), new Markup(source.SelectPrompt?.ToString()))""");
    }
    
    [Theory]
    [InlineData("SomeConverter", false, "source.SomeConverter")]
    [InlineData("SomeConverter", true, "Someclass.SomeConverter")]
    public void GenerateDumpMethod_WithSelectPromptWithConverterAnnotatedProperty(string converter, bool isStatic, string expectedConverter)
    {
        var symbol = RoslynTestUtil.CreateNamedTypeSymbol(@"public class Someclass {}");
        var methodBuilder = new DumpMethodBuilder(symbol,
            new() { GenerateSelectPromptBuildContext("SelectPrompt", "Select prompt", symbol, false, isStatic,converter)}, new SingleFormEvaluationContext());

        var method = methodBuilder
            .GenerateDumpMethods();
        method.AsMethodShould()
            .HaveBodyThat.ContainsLineWithExactly($"""table.AddRow(new Markup("SelectPrompt"), new Markup(source.SelectPrompt == null ? String.Empty : {expectedConverter}(source.SelectPrompt)))""");
    }

    [Fact]
    public void GenerateDumpMethod_WithMultiSelectPromptAnnotatedProperty()
    {
        var symbol = RoslynTestUtil.CreateNamedTypeSymbol(@"public class Someclass {}");
        var methodBuilder = new DumpMethodBuilder(symbol,
            new() { GenerateMultiSelectPromptBuildContext("MultiSelectPrompt", "Multi Select prompt", symbol)}, new SingleFormEvaluationContext());

        var method = methodBuilder
            .GenerateDumpMethods();
        method.AsMethodShould()
            .HaveBodyThat.Contains($"""source.MultiSelectPrompt?.Select(x =>x.ToString())?.Select(x => new Markup(x)).ToList() ?? new ()""");
    }
    
    [Theory]
    [InlineData(false, "Converter", "source")]
    [InlineData(true, "Converter", "Someclass")]
    public void GenerateDumpMethod_WithMultiSelectPromptAnnotatedWithConverterProperty(bool isStatic, string converter, string access)
    {
        var symbol = RoslynTestUtil.CreateNamedTypeSymbol(@"public class Someclass {}");
        var methodBuilder = new DumpMethodBuilder(symbol,
            new() { GenerateMultiSelectPromptBuildContext("MultiSelectPrompt", "Multi Select prompt", symbol, isStatic, converter)},new SingleFormEvaluationContext());

        var method = methodBuilder
            .GenerateDumpMethods();
        method.AsMethodShould()
            .HaveBodyThat.Contains($"""source.MultiSelectPrompt?.Select(x =>x == null ? String.Empty : {access}.Converter(x))?.Select(x => new Markup(x)).ToList() ?? new ()""");
    }
    
    private static PropertyContext GenerateConfirmPropertyContext(bool nullable)
    {
        var singlePropertyEvaluationContext = SinglePropertyEvaluationContext.Empty;
        singlePropertyEvaluationContext.IsNullable = nullable;
        return new PropertyContext("First",
            RoslynTestUtil.CreatePropertySymbol($"public bool{(nullable ? "?" : string.Empty)} First {{get;set;}}"),
            new ConfirmPromptBuildContext("ConfirmTitle", false, singlePropertyEvaluationContext));
    }
    
    private static PropertyContext GenerateEnumPropertyContext(string propertyName, string enumName, string title,
        bool nullable)
    {
        var singlePropertyEvaluationContext = SinglePropertyEvaluationContext.Empty;
        singlePropertyEvaluationContext.IsNullable = nullable;
        return new PropertyContext(propertyName,
            RoslynTestUtil.CreatePropertySymbol($"public {enumName} {propertyName} {{get;set;}}"),
            new EnumPromptBuildContext(title, RoslynTestUtil.CreateEnumType(enumName),false, singlePropertyEvaluationContext));
    }

    private static PropertyContext GenerateTextPromptBuildContext(string propertyName, string title, INamedTypeSymbol parent, bool nullable)
    {
        var attributeData = TranslatedMemberAttributeData.TextPrompt(title, null,null, false,false, null,null,null,null,null,null,null,null);
        var propertySymbol = RoslynTestUtil.CreatePropertySymbol($$"""public string{{(nullable ? "?" : string.Empty)}} {{propertyName}} {get;set;}""");
        var singlePropertyEvaluationContext =
            SinglePropertyEvaluationContext.GenerateFromPropertySymbol(propertySymbol, parent);

        return new PropertyContext(propertyName, propertySymbol,
            new TextPromptBuildContext(attributeData, propertySymbol.Type, false, singlePropertyEvaluationContext));
    } 
    
    private static PropertyContext GenerateSelectPromptBuildContext(string propertyName, string title, INamedTypeSymbol parent, bool nullable, bool isStatic = false,string? converter = null)
    {
        //var attributeData = TranslatedMemberAttributeData.SelectPrompt(title, "SelectionSource", null, null, false, null, null, null, null, null);
        var propertySymbol = RoslynTestUtil.CreatePropertySymbol($$"""public string{{(nullable ? "?" : string.Empty)}} {{propertyName}} {get;set;}""");
        var singlePropertyEvaluationContext =
            SinglePropertyEvaluationContext.GenerateFromPropertySymbol(propertySymbol, parent);
        singlePropertyEvaluationContext.ConfirmedSelectionSource =
            new ConfirmedSelectionSource("SelectionSource", SelectionPromptSelectionType.Method, false);

        if (converter is { })
        {
            singlePropertyEvaluationContext.ConfirmedConverter = new ConfirmedConverter(converter, isStatic);
        }
        

        return new PropertyContext(propertyName, propertySymbol,
            new SelectionPromptBuildContext(title,singlePropertyEvaluationContext));
    } 
    
    private static PropertyContext GenerateMultiSelectPromptBuildContext(string propertyName, string title, INamedTypeSymbol parent, bool isStatic = false, string? converter = null)
    {
        var propertySymbol = RoslynTestUtil.CreatePropertySymbol($$"""public string[] {{propertyName}} {get;set;}""");
        var singlePropertyEvaluationContext =
            SinglePropertyEvaluationContext.GenerateFromPropertySymbol(propertySymbol, parent);
        singlePropertyEvaluationContext.ConfirmedSelectionSource =
            new ConfirmedSelectionSource("SelectionSource", SelectionPromptSelectionType.Method, false);

        if (converter is { })
        {
            singlePropertyEvaluationContext.ConfirmedConverter = new ConfirmedConverter(converter, isStatic);
        }

        return new PropertyContext(propertyName, propertySymbol,
            new MultiSelectionBuildContext(title,singlePropertyEvaluationContext, LazyTypes.Empty()));
    } 
}

public static class FluentExtensions
{
    public static ClassAssertions AsClassShould(this string sourceCode)
    {
        if (SyntaxFactory.ParseCompilationUnit(sourceCode) is { Members.Count: 1 } cmp &&
            cmp.Members[0] is ClassDeclarationSyntax classDeclaration)
        {
            var compilation = classDeclaration.CreateCompilation();
            return new ClassAssertions(classDeclaration, compilation);
        }
        else
        {
            Execute.Assertion.FailWith("Source code was not a class declaration. {0}", () => sourceCode);
            throw new InvalidOperationException("Should never get here");
        }
    }

    
    public static MethodAssertions AsMethodShould(this string code)
    {
        if (SyntaxFactory.ParseMemberDeclaration(code) is MethodDeclarationSyntax method)
        {
            return new MethodAssertions(method, null);
        }
        else
        {
            Execute.Assertion.FailWith("Source code was not a class declaration. {0}", () => code);
            throw new InvalidOperationException("Should never get here");
        }
    }

    public static MethodAssertions Should(this MethodDeclarationSyntax method)
    {
        return new MethodAssertions(method, null);
    }
}

public abstract class SyntaxNodeAssertions<TSyntaxNode, TAssertions> : ReferenceTypeAssertions<TSyntaxNode, TAssertions>
    where TSyntaxNode : SyntaxNode? where TAssertions : SyntaxNodeAssertions<TSyntaxNode, TAssertions>
{
    protected readonly Compilation? Compilation;

    protected SyntaxNodeAssertions(TSyntaxNode subject, Compilation? compilation) : base(subject)
    {
        Compilation = compilation;
    }
}

public class MethodAssertions : MemberDeclarationAssertions<MethodDeclarationSyntax, MethodAssertions>
{
    public MethodAssertions(MethodDeclarationSyntax subject, Compilation? compilation) : base(subject, compilation)
    {
    }

    protected override string Identifier => "method";

    public AndConstraint<MethodAssertions> HaveName(string name, string because = "", params object[] becauseParameters)
    {
        Execute.Assertion
            .BecauseOf(because, becauseParameters)
            .ForCondition(!string.IsNullOrWhiteSpace(name))
            .FailWith("Name cannot be null or whitespace")
            .Then
            .ForCondition(name == Subject.Identifier.Text)
            .FailWith("Expected {Identifier} to have name {0}{reason}, but was {1}. \r\nCode:\r\n{2}", () => name,
                () => Subject.Identifier.Text,
                () => Subject.NormalizeWhitespace().ToString());

        return new AndConstraint<MethodAssertions>(this);
    }

    public AndConstraint<MethodAssertions> ReturnVoid(string because = "", params object[] becauseParameters)
    {
        Execute.Assertion
            .BecauseOf(because, becauseParameters)
            .ForCondition(Subject.ReturnType ==
                          SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)))
            .FailWith("Expected {Identifier} to have return type void{reason}, but it was {0}. \r\nCode:\r\n {1}",
                Subject.ReturnType, Subject.ToFullString());

        return new AndConstraint<MethodAssertions>(this);
    }

    public MethodBodyAssertions HaveBodyThat => new MethodBodyAssertions(this.Subject, this.Subject.Body, null);
}

public class MethodBodyAssertions : SyntaxNodeAssertions<BlockSyntax?, MethodBodyAssertions>
{
    private readonly MethodDeclarationSyntax _methodDeclarationSyntax;

    public MethodBodyAssertions(MethodDeclarationSyntax methodDeclarationSyntax, BlockSyntax? subject,
        Compilation? compilation) : base(subject, compilation)
    {
        _methodDeclarationSyntax = methodDeclarationSyntax;
    }

    protected override string Identifier => "method body";

    
    public AndConstraint<MethodBodyAssertions> ContainsLineWithExactly(string line, string because = "",
        params object[] becauseParameters)
    {
        var trimmedLine = line.TrimEnd(';');

        Execute.Assertion
            .BecauseOf(because, becauseParameters)
            .ForCondition(Subject != null)
            .FailWith("Method contains no body")
            .Then
            .Given(() => Subject!.Statements.Select(x => x.ToString().TrimEnd(';')))
            .ForCondition(x => x.Any(y => y.Equals(trimmedLine)))
            .FailWith("Expected {Identifier} to contain a line matching {0}{reason}, Code:\r\n{1}", line,
                _methodDeclarationSyntax.ToString());

        return new AndConstraint<MethodBodyAssertions>(this);
    }

    public AndConstraint<MethodBodyAssertions> Contains(string text, string because = "",
        params object[] becauseParameters)
    {
        Execute.Assertion
            .BecauseOf(because, becauseParameters)
            .Given(() => Subject?.ToString() ?? string.Empty)
            .ForCondition(x => x.Contains(text))
            .FailWith("Expected {Identifier} to contain {0}{reason}, Code:\r\n{1}", text,
                _methodDeclarationSyntax.ToString());

        return new AndConstraint<MethodBodyAssertions>(this);
    }
}

public abstract class MemberDeclarationAssertions<TMemberDeclaration, TMemberDeclarationAssertions> :
    SyntaxNodeAssertions<
        TMemberDeclaration, TMemberDeclarationAssertions> where TMemberDeclaration : MemberDeclarationSyntax
    where TMemberDeclarationAssertions : MemberDeclarationAssertions<TMemberDeclaration, TMemberDeclarationAssertions>
{
    protected MemberDeclarationAssertions(TMemberDeclaration subject, Compilation? compilation) : base(subject,
        compilation)
    {
    }

    public AndConstraint<TMemberDeclarationAssertions> BePublic(string because = "", params object[] becauseParameters)
    {
        Execute.Assertion
            .BecauseOf(because, becauseParameters)
            .ForCondition(Subject.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword)))
            .FailWith("Expected {context} to be public{reason}, but it was {0}", Subject.Modifiers.ToString());

        return new AndConstraint<TMemberDeclarationAssertions>((TMemberDeclarationAssertions)this);
    }
}

public class ClassAssertions : MemberDeclarationAssertions<ClassDeclarationSyntax, ClassAssertions>
{
    public ClassAssertions(ClassDeclarationSyntax subject, Compilation compilation) : base(subject, compilation)
    {
    }

    protected override string Identifier => "class";

    public AndConstraint<ClassAssertions> HaveClassName(string name, string because = "",
        params object[] becauseParameters)
    {
        Execute.Assertion
            .BecauseOf(because, becauseParameters)
            .ForCondition(!string.IsNullOrWhiteSpace(name))
            .FailWith("Name cannot be null or whitespace")
            .Then
            .ForCondition(name == Subject.Identifier.Text)
            .FailWith("Expected {context:class} to have name {0}{reason}, but was {1}. \r\nCode:\r\n {2}", () => name,
                () => Subject.Identifier.Text,
                () => Subject.NormalizeWhitespace().ToString());

        return new AndConstraint<ClassAssertions>(this);
    }
}

public static class RoslynTestUtil
{
    private const string DefaultNamespace = "Test";
    private const string DefaultClassName = "TestClass";

    public static INamedTypeSymbol CreateNamedTypeSymbol(string source)
    {
        var compilationUnitSyntax = SyntaxFactory.ParseCompilationUnit(source);
        if (compilationUnitSyntax.Members.FirstOrDefault() is ClassDeclarationSyntax classDeclaration)
        {
            var classInNamespace = compilationUnitSyntax.AddMembers(SyntaxFactory
                .NamespaceDeclaration(SyntaxFactory.IdentifierName(DefaultNamespace)).AddMembers(classDeclaration));

            var compilation = CreateCompilation(classInNamespace.SyntaxTree);
            return compilation.GetTypeByMetadataName($"{DefaultNamespace}.{classDeclaration.Identifier}") ??
                   throw new InvalidOperationException("Failed to create a valid symbol");
        }
        else
        {
            throw new InvalidOperationException("The passedin source must be for a class");
        }
    }

    public static IPropertySymbol CreatePropertySymbol(string propertySyntax)
    {
        if (SyntaxFactory.ParseMemberDeclaration(propertySyntax) is PropertyDeclarationSyntax propertyDeclaration)
        {
            var classInNamespace = SyntaxFactory.CompilationUnit()
                .AddMembers(SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(DefaultNamespace))
                    .AddMembers(SyntaxFactory.ClassDeclaration(DefaultClassName).AddMembers(propertyDeclaration)));

            var compilation = CreateCompilation(classInNamespace.SyntaxTree);
            return compilation.GetTypeByMetadataName($"{DefaultNamespace}.{DefaultClassName}")?.GetMembers()
                       .OfType<IPropertySymbol>().FirstOrDefault(x => x.Name == propertyDeclaration.Identifier.Text) ??
                   throw new InvalidOperationException("Failed to create a valid symbol");
        }
        else
        {
            throw new InvalidOperationException("The passed in source must be for a class");
        }
    }

    public static Compilation CreateCompilation(this SyntaxNode node)
    {
        return CreateCompilation(node.SyntaxTree);
    }

    public static Compilation CreateCompilation(this SyntaxTree tree)
    {
        // Create compilation
        return CSharpCompilation.Create(DefaultNamespace, new[] { tree });
    }

    public static ITypeSymbol CreateEnumType(string enumName)
    {
        var enumCode = $$"""public enum {{enumName}} { First }""";
        
        var compilationUnitSyntax = SyntaxFactory.ParseCompilationUnit(enumCode);
        if (compilationUnitSyntax.Members.FirstOrDefault() is EnumDeclarationSyntax enumDeclarationSyntax)
        {
            var classInNamespace = compilationUnitSyntax.AddMembers(SyntaxFactory
                .NamespaceDeclaration(SyntaxFactory.IdentifierName(DefaultNamespace)).AddMembers(enumDeclarationSyntax));

            var compilation = CreateCompilation(classInNamespace.SyntaxTree);
            return compilation.GetTypeByMetadataName($"{DefaultNamespace}.{enumDeclarationSyntax.Identifier}") ??
                   throw new InvalidOperationException("Failed to create a valid symbol");
        }

        throw new InvalidOperationException("An unexpected error occurred");
    }
}