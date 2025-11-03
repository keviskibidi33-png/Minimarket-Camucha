# TASK ASSIGNMENT - Business Logic Validator - Unit Tests for Specifications

**Fecha**: [Fecha Actual]  
**Agente**: @Business-Logic-Validator  
**Prioridad**: 🔴 CRÍTICA  
**Deadline**: Esta semana (3 días hábiles)

---

## CONTEXTO Y OBJETIVO

Como Business Logic Validator, eres responsable de validar que todas las reglas de negocio estén correctamente implementadas. Ya completaste la implementación de validaciones, pero ahora necesitas **demostrar que funcionan correctamente** mediante tests unitarios.

**Objetivo**: Crear suite completa de tests unitarios para las especificaciones de dominio y validadores que implementaste, alcanzando **>90% coverage** en estos componentes críticos.

---

## RESPONSABILIDADES ESPECÍFICAS

### 1. Testing de Especificaciones
- Validar que todas las especificaciones funcionan correctamente
- Cubrir casos edge (null, valores límite, estados inválidos)
- Verificar que ToExpression() retorna expresiones correctas

### 2. Testing de Validadores
- Validar que FluentValidation detecta todos los casos de error
- Verificar mensajes de error son claros y específicos
- Testear validaciones asíncronas (foreign keys, unicidad)

### 3. Testing de Cálculos Monetarios
- Verificar redondeo comercial correcto
- Validar cálculos de IGV, totales, vuelto
- Testear casos edge (descuentos, montos grandes/pequeños)

---

## TAREAS ASIGNADAS

### TAREA 1: Unit Tests - Specifications (Día 1 - 4 horas)

**PRIORITY**: 🔴 CRÍTICA  
**DELIVERABLE**: Tests unitarios completos para las 3 especificaciones

#### Acceptance Criteria:
- [ ] Tests para `ProductHasSufficientStockSpecification`:
  - [ ] Test: `IsSatisfiedBy_WhenStockIsSufficient_ReturnsTrue`
  - [ ] Test: `IsSatisfiedBy_WhenStockIsInsufficient_ReturnsFalse`
  - [ ] Test: `IsSatisfiedBy_WhenProductIsInactive_ReturnsFalse`
  - [ ] Test: `IsSatisfiedBy_WhenProductIsNull_ReturnsFalse`
  - [ ] Test: `Constructor_WhenRequiredQuantityIsZero_ThrowsArgumentException`
  - [ ] Test: `Constructor_WhenRequiredQuantityIsNegative_ThrowsArgumentException`
  - [ ] Test: `ToExpression_ReturnsCorrectExpression`
  - [ ] Test: `IsSatisfiedBy_WhenStockEqualsRequiredQuantity_ReturnsTrue` (boundary)

- [ ] Tests para `ProductIsActiveSpecification`:
  - [ ] Test: `IsSatisfiedBy_WhenProductIsActive_ReturnsTrue`
  - [ ] Test: `IsSatisfiedBy_WhenProductIsInactive_ReturnsFalse`
  - [ ] Test: `IsSatisfiedBy_WhenProductIsNull_ReturnsFalse`
  - [ ] Test: `ToExpression_ReturnsCorrectExpression`

- [ ] Tests para `SaleCanBeCancelledSpecification`:
  - [ ] Test: `IsSatisfiedBy_WhenSaleIsPending_ReturnsTrue`
  - [ ] Test: `IsSatisfiedBy_WhenSaleIsPaid_ReturnsTrue`
  - [ ] Test: `IsSatisfiedBy_WhenSaleIsCancelled_ReturnsFalse`
  - [ ] Test: `IsSatisfiedBy_WhenSaleIsNull_ReturnsFalse`
  - [ ] Test: `ToExpression_ReturnsCorrectExpression`

#### Coverage Target: 100% de especificaciones

#### Reference Files:
- `src/Minimarket.Domain/Specifications/ProductHasSufficientStockSpecification.cs`
- `src/Minimarket.Domain/Specifications/ProductIsActiveSpecification.cs`
- `src/Minimarket.Domain/Specifications/SaleCanBeCancelledSpecification.cs`

#### Test Structure:
```csharp
// tests/Minimarket.UnitTests/Domain/Specifications/ProductHasSufficientStockSpecificationTests.cs
using FluentAssertions;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Specifications;
using Xunit;

namespace Minimarket.UnitTests.Domain.Specifications;

public class ProductHasSufficientStockSpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_WhenStockIsSufficient_ReturnsTrue()
    {
        // Arrange
        var product = new Product 
        { 
            Id = Guid.NewGuid(),
            Stock = 10, 
            IsActive = true 
        };
        var spec = new ProductHasSufficientStockSpecification(5);
        
        // Act
        var result = spec.IsSatisfiedBy(product);
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Theory]
    [InlineData(10, 5, true)]   // Stock suficiente
    [InlineData(5, 5, true)]    // Stock exacto (boundary)
    [InlineData(4, 5, false)]   // Stock insuficiente
    [InlineData(0, 5, false)]   // Stock cero
    public void IsSatisfiedBy_WithVariousStockLevels_ReturnsExpectedResult(
        int stock, int required, bool expected)
    {
        // Arrange
        var product = new Product 
        { 
            Stock = stock, 
            IsActive = true 
        };
        var spec = new ProductHasSufficientStockSpecification(required);
        
        // Act
        var result = spec.IsSatisfiedBy(product);
        
        // Assert
        result.Should().Be(expected);
    }
    
    // Más tests...
}
```

---

### TAREA 2: Unit Tests - CreateSaleCommandValidator (Día 1-2 - 5 horas)

**PRIORITY**: 🔴 CRÍTICA  
**DELIVERABLE**: Tests completos para validaciones de ventas

#### Acceptance Criteria:
- [ ] Tests de validación básica:
  - [ ] Test: `Validate_WhenSaleDetailsIsEmpty_ReturnsValidationError`
  - [ ] Test: `Validate_WhenQuantityIsZero_ReturnsValidationError`
  - [ ] Test: `Validate_WhenQuantityIsNegative_ReturnsValidationError`
  - [ ] Test: `Validate_WhenUnitPriceIsZero_ReturnsValidationError`
  - [ ] Test: `Validate_WhenUnitPriceIsNegative_ReturnsValidationError`
  - [ ] Test: `Validate_WhenDiscountIsNegative_ReturnsValidationError`

- [ ] Tests de reglas de negocio:
  - [ ] Test: `Validate_WhenDiscountExceedsSubtotal_ReturnsValidationError`
  - [ ] Test: `Validate_WhenAmountPaidLessThanTotal_ReturnsValidationError`
  - [ ] Test: `Validate_WhenInvoiceWithoutCustomer_ReturnsValidationError`
  - [ ] Test: `Validate_WhenInvoiceWithCustomer_PassesValidation`

- [ ] Tests de validaciones asíncronas (usar Moq):
  - [ ] Test: `Validate_WhenProductsDoNotExist_ReturnsValidationError`
  - [ ] Test: `Validate_WhenStockInsufficient_ReturnsValidationError`
  - [ ] Test: `Validate_WhenProductsInactive_ReturnsValidationError`
  - [ ] Test: `Validate_WhenCustomerDoesNotExist_ReturnsValidationError`
  - [ ] Test: `Validate_WhenAllValidationsPass_ReturnsSuccess`

#### Coverage Target: >90% del validador

#### Reference Files:
- `src/Minimarket.Application/Features/Sales/Commands/CreateSaleCommandValidator.cs`
- `CODE_REVIEW_BusinessLogicValidator.md` (para contexto)

#### Test Structure:
```csharp
// tests/Minimarket.UnitTests/Application/Features/Sales/Commands/CreateSaleCommandValidatorTests.cs
using FluentValidation.TestHelper;
using Minimarket.Application.Features.Sales.Commands;
using Minimarket.Application.Features.Sales.DTOs;
using Minimarket.Domain.Entities;
using Minimarket.Domain.Enums;
using Minimarket.Domain.Interfaces;
using Moq;
using Xunit;

namespace Minimarket.UnitTests.Application.Features.Sales.Commands;

public class CreateSaleCommandValidatorTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateSaleCommandValidator _validator;

    public CreateSaleCommandValidatorTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validator = new CreateSaleCommandValidator(_unitOfWorkMock.Object);
    }

    [Fact]
    public void Validate_WhenSaleDetailsIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateSaleCommand
        {
            Sale = new CreateSaleDto
            {
                SaleDetails = new List<CreateSaleDetailDto>(),
                DocumentType = DocumentType.Boleta,
                PaymentMethod = PaymentMethod.Efectivo,
                AmountPaid = 100m
            },
            UserId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Sale.SaleDetails)
            .WithErrorMessage("La venta debe tener al menos un producto");
    }

    [Fact]
    public void Validate_WhenDiscountExceedsSubtotal_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateSaleCommand
        {
            Sale = new CreateSaleDto
            {
                SaleDetails = new List<CreateSaleDetailDto>
                {
                    new() { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 100m }
                },
                Discount = 150m, // Mayor que subtotal (100)
                DocumentType = DocumentType.Boleta,
                PaymentMethod = PaymentMethod.Efectivo,
                AmountPaid = 100m
            },
            UserId = Guid.NewGuid()
        };

        // Mock productos existentes
        SetupMockProducts(command.Sale.SaleDetails);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Sale.Discount)
            .WithErrorMessage("El descuento no puede exceder el subtotal");
    }

    private void SetupMockProducts(List<CreateSaleDetailDto> details)
    {
        var products = details.Select(d => new Product
        {
            Id = d.ProductId,
            Stock = 100,
            IsActive = true
        }).ToList();

        _unitOfWorkMock
            .Setup(x => x.Products.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
    }

    // Más tests...
}
```

---

### TAREA 3: Unit Tests - Product Validators (Día 2 - 3 horas)

**PRIORITY**: 🔴 CRÍTICA  
**DELIVERABLE**: Tests para CreateProductCommandValidator y UpdateProductCommandValidator

#### Acceptance Criteria:
- [ ] Tests para `CreateProductCommandValidator`:
  - [ ] Test: `Validate_WhenSalePriceLessThanPurchasePrice_ReturnsValidationError`
  - [ ] Test: `Validate_WhenSalePriceEqualsPurchasePrice_ReturnsValidationError`
  - [ ] Test: `Validate_WhenSalePriceGreaterThanPurchasePrice_PassesValidation`
  - [ ] Test: `Validate_WhenCategoryDoesNotExist_ReturnsValidationError`
  - [ ] Test: `Validate_WhenStockIsNegative_ReturnsValidationError`
  - [ ] Test: `Validate_WhenAllValidationsPass_PassesValidation`

- [ ] Tests para `UpdateProductCommandValidator`:
  - [ ] Mismos casos que CreateProductCommandValidator
  - [ ] Test adicional: `Validate_WhenIdIsEmpty_ReturnsValidationError`

#### Coverage Target: >90% de validadores de productos

---

### TAREA 4: Unit Tests - Customer Validator (Día 2 - 2 horas)

**PRIORITY**: 🔴 CRÍTICA  
**DELIVERABLE**: Tests completos para CreateCustomerCommandValidator

#### Acceptance Criteria:
- [ ] Tests de formato de documento:
  - [ ] Test: `Validate_WhenDniHasInvalidLength_ReturnsValidationError`
  - [ ] Test: `Validate_WhenDniHasNonNumericChars_ReturnsValidationError`
  - [ ] Test: `Validate_WhenRucHasInvalidLength_ReturnsValidationError`
  - [ ] Test: `Validate_WhenValidDni_PassesValidation`
  - [ ] Test: `Validate_WhenValidRuc_PassesValidation`

- [ ] Tests de formato de teléfono:
  - [ ] Test: `Validate_WhenPhoneDoesNotStartWith9_ReturnsValidationError`
  - [ ] Test: `Validate_WhenPhoneHasLessThan9Digits_ReturnsValidationError`
  - [ ] Test: `Validate_WhenPhoneHasMoreThan9Digits_ReturnsValidationError`
  - [ ] Test: `Validate_WhenValidPeruvianPhone_PassesValidation`
  - [ ] Test: `Validate_WhenPhoneIsEmpty_PassesValidation` (opcional)

- [ ] Tests de unicidad:
  - [ ] Test: `Validate_WhenDocumentAlreadyExists_ReturnsValidationError`
  - [ ] Test: `Validate_WhenDocumentIsUnique_PassesValidation`

#### Coverage Target: >90% del validador

---

### TAREA 5: Unit Tests - Monetary Calculations (Día 3 - 3 horas)

**PRIORITY**: 🔴 CRÍTICA  
**DELIVERABLE**: Tests para cálculos monetarios en CreateSaleCommandHandler

#### Acceptance Criteria:
- [ ] Tests de cálculo de subtotal:
  - [ ] Test: `CalculateSubtotal_WithMultipleItems_ReturnsCorrectSum`
  - [ ] Test: `CalculateSubtotal_WithDecimalPrices_RoundsCorrectly`

- [ ] Tests de cálculo de IGV:
  - [ ] Test: `CalculateIGV_WithSubtotal100_Returns18`
  - [ ] Test: `CalculateIGV_WithSubtotalAfterDiscount_RoundsCorrectly`
  - [ ] Test: `CalculateIGV_WithDecimalSubtotal_UsesCommercialRounding`

- [ ] Tests de cálculo de total:
  - [ ] Test: `CalculateTotal_WithSubtotalAndIGV_ReturnsCorrectSum`
  - [ ] Test: `CalculateTotal_WithDiscount_AppliesDiscountCorrectly`
  - [ ] Test: `CalculateTotal_RoundsTo2Decimals`

- [ ] Tests de cálculo de vuelto:
  - [ ] Test: `CalculateChange_WhenAmountPaidGreaterThanTotal_ReturnsCorrectChange`
  - [ ] Test: `CalculateChange_WhenAmountPaidEqualsTotal_ReturnsZero`
  - [ ] Test: `CalculateChange_RoundsTo2Decimals`

- [ ] Tests de casos edge:
  - [ ] Test: `CalculateTotal_WithLargeAmounts_HandlesPrecision`
  - [ ] Test: `CalculateTotal_WithVerySmallAmounts_HandlesPrecision`
  - [ ] Test: `CalculateIGV_WithZeroSubtotal_ReturnsZero`

#### Test Structure:
```csharp
// tests/Minimarket.UnitTests/Application/Features/Sales/Commands/MonetaryCalculationsTests.cs
using FluentAssertions;
using Xunit;

namespace Minimarket.UnitTests.Application.Features.Sales.Commands;

public class MonetaryCalculationsTests
{
    [Theory]
    [InlineData(100.00, 0.00, 18.00, 118.00)]  // Sin descuento
    [InlineData(100.00, 10.00, 16.20, 106.20)] // Con descuento
    [InlineData(100.555, 0.00, 18.10, 118.66)] // Con redondeo
    public void CalculateTotal_WithVariousInputs_ReturnsCorrectResult(
        decimal subtotal, decimal discount, decimal expectedTax, decimal expectedTotal)
    {
        // Arrange
        const decimal IGV_RATE = 0.18m;
        var subtotalAfterDiscount = Math.Round(subtotal - discount, 2, MidpointRounding.AwayFromZero);
        var tax = Math.Round(subtotalAfterDiscount * IGV_RATE, 2, MidpointRounding.AwayFromZero);
        var total = Math.Round(subtotalAfterDiscount + tax, 2, MidpointRounding.AwayFromZero);

        // Assert
        tax.Should().Be(expectedTax);
        total.Should().Be(expectedTotal);
    }

    [Fact]
    public void CalculateIGV_UsesCommercialRounding()
    {
        // Arrange - Caso que requiere redondeo comercial
        const decimal subtotal = 100.115m; // Subtotal que genera IGV con más de 2 decimales
        const decimal IGV_RATE = 0.18m;
        
        // Act
        var tax = Math.Round(subtotal * IGV_RATE, 2, MidpointRounding.AwayFromZero);
        
        // Assert - Debe redondear hacia arriba (away from zero)
        tax.Should().Be(18.02m); // 100.115 * 0.18 = 18.0207 → 18.02
    }

    // Más tests...
}
```

---

## ESTRUCTURA DE ARCHIVOS

```
tests/Minimarket.UnitTests/
├── Domain/
│   └── Specifications/
│       ├── ProductHasSufficientStockSpecificationTests.cs
│       ├── ProductIsActiveSpecificationTests.cs
│       └── SaleCanBeCancelledSpecificationTests.cs
├── Application/
│   └── Features/
│       ├── Sales/
│       │   └── Commands/
│       │       ├── CreateSaleCommandValidatorTests.cs
│       │       └── MonetaryCalculationsTests.cs
│       ├── Products/
│       │   └── Commands/
│       │       ├── CreateProductCommandValidatorTests.cs
│       │       └── UpdateProductCommandValidatorTests.cs
│       └── Customers/
│           └── Commands/
│               └── CreateCustomerCommandValidatorTests.cs
└── Helpers/
    └── ValidatorTestHelper.cs
```

---

## ESTÁNDARES DE TESTING

### Frameworks Requeridos
- **xUnit**: Framework de testing
- **Moq**: Para mocking de dependencias
- **FluentAssertions**: Para assertions más legibles
- **FluentValidation.TestHelper**: Para testing de validadores

### Naming Conventions
- **Test Classes**: `[ClassUnderTest]Tests.cs`
- **Test Methods**: `[MethodName]_[Scenario]_[ExpectedResult]`
- **Ejemplo**: `IsSatisfiedBy_WhenStockIsSufficient_ReturnsTrue`

### Test Structure (AAA Pattern)
```csharp
[Fact]
public void TestName_Scenario_ExpectedResult()
{
    // Arrange
    // Setup test data
    
    // Act
    // Execute code under test
    
    // Assert
    // Verify results
}
```

---

## MÉTRICAS Y OBJETIVOS

### Coverage Targets
- **Specifications**: 100% (crítico - lógica de dominio)
- **Validators**: >90% (crítico - validaciones de entrada)
- **Monetary Calculations**: 100% (crítico - cálculos financieros)

### Test Count Targets
- **Specifications Tests**: Mínimo 15 tests
- **Validator Tests**: Mínimo 30 tests
- **Calculation Tests**: Mínimo 10 tests
- **Total**: Mínimo 55 tests

### Quality Metrics
- **Tests Passing**: 100%
- **Test Independence**: Cada test ejecutable solo
- **Test Readability**: Tests claros y auto-documentados

---

## DEPENDENCIAS Y BLOQUEOS

### Dependencias
- ✅ Especificaciones ya implementadas
- ✅ Validadores ya implementados
- ✅ Proyecto de tests existe
- ⚠️ Necesita verificar frameworks instalados

### Bloqueos Potenciales
- Si falta FluentValidation.TestHelper package
- Si hay problemas con mocking de IUnitOfWork

### Acción si Bloqueado
- Reportar inmediatamente a Tech Lead
- Documentar el bloqueo específico

---

## REPORTE DIARIO REQUERIDO

Al final de cada día, reportar:

```
## DAILY PROGRESS - Business Logic Validator - [Fecha]

### Tests Escritos Hoy:
- Specifications: X tests
- Validators: Y tests
- Calculations: Z tests
- Total: X + Y + Z tests

### Coverage Actual:
- Specifications: X%
- Validators: Y%
- Calculations: Z%

### Tests Passing:
- ✅ Todos pasando / ⚠️ X tests fallando

### Blockers:
- [Lista de blockers si los hay]

### Plan Mañana:
- [Tareas específicas para mañana]
```

---

## ACCEPTANCE CRITERIA FINAL

El trabajo está **COMPLETO** cuando:

- [ ] ✅ Todas las especificaciones tienen tests (100% coverage)
- [ ] ✅ Todos los validadores tienen tests (>90% coverage)
- [ ] ✅ Todos los cálculos monetarios tienen tests (100% coverage)
- [ ] ✅ Todos los tests pasan (green)
- [ ] ✅ Tests son independientes y ejecutables solos
- [ ] ✅ Tests documentan casos edge y boundary conditions
- [ ] ✅ Reporte de coverage generado
- [ ] ✅ PR creado con todos los tests
- [ ] ✅ Code review aprobado por Tech Lead

---

## RECURSOS Y REFERENCIAS

### Documentación
- [FluentValidation Testing](https://docs.fluentvalidation.net/en/latest/testing.html)
- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [FluentAssertions](https://fluentassertions.com/)

### Archivos de Referencia
- `src/Minimarket.Domain/Specifications/` - Código a testear
- `src/Minimarket.Application/Features/*/Commands/*Validator.cs` - Validadores a testear
- `CODE_REVIEW_BusinessLogicValidator.md` - Contexto completo
- `TECHNICAL_AUDIT.md` - Auditoría técnica

---

## PRIORIZACIÓN DE TAREAS

**Orden de Ejecución Recomendado**:
1. **Día 1**: Tarea 1 (Specifications) → Tarea 2 (CreateSaleCommandValidator - inicio)
2. **Día 2**: Tarea 2 (CreateSaleCommandValidator - completar) → Tarea 3 (Product Validators) → Tarea 4 (Customer Validator)
3. **Día 3**: Tarea 5 (Monetary Calculations) → Revisar y completar cualquier test faltante

---

## NOTAS FINALES

**@Business-Logic-Validator**: 

Ya implementaste excelentes validaciones y especificaciones. Ahora necesitas **demostrar que funcionan correctamente** mediante tests.

**ENFÓCATE EN**:
- ✅ Tests que demuestren que las reglas de negocio funcionan
- ✅ Casos edge y boundary conditions
- ✅ Coverage alto en lógica crítica
- ✅ Tests claros y mantenibles

**ESTA TAREA ES COMPLEMENTARIA A LA DE QA-Backend PERO CRÍTICA PARA TU ROL.**

---

**ASIGNADO POR**: Tech Lead  
**FECHA**: [Fecha Actual]  
**DEADLINE**: [Fecha + 3 días hábiles]  
**STATUS**: 🟡 EN PROGRESO

