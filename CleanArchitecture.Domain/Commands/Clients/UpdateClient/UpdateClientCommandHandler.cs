using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Errors;
using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Domain.Interfaces.Repositories;
using CleanArchitecture.Domain.Notifications;
using MediatR;

namespace CleanArchitecture.Domain.Commands.Clients.UpdateClient;

public sealed class UpdateClientCommandHandler : CommandHandlerBase,
    IRequestHandler<UpdateClientCommand>
{
    private readonly IClientRepository _clientRepository;

    public UpdateClientCommandHandler(
        IMediatorHandler bus,
        IUnitOfWork unitOfWork,
        INotificationHandler<DomainNotification> notifications,
        IClientRepository clientRepository) : base(bus, unitOfWork, notifications)
    {
        _clientRepository = clientRepository;
    }

    public async Task Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        if (!await TestValidityAsync(request))
        {
            return;
        }

        var client = await _clientRepository.GetByIdAsync(request.AggregateId);

        if (client is null || client.DeletedAt is not null)
        {
            await NotifyAsync(
                new DomainNotification(
                    request.MessageType,
                    $"There is no active client with Id {request.AggregateId}",
                    ErrorCodes.ObjectNotFound));

            return;
        }

        client.UpdateDetails(request.Name!, request.Email!, request.Phone!);
        await CommitAsync();
    }
}
