using System;

namespace CleanArchitecture.Domain.Commands.Clients.CreateClient;

public sealed class CreateClientCommand : CommandBase
{
    private static readonly CreateClientCommandValidation s_validation = new();

    public string Name { get; }
    public string Email { get; }
    public string Phone { get; }

    public CreateClientCommand(Guid clientId, string name, string email, string phone) : base(clientId)
    {
        Name = name;
        Email = email;
        Phone = phone;
    }

    public override bool IsValid()
    {
        ValidationResult = s_validation.Validate(this);
        return ValidationResult.IsValid;
    }
}
