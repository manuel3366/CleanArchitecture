using System;
using System.Net.Mail;

namespace CleanArchitecture.Domain.Entities;

public class Cliente : Entity
{
    public string Nombres { get; private set; } = string.Empty;
    public string Apellidos { get; private set; } = string.Empty;
    public string DocumentoIdentidad { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Telefono { get; private set; }
    public string Estado { get; private set; } = string.Empty;
    public DateTimeOffset FechaRegistro { get; private set; }
    public DateTimeOffset FechaActualizacion { get; private set; }

    public Cliente(
        Guid id,
        string nombres,
        string apellidos,
        string documentoIdentidad,
        string email,
        string? telefono,
        string estado)
        : this(id, nombres, apellidos, documentoIdentidad, email, telefono, estado, DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow)
    {
    }

    public Cliente(
        Guid id,
        string nombres,
        string apellidos,
        string documentoIdentidad,
        string email,
        string? telefono,
        string estado,
        DateTimeOffset fechaRegistro,
        DateTimeOffset fechaActualizacion) : base(id)
    {
        SetNombres(nombres);
        SetApellidos(apellidos);
        SetDocumentoIdentidad(documentoIdentidad);
        SetEmail(email);
        SetTelefono(telefono);
        SetEstado(estado);

        FechaRegistro = fechaRegistro;
        FechaActualizacion = fechaActualizacion;
    }

    public void SetNombres(string nombres)
    {
        Nombres = EnsureRequired(nombres, nameof(Nombres));
        FechaActualizacion = DateTimeOffset.UtcNow;
    }

    public void SetApellidos(string apellidos)
    {
        Apellidos = EnsureRequired(apellidos, nameof(Apellidos));
        FechaActualizacion = DateTimeOffset.UtcNow;
    }

    public void SetDocumentoIdentidad(string documentoIdentidad)
    {
        DocumentoIdentidad = EnsureRequired(documentoIdentidad, nameof(DocumentoIdentidad));
        FechaActualizacion = DateTimeOffset.UtcNow;
    }

    public void SetEmail(string email)
    {
        var normalized = EnsureRequired(email, nameof(Email));

        try
        {
            _ = new MailAddress(normalized);
        }
        catch (FormatException)
        {
            throw new ArgumentException("El correo electrónico no tiene un formato válido.", nameof(email));
        }

        Email = normalized;
        FechaActualizacion = DateTimeOffset.UtcNow;
    }

    public void SetTelefono(string? telefono)
    {
        Telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();
        FechaActualizacion = DateTimeOffset.UtcNow;
    }

    public void SetEstado(string estado)
    {
        Estado = EnsureRequired(estado, nameof(Estado));
        FechaActualizacion = DateTimeOffset.UtcNow;
    }

    private static string EnsureRequired(string value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{propertyName} es obligatorio.", propertyName);
        }

        return value.Trim();
    }
}
