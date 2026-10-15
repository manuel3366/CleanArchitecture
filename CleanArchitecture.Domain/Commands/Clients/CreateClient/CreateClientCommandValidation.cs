using CleanArchitecture.Domain.Errors;
using FluentValidation;

namespace CleanArchitecture.Domain.Commands.Clients.CreateClient;

public sealed class CreateClientCommandValidation : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidation()
    {
        RuleFor(command => command.AggregateId)
            .NotEmpty()
            .WithErrorCode(DomainErrorCodes.Client.EmptyId)
            .WithMessage("Client id may not be empty");

        RuleFor(command => command.Name)
            .NotEmpty()
            .WithErrorCode(DomainErrorCodes.Client.EmptyName)
            .WithMessage("Name may not be empty");

        RuleFor(command => command.Email)
            .NotEmpty()
            .WithErrorCode(DomainErrorCodes.Client.EmptyEmail)
            .WithMessage("Email may not be empty")
            .EmailAddress()
            .WithErrorCode(DomainErrorCodes.Client.InvalidEmail)
            .WithMessage("Email is not a valid email address");

        RuleFor(command => command.Phone)
            .NotEmpty()
            .WithErrorCode(DomainErrorCodes.Client.EmptyPhone)
            .WithMessage("Phone may not be empty");
    }
}
