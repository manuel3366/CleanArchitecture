using CleanArchitecture.Domain.Errors;
using FluentValidation;

namespace CleanArchitecture.Domain.Commands.Clients.DeleteClient;

public sealed class DeleteClientCommandValidation : AbstractValidator<DeleteClientCommand>
{
    public DeleteClientCommandValidation()
    {
        RuleFor(command => command.AggregateId)
            .NotEmpty()
            .WithErrorCode(DomainErrorCodes.Client.EmptyId)
            .WithMessage("Client id may not be empty");
    }
}
