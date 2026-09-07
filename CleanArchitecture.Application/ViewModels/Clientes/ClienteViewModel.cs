using System;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.ViewModels.Clientes;

public sealed class ClienteViewModel
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

    public static ClienteViewModel FromCliente(Cliente cliente)
    {
        return new ClienteViewModel
        {
            Id = cliente.Id,
            Nombres = cliente.Nombres,
            Apellidos = cliente.Apellidos,
            DocumentoIdentidad = cliente.DocumentoIdentidad,
            Email = cliente.Email,
            Telefono = cliente.Telefono,
            Estado = cliente.Estado,
            FechaRegistro = cliente.FechaRegistro,
            FechaActualizacion = cliente.FechaActualizacion
        };
    }
}
