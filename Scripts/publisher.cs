#:package FileBasedApp.Toolkit@0.20.0
#:package FileBasedApp.Toolkit.Dotnet@0.20.0
#:package AutoSpectre@0.13.0-alpha-04
#:property PublishAot=false 
using Spectre.Console.Cli;
using Spectre.Console;
using FileBasedApp.Toolkit;
using System.IO.Abstractions;
using AutoSpectre;
using FileBasedApp.Toolkit.CommandCli;
using FileBasedApp.Toolkit.Dotnet;

var commandApp = new CommandApp<RunCommand>().WithDescription("Enter the description here");
commandApp.Configure(ctx =>
{
});
return await commandApp.RunAsync(args);
public class RunCommand : AsyncCommand<RunCommand.Settings> // For sync only you can use Command (and have Execute instead of ExecuteAsync
{
    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var autospectreCsproj = PathUtil.GetExecutionFolder() / ".." / "Src" / "SpectreSourceGenerator.slnx";
        
        if (!autospectreCsproj.FileExists())
        {
            throw new InvalidOperationException(
                $"Failed to locate path {autospectreCsproj.Value}");
        }
        
        var prompt = new Prompt(settings.Source, settings.Release).Prompt();

        var executionFolder = PathUtil.GetExecutionFolder() / "Artifacts";

        try
        {
            executionFolder.CreateDirectory();
            var runner = DotnetPackSimpleRunner.Init()
                .WithOutput(executionFolder)
                .WithProject(autospectreCsproj)
                .WithConfiguration(prompt.Release ? "Release" : "Debug");

            if (settings.Release)
            { 
                runner
                    .AddArgument("-p:IncludeSymbols=true")
                    .AddArgument("-p:SymbolPackageFormat=snupkg");
            }
            else
            {
                runner.AddArgument("-p:EmbedAllSources=true");
                runner.AddArgument("-p:GenerateDocumentationFile=true");
                runner.AddArgument("-p:DebugType=embedded");
            }

            await runner.RunAsync(token: cancellationToken);

            
            
            foreach (var absolutePath in executionFolder.GetAllFiles("*nupkg"))
            {
                AnsiConsole.Write(new Rule(absolutePath.FileName));
                await DotnetNugetPushSimpleRunner.Init()
                    .WithPackage(absolutePath)
                    .WithSource(prompt.Source!)
                    .WithSkipDuplicate()
                    .WithConditionalApiKey(prompt.ApiKey).RunAsync(token: cancellationToken);

            }
            
        }
        finally
        {
            executionFolder.SafeDeleteDirectory();
        }
        
        
        
        return 0; // 0 for success
    }

    public class Settings : ExtendedCommandSettings
    {
        [CommandOption("--release", false)]
        public bool Release { get; set; }
        
        [CommandOption("--source")]
        public string Source { get; set; }
        

        protected override ValidationResult DoValidate()
        {
            
            return base.DoValidate();
        }
    }
}


    [AutoSpectreForm]

    public class Prompt
    {
        public Prompt(string source, bool settingsRelease)
        {
            Source = source;
            Release = settingsRelease;
        }

        public bool SourceCondition() => string.IsNullOrWhiteSpace(Source);

        [TextPrompt(Condition = nameof(Release), NegateCondition = true)]
        public bool Release { get; set; }
        
        [SelectPrompt] public string? Source { get; set; }

        public string[] SourceSource = ["local", "github", "nuget"];

        [TextPrompt(Secret = true)] public string ApiKey { get; set; }

        public bool ApiKeyCondition() => Source == "nuget";
    }
