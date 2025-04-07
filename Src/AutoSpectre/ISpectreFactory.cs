using System;
using System.Collections;

namespace AutoSpectre;

/// <summary>
/// A Common interface for presenting a way to prompt for data 
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ISpectreFactory<T> : ISpectreFactory where T : notnull
{ 
    /// <summary>
    /// Prompt and enrich a given item of type <see cref="T"/>
    /// </summary>
    /// <param name="form">The form to enrich</param>
    /// <returns></returns>
    T Prompt(T form);
}

/// <summary>
/// Marker interface for SpectreFactory. An implementation can be sync or async
/// </summary>
public interface ISpectreFactory
{

}