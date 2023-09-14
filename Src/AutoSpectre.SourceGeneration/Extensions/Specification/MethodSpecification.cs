using Microsoft.CodeAnalysis;

namespace AutoSpectre.SourceGeneration.Extensions.Specification;

public class MethodSpecification<T> : Specification<T> where T : ISymbol
{
    private uint? _parameters;
    private ITypeSymbol? _returnType;
    private Specification<ITypeSymbol>? _typeSpec;

    public override bool IsSatisfiedBy(T obj)
    {
        if (obj.Kind != SymbolKind.Method)
            return false;
        if (obj is IMethodSymbol methodSymbol)
        {
            if (_parameters.HasValue && methodSymbol.Parameters.Length != _parameters)
            {
                return false;
            }

            if (_returnType is {} && !_returnType.Equals(methodSymbol.ReturnType, SymbolEqualityComparer.Default))
            {
                return false;
            }

            if (_typeSpec is { } && _typeSpec != methodSymbol.ReturnType)
                return false;

            return true;
        }

        return false;
    }

    public MethodSpecification<T> WithParameters(uint parameters)
    {
        _parameters = parameters;
        return this;
    }

    public MethodSpecification<T> WithReturnType(ITypeSymbol type)
    {
        _returnType = type;
        return this;
    }

    public MethodSpecification<T> WithTypeSpec(Specification<ITypeSymbol> typeSpec) 
    {
        _typeSpec = typeSpec;
        return this;
    }
    
    
}