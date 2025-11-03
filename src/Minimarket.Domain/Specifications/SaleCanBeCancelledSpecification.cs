using System.Linq.Expressions;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Enums;

namespace Minimarket.Domain.Specifications;

public class SaleCanBeCancelledSpecification : ISpecification<Sale>
{
    public bool IsSatisfiedBy(Sale sale)
    {
        if (sale == null)
            return false;

        // Solo se puede anular si no está ya anulado
        // Transiciones válidas: Pendiente → Anulado, Pagado → Anulado
        return sale.Status != SaleStatus.Anulado;
    }

    public Expression<Func<Sale, bool>> ToExpression()
    {
        return s => s.Status != SaleStatus.Anulado;
    }
}

