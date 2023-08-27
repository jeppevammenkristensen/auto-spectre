using AutoSpectre;
using Autospectre.Examples.Examples.Database;

namespace Autospectre.Examples.Examples;

[AutoSpectreForm]
public class Examples
{
    [SelectPrompt]
    public IExample SelectExample { get; set; }
    
    public IEnumerable<IExample> SelectExampleSource()
    {
        yield return new LoginExample();
        yield return new DatabaseExample();
    }

    public string SelectExampleConverter(IExample example)
    {
        return example.GetType().Name;
    }
}