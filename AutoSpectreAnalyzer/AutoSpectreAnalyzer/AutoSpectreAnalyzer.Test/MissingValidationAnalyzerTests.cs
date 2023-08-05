using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using AutoSpectreAnalyzer.Test.Utilities;
using Xunit;
using Verify = AutoSpectreAnalyzer.Test.Utilities.CSharpVerifier<AutoSpectreAnalyzer.MissingValidationAnalyzer>;


namespace AutoSpectreAnalyzer.Test;

public class MissingValidationAnalyzerTests
{
    [Fact]
    public async Task HasNoValidationMethodAndNoValidatorSetInAskAttributeReturnsDiagnostic()
    {
        var test = @"
    using System;
    using AutoSpectre;

    namespace ConsoleApplication1
    {
        [AutoSpectreForm]
        public class TestForm 
        {   
            [Ask]
            public string FirstName {get;set;}
        }
    }";
        var changed = @"using System;
using AutoSpectre;

namespace ConsoleApplication1
{
    [AutoSpectreForm]
    public class TestForm
    {
        [Ask]
        public string FirstName { get; set; }
    }
}";

        var expected = Verify.Diagnostic(MissingValidationAnalyzer.DiagnosticId)
            .WithSpan(11, 27, 11, 36)
            .WithArguments("FirstName");
        await Verify.VerifyAnalyzerAsyncV2(test,expected);
    }

    [Fact]
    public async Task HasNoValidationMethodButValidatorSetInAskAttributeReturnsNoDiagnostic()
    {
        var test = """
    using System;
    using AutoSpectre;

    namespace ConsoleApplication1
    {
        [AutoSpectreForm]
        public class TestForm 
        {   
            [Ask(Validator="")]
            public string FirstName {get;set;}
        }
    }
    """;
      
        await Verify.VerifyAnalyzerAsyncV2(test);
    }

    [Fact]
    public async Task SingleItemPropertyHasValidationMethodWithWrongParametersReturnsDiagnostic()
    {
        var test = @"
    using System;
    using AutoSpectre;

    namespace ConsoleApplication1
    {
        [AutoSpectreForm]
        public class TestForm 
        {   
            [Ask]
            public string FirstName {get;set;}

            [Ask]
            public string FirstNameValidator(string[] items, string lastName)
            {
                return null;
            }
        }        
    }";
       

        var expected = Verify.Diagnostic(MissingValidationAnalyzer.DiagnosticId)
            .WithSpan(11, 27, 11, 36)
            .WithArguments("FirstName");
        await Verify.VerifyAnalyzerAsyncV2(test,expected);
    }

    [Fact]
    public async Task SingleItemPropertyHasValidationMethodWithCorrectParametersReturnsNoDiagnostic()
    {
        var test = @"
    using System;
    using AutoSpectre;

    namespace ConsoleApplication1
    {
        [AutoSpectreForm]
        public class TestForm 
        {   
            [Ask]
            public string FirstName {get;set;}

            [Ask]
            public string FirstNameValidator(string lastName)
            {
                return null;
            }
        }        
    }";
       
        await Verify.VerifyAnalyzerAsyncV2(test);
    }


    [Fact]
    public async Task EnumerableItemPropertyHasValidationMethodWithWrongParametersReturnsDiagnostic()
    {
        var test = @"
    using System;
    using AutoSpectre;

    namespace ConsoleApplication1
    {
        [AutoSpectreForm]
        public class TestForm 
        {   
            [Ask]
            public string[] FirstName {get;set;}

            [Ask]
            public string FirstNameValidator(string lastName)
            {
                return null;
            }
        }        
    }";
        
        var expected = Verify.Diagnostic(MissingValidationAnalyzer.DiagnosticId)
            .WithSpan(11, 29, 11, 38)
            .WithArguments("FirstName");
        await Verify.VerifyAnalyzerAsyncV2(test,expected);
    }

    [Fact]
    public async Task EnumerableItemPropertyHasValidationMethodWithCorrectParametersReturnsNoDiagnostic()
    {
        var test = @"
    using System;
    using AutoSpectre;

    namespace ConsoleApplication1
    {
        [AutoSpectreForm]
        public class TestForm 
        {   
            [Ask]
            public string[] FirstName {get;set;}

            [Ask]
            public string FirstNameValidator(string[] items, string lastName)
            {
                return null;
            }
        }        
    }";
        
        
        await Verify.VerifyAnalyzerAsyncV2(test);
    }

    [Fact]
    public async Task MissingMethodGeneratedBasedOnDiagnostic()
    {
        var test = @"
    using System;
    using AutoSpectre;

    namespace ConsoleApplication1
    {
        [AutoSpectreForm]
        public class TestForm 
        {   
            [Ask]
            public string[] FirstName {get;set;}

            
            
        }        
    }";

        var after = """
using System;
using AutoSpectre;

namespace ConsoleApplication1
{
    [AutoSpectreForm]
    public class TestForm
    {
        [Ask]
        public string[] FirstName { get; set; }


        public string? FirstNameValidator(string[] items, string item)
        {
            return null;
        }
    }
}
""";

        var expected = Verify.Diagnostic(MissingValidationAnalyzer.DiagnosticId)
            .WithSpan(11, 29, 11, 38)
            .WithArguments("FirstName");
        
        
        await Verify.VerifyCodeFixAsyncV2(test, after, null,expected);
    }

    [Fact]
    public async Task MissingMethodGeneratedBasedOnDiagnosticWithCorrectParameters()
    {
        var test = """
            using System;
            using AutoSpectre;
            using System.Collections.Generic;

            [AutoSpectreForm]
            public class Inner
            {
                private HashSet<string> AcceptedNames { get; } = new HashSet<string>(new[] { "Jeppe", "Lene", "Andreas", "Emilie", "Alberte" }, StringComparer.OrdinalIgnoreCase);

                [Ask]
                public string Name { get; set; }

                [Ask]
                public string? LastName { get; set; }

                public string? NameValidator(string name)
                {
                    if (AcceptedNames.Contains(name.Trim()))
                        return null;
                    return "Name was not in the list of accepted names.";
                }
            }

            
            [AutoSpectreForm]
            public class Outer
            {
                [Ask]
                public string Name { get; set; }                
            }
            """;

        var after = "";

        var expected = Verify.Diagnostic(MissingValidationAnalyzer.DiagnosticId)
            .WithSpan(14, 20, 14, 28)
            .WithArguments("LastName");
        
        
        await Verify.VerifyCodeFixAsyncV2(test, after, null,expected);
    }
}