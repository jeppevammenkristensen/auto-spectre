using System.Text;
using AutoSpectre.SourceGeneration.Evaluation;

namespace AutoSpectre.SourceGeneration.Extensions;

public static class ConfirmedExtensions
{
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