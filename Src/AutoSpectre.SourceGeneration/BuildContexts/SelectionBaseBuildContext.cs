using System;

namespace AutoSpectre.SourceGeneration.BuildContexts;

/// <summary>
/// Since a SelectPrompt and MultiSelectPrompt are so similar, this class contains some common code
/// </summary>
internal abstract class SelectionBaseBuildContext : PromptBuilderContextWithPropertyContext
{
    public string Title { get; }


    protected SelectionBaseBuildContext(string title,SinglePropertyEvaluationContext context) : base(context)
    {
        Title = title;
    }

    protected string GenerateMoreChoicesText()
    {
        if (Context.MoreChoicesText is { })
        {
            return $""".MoreChoicesText("{Context.MoreChoicesText}")""";
        }

        return string.Empty;
    }

    protected string GenerateWrapAround()
    {
        if (Context.WrapAround is { })
        {
            return $"""".WrapAround({(Context.WrapAround.Value ? "true" : "false")})"""";
        }

        return string.Empty;
    }

    protected string GeneratePageSize()
    {
        return $""".PageSize({(Context.PageSize == null ? "10" : Context.PageSize.ToString())})""";
    }

    protected string GenerateHighlightStyle()
    {
        if (Context.HighlightStyle is { })
        {
            return $""".HighlightStyle("{Context.HighlightStyle}")""";
        }

        return string.Empty;
    }

    protected string GetChoicePrepend()
    {
        return Context.ConfirmedSelectionSource.IsStatic ? Context.TargetType.Name : "destination";
    }

    protected string GetSelector() => Context.ConfirmedSelectionSource!.Source switch
    {
        SelectionPromptSelectionType.Method => $"{Context.ConfirmedSelectionSource.Name}()",
        SelectionPromptSelectionType.Property => Context.ConfirmedSelectionSource.Name,
        _ => throw new InvalidOperationException("Unsupported SelectionType")
    };
}