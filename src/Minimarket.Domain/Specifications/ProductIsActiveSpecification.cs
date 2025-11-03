using System.Linq.Expressions;
using Minimarket.Domain.Entities;

namespace Minimarket.Domain.Specifications;

public class ProductIsActiveSpecification : ISpecification<Product>
{
    public bool IsSatisfiedBy(Product product)
    {
        if (product == null)
            return false;

        return product.IsActive;
    }

    public Expression<Func<Product, bool>> ToExpression()
    {
        return p => p.IsActive;
    }
}

