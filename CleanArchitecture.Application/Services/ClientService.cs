using System;
using System.Threading.Tasks;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Queries.Clients.GetAll;
using CleanArchitecture.Application.ViewModels;
using CleanArchitecture.Application.ViewModels.Clients;
using CleanArchitecture.Application.ViewModels.Sorting;
using CleanArchitecture.Domain.Commands.Clients.CreateClient;
using CleanArchitecture.Domain.Interfaces;

namespace CleanArchitecture.Application.Services;

public sealed class ClientService : IClientService
{
    private readonly IMediatorHandler _bus;

    public ClientService(IMediatorHandler bus)
    {
        _bus = bus;
    }

    public async Task<Guid> CreateClientAsync(CreateClientViewModel client)
    {
        var clientId = Guid.NewGuid();

        await _bus.SendCommandAsync(new CreateClientCommand(
            clientId,
            client.Name,
            client.Email,
            client.Phone));

        return clientId;
    }

    public Task<PagedResult<ClientViewModel>> GetAllClientsAsync(
        int page,
        int pageSize,
        bool includeDeleted,
        string searchTerm = "",
        SortQuery? sortQuery = null)
    {
        return _bus.QueryAsync(new GetAllClientsQuery(
            page,
            pageSize,
            includeDeleted,
            searchTerm,
            sortQuery));
    }
}
