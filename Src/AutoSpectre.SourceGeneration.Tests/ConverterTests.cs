using Xunit;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests;

public class ConverterTests : AutoSpectreGeneratorTestsBase
{
    public ConverterTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Theory]
    [InlineData("public static string PropertyConverter(DateTime input) => string.Empty;",".UseConverter(Test.TestForm.PropertyConverter)")]
    [InlineData("public string PropertyConverter(DateTime input) => string.Empty;",".UseConverter(form.PropertyConverter)")]
    public void ValidConverterSinglePromptGeneratesExpectedResult(string converter, string expected)
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;
                    using System;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [SelectPrompt()]
                         public DateTime Property {get;set;}
                         
                         public List<DateTime> PropertySource {get;set;}
                         
                         {{converter}}
                    }
                    """).OutputShouldContain(expected);
    }
    
    [Theory]
    [InlineData("public static string PropertyConverter(DateTime input) => string.Empty;",".UseConverter(Test.TestForm.PropertyConverter)")]
    [InlineData("public string PropertyConverter(DateTime input) => string.Empty;",".UseConverter(form.PropertyConverter)")]
    public void ValidConverterMultiSelectPromptGeneratesExpectedResult(string converter, string expected)
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;
                    using System;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm
                    {
                         [SelectPrompt(Converter = nameof(PropertyConverter))]
                         public List<DateTime> Properties {get;set;}
                         
                         public List<DateTime> PropertiesSource {get;set;}
                         
                         {{converter}}
                    }
                    """).OutputShouldContain(expected);
    }
}