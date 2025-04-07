using System.Threading.Tasks;

namespace AutoSpectre;

public abstract class AsyncSpectreFactoryBase<T> : IAsyncSpectreFactory<T> where T : notnull
{
    public T Prompt(T form)
    {
        return PromptAsync(form).GetAwaiter().GetResult();
    }

    public abstract Task<T> PromptAsync(T? form);

}