using CleanArchitecture.Application.ViewModels;
using CleanArchitecture.Application.ViewModels.Clients;
using CleanArchitecture.Application.ViewModels.Sorting;
using MediatR;

namespace CleanArchitecture.Application.Queries.Clients.GetAll;

public sealed record GetAllClientsQuery(
    PageQuery Query,
    bool IncludeDeleted,
    string SearchTerm = "",
    SortQuery? SortQuery = null) : IRequest<PagedResult<ClientViewModel>>;
