using System;
using System.Globalization;
using Spectre.Console;

namespace AutoSpectre.Extensions;

public static class Extensions
{
    public static T WithCulture<T>(this T prompt, CultureInfo culture) where T : IHasCulture
    {
        if (prompt == null) throw new ArgumentNullException(nameof(prompt));
        prompt.Culture = culture;
        return prompt;
    }
}