using System.Collections;
using System.Collections.Generic;
using System.Composition.Hosting.Core;
using System.Linq;
using AutoSpectre.SourceGeneration.Extensions;
using Microsoft.CodeAnalysis.CSharp;

namespace AutoSpectre.SourceGeneration;

public abstract class PromptBuildContext
{
    public PromptBuildContext(SinglePropertyEvaluationContext context, string title)
    {
        Context = context;
        Title = title;
    }

    public virtual bool DeclaresVariable
    {
        get { return false; }
    }

    public SinglePropertyEvaluationContext Context { get; }

    public abstract string GenerateOutput(string destination);
    
    public string Title { get; }
    
    /// <summary>
    /// Generates a title for the given string with quotes. It will ensure that
    /// special characters are correctly handled for instance. That "Some \"name\"" will be
    /// outputted as just that and not "Some "name""
    /// </summary>
    /// <param name="title">The string to generate a title for.</param>
    /// <returns>The generated title.</returns>
    protected string GenerateTitleString() => SymbolDisplay.FormatLiteral(Title, true);

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
    
    protected string GetStaticOrInstancePrepend(bool isStatic)
    {
        return "destination".GetStaticOrInstance(Context.TargetType.FullName(), isStatic);
    }
    
}