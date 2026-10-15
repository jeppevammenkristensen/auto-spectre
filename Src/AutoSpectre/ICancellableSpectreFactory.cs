using System.Threading;
using System.Threading.Tasks;

namespace AutoSpectre;

/// <summary>
/// Optional capability for factories that accept cancellation when asynchronously enriching a form.
/// </summary>
/// <typeparam name="T">The form type to prompt for.</typeparam>
/// <remarks>
/// Cancellation is cooperative. Implementations determine which prompt operations can be interrupted.
/// </remarks>
public interface ICancellableSpectreFactory<T> : ISpectreFactory where T : notnull
{
    /// <summary>
    /// Asynchronously prompts and enriches the given form with cancellation support.
    /// </summary>
    /// <param name="form">The form to enrich.</param>
    /// <param name="cancellationToken">The token used to request cancellation.</param>
    /// <returns>The enriched form.</returns>
    /// <exception cref="System.OperationCanceledException">Cancellation is requested and observed.</exception>
    Task<T> PromptAsync(T form, CancellationToken cancellationToken);
}
