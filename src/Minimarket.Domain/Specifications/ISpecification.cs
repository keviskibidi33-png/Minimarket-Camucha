using System.Linq.Expressions;

namespace Minimarket.Domain.Specifications;

public interface ISpecification<T>
{
    bool IsSatisfiedBy(T entity);
    Expression<Func<T, bool>> ToExpression();
}

