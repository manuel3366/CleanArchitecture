using System;

namespace CleanArchitecture.Domain.Commands.Clients.CreateClient;

public sealed class CreateClientCommand : CommandBase
{
    private static readonly CreateClientCommandValidation s_validation = new();

    public Guid ClientId { get; }
    public string Nombres { get; }
    public string Apellidos { get; }
    public string DocumentoIdentidad { get; }
    public string Email { get; }
    public string? Telefono { get; }
    public string Estado { get; }
    public DateTimeOffset FechaRegistro { get; }
    public DateTimeOffset FechaActualizacion { get; }

    public CreateClientCommand(
        Guid clientId,
        string nombres,
        string apellidos,
        string documentoIdentidad,
        string email,
        string? telefono,
        string estado,
        DateTimeOffset fechaRegistro,
        DateTimeOffset? fechaActualizacion = null) : base(clientId)
    {
        ClientId = clientId;
        Nombres = nombres;
        Apellidos = apellidos;
        DocumentoIdentidad = documentoIdentidad;
        Email = email;
        Telefono = telefono;
        Estado = estado;
        FechaRegistro = fechaRegistro;
        FechaActualizacion = fechaActualizacion ?? fechaRegistro;
    }

    public override bool IsValid()
    {
        ValidationResult = s_validation.Validate(this);
        return ValidationResult.IsValid;
    }
}
