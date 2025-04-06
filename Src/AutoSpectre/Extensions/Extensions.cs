using System;
using System.Globalization;
using System.Threading.Tasks;
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
    
    public static T Prompt<T>(this T source, ISpectreFactory<T> factory)
    {
        return factory.Prompt(source);
    }

    public static T Prompt<T>(this ISpectreFactory<T> factory) where T : notnull, new()
    {
        var res = new T();
        return factory.Prompt(res);
    }

    public static async Task<T> PromptAsync<T>(this T source, IAsyncSpectreFactory<T> factory) where T : notnull
    {
        return await factory.PromptAsync(source);
    }

    public static async Task<T> PromptAsync<T>(this IAsyncSpectreFactory<T> factory) where T : notnull,new()
    {
        var result = new T();
        return await factory.PromptAsync(result);
    }
}