using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace AutoSpectre.SourceGeneration.Tests;

public class ConditionTests : AutoSpectreGeneratorTestsBase
{
    public ConditionTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public void PropertyWithConditionValidPropertyByConventionReturnsExpected()
    {
        GetGeneratedOutput("""
                           using AutoSpectre;
                           using System.Collections.Generic;

                           namespace Test;

                           [AutoSpectreForm]
                           public class ConditionClass
                           {
                               [TextPrompt]
                               public string Name { get; set;}
                               
                               public bool NameCondition => true;
                           }
                           """).Should().Contain(@"if (destination.NameCondition == true)");
    }
    
    [Fact]
    public void PropertyWithConditionValidMethodByConventionReturnsExpected()
    {
        GetGeneratedOutput("""
                           using AutoSpectre;
                           using System.Collections.Generic;

                           namespace Test;

                           [AutoSpectreForm]
                           public class ConditionClass
                           {
                               [Ask]
                               public string Name { get; set;}
                               
                               public bool NameCondition() => true
                           }
                           """).Should().Contain(@"if (destination.NameCondition() == true)");
    }
    
    [Fact]
    public void PropertyWithConditionValidMethodByReferenceReturnsExpected()
    {
        GetGeneratedOutput("""
                           using AutoSpectre;
                           using System.Collections.Generic;

                           namespace Test;

                           [AutoSpectreForm]
                           public class ConditionClass
                           {
                               [Ask(Condition = "CustomCondition")]
                               public string Name { get; set;}
                               
                               public bool CustomCondition() => true
                           }
                           """).Should().Contain(@"if (destination.CustomCondition() == true)");
    }
    
    [Fact]
    public void PropertyWithConditionNegatedValidMethodByReferenceReturnsExpected()
    {
        GetGeneratedOutput("""
                           using AutoSpectre;
                           using System.Collections.Generic;

                           namespace Test;

                           [AutoSpectreForm]
                           public class ConditionClass
                           {
                               [Ask(Condition = "CustomCondition", NegateCondition=true)]
                               public string Name { get; set;}
                               
                               public bool CustomCondition() => true;
                           }
                           """).Should().Contain(@"if (destination.CustomCondition() == false)");
    }
}