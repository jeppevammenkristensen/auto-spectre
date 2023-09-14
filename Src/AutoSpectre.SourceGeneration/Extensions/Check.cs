using AutoSpectre.SourceGeneration.Extensions.Specification;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration.Extensions;

public static class Check
{
    public static MethodSpecification<T> Method<T>() where T : ISymbol
    {
        return new MethodSpecification<T>();
    }

    public static PropertySpecification<T> Property<T>() where T : ISymbol
    {
        return new PropertySpecification<T>();
    }
}