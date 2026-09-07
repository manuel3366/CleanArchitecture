using System;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.ViewModels.Clients;

public sealed class ClientViewModel
{
    public Guid Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string DocumentoIdentidad { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTimeOffset FechaRegistro { get; set; }
    public DateTimeOffset FechaActualizacion { get; set; }

    public static ClientViewModel FromClient(Client client)
    {
        return new ClientViewModel
        {
            Id = client.Id,
            Nombres = client.Nombres,
            Apellidos = client.Apellidos,
            DocumentoIdentidad = client.DocumentoIdentidad,
            Email = client.Email,
            Telefono = client.Telefono,
            Estado = client.Estado,
            FechaRegistro = client.FechaRegistro,
            FechaActualizacion = client.FechaActualizacion
        };
    }
}
