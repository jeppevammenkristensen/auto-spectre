using System.Threading.Tasks;
using Xunit;
using Verify = AutoSpectreAnalyzer.Test.Utilities.CSharpVerifier<AutoSpectreAnalyzer.MissingAskAttributeAnalyzer>;

namespace AutoSpectreAnalyzer.Test
{
    public class AskAnalyzerUnitTest
    {
        //No diagnostics expected to show up
        [Fact]
        public async Task TestMethod1()
        {
            var test = @"";

            await Verify.VerifyAnalyzerAsyncV2(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [Fact]
        public async Task TestMethod2()
        {
            var test = @"
    using System;
    using AutoSpectre;

    namespace ConsoleApplication1
    {
        [AutoSpectreForm]
        public class TestForm 
        {   
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

            var expected = Verify.Diagnostic(MissingAskAttributeAnalyzer.DiagnosticId)
                .WithSpan(10, 27, 10, 36)
                .WithArguments("FirstName");
            await Verify.VerifyCodeFixAsyncV2(test,changed,0, expected);
        }
    }
}