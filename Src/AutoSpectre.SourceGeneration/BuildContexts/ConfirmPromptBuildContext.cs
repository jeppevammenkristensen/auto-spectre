namespace AutoSpectre.SourceGeneration.BuildContexts;

public class ConfirmPromptBuildContext : PromptBuildContext
{
    public string Title { get; }
    public bool IsNullable { get; }

    public ConfirmPromptBuildContext(string title, bool isNullable,
        SinglePropertyEvaluationContext evaluationContext) : base(evaluationContext)
    {
        Title = title;
        IsNullable = isNullable;
    }

    public override string GenerateOutput(string destination)
    {
        return $"""{destination} = {PromptPart()};""";
    }

    public override string PromptPart(string? variableName = null)
    {
        return $"""AnsiConsole.Confirm("{Title}")""";
    }
}