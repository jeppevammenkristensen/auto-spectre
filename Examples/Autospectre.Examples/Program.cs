// See https://aka.ms/new-console-template for more information

using Autospectre.Examples.Examples;
using Spectre.Console;
var examplesSpectreFactory = new ExamplesSpectreFactory();
while (true)
{
    
    var examples = examplesSpectreFactory.Get();
    
    await examples.SelectExample.Run();
}
