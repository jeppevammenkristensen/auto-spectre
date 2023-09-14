namespace AutoSpectre.SourceGeneration.Extensions.Specification;

public interface ISpecification
{
    bool IsSatisfiedBy(object? obj);
}