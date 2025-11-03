namespace Minimarket.Application.Common.Exceptions;

public class InsufficientStockException : BusinessRuleViolationException
{
    public InsufficientStockException(string productName, int available, int requested)
        : base($"Stock insuficiente para '{productName}'. Disponible: {available}, Solicitado: {requested}")
    {
    }
}

