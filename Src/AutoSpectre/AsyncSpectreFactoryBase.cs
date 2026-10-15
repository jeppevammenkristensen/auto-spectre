using System.Threading;
using System.Threading.Tasks;

namespace AutoSpectre;

/// <summary>
/// Base class for async spectre factories that prompt for data and enrich a form of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The form type to prompt for.</typeparam>
public abstract class AsyncSpectreFactoryBase<T> : IAsyncSpectreFactory<T>, ICancellableSpectreFactory<T> where T : notnull
{
    /// <summary>
    /// Synchronously prompts and enriches the given form by blocking on <see cref="PromptAsync(T)"/>.
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

    /// <summary>
    /// Asynchronously prompts and enriches the given form after checking for cancellation.
    /// </summary>
    /// <param name="form">The form to enrich.</param>
    /// <param name="cancellationToken">The token used to request cancellation.</param>
    /// <returns>The enriched form.</returns>
    /// <remarks>
    /// The default implementation checks cancellation before delegating to <see cref="PromptAsync(T)"/>.
    /// It cannot interrupt legacy implementations once prompting has begun. Override this overload
    /// to observe cancellation during prompting.
    /// </remarks>
    /// <exception cref="System.OperationCanceledException">The token is already canceled.</exception>
    public virtual Task<T> PromptAsync(T? form, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return PromptAsync(form);
    }

}
