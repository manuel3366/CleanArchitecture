using System;

namespace CleanArchitecture.Domain.Entities;

public class Client : Entity
{
    public string Nombres { get; private set; }
    public string Apellidos { get; private set; }
    public string DocumentoIdentidad { get; private set; }
    public string Email { get; private set; }
    public string? Telefono { get; private set; }
    public string Estado { get; private set; }
    public DateTimeOffset FechaRegistro { get; private set; }
    public DateTimeOffset FechaActualizacion { get; private set; }

    public Client(
        Guid id,
        string nombres,
        string apellidos,
        string documentoIdentidad,
        string email,
        string estado,
        DateTimeOffset fechaRegistro,
        string? telefono = null,
        DateTimeOffset fechaActualizacion = default) : base(id)
    {
        if (string.IsNullOrWhiteSpace(nombres)) throw new ArgumentException("Nombres are required", nameof(nombres));
        if (string.IsNullOrWhiteSpace(apellidos)) throw new ArgumentException("Apellidos are required", nameof(apellidos));
        if (string.IsNullOrWhiteSpace(documentoIdentidad)) throw new ArgumentException("DocumentoIdentidad is required", nameof(documentoIdentidad));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required", nameof(email));
        if (string.IsNullOrWhiteSpace(estado)) throw new ArgumentException("Estado is required", nameof(estado));

        Nombres = nombres;
        Apellidos = apellidos;
        DocumentoIdentidad = documentoIdentidad;
        Email = email;
        Telefono = telefono;
        Estado = estado;
        FechaRegistro = fechaRegistro;
        FechaActualizacion = fechaActualizacion == default ? fechaRegistro : fechaActualizacion;
    }

    public void SetNombres(string nombres)
    {
        Nombres = nombres;
    }

    public void SetApellidos(string apellidos)
    {
        Apellidos = apellidos;
    }

    public void SetDocumentoIdentidad(string documentoIdentidad)
    {
        DocumentoIdentidad = documentoIdentidad;
    }

    public void SetEmail(string email)
    {
        Email = email;
    }

    public void SetTelefono(string? telefono)
    {
        Telefono = telefono;
    }

    public void SetEstado(string estado)
    {
        Estado = estado;
    }

    public void SetFechaActualizacion(DateTimeOffset fechaActualizacion)
    {
        FechaActualizacion = fechaActualizacion;
    }
}
