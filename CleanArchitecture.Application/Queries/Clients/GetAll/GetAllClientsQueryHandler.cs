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
    private readonly IClientRepository _clientRepository;
    private readonly ISortingExpressionProvider<ClientViewModel, Client> _sortingExpressionProvider;

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
        clientsQuery = clientsQuery.GetOrderedQueryable(
            request.SortQuery,
            _sortingExpressionProvider.GetSortingExpressions());

        var offset = (long)(request.Query.Page - 1) * request.Query.PageSize;
        var clients = offset > int.MaxValue
            ? new System.Collections.Generic.List<ClientViewModel>()
            : await clientsQuery
                .Skip((int)offset)
                .Take(request.Query.PageSize)
                .Select(client => new ClientViewModel
                {
                    Id = client.Id,
                    Name = client.Name,
                    Email = client.Email,
                    Phone = client.Phone,
                    DeletedAt = client.DeletedAt
                })
                .ToListAsync(cancellationToken);

        return new PagedResult<ClientViewModel>(
            totalCount, clients, request.Query.Page, request.Query.PageSize);
    }
}
