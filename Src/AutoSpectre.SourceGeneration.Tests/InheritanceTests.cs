using Xunit;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests;

public class InheritanceTests : AutoSpectreGeneratorTestsBase
{
    public InheritanceTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public void InheritedValuesAreUsed()
    {
        GetOutput($$"""
                    using AutoSpectre;
                    using System.Collections.Generic;

                    namespace Test;

                    [AutoSpectreForm]
                    public class TestForm : BaseClass
                    {
                         
                    }

                    public class BaseClass
                    {
                        [TextPrompt]
                        public string InheritedValue {get;set;}
                    }
                    """).OutputShouldContain("form.InheritedValue = ");
    }
}