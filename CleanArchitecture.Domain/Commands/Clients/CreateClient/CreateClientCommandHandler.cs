using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Errors;
using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Domain.Interfaces.Repositories;
using CleanArchitecture.Domain.Notifications;
using MediatR;

namespace CleanArchitecture.Domain.Commands.Clients.CreateClient;

public sealed class CreateClientCommandHandler : CommandHandlerBase,
    IRequestHandler<CreateClientCommand>
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

        var existingClientByDocument = await _clientRepository.GetByDocumentoIdentidadAsync(request.DocumentoIdentidad);
        if (existingClientByDocument is not null)
        {
            await NotifyAsync(
                new DomainNotification(
                    request.MessageType,
                    $"There is already a client with DocumentoIdentidad {request.DocumentoIdentidad}",
                    DomainErrorCodes.Client.AlreadyExists));
            return;
        }

        var existingClientByEmail = await _clientRepository.GetByEmailAsync(request.Email);
        if (existingClientByEmail is not null)
        {
            await NotifyAsync(
                new DomainNotification(
                    request.MessageType,
                    $"There is already a client with email {request.Email}",
                    DomainErrorCodes.Client.AlreadyExists));
            return;
        }

        var client = new Client(
            request.ClientId,
            request.Nombres,
            request.Apellidos,
            request.DocumentoIdentidad,
            request.Email,
            request.Estado,
            request.FechaRegistro,
            request.Telefono,
            request.FechaActualizacion);

        _clientRepository.Add(client);

        await CommitAsync();
    }
}
