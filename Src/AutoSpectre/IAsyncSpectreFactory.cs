using System.Threading.Tasks;

namespace AutoSpectre;

public interface IAsyncSpectreFactory<T> : ISpectreFactory where T : notnull
{
    Task<T> PromptAsync(T form);
}