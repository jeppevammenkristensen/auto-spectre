using AutoSpectre;

namespace Test.OneDirectory;

[AutoSpectreForm]
public class Subclass2
{
    [TextPrompt]
    public string Name { get; set; }
}