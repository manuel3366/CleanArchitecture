using System;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Queries.Clients.GetAll;
using CleanArchitecture.Application.ViewModels.Clients;
using CleanArchitecture.Application.SortProviders;
using CleanArchitecture.Application.ViewModels;
using CleanArchitecture.Application.ViewModels.Sorting;
using CleanArchitecture.Domain.Errors;
using CleanArchitecture.Domain.Commands.Clients.CreateClient;
using CleanArchitecture.Domain.Commands.Clients.DeleteClient;
using CleanArchitecture.Domain.Commands.Clients.UpdateClient;
using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Domain.Notifications;

namespace CleanArchitecture.Application.Services;

public sealed class ClientService : IClientService
{
    private readonly IMediatorHandler _bus;

    public ClientService(IMediatorHandler bus)
    {
        _bus = bus;
    }

    public async Task<Guid> CreateClientAsync(CreateClientViewModel? client)
    {
        var clientId = Guid.NewGuid();

        await _bus.SendCommandAsync(new CreateClientCommand(
            clientId,
            client?.Name,
            client?.Email,
            client?.Phone));

        return clientId;
    }

    public async Task DeleteClientAsync(Guid clientId)
    {
        await _bus.SendCommandAsync(new DeleteClientCommand(clientId));
    }

    public async Task<ClientViewModel> UpdateClientAsync(UpdateClientViewModel? client)
    {
        var clientId = client?.Id ?? Guid.Empty;
        var name = client?.Name;
        var email = client?.Email;
        var phone = client?.Phone;

        await _bus.SendCommandAsync(new UpdateClientCommand(clientId, name, email, phone));

        return new ClientViewModel
        {
            Id = clientId,
            Name = name ?? string.Empty,
            Email = email ?? string.Empty,
            Phone = phone ?? string.Empty,
            DeletedAt = null
        };
    }

    public async Task<PagedResult<ClientViewModel>> GetAllClientsAsync(
        int page,
        int pageSize,
        bool includeDeleted,
        string searchTerm = "",
        SortQuery? sortQuery = null)
    {
        if (page < 1)
        {
            await NotifyInvalidQueryAsync("page", "Page must be greater than or equal to 1.");
            return PagedResult<ClientViewModel>.Empty();
        }

        if (pageSize < 1 || pageSize > 100)
        {
            await NotifyInvalidQueryAsync("pageSize", "Page size must be between 1 and 100.");
            return PagedResult<ClientViewModel>.Empty();
        }

        var allowedFields = new ClientViewModelSortProvider().GetSortingExpressions();
        var invalidField = sortQuery?.Parameters
            .Where(parameter => !allowedFields.ContainsKey(parameter.ParameterName))
            .Select(parameter => (SortParameter?)parameter)
            .FirstOrDefault();

        if (invalidField.HasValue)
        {
            await NotifyInvalidQueryAsync(
                "order_by",
                $"'{invalidField.Value.ParameterName}' is not a sortable field.");
            return PagedResult<ClientViewModel>.Empty();
        }

        var query = new PageQuery
        {
            Page = page,
            PageSize = pageSize
        };

        return await _bus.QueryAsync(new GetAllClientsQuery(query, includeDeleted, searchTerm, sortQuery));
    }

    private Task NotifyInvalidQueryAsync(string key, string message)
    {
        return _bus.RaiseEventAsync(
            new DomainNotification(key, message, ErrorCodes.ValidationFailed, new { key }));
    }
}
