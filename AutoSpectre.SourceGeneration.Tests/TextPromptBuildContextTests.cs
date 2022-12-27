﻿using System.Reflection;
using AutoSpectre.SourceGeneration.BuildContexts;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests;

public class TextPromptBuildContextTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TextPromptBuildContextTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void StringTypeNotNullGeneratesExpectedOutput()
    {
        var propertyType = FindFirstPropertyAndReturnTypeAsITypeSymbol("""
            public class TestClass 
            {
                public string Somestring {get;set;}
            }
            """);
        
            TextPromptBuildContext sut = new("Custom title", propertyType, false);
            var generateOutput = sut.GenerateOutput();
            generateOutput.Should().Be("""
AnsiConsole.Prompt(
new TextPrompt<string>("Custom title")
)
""");
            
    }

    [Fact]
    public void StringTypeCanBeNullGeneratesExpectedOutput()
    {
        var propertyType = FindFirstPropertyAndReturnTypeAsITypeSymbol("""
            public class TestClass 
            {
                public string Somestring {get;set;}
            }
            """);

        TextPromptBuildContext sut = new("Custom title", propertyType, true);
        var generateOutput = sut.GenerateOutput();
        generateOutput.Should().Be("""
AnsiConsole.Prompt(
new TextPrompt<string>("Custom title")
.AllowEmpty()
)
""");
    }


    private void Test(string code, out SemanticModel semanticModel, out SyntaxNode syntaxNode)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        var references = new List<MetadataReference>();
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            if (!assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            {
                references.Add(MetadataReference.CreateFromFile(assembly.Location));
            }
        }

        var compilation = CSharpCompilation.Create("foo", new SyntaxTree[] { syntaxTree }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        semanticModel = compilation.GetSemanticModel(syntaxTree);
        syntaxNode = syntaxTree.GetRoot();
    }

    private ITypeSymbol FindFirstPropertyAndReturnTypeAsITypeSymbol(string code)
    {

        Test(code, out var semanticModel, out var syntaxNode);

        var firstProperty = syntaxNode.DescendantNodes().OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault();
        firstProperty.Should().NotBeNull();

        if (semanticModel.GetDeclaredSymbol(firstProperty!) is { } property)
        {
            return property.Type;
        }

        Assert.Fail($"Could not retrieve property from {code}");
        return default;
    }
}