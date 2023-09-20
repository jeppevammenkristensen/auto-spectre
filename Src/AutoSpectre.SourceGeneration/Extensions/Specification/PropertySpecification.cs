using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AutoSpectre.SourceGeneration.Extensions.Specification;

public class EnumerableSpecification<T> : Specification<T> where T : ITypeSymbol
{
    public ITypeSymbol? Type { get; set; }
    
    public override bool IsSatisfiedBy(T obj)
    {
        var (isEnumerable, underlyingType) = obj.IsEnumerableOfTypeButNotString();
        if (!isEnumerable) return false;

        if (Type is { })
        {
            if (!underlyingType.Equals(Type, SymbolEqualityComparer.Default))
            {
                return false;
            }
        }

        return true;
    }
    
    public EnumerableSpecification<T> WithUnderlyingType(ITypeSymbol typeSymbol)
    {
        Type = typeSymbol;
        return this;
    }
}

public class FieldSpecification<T> : Specification<T> where T : ISymbol
{
    private ITypeSymbol? _typeSymbol;

    public override bool IsSatisfiedBy(T obj)
    {
        if (obj.Kind != SymbolKind.Field) return false;

        if (obj is IFieldSymbol fieldSymbol)
        {
            if (_typeSymbol is {} && !fieldSymbol.Type.Equals(_typeSymbol, SymbolEqualityComparer.Default))
                return false;
        }
        else
        {
            return false;
        }
        
        return true;
    }

    public FieldSpecification<T> WithType(ITypeSymbol typeSymbol)
    {
        _typeSymbol = typeSymbol;
        return this;
    }
}

public class PropertySpecification<T> : Specification<T> where T : ISymbol
{
    private ITypeSymbol? _propertyType;
    private Specification<ITypeSymbol>? _typeSpec;

    public override bool IsSatisfiedBy(T obj)
    {
        if (obj.Kind != SymbolKind.Property) return false;
        if (obj is not IPropertySymbol property) return false;

        if (_propertyType is {} && !property.Type.Equals(_propertyType, SymbolEqualityComparer.Default))
        {
            return false;
        }

        if (_typeSpec is { } && _typeSpec != property.Type)
            return false;

        return true;
    }

    public PropertySpecification<T> WithExpectedType(ITypeSymbol type)
    {
        _propertyType = type ?? throw new ArgumentNullException(nameof(type));
        return this;
    }

    public PropertySpecification<T> WithTypeSpec(Specification<ITypeSymbol> typeSpec)
    {
        _typeSpec = typeSpec;
        return this;
    }
}