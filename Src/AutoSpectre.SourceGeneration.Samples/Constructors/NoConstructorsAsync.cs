namespace AutoSpectre.SourceGeneration.Samples;

[AutoSpectreForm]
public class NoConstructorsAsync
{
    [TaskStep]
    public Task TaskStep()
    {
        return Task.CompletedTask;
    }
}