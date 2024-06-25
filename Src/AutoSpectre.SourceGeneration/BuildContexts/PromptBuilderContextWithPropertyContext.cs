using System;
using Microsoft.CodeAnalysis.CSharp;

namespace AutoSpectre.SourceGeneration.BuildContexts;

internal abstract class PromptBuilderContextWithPropertyContext : PromptBuildContext
{
    protected PromptBuilderContextWithPropertyContext(SinglePropertyEvaluationContext context, string title) : base(context, title)
    {
    }

    protected virtual string GenerateConverter()
    {
        if (Context.ConfirmedConverter is { } converter)
        {
            return $".UseConverter({GetStaticOrInstancePrepend(converter.IsStatic)}.{converter.Converter})";
        }

        return string.Empty;
    }


    



}