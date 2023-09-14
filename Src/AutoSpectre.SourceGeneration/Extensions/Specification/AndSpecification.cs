using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration.Extensions.Specification;

public class AndSpecification<T> : Specification<T>
{
    public AndSpecification(Specification<T> left, Specification<T> right)
    {
        Left = left;
        Right = right;
    }

    private Specification<T> Left { get; }
    private Specification<T> Right { get; }

    

    public override bool IsSatisfiedBy(T obj)
    {
        return Left.IsSatisfiedBy(obj) && Right.IsSatisfiedBy(obj);
    }
}

public static class SpecificationRecipes
{
    /// <summary>
    /// Returns a specification that check if a given type is any kind of known IEnumerable or List or other
    /// of a given type. Only exception is that is will not treat string as a collection of chars.
    /// </summary>
    /// <param name="typeSymbol"></param>
    /// <returns></returns>
    public static EnumerableSpecification<ITypeSymbol> EnumerableOfTypeSpec(ITypeSymbol typeSymbol) =>
        new EnumerableSpecification<ITypeSymbol>()
            .WithUnderlyingType(typeSymbol);
    
    /// <summary>
    /// Returns a <see cref="Specification{T}"/> that evaluates if a given symbol is Public and an Instance
    /// </summary>
    public static Specification<ISymbol> IsPublicAndInstanceSpec => new IsPublicSpecification<ISymbol>() & new IsInstanceSpecification<ISymbol>();
    
    public static MethodSpecification<ISymbol> MethodWithNoParametersSpec =>
        new MethodSpecification<ISymbol>().WithParameters(0);

    public static MethodSpecification<ISymbol> MethodWithTypeSpec(Specification<ITypeSymbol> typeSpec) =>
        new MethodSpecification<ISymbol>().WithTypeSpec(typeSpec);

    public static Specification<ISymbol> PropertyOfTypeSpec(ITypeSymbol typeSymbol) =>
        new PropertySpecification<ISymbol>().WithExpectedType(typeSymbol);

    public static Specification<ISymbol> PropertyOfTypeSpec(Specification<ITypeSymbol> typeSymbol) =>
        new PropertySpecification<ISymbol>().WithTypeSpec(typeSymbol);



}