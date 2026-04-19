using System;
using System.Text;
using AutoSpectre.SourceGeneration.Evaluation;
using AutoSpectre.SourceGeneration.Extensions;

namespace AutoSpectre.SourceGeneration.BuildContexts;

/// <summary>
/// Since a SelectPrompt and MultiSelectPrompt are so similar, this class contains some common code
/// </summary>
internal abstract class SelectionBaseBuildContext : PromptBuilderContextWithPropertyContext
{
    
    protected SelectionBaseBuildContext(string title,SinglePropertyEvaluationContext context) : base(context, title)
    {
        
    }

    /// <summary>
    /// Emits <c>.MoreChoicesText("...")</c> when set on the attribute; otherwise empty.
    /// </summary>
    protected string GenerateMoreChoicesText()
    {
        if (Context.MoreChoicesText is { })
        {
            return $""".MoreChoicesText({Context.MoreChoicesText.GetSafeTextWithQuotes()})""";
        }

        return string.Empty;
    }

    /// <summary>
    /// Emits <c>.WrapAround(true|false)</c> when set on the attribute; otherwise empty.
    /// </summary>
    protected string GenerateWrapAround()
    {
        if (Context.WrapAround is { })
        {
            return $"""".WrapAround({(Context.WrapAround.Value ? "true" : "false")})"""";
        }

        return string.Empty;
    }

    /// <summary>
    /// Emits <c>.PageSize(n)</c>, defaulting to 10 when the attribute does not specify a value.
    /// </summary>
    protected string GeneratePageSize()
    {
        return $""".PageSize({(Context.PageSize == null ? "10" : Context.PageSize.ToString())})""";
    }

    /// <summary>
    /// Emits <c>.HighlightStyle("...")</c> when set on the attribute; otherwise empty.
    /// </summary>
    protected string GenerateHighlightStyle()
    {
        if (Context.HighlightStyle is { })
        {
            return $""".HighlightStyle({Context.HighlightStyle.GetSafeTextWithQuotes()})""";
        }

        return string.Empty;
    }

    /// <summary>
    /// Returns the static or instance prefix (e.g. <c>form</c> or the fully-qualified type name)
    /// used in front of the selection source member.
    /// </summary>
    protected string GetChoicePrepend()
    {
        return GetStaticOrInstancePrepend(Context.ConfirmedSelectionSource!.IsStatic);
    }

    /// <summary>
    /// Emits <c>.AddCancelResult(...)</c> pointing at the confirmed cancel-result member; otherwise empty.
    /// </summary>
    protected string GenerateCancel()
    {
        StringBuilder builder = new();
        
        if (Context.ConfirmedCancelResult is { } confirmed)
        {
            confirmed.GeneratePrompt(Context,builder);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Emits <c>.DefaultValue(...)</c> pointing at the confirmed default-value member; otherwise empty.
    /// Note the polarity flip: <see cref="ConfirmedDefaultValue.Instance"/> is the inverse of the
    /// <c>isStatic</c> flag expected by <c>GetStaticOrInstancePrepend</c>.
    /// </summary>
    protected string GenerateDefaultValue()
    {
        var stringBuilder = new StringBuilder();
        
        Context.ConfirmedDefaultValue?.GeneratePrompt(Context,stringBuilder);
        return stringBuilder.ToString();
    }

    /// <summary>
    /// Returns the selection-source accessor — <c>Name()</c> for methods, <c>Name</c> for properties —
    /// to be appended after <see cref="GetChoicePrepend"/> when building the <c>.AddChoices(...)</c> call.
    /// </summary>
    protected string GetSelector() => Context.ConfirmedSelectionSource!.Source switch
    {
        SelectionPromptSelectionType.Method => $"{Context.ConfirmedSelectionSource.Name}()",
        SelectionPromptSelectionType.Property => Context.ConfirmedSelectionSource.Name,
        _ => throw new InvalidOperationException("Unsupported SelectionType")
    };
}