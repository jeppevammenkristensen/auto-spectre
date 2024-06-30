namespace AutoSpectre.SourceGeneration.Samples;

[AutoSpectreForm]
public class MultipleConstructorsAsync
{
    private readonly string _name;

    public MultipleConstructorsAsync(string name)
    {
        _name = name;
    }
    
    [TaskStep]
    public Task TaskStep()
    {
        return Task.CompletedTask;
    }
}