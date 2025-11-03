using System;
using System.Linq.Expressions;
using Minimarket.Domain.Entities;

namespace Minimarket.Domain.Specifications;

public class ProductHasSufficientStockSpecification : ISpecification<Product>
{
    private readonly int _requiredQuantity;

    public ProductHasSufficientStockSpecification(int requiredQuantity)
    {
        if (requiredQuantity <= 0)
            throw new ArgumentException("La cantidad requerida debe ser mayor a 0", nameof(requiredQuantity));
        
        _requiredQuantity = requiredQuantity;
    }

    public bool IsSatisfiedBy(Product product)
    {
        if (product == null)
            return false;

        return product.Stock >= _requiredQuantity && product.IsActive;
    }

    public Expression<Func<Product, bool>> ToExpression()
    {
        return p => p.Stock >= _requiredQuantity && p.IsActive;
    }
}

