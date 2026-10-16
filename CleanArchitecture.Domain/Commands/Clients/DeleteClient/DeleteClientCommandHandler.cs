using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Errors;
using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Domain.Interfaces.Repositories;
using CleanArchitecture.Domain.Notifications;
using MediatR;

namespace CleanArchitecture.Domain.Commands.Clients.DeleteClient;

public sealed class DeleteClientCommandHandler : CommandHandlerBase,
    IRequestHandler<DeleteClientCommand>
{
    private readonly IClientRepository _clientRepository;

    public DeleteClientCommandHandler(
        IMediatorHandler bus,
        IUnitOfWork unitOfWork,
        INotificationHandler<DomainNotification> notifications,
        IClientRepository clientRepository) : base(bus, unitOfWork, notifications)
    {
        _clientRepository = clientRepository;
    }

    public async Task Handle(DeleteClientCommand request, CancellationToken cancellationToken)
    {
        if (!await TestValidityAsync(request))
        {
            return;
        }

        var client = await _clientRepository.GetByIdAsync(request.AggregateId);

        if (client is null)
        {
            await NotifyAsync(
                new DomainNotification(
                    request.MessageType,
                    $"There is no client with Id {request.AggregateId}",
                    ErrorCodes.ObjectNotFound));

            return;
        }

        _clientRepository.Remove(client);
        await CommitAsync();
    }
}
