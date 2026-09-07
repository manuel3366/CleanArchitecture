using CleanArchitecture.Domain.Constants;
using CleanArchitecture.Domain.Errors;
using FluentValidation;

namespace CleanArchitecture.Domain.Commands.Clients.CreateClient;

public sealed class CreateClientCommandValidation : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidation()
    {
        AddRuleForId();
        AddRuleForNombres();
        AddRuleForApellidos();
        AddRuleForDocumentoIdentidad();
        AddRuleForEmail();
        AddRuleForTelefono();
        AddRuleForEstado();
    }

    private void AddRuleForId()
    {
        RuleFor(cmd => cmd.AggregateId)
            .NotEmpty()
            .WithErrorCode(DomainErrorCodes.Client.EmptyId)
            .WithMessage("Client id may not be empty");
    }

    private void AddRuleForNombres()
    {
        RuleFor(cmd => cmd.Nombres)
            .NotEmpty()
            .WithErrorCode(DomainErrorCodes.Client.EmptyNombres)
            .WithMessage("Nombres may not be empty")
            .MaximumLength(MaxLengths.Client.Nombres)
            .WithErrorCode(DomainErrorCodes.Client.NombresExceedsMaxLength)
            .WithMessage($"Nombres may not be longer than {MaxLengths.Client.Nombres} characters");
    }

    private void AddRuleForApellidos()
    {
        RuleFor(cmd => cmd.Apellidos)
            .NotEmpty()
            .WithErrorCode(DomainErrorCodes.Client.EmptyApellidos)
            .WithMessage("Apellidos may not be empty")
            .MaximumLength(MaxLengths.Client.Apellidos)
            .WithErrorCode(DomainErrorCodes.Client.ApellidosExceedsMaxLength)
            .WithMessage($"Apellidos may not be longer than {MaxLengths.Client.Apellidos} characters");
    }

    private void AddRuleForDocumentoIdentidad()
    {
        RuleFor(cmd => cmd.DocumentoIdentidad)
            .NotEmpty()
            .WithErrorCode(DomainErrorCodes.Client.EmptyDocumentoIdentidad)
            .WithMessage("DocumentoIdentidad may not be empty")
            .MaximumLength(MaxLengths.Client.DocumentoIdentidad)
            .WithErrorCode(DomainErrorCodes.Client.DocumentoIdentidadExceedsMaxLength)
            .WithMessage($"DocumentoIdentidad may not be longer than {MaxLengths.Client.DocumentoIdentidad} characters");
    }

    private void AddRuleForEmail()
    {
        RuleFor(cmd => cmd.Email)
            .NotEmpty()
            .WithErrorCode(DomainErrorCodes.Client.EmptyEmail)
            .WithMessage("Email may not be empty")
            .EmailAddress()
            .WithErrorCode(DomainErrorCodes.Client.InvalidEmail)
            .WithMessage("Email is not a valid email address")
            .MaximumLength(MaxLengths.Client.Email)
            .WithErrorCode(DomainErrorCodes.Client.EmailExceedsMaxLength)
            .WithMessage($"Email may not be longer than {MaxLengths.Client.Email} characters");
    }

    private void AddRuleForTelefono()
    {
        RuleFor(cmd => cmd.Telefono)
            .MaximumLength(MaxLengths.Client.Telefono)
            .WithErrorCode(DomainErrorCodes.Client.TelefonoExceedsMaxLength)
            .WithMessage($"Telefono may not be longer than {MaxLengths.Client.Telefono} characters")
            .When(cmd => !string.IsNullOrWhiteSpace(cmd.Telefono));
    }

    private void AddRuleForEstado()
    {
        RuleFor(cmd => cmd.Estado)
            .NotEmpty()
            .WithErrorCode(DomainErrorCodes.Client.EmptyEstado)
            .WithMessage("Estado may not be empty")
            .MaximumLength(MaxLengths.Client.Estado)
            .WithErrorCode(DomainErrorCodes.Client.EstadoExceedsMaxLength)
            .WithMessage($"Estado may not be longer than {MaxLengths.Client.Estado} characters");
    }
}
