using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Application.Extensions;
using CleanArchitecture.Application.SortProviders;
using CleanArchitecture.Application.ViewModels;
using CleanArchitecture.Application.ViewModels.Clients;
using CleanArchitecture.Application.ViewModels.Sorting;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Errors;
using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Domain.Interfaces.Repositories;
using CleanArchitecture.Domain.Notifications;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Queries.Clients.GetAll;

public sealed class GetAllClientsQueryHandler :
    IRequestHandler<GetAllClientsQuery, PagedResult<ClientViewModel>>
{
    private readonly IClientRepository _clientRepository;
    private readonly ISortingExpressionProvider<ClientViewModel, Client> _sortingExpressionProvider;
    private readonly IMediatorHandler _bus;

    public GetAllClientsQueryHandler(
        IClientRepository clientRepository,
        ISortingExpressionProvider<ClientViewModel, Client> sortingExpressionProvider,
        IMediatorHandler bus)
    {
        _clientRepository = clientRepository;
        _sortingExpressionProvider = sortingExpressionProvider;
        _bus = bus;
    }

    public async Task<PagedResult<ClientViewModel>> Handle(
        GetAllClientsQuery request,
        CancellationToken cancellationToken)
    {
        var sortingExpressions = _sortingExpressionProvider.GetSortingExpressions();
        if (request.Page < 1)
        {
            await NotifyInvalidQueryAsync("Page must be at least 1.");
            return EmptyResult(request);
        }

        if (request.PageSize is < 1 or > 100)
        {
            await NotifyInvalidQueryAsync("Page size must be between 1 and 100.");
            return EmptyResult(request);
        }

        if (request.SortQuery is not null &&
            request.SortQuery.Parameters.Any(parameter =>
                !sortingExpressions.ContainsKey(parameter.ParameterName)))
        {
            await NotifyInvalidQueryAsync("The requested sort field is not supported.");
            return EmptyResult(request);
        }

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
        var skip = (long)(request.Page - 1) * request.PageSize;

        if (skip >= totalCount)
        {
            return new PagedResult<ClientViewModel>(
                totalCount,
                Array.Empty<ClientViewModel>(),
                request.Page,
                request.PageSize);
        }

        var clients = await clientsQuery
            .GetOrderedQueryable(request.SortQuery, sortingExpressions)
            .Skip((int)skip)
            .Take(request.PageSize)
            .Select(client => ClientViewModel.FromClient(client))
            .ToListAsync(cancellationToken);

        return new PagedResult<ClientViewModel>(
            totalCount,
            clients,
            request.Page,
            request.PageSize);
    }

    private async Task NotifyInvalidQueryAsync(string message)
    {
        await _bus.RaiseEventAsync(new DomainNotification(
            nameof(GetAllClientsQuery),
            message,
            DomainErrorCodes.Client.InvalidQuery));
    }

    private static PagedResult<ClientViewModel> EmptyResult(GetAllClientsQuery request)
    {
        return new PagedResult<ClientViewModel>(
            0,
            Array.Empty<ClientViewModel>(),
            request.Page,
            request.PageSize);
    }
}
