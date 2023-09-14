namespace AutoSpectre.SourceGeneration.Extensions.Specification;

public class OrSpecification<T> : Specification<T>
{
    public OrSpecification(Specification<T> left, Specification<T> right)
    {
        Left = left;
        Right = right;
    }

    private Specification<T> Left { get; }
    private Specification<T> Right { get; }


    public override bool IsSatisfiedBy(T obj)
    {
        return Left.IsSatisfiedBy(obj) || Right.IsSatisfiedBy(obj);
    }
}