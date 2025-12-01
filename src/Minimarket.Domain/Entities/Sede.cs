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
    public string? GoogleMapsUrl { get; set; } // URL de Google Maps para la ubicación de la sede

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
        if (horarios == null)
        {
            HorariosJson = "{}";
            return;
        }
        
        try
        {
            HorariosJson = System.Text.Json.JsonSerializer.Serialize(horarios);
        }
        catch
        {
            HorariosJson = "{}";
        }
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

        var nombresDias = new Dictionary<string, string>
        {
            { "lunes", "Lunes" },
            { "martes", "Martes" },
            { "miercoles", "Miércoles" },
            { "jueves", "Jueves" },
            { "viernes", "Viernes" },
            { "sabado", "Sábado" },
            { "domingo", "Domingo" }
        };

        // Si está abierto ahora, no hay próxima apertura
        if (IsOpen(ahora))
        {
            return null;
        }

        // Buscar en los próximos 7 días
        for (int i = 0; i < 7; i++)
        {
            var fecha = ahora.AddDays(i);
            var diaKey = diaMap[fecha.DayOfWeek];

            if (horarios.TryGetValue(diaKey, out var horarioDia))
            {
                if (horarioDia.TryGetValue("abre", out var horaAbre) && 
                    horarioDia.TryGetValue("cierra", out var horaCierra))
                {
                    // Si es el día actual
                    if (i == 0)
                    {
                        var horaActual = ahora.TimeOfDay;
                        if (TimeSpan.TryParse(horaAbre, out var abre) && 
                            TimeSpan.TryParse(horaCierra, out var cierra))
                        {
                            // Si ya pasó la hora de cierre de hoy, buscar el próximo día
                            if (horaActual > cierra)
                            {
                                continue; // Continuar buscando en el próximo día
                            }
                            // Si aún no ha llegado la hora de apertura de hoy
                            if (horaActual < abre)
                            {
                                return $"Hoy a las {horaAbre}";
                            }
                        }
                    }
                    // Si es mañana
                    else if (i == 1)
                    {
                        return $"Mañana a las {horaAbre}";
                    }
                    // Si es otro día de la semana
                    else
                    {
                        var nombreDia = nombresDias.TryGetValue(diaKey, out var nombre) ? nombre : diaKey;
                        return $"{nombre} a las {horaAbre}";
                    }
                }
            }
        }

        return "Próximamente";
    }
}

