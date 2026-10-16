using CleanArchitecture.Domain.Errors;
using FluentValidation;

namespace CleanArchitecture.Domain.Commands.Clients.UpdateClient;

public sealed class UpdateClientCommandValidation : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientCommandValidation()
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
            .WithMessage("Email may not be empty");

        RuleFor(command => command.Phone)
            .NotEmpty()
            .WithErrorCode(DomainErrorCodes.Client.EmptyPhone)
            .WithMessage("Phone may not be empty");
    }
}
