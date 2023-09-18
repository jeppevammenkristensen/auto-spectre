using System;
using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration.Extensions.Specification;

/// <summary>
/// A specification can evaluate a given value of T and return
/// true or false if the given condition is satisfied. It's it implicitly converted to a Func&lt;T&gt; so
/// you can inject it in a Where clause or FirstOrDefault directly. 
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Specification<T> : ISpecification
{
    public abstract bool IsSatisfiedBy(T obj);
    
    bool ISpecification.IsSatisfiedBy(object? obj)
    {
        if (obj is T result)
            return IsSatisfiedBy(result);

        return false;
    }

    public static implicit operator Func<T, bool>(Specification<T> specification)
    {
        return specification.IsSatisfiedBy;
    }
    
    public Specification<T> And(Specification<T> specification) => new AndSpecification<T>(this, specification);
    public Specification<T> Or(Specification<T> specification) => new OrSpecification<T>(this, specification);

    public static Specification<T> operator &(Specification<T> left, Specification<T> right)
    {
        return left.And(right);
    }

    public static Specification<T> operator |(Specification<T> left, Specification<T> right)
    {
        return left.Or(right);
    }

    public static bool operator ==(Specification<T> left, T? right)
    {
        if (right == null)
            return false;
        
        return left.IsSatisfiedBy(right);
    }

    public static bool operator !=(Specification<T> left, T? right)
    {
        return !(left == right);
    }

    public Specification<T> Not()
    {
        return new NotSpecification<T>(this);
    }
}

public class NotSpecification<T> : Specification<T>
{
    private readonly Specification<T> _specification;

    public NotSpecification(Specification<T> specification)
    {
        _specification = specification;
    }

    public override bool IsSatisfiedBy(T obj)
    {
        return !_specification.IsSatisfiedBy(obj);
    }
}