using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace AutoSpectre.SourceGeneration.Tests;

public class CancellationTokenTests : AutoSpectreGeneratorTestsBase
{
    public CancellationTokenTests(ITestOutputHelper helper) : base(helper)
    {
    }

    [Fact]
    public void SynchronousForm_RetainsPromptAndAddsCancellableAsyncContract()
    {
        var output = GetOutput(Form("[TextPrompt] public string Name { get; set; } = \"\";"))
            .ShouldCompileWithoutErrors();

        var root = CSharpSyntaxTree.ParseText(output.Output, cancellationToken: TestContext.Current.CancellationToken)
            .GetRoot(TestContext.Current.CancellationToken);
        var factoryInterface = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>().Single();
        factoryInterface.BaseList!.ToString().Should()
            .Contain("ISpectreFactory<Test.TestForm>")
            .And.Contain("IAsyncSpectreFactory<Test.TestForm>")
            .And.Contain("ICancellableSpectreFactory<Test.TestForm>");
        FactoryMethod(output, "Prompt", 1).ReturnType.ToString().Should().Be("Test.TestForm");
        FactoryMethod(output, "PromptAsync", 1).ToString().Should()
            .Contain("PromptAsync(form, CancellationToken.None)");
        FactoryMethod(output, "PromptAsync", 2).Modifiers.Should()
            .Contain(x => x.IsKind(SyntaxKind.AsyncKeyword));
    }

    [Fact]
    public void AsynchronousForm_PreservesLegacyOverrideAndOverridesTokenOverload()
    {
        var output = GetOutput(Form("[TaskStep] public Task Work() => Task.CompletedTask;"))
            .ShouldCompileWithoutErrors();

        var legacy = FactoryMethod(output, "PromptAsync", 1);
        legacy.Modifiers.Should().Contain(x => x.IsKind(SyntaxKind.OverrideKeyword));
        legacy.ToString().Should().Contain("PromptAsync(form, CancellationToken.None)");
        var cancellable = FactoryMethod(output, "PromptAsync", 2);
        cancellable.Modifiers.Should().Contain(x => x.IsKind(SyntaxKind.OverrideKeyword));
        cancellable.Modifiers.Should().Contain(x => x.IsKind(SyntaxKind.AsyncKeyword));
        cancellable.ToString().Should().Contain("await form.Work()");
    }

    [Theory]
    [InlineData("[TextPrompt] public string Name { get; set; } = \"\";", "TextPrompt<string>")]
    [InlineData("[TextPrompt] public bool Enabled { get; set; }", "ConfirmationPrompt")]
    [InlineData("[TextPrompt] public DayOfWeek Day { get; set; }", "SelectionPrompt<System.DayOfWeek>")]
    [InlineData(
        "[SelectPrompt] public string Choice { get; set; } = \"\"; public string[] ChoiceSource => new[] { \"a\", \"b\" };",
        "SelectionPrompt<string>")]
    [InlineData(
        "[SelectPrompt] public List<string> Choices { get; set; } = new(); public string[] ChoicesSource => new[] { \"a\", \"b\" };",
        "MultiSelectionPrompt<string>")]
    public void EveryPromptKind_UsesAsyncPromptWithToken(string member, string promptType)
    {
        var output = GetOutput(Form(member)).ShouldCompileWithoutErrors();
        var method = FactoryMethod(output, "PromptAsync", 2);
        method.ToString().Should().Contain(promptType);
        var prompts = method.DescendantNodes().OfType<InvocationExpressionSyntax>()
            .Where(x => x.Expression.ToString() == "AnsiConsole.PromptAsync").ToList();
        prompts.Should().NotBeEmpty();
        foreach (var prompt in prompts)
        {
            prompt.ArgumentList.Arguments.Last().Expression.ToString().Should().Be("cancellationToken");
            prompt.Ancestors().OfType<InvocationExpressionSyntax>().Should()
                .Contain(x => x.Expression.ToString().EndsWith(".ConfigureAwait") &&
                              x.ArgumentList.ToString() == "(false)");
            prompt.Ancestors().Should().Contain(x => x is AwaitExpressionSyntax);
        }

        method.ToString().Should().NotContain("AnsiConsole.Prompt(");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NestedForms_ForwardTokenIncludingCollectionItems(bool collection)
    {
        var member = collection
            ? "[TextPrompt] public List<ChildForm> Children { get; set; } = new();"
            : "[TextPrompt] public ChildForm Child { get; set; } = new();";
        var output = GetOutput(Form(member) + """

                                              [AutoSpectreForm]
                                              public class ChildForm
                                              {
                                                  [TextPrompt] public string Name { get; set; } = "";
                                              }
                                              """).ShouldCompileWithoutErrors();
        var method = FactoryMethod(output, "PromptAsync", 2);
        var nestedPrompts = method.DescendantNodes().OfType<InvocationExpressionSyntax>()
            .Where(x => x.Expression is MemberAccessExpressionSyntax access &&
                        access.Name.Identifier.ValueText == "PromptAsync" &&
                        access.Expression.ToString() != "AnsiConsole" &&
                        x.ArgumentList.Arguments.Count == 2).ToList();

        nestedPrompts.Should().NotBeEmpty();
        foreach (var prompt in nestedPrompts)
        {
            prompt.ArgumentList.Arguments.Should().HaveCount(2);
            prompt.ArgumentList.Arguments.Last().Expression.ToString().Should().Be("cancellationToken");
            prompt.Ancestors().Should().Contain(x => x is AwaitExpressionSyntax);
        }
    }

    [Fact]
    public void MultiAdd_UsesAsyncConfirmationWithToken()
    {
        var output = GetOutput(Form("[TextPrompt] public List<string> Names { get; set; } = new();"))
            .ShouldCompileWithoutErrors();
        var method = FactoryMethod(output, "PromptAsync", 2);

        method.ToString().Should().Contain("await AnsiConsole.PromptAsync(")
            .And.Contain("await AnsiConsole.ConfirmAsync(")
            .And.Contain("cancellationToken: cancellationToken).ConfigureAwait(false)")
            .And.NotContain("AnsiConsole.Confirm(");
    }

    [Theory]
    [InlineData("CancellationToken token", "cancellationToken", "CancellationToken.None")]
    [InlineData("IAnsiConsole console, CancellationToken token", "AnsiConsole.Console, cancellationToken",
        "AnsiConsole.Console, CancellationToken.None")]
    [InlineData("CancellationToken token, IAnsiConsole console", "cancellationToken, AnsiConsole.Console",
        "CancellationToken.None, AnsiConsole.Console")]
    public void SynchronousTaskSteps_InjectTokenInDeclaredParameterOrder(
        string parameters, string cancellableArguments, string legacyArguments)
    {
        var output = GetOutput(Form($"[TaskStep] public void Work({parameters}) {{ }}"))
            .ShouldCompileWithoutErrors();

        FactoryMethod(output, "Prompt", 1).ToString().Should().Contain($"form.Work({legacyArguments})");
        FactoryMethod(output, "PromptAsync", 2).ToString().Should().Contain($"form.Work({cancellableArguments})");
        output.GeneratorDiagnostics.Should().NotContain(x => x.Severity == DiagnosticSeverity.Error);
    }

    [Theory]
    [InlineData("CancellationToken token", "cancellationToken")]
    [InlineData("IAnsiConsole console, CancellationToken token", "AnsiConsole.Console, cancellationToken")]
    [InlineData("CancellationToken token, IAnsiConsole console", "cancellationToken, AnsiConsole.Console")]
    public void AsynchronousTaskSteps_InjectTokenInDeclaredParameterOrder(string parameters, string arguments)
    {
        var output = GetOutput(Form($"[TaskStep] public Task Work({parameters}) => Task.CompletedTask;"))
            .ShouldCompileWithoutErrors();

        FactoryMethod(output, "PromptAsync", 2).ToString().Should().Contain($"await form.Work({arguments})");
        output.GeneratorDiagnostics.Should().NotContain(x => x.Severity == DiagnosticSeverity.Error);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void StatusTaskSteps_ForwardToken(bool asynchronous)
    {
        var member = asynchronous
            ? "[TaskStep(UseStatus = true, StatusText = \"Working\")] public Task Work(CancellationToken token) => Task.CompletedTask;"
            : "[TaskStep(UseStatus = true, StatusText = \"Working\")] public void Work(CancellationToken token) { }";
        var output = GetOutput(Form(member)).ShouldCompileWithoutErrors();

        FactoryMethod(output, "PromptAsync", 2).ToString().Should().Contain("form.Work(cancellationToken)");
    }

    [Fact]
    public void CancellationIsCheckedBeforeEachStep()
    {
        var output = GetOutput(Form("""
                                    [TaskStep] public void First() { }
                                    [TaskStep] public void Second() { }
                                    """)).ShouldCompileWithoutErrors();
        var body = FactoryMethod(output, "PromptAsync", 2).Body!.ToString();
        var firstStep = body.IndexOf("form.First()", StringComparison.Ordinal);
        var secondStep = body.IndexOf("form.Second()", StringComparison.Ordinal);

        firstStep.Should().BeGreaterThan(0);
        secondStep.Should().BeGreaterThan(firstStep);
        body[..firstStep].Should().Contain("cancellationToken.ThrowIfCancellationRequested()");
        body[firstStep..secondStep].Should().Contain("cancellationToken.ThrowIfCancellationRequested()");
    }

    [Fact]
    public void LegacyInterfaceImplementationsAndBaseSubclasses_StillCompile()
    {
        GetOutput(Form("[TextPrompt] public string Name { get; set; } = \"\";") + """

            public sealed class LegacySyncFactory : ISpectreFactory<TestForm>
            {
                public TestForm Prompt(TestForm form) => form;
            }

            public sealed class LegacyAsyncFactory : IAsyncSpectreFactory<TestForm>
            {
                public TestForm Prompt(TestForm form) => form;
                public Task<TestForm> PromptAsync(TestForm form) => Task.FromResult(form);
            }

            public sealed class LegacyBaseFactory : AsyncSpectreFactoryBase<TestForm>
            {
                public override Task<TestForm> PromptAsync(TestForm form) => Task.FromResult(form);
            }
            """).ShouldCompileWithoutErrors();
    }

    [Fact]
    public async Task LegacyBaseFallback_PreCanceledTokenDoesNotInvokeLegacyMethod()
    {
        var factory = new LegacyStringFactory();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await factory.PromptAsync("value", cancellation.Token));

        factory.Calls.Should().Be(0);
    }

    [Fact]
    public async Task LegacyBaseFallback_UncanceledTokenDelegatesToLegacyMethod()
    {
        var factory = new LegacyStringFactory();

        var result = await factory.PromptAsync("value", CancellationToken.None);

        result.Should().Be("value");
        factory.Calls.Should().Be(1);
    }

    [Fact]
    public async Task GeneratedFactory_PreCanceledTokenDoesNotExecuteSteps()
    {
        var (factory, form, method) =
            CreateGeneratedFactory(
                """[TaskStep] public void Work() => throw new InvalidOperationException("Must not run");""");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var task = (Task) method.Invoke(factory, new[] {form, (object) cancellation.Token})!;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
    }

    [Fact]
    public async Task GeneratedFactory_CancellationBetweenStepsRetainsCompletedChanges()
    {
        var (factory, form, method) = CreateGeneratedFactory("""
                                                             public CancellationTokenSource Stop { get; set; } = new();
                                                             public int Completed { get; set; }
                                                             [TaskStep] public void First() { Completed = 1; Stop.Cancel(); }
                                                             [TaskStep] public void Second() { Completed = 2; }
                                                             """);
        using var cancellation = (CancellationTokenSource) form.GetType().GetProperty("Stop")!.GetValue(form)!;

        var task = (Task) method.Invoke(factory, new[] {form, (object) cancellation.Token})!;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        form.GetType().GetProperty("Completed")!.GetValue(form).Should().Be(1);
    }

    [Fact]
    public async Task GeneratedFactory_TaskStepReceivesCallerToken()
    {
        var (factory, form, method) = CreateGeneratedFactory("""
                                                             public CancellationToken Seen { get; set; }
                                                             [TaskStep] public Task Work(CancellationToken token) { Seen = token; return Task.CompletedTask; }
                                                             """);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);

        await (Task) method.Invoke(factory, new[] {form, (object) cancellation.Token})!;

        form.GetType().GetProperty("Seen")!.GetValue(form).Should().Be(cancellation.Token);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GeneratedFactory_BreakStepDoesNotSwallowCancellation(bool asynchronous)
    {
        var step = asynchronous
            ? "[Break] public Task StopNow() { Stop.Cancel(); return Task.CompletedTask; }"
            : "[Break] public void StopNow() { Stop.Cancel(); }";
        var (factory, form, method) =
            CreateGeneratedFactory("public CancellationTokenSource Stop { get; set; } = new(); " + step);
        using var cancellation = (CancellationTokenSource) form.GetType().GetProperty("Stop")!.GetValue(form)!;

        var task = (Task) method.Invoke(factory, new[] {form, (object) cancellation.Token})!;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
    }

    private (object Factory, object Form, System.Reflection.MethodInfo Method) CreateGeneratedFactory(string members)
    {
        var output = GetOutput(Form(members)).ShouldCompileWithoutErrors();
        using var stream = new System.IO.MemoryStream();
        var result = output.OutputCompilation.WithAssemblyName("CancellationTest_" + Guid.NewGuid().ToString("N"))
            .Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
        result.Success.Should().BeTrue(string.Join(Environment.NewLine, result.Diagnostics));
        var assembly = System.Reflection.Assembly.Load(stream.ToArray());
        var factoryType = assembly.GetType("Test.TestFormSpectreFactory")!;
        var formType = assembly.GetType("Test.TestForm")!;
        return (Activator.CreateInstance(factoryType)!, Activator.CreateInstance(formType)!,
            factoryType.GetMethod("PromptAsync", new[] {formType, typeof(CancellationToken)})!);
    }

    private static string Form(string members) => $$"""
                                                    using System;
                                                    using System.Collections.Generic;
                                                    using System.Threading;
                                                    using System.Threading.Tasks;
                                                    using AutoSpectre;
                                                    using Spectre.Console;

                                                    namespace Test;

                                                    [AutoSpectreForm]
                                                    public class TestForm
                                                    {
                                                        {{members}}
                                                    }
                                                    """;

    private static MethodDeclarationSyntax FactoryMethod(TestOutput output, string name, int parameterCount)
    {
        return output.OutputCompilation.SyntaxTrees.SelectMany(x => x.GetRoot().DescendantNodes())
            .OfType<ClassDeclarationSyntax>().Single(x => x.Identifier.ValueText == "TestFormSpectreFactory")
            .Members.OfType<MethodDeclarationSyntax>()
            .Single(x => x.Identifier.ValueText == name && x.ParameterList.Parameters.Count == parameterCount);
    }

    private sealed class LegacyStringFactory : AsyncSpectreFactoryBase<string>
    {
        public int Calls { get; private set; }

        public override Task<string> PromptAsync(string? form)
        {
            Calls++;
            return Task.FromResult(form!);
        }
    }
}
