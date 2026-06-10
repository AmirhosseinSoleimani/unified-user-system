namespace Academy.src.Shared.Academy.SharedKernel.Domain.Specifications;

public abstract class Specification<T> : ISpecification<T>
{
    public abstract bool IsSatisfiedBy(T entity);

    public ISpecification<T> And(ISpecification<T> other)
    {
        return new AndSpecification<T>(this, other);
    }

    public ISpecification<T> Or(ISpecification<T> other)
    {
        return new OrSpecification<T>(this, other);
    }

    public ISpecification<T> Not()
    {
        return new NotSpecification<T>(this);
    }
}

internal sealed class AndSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _left, _right;
    public AndSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left; _right = right;
    }
    public override bool IsSatisfiedBy(T entity) => _left.IsSatisfiedBy(entity) && _right.IsSatisfiedBy(entity);
}

internal sealed class OrSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _left, _right;
    public OrSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left; _right = right;
    }
    public override bool IsSatisfiedBy(T entity) => _left.IsSatisfiedBy(entity) || _right.IsSatisfiedBy(entity);
}

internal sealed class NotSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _spec;
    public NotSpecification(ISpecification<T> spec) => _spec = spec;
    public override bool IsSatisfiedBy(T entity) => !_spec.IsSatisfiedBy(entity);
}