namespace AutoSpectre.SourceGeneration;

public abstract class PromptBuildContext
{
    public abstract string GenerateOutput(string destination);

    public abstract string PromptPart();
}