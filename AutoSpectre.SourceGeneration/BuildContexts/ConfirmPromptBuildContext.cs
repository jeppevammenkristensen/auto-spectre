using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration.BuildContexts;

public class ConfirmPromptBuildContext : PromptBuildContext
{
    public string Title { get; }
    public ITypeSymbol Type { get; }
    public bool IsNullable { get; }

    public ConfirmPromptBuildContext(string title, ITypeSymbol type, bool isNullable)
    {
        Title = title;
        Type = type;
        IsNullable = isNullable;
    }

    public override string GenerateOutput(string destination)
    {
        return $"""{destination} = AnsiConsole.Confirm("{Title}")""";
    }
}