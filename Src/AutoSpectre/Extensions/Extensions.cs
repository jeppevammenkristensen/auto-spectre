using System;
using System.Globalization;
using System.Threading.Tasks;
using Spectre.Console;

namespace AutoSpectre.Extensions;

/// <summary>
/// Extension methods for AutoSpectre factories and prompts.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Sets the culture on a prompt that supports culture configuration.
    /// </summary>
    /// <typeparam name="T">The prompt type.</typeparam>
    /// <param name="prompt">The prompt to configure.</param>
    /// <param name="culture">The culture to use.</param>
    /// <returns>The same instance so that multiple calls can be chained.</returns>
    public static T WithCulture<T>(this T prompt, CultureInfo culture) where T : IHasCulture
    {
        if (prompt == null) throw new ArgumentNullException(nameof(prompt));
        prompt.Culture = culture;
        return prompt;
    }

    /// <summary>
    /// Creates a new instance of <typeparamref name="T"/> and prompts the user to fill it in.
    /// </summary>
    /// <typeparam name="T">The form type.</typeparam>
    /// <param name="factory">The factory to use for prompting.</param>
    /// <returns>The enriched form.</returns>
    public static T Prompt<T>(this ISpectreFactory<T> factory) where T : notnull, new()
    {
        var res = new T();
        return factory.Prompt(res);
    }


    /// <summary>
    /// Creates a new instance of <typeparamref name="T"/> and asynchronously prompts the user to fill it in.
    /// </summary>
    /// <typeparam name="T">The form type.</typeparam>
    /// <param name="factory">The factory to use for prompting.</param>
    /// <returns>The enriched form.</returns>
    public static async Task<T> PromptAsync<T>(this IAsyncSpectreFactory<T> factory) where T : notnull,new()
    {
        var result = new T();
        return await factory.PromptAsync(result);
    }
}