using System.Threading.Tasks;

namespace AutoSpectre;

/// <summary>
/// Interface for async spectre factories that prompt for data and enrich a form of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The form type to prompt for.</typeparam>
public interface IAsyncSpectreFactory<T> : ISpectreFactory where T : notnull
{
    /// <summary>
    /// Asynchronously prompts and enriches the given form.
    /// </summary>
    /// <param name="form">The form to enrich.</param>
    /// <returns>The enriched form.</returns>
    Task<T> PromptAsync(T form);
}