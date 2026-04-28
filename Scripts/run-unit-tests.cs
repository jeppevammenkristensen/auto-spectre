#:package FileBasedApp.Toolkit@0.20.0
#:package FileBasedApp.Toolkit.Dotnet@0.20.0
#:property PublishAot=false
using System.IO.Abstractions;
using Spectre.Console.Cli;
using Spectre.Console;
using FileBasedApp.Toolkit;
using FileBasedApp.Toolkit.CommandCli;
using FileBasedApp.Toolkit.SimpleExec;

var commandApp = new CommandApp<RunCommand>().WithDescription("Discovers and runs all test projects under ../Src (intended for GitHub Actions CI).");
commandApp.Configure(ctx => { });
return await commandApp.RunAsync(args);

public class RunCommand : AsyncCommand<RunCommand.Settings>
{
    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var srcRoot = PathUtil.GetExecutionFolder() / ".." / "Src";
        if (!srcRoot.DirectoryExists())
        {
            AnsiConsole.MarkupLineInterpolated($"[red]Src directory not found at {srcRoot.Value}[/]");
            return 1;
        }

        var spectresourcegeneratorSlnx = srcRoot / "SpectreSourceGenerator.slnx";
        
        AnsiConsole.Write(new Rule("[green]Restore[/]"));
        
        await SimpleExecRunner.Init("dotnet")
            .AddArgumentPair("restore", spectresourcegeneratorSlnx)
            //.AddArgumentPair("-c", "Release")
            .RunAsync(token: cancellationToken);

        AnsiConsole.Write(new Rule("[green]Build[/]"));
        
        await SimpleExecRunner.Init("dotnet")
            .AddArgumentPair("build", spectresourcegeneratorSlnx)
            .AddArgumentPair("-c", "Release")
            .RunAsync(token: cancellationToken);

        AnsiConsole.Write(new Rule("[green]Run unit-tests[/]"));
        
        var testProjects = srcRoot
            .EnumerateFiles("*.csproj", SearchOption.AllDirectories)
            .Where(p => p.FileName.EndsWith(".Tests.csproj", StringComparison.OrdinalIgnoreCase)
                     || p.FileName.EndsWith(".Test.csproj", StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.Value, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (testProjects.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No test projects found.[/]");
            return 0;
        }

        AnsiConsole.MarkupLineInterpolated($"[bold green]Found {testProjects.Count} test project(s):[/]");
        foreach (var project in testProjects)
        {
            AnsiConsole.MarkupLineInterpolated($"  [dim]- {project.RelativeTo(srcRoot)}[/]");
        }

        var failed = new List<string>();
        foreach (var project in testProjects)
        {
            AnsiConsole.Write(new Rule($"[bold]{project.FileName}[/]") { Justification = Justify.Left });

            try
            {
                await new SimpleExecRunner("dotnet")
                    .AddArgument("test")
                    .AddArgument(project.Value)
                    .AddArgument("--configuration").AddArgument(settings.Configuration)
                    .AddArgument("--nologo")
                    .AddArgument("-tl:off")
                    .AddArgument("--logger").AddArgument("console;verbosity=normal")
                    .RunAsync(token: cancellationToken);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLineInterpolated($"[red]FAILED: {project.FileName} — {ex.Message}[/]");
                failed.Add(project.RelativeTo(srcRoot).Value);
            }
        }

        AnsiConsole.Write(new Rule("[bold]Summary[/]") { Justification = Justify.Left });
        if (failed.Count == 0)
        {
            AnsiConsole.MarkupLineInterpolated($"[bold green]All {testProjects.Count} test project(s) passed.[/]");
            return 0;
        }

        AnsiConsole.MarkupLineInterpolated($"[bold red]{failed.Count} of {testProjects.Count} test project(s) failed:[/]");
        foreach (var f in failed)
        {
            AnsiConsole.MarkupLineInterpolated($"  [red]- {f}[/]");
        }
        return 1;
    }

    public class Settings : ExtendedCommandSettings
    {
        [CommandOption("-c|--configuration")]
        [System.ComponentModel.DefaultValue("Release")]
        public string Configuration { get; set; } = "Release";

        protected override ValidationResult DoValidate() => base.DoValidate();
    }
}
