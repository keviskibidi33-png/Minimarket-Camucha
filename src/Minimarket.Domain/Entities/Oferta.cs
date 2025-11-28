namespace Minimarket.Domain.Entities;

public enum DescuentoTipo
{
    Porcentaje = 0,
    MontoFijo = 1
}

public class Oferta : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DescuentoTipo DescuentoTipo { get; set; } = DescuentoTipo.Porcentaje;
    public decimal DescuentoValor { get; set; }
    public string CategoriasIdsJson { get; set; } = "[]"; // JSON array de category_ids
    public string ProductosIdsJson { get; set; } = "[]"; // JSON array de product_ids
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public bool Activa { get; set; } = true;
    public int Orden { get; set; } = 0;
    public string? ImagenUrl { get; set; }

    // Helper methods
    public List<Guid> GetCategoriasIds()
    {
        if (string.IsNullOrEmpty(CategoriasIdsJson)) return new List<Guid>();
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(CategoriasIdsJson) ?? new List<Guid>();
        }
        catch
        {
            return new List<Guid>();
        }
    }

    public List<Guid> GetProductosIds()
    {
        if (string.IsNullOrEmpty(ProductosIdsJson)) return new List<Guid>();
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(ProductosIdsJson) ?? new List<Guid>();
        }
        catch
        {
            return new List<Guid>();
        }
    }

    public void SetCategoriasIds(List<Guid> ids)
    {
        CategoriasIdsJson = System.Text.Json.JsonSerializer.Serialize(ids);
    }

    public void SetProductosIds(List<Guid> ids)
    {
        ProductosIdsJson = System.Text.Json.JsonSerializer.Serialize(ids);
    }

    public bool IsActive(DateTime fecha)
    {
        return Activa && fecha >= FechaInicio && fecha <= FechaFin;
    }

    public decimal CalculateDiscount(decimal precioOriginal)
    {
        if (!IsActive(DateTime.Now))
            return 0;

        if (DescuentoTipo == DescuentoTipo.Porcentaje)
        {
            return precioOriginal * (DescuentoValor / 100m);
        }
        else
        {
            return DescuentoValor > precioOriginal ? precioOriginal : DescuentoValor;
        }
    }
}

