using System.Threading.Tasks;

namespace AutoSpectre;

/// <summary>
/// Base class for async spectre factories that prompt for data and enrich a form of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The form type to prompt for.</typeparam>
public abstract class AsyncSpectreFactoryBase<T> : IAsyncSpectreFactory<T> where T : notnull
{
    /// <summary>
    /// Synchronously prompts and enriches the given form by blocking on <see cref="PromptAsync"/>.
    /// </summary>
    /// <param name="form">The form to enrich.</param>
    /// <returns>The enriched form.</returns>
    public T Prompt(T form)
    {
        return PromptAsync(form).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Asynchronously prompts and enriches the given form.
    /// </summary>
    /// <param name="form">The form to enrich.</param>
    /// <returns>The enriched form.</returns>
    public abstract Task<T> PromptAsync(T? form);

}