using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Application.Extensions;
using CleanArchitecture.Application.ViewModels;
using CleanArchitecture.Application.ViewModels.Clients;
using CleanArchitecture.Application.ViewModels.Sorting;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Queries.Clients.GetAll;

public sealed class GetAllClientsQueryHandler :
    IRequestHandler<GetAllClientsQuery, PagedResult<ClientViewModel>>
{
    private readonly ISortingExpressionProvider<ClientViewModel, Client> _sortingExpressionProvider;
    private readonly IClientRepository _clientRepository;

    public GetAllClientsQueryHandler(
        IClientRepository clientRepository,
        ISortingExpressionProvider<ClientViewModel, Client> sortingExpressionProvider)
    {
        _clientRepository = clientRepository;
        _sortingExpressionProvider = sortingExpressionProvider;
    }

    public async Task<PagedResult<ClientViewModel>> Handle(
        GetAllClientsQuery request,
        CancellationToken cancellationToken)
    {
        var clientsQuery = _clientRepository
            .GetAllNoTracking()
            .IgnoreQueryFilters()
            .Where(client => request.IncludeDeleted || client.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            clientsQuery = clientsQuery.Where(client =>
                client.Name.Contains(request.SearchTerm) ||
                client.Email.Contains(request.SearchTerm) ||
                client.Phone.Contains(request.SearchTerm));
        }

        var totalCount = await clientsQuery.CountAsync(cancellationToken);

        var skip = ((long)request.Query.Page - 1) * request.Query.PageSize;
        if (skip > int.MaxValue)
        {
            return new PagedResult<ClientViewModel>(
                totalCount,
                Array.Empty<ClientViewModel>(),
                request.Query.Page,
                request.Query.PageSize);
        }

        clientsQuery = clientsQuery.GetOrderedQueryable(request.SortQuery, _sortingExpressionProvider);

        var clients = await clientsQuery
            .Skip((int)skip)
            .Take(request.Query.PageSize)
            .Select(client => ClientViewModel.FromClient(client))
            .ToListAsync(cancellationToken);

        return new PagedResult<ClientViewModel>(
            totalCount, clients, request.Query.Page, request.Query.PageSize);
    }
}
