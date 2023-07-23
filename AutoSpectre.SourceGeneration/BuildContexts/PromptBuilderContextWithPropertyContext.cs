using System;

namespace AutoSpectre.SourceGeneration.BuildContexts;

internal abstract class PromptBuilderContextWithPropertyContext : PromptBuildContext
{
    public SinglePropertyEvaluationContext Context { get; }

    public PromptBuilderContextWithPropertyContext(SinglePropertyEvaluationContext context)
    {
        Context = context;
    }

    protected virtual string GenerateConverter()
    {
        if (Context.ConfirmedConverter is { } converter)
        {
            return $"{Environment.NewLine}.UseConverter(destination.{converter.Converter})";
        }

        return string.Empty;
    }

}