using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoSpectre.SourceGeneration;

public abstract class PromptBuildContext
{
    public virtual bool DeclaresVariable
    {
        get { return false; }
    }

    public abstract string GenerateOutput(string destination);

    /// <summary>
    /// The parts that does the prompting. If the prompt result is stored in a variable
    /// this name can be replaced in the variableName. It can bee 
    /// </summary>
    /// <param name="variableName">If the value is stored in a variable it can be replaced here</param>
    /// <returns></returns>
    public abstract string PromptPart(string? variableName = null);

    /// <summary>
    /// This defaults to not do anything. But can be overloaded to for example
    /// initialize a necessary class
    /// </summary>
    public virtual IEnumerable<string> CodeInitializing()
    {
        return Enumerable.Empty<string>();
    }

    public virtual IEnumerable<string> Namespaces()
    {
        return Enumerable.Empty<string>();
    }
}