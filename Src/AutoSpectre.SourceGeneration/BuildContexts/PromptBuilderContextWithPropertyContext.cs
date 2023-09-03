using System;

namespace AutoSpectre.SourceGeneration.BuildContexts;

internal abstract class PromptBuilderContextWithPropertyContext : PromptBuildContext
{
    protected PromptBuilderContextWithPropertyContext(SinglePropertyEvaluationContext context) : base(context)
    {
    }

    protected virtual string GenerateConverter()
    {
        if (Context.ConfirmedConverter is { } converter)
        {
            return $".UseConverter(destination.{converter.Converter})";
        }

        return string.Empty;
    }

}