using System.Text;
using AutoSpectre.SourceGeneration.BuildContexts;
using AutoSpectre.SourceGeneration.Evaluation;

namespace AutoSpectre.SourceGeneration.Extensions;

    
public static class ConfirmedExtensions
{
    /// <summary>
    /// For all <see cref="conditions"/> that implemets ISummaryCondition, calls WriteToSummary
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="conditions"></param>
    public static void ApplySummaries(this StringBuilder builder, params object?[] conditions)
    {
        ApplySummaries(new SummaryLineWriter(builder), conditions);
    }

    
    public static void ApplySummaries(this SummaryLineWriter builder, params object?[] conditions)
    {
        foreach (var condition in conditions)
        {
            if (condition is ISummaryCondition summaryCondition)
            {
                summaryCondition.WriteToSummary(builder);        
            }
        }
    }
    
    
    /// <summary>
    /// Adds EnableSearch and SearchPlaceholderText if SearchEnabled is true
    /// or the values are not null 
    /// </summary>
    /// <param name="search"></param>
    /// <returns></returns>
    public static string GetSearchString(this ConfirmedSearchEnabled? search)
    {
        if (search is not {SearchEnabled: true})
            return string.Empty;

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(".EnableSearch()");
        if (search.SearchPlaceholderText is { })
        {
            stringBuilder.AppendLine($".SearchPlaceholderText({search.SearchPlaceholderText.GetSafeTextWithQuotes()})");
        }

        return stringBuilder.ToString();
    }
}