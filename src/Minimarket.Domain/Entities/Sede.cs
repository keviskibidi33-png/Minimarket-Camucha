namespace Minimarket.Domain.Entities;

public class Sede : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Pais { get; set; } = "Perú";
    public decimal Latitud { get; set; }
    public decimal Longitud { get; set; }
    public string? Telefono { get; set; }
    public string HorariosJson { get; set; } = "{}"; // JSON string con horarios por día
    public string? LogoUrl { get; set; }
    public bool Estado { get; set; } = true; // Activo/Inactivo

    // Métodos helper para trabajar con horarios
    public Dictionary<string, Dictionary<string, string>> GetHorarios()
    {
        if (string.IsNullOrEmpty(HorariosJson))
            return new Dictionary<string, Dictionary<string, string>>();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(HorariosJson)
                ?? new Dictionary<string, Dictionary<string, string>>();
        }
        catch
        {
            return new Dictionary<string, Dictionary<string, string>>();
        }
    }

    public void SetHorarios(Dictionary<string, Dictionary<string, string>> horarios)
    {
        HorariosJson = System.Text.Json.JsonSerializer.Serialize(horarios);
    }

    public bool IsOpen(DateTime fechaHora)
    {
        var horarios = GetHorarios();
        var diaSemana = fechaHora.DayOfWeek.ToString().ToLower();
        var diaMap = new Dictionary<DayOfWeek, string>
        {
            { DayOfWeek.Monday, "lunes" },
            { DayOfWeek.Tuesday, "martes" },
            { DayOfWeek.Wednesday, "miercoles" },
            { DayOfWeek.Thursday, "jueves" },
            { DayOfWeek.Friday, "viernes" },
            { DayOfWeek.Saturday, "sabado" },
            { DayOfWeek.Sunday, "domingo" }
        };

        if (!diaMap.TryGetValue(fechaHora.DayOfWeek, out var diaKey))
            return false;

        if (!horarios.TryGetValue(diaKey, out var horarioDia))
            return false;

        if (!horarioDia.TryGetValue("abre", out var horaAbre) || !horarioDia.TryGetValue("cierra", out var horaCierra))
            return false;

        var horaActual = fechaHora.TimeOfDay;
        if (TimeSpan.TryParse(horaAbre, out var abre) && TimeSpan.TryParse(horaCierra, out var cierra))
        {
            return horaActual >= abre && horaActual <= cierra;
        }

        return false;
    }

    public string? GetNextOpenTime()
    {
        var ahora = DateTime.Now;
        var horarios = GetHorarios();
        var diaMap = new Dictionary<DayOfWeek, string>
        {
            { DayOfWeek.Monday, "lunes" },
            { DayOfWeek.Tuesday, "martes" },
            { DayOfWeek.Wednesday, "miercoles" },
            { DayOfWeek.Thursday, "jueves" },
            { DayOfWeek.Friday, "viernes" },
            { DayOfWeek.Saturday, "sabado" },
            { DayOfWeek.Sunday, "domingo" }
        };

        // Buscar en los próximos 7 días
        for (int i = 0; i < 7; i++)
        {
            var fecha = ahora.AddDays(i);
            var diaKey = diaMap[fecha.DayOfWeek];

            if (horarios.TryGetValue(diaKey, out var horarioDia))
            {
                if (horarioDia.TryGetValue("abre", out var horaAbre))
                {
                    if (i == 0 && IsOpen(fecha))
                    {
                        return null; // Ya está abierto
                    }
                    return i == 0 ? $"Hoy a las {horaAbre}" : $"Mañana a las {horaAbre}";
                }
            }
        }

        return "Próximamente";
    }
}

