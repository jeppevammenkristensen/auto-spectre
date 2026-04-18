using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests;

public class ClearOnFinishGeneratorTests : AutoSpectreGeneratorTestsBase
{
    public ClearOnFinishGeneratorTests(ITestOutputHelper helper) : base(helper)
    {
    }
    
    [Fact]
    public void ClearOnFinishEmittedWhenTrue()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(ClearOnFinish=true)]
                         public string Property {get;set;} = "Jeppe"
                    }
                    """).OutputShouldContain(".ClearOnFinish()");
    }
}

public class DefaultValueGeneratorTests : AutoSpectreGeneratorTestsBase
{
    public DefaultValueGeneratorTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public void DefaultValueCanPointToSelf()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(DefaultValueSource=nameof(Property))]
                         public string Property {get;set;} = "Jeppe"
                    }
                    """).OutputShouldContain(".DefaultValue(form.Property)");
    }
    
    [Fact]
    public void DefaultStyleUsedOnConfirmation()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(DefaultValueStyle="yellow")]
                         public bool Property {get;set;} 
                    }
                    """).OutputShouldContain(""".DefaultValueStyle("yellow")""");
    }
    
    [Fact]
    public void ChoicesStyleUsedOnConfirmation()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(ChoicesStyle="yellow")]
                         public bool Property {get;set;}
                    }
                    """).OutputShouldContain(""".ChoicesStyle("yellow")""").OutputShouldContain("new ConfirmationPrompt");
    }
    
    [Theory]
    [InlineData("public string DefaultValueSource {get;set}", ".DefaultValue(form.DefaultValueSource)")]
    [InlineData("public static string DefaultValueSource {get;set}", ".DefaultValue(Test.TestForm.DefaultValueSource)")]
    [InlineData("public static string DefaultValueSource() => string.Empty;", ".DefaultValue(Test.TestForm.DefaultValueSource())")]
    [InlineData("public string DefaultValueSource() => string.Empty;", ".DefaultValue(form.DefaultValueSource())")]
    [InlineData("public const string DefaultValueSource = string.Empty;", "TestForm.DefaultValueSource")]
    [InlineData("public static readonly string DefaultValueSource = string.Empty;", "TestForm.DefaultValueSource")]
    [InlineData("public static string DefaultValueSource = string.Empty;", "TestForm.DefaultValueSource")]
    [InlineData("public string DefaultValueSource = string.Empty;", "form.DefaultValueSource")]
    [InlineData("public readonly string DefaultValueSource = string.Empty;", "form.DefaultValueSource")]
    public void DefaultValueValidSource(string source, string expected)
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(DefaultValueSource=nameof(DefaultValueSource))]
                         public string Property {get;set;} = "Jeppe"
                         
                         {{ source }}
                    }
                    """).OutputShouldContain(expected);
    }
    
    [Theory]
    [InlineData("private string DefaultValueSource {get;set}")]
    [InlineData("private static string DefaultValueSource {get;set}")]
    [InlineData("private static string DefaultValueSource() => string.Empty;")]
    [InlineData("public int DefaultValueSource() => 45")]
  
    public void DefaultValueIsInvalidReturnsDiagnostic(string source)
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(DefaultValueSource=nameof(DefaultValueSource))]
                         public string Property {get;set;} = "Jeppe"
                         
                         {{ source }}
                    }
                    """).ShouldHaveSourceGeneratorDiagnosticOnlyOnce(DiagnosticIds.Id0025_DefaultValueSource_NotFound);
    }
    
    [Theory]
    [InlineData("public string DefaultValueSource {get;set}", ".DefaultValue(form.DefaultValueSource)")]
    [InlineData("public static string DefaultValueSource {get;set}", ".DefaultValue(Test.TestForm.DefaultValueSource)")]
    [InlineData("public static string DefaultValueSource() => string.Empty;", ".DefaultValue(Test.TestForm.DefaultValueSource())")]
    [InlineData("public string DefaultValueSource() => string.Empty;", ".DefaultValue(form.DefaultValueSource())")]
    public void DefaultValueValidSourceTypeEnumerable(string source, string expected)
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(DefaultValueSource=nameof(DefaultValueSource))]
                         public List<string> Property {get;set;} = "Jeppe"

                         {{ source }}
                    }
                    """).OutputShouldContain(expected);
    }

    [Fact]
    public void EditableDefaultValueEmittedWhenDefaultResolves()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(DefaultValueSource=nameof(PropertyDefault), EditableDefaultValue=true)]
                         public string Property {get;set;}

                         public string PropertyDefault => "Jeppe";
                    }
                    """)
            .OutputShouldContain(".DefaultValue(form.PropertyDefault)")
            .OutputShouldContain(".EditableDefaultValue(true)")
            .HasNoSourceGeneratorDiagnosticWith(DiagnosticIds.Id0030_EditableDefaultValue_Ignored);
    }

    [Fact]
    public void EditableDefaultValueFalseByDefault()
    {
        var output = GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(DefaultValueSource=nameof(PropertyDefault))]
                         public string Property {get;set;}

                         public string PropertyDefault => "Jeppe";
                    }
                    """);
        output.Output.Should().NotContain(".EditableDefaultValue");
    }

    [Fact]
    public void EditableDefaultValueWithoutDefaultSourceReportsDiagnostic()
    {
        var output = GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(EditableDefaultValue=true)]
                         public string Property {get;set;}
                    }
                    """)
            .ShouldHaveSourceGeneratorDiagnosticOnlyOnce(DiagnosticIds.Id0030_EditableDefaultValue_Ignored);
        output.Output.Should().NotContain(".EditableDefaultValue");
    }

    [Fact]
    public void EditableDefaultValueOnBoolReportsDiagnostic()
    {
        var output = GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [TextPrompt(EditableDefaultValue=true)]
                         public bool Property {get;set;}
                    }
                    """)
            .ShouldHaveSourceGeneratorDiagnosticOnlyOnce(DiagnosticIds.Id0030_EditableDefaultValue_Ignored)
            .OutputShouldContain("new ConfirmationPrompt");
        output.Output.Should().NotContain(".EditableDefaultValue");
    }
}
