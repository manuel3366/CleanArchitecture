using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Application.ViewModels;
using CleanArchitecture.Application.ViewModels.Clients;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Queries.Clients.GetAll;

public sealed class GetAllClientsQueryHandler : IRequestHandler<GetAllClientsQuery, PagedResult<ClientViewModel>>
{
    private readonly IClientRepository _clientRepository;

    public GetAllClientsQueryHandler(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<PagedResult<ClientViewModel>> Handle(
        GetAllClientsQuery request,
        CancellationToken cancellationToken)
    {
        var clientsQuery = _clientRepository
            .GetAllNoTracking()
            .IgnoreQueryFilters()
            .Where(client => client.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.Trim();
            var normalizedSearchTerm = searchTerm.ToLower();
            clientsQuery = clientsQuery.Where(client =>
                client.Nombres.ToLower().Contains(normalizedSearchTerm) ||
                client.Apellidos.ToLower().Contains(normalizedSearchTerm) ||
                client.DocumentoIdentidad.ToLower().Contains(normalizedSearchTerm) ||
                client.Email.ToLower().Contains(normalizedSearchTerm));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = request.Status.Trim();
            var normalizedStatus = status.ToLower();
            clientsQuery = clientsQuery.Where(client =>
                client.Estado.ToLower() == normalizedStatus);
        }

        if (request.FechaRegistroDesde.HasValue)
        {
            clientsQuery = clientsQuery.Where(client => client.FechaRegistro >= request.FechaRegistroDesde.Value);
        }

        if (request.FechaRegistroHasta.HasValue)
        {
            clientsQuery = clientsQuery.Where(client => client.FechaRegistro <= request.FechaRegistroHasta.Value);
        }

        var totalCount = await clientsQuery.CountAsync(cancellationToken);

        clientsQuery = clientsQuery
            .OrderBy(client => client.FechaRegistro)
            .ThenBy(client => client.Id);

        var clients = await clientsQuery
            .Skip((request.Query.Page - 1) * request.Query.PageSize)
            .Take(request.Query.PageSize)
            .Select(client => ClientViewModel.FromClient(client))
            .ToListAsync(cancellationToken);

        return new PagedResult<ClientViewModel>(
            totalCount,
            clients,
            request.Query.Page,
            request.Query.PageSize);
    }
}
