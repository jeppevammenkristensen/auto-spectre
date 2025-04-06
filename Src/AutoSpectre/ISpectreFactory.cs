using System;
using System.Threading.Tasks;

namespace AutoSpectre;

/// <summary>
/// A Common interface for presenting a way to prompt for data 
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ISpectreFactory<T> where T : notnull
{ 
    /// <summary>
    /// Prompt and enrich a given item of type <see cref="T"/>
    /// </summary>
    /// <param name="form">The form to enrich</param>
    /// <returns></returns>
    T Prompt(T form);
}

public interface IAsyncSpectreFactory<T> : ISpectreFactory<T> where T : notnull
{
    Task<T> PromptAsync(T form);
}

public abstract class AsyncSpectreFactoryBase<T> : IAsyncSpectreFactory<T> where T : notnull
{
    public T Prompt(T form)
    {
        return PromptAsync(form).GetAwaiter().GetResult();
    }

    public abstract Task<T> PromptAsync(T? form);

}