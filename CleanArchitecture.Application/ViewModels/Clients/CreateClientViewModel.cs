using System;

namespace CleanArchitecture.Application.ViewModels.Clients;

public sealed record CreateClientViewModel(
    string Nombres,
    string Apellidos,
    string DocumentoIdentidad,
    string Email,
    string? Telefono,
    string Estado,
    DateTimeOffset? FechaRegistro = null,
    DateTimeOffset? FechaActualizacion = null);
