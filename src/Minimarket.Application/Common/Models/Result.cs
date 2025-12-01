using System.Text.Json.Serialization;

namespace Minimarket.Application.Common.Models;

public class Result
{
    public bool Succeeded { get; set; }
    public string[] Errors { get; set; } = Array.Empty<string>();

    // Propiedades adicionales para compatibilidad con el patrón estándar
    public bool IsSuccess => Succeeded;
    public bool IsFailure => !Succeeded;
    public string Error => Errors.Length > 0 ? string.Join("; ", Errors) : string.Empty;

    public static Result Success() => new Result { Succeeded = true };
    public static Result Failure(params string[] errors) => new Result { Succeeded = false, Errors = errors };
    
    // Métodos adicionales para compatibilidad
    public static Result Failure(string error) => new Result { Succeeded = false, Errors = new[] { error } };
}

public class Result<T> : Result
{
    public T? Data { get; set; }

    // Propiedad Value para acceso seguro (lanzará excepción si es failure)
    // Ignorar en serialización JSON para evitar errores cuando el resultado es un fallo
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public T Value => IsSuccess && Data != null
        ? Data
        : throw new InvalidOperationException("Cannot access value of a failed result");

    public static Result<T> Success(T data) => new Result<T> { Succeeded = true, Data = data };
    public static new Result<T> Failure(params string[] errors) => new Result<T> { Succeeded = false, Errors = errors };
    public static new Result<T> Failure(string error) => new Result<T> { Succeeded = false, Errors = new[] { error } };
}

