using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Errors;
using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Domain.Interfaces.Repositories;
using CleanArchitecture.Domain.Notifications;
using MediatR;

namespace CleanArchitecture.Domain.Commands.Clients.CreateClient;

public sealed class CreateClientCommandHandler : CommandHandlerBase, IRequestHandler<CreateClientCommand>
{
    private readonly IClientRepository _clientRepository;

    public CreateClientCommandHandler(
        IMediatorHandler bus,
        IUnitOfWork unitOfWork,
        INotificationHandler<DomainNotification> notifications,
        IClientRepository clientRepository) : base(bus, unitOfWork, notifications)
    {
        _clientRepository = clientRepository;
    }

    public async Task Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        if (!await TestValidityAsync(request))
        {
            return;
        }

        if (await _clientRepository.ExistsAsync(request.AggregateId))
        {
            await NotifyAsync(
                new DomainNotification(
                    request.MessageType,
                    $"There is already a client with Id {request.AggregateId}",
                    DomainErrorCodes.Client.AlreadyExists));
            return;
        }

        var client = new Client(
            request.AggregateId,
            request.Name,
            request.Email,
            request.Phone);

        _clientRepository.Add(client);
        await CommitAsync();
    }
}
