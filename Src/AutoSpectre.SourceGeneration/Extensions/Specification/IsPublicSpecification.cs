using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration.Extensions.Specification;

public class IsPublicSpecification<T> : Specification<T> where T : ISymbol
{
    public override bool IsSatisfiedBy(T obj)
    {
        return obj.IsPublic();
    }
}

public class IsPartialSpecficiation<T> : Specification<T> where T : ISymbol
{
    public override bool IsSatisfiedBy(T obj)
    {
        return obj.IsPartial();
    }   
}

public class IsInternalSpecification<T> : Specification<T> where T : ISymbol
{
    public override bool IsSatisfiedBy(T obj)
    {
        return obj.IsInternal();
    }
}