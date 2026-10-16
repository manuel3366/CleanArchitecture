using System;

namespace CleanArchitecture.Domain.Commands.Clients.DeleteClient;

public sealed class DeleteClientCommand : CommandBase
{
    private static readonly DeleteClientCommandValidation s_validation = new();

    public DeleteClientCommand(Guid clientId) : base(clientId)
    {
    }

    public override bool IsValid()
    {
        ValidationResult = s_validation.Validate(this);
        return ValidationResult.IsValid;
    }
}
