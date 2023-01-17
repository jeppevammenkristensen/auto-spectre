using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoSpectre.SourceGeneration;

public abstract class PromptBuildContext
{
    public abstract string GenerateOutput(string destination);

    public abstract string PromptPart();

    /// <summary>
    /// This defaults to not do anything. But can be overloaded to for example
    /// initialize a necessary class
    /// </summary>
    /// <param name="builder"></param>
    public virtual IEnumerable<string> CodeInitializing()
    {
        return Enumerable.Empty<string>();
    }

    public virtual IEnumerable<string> Namespaces()
    {
        return Enumerable.Empty<string>();
    }
}