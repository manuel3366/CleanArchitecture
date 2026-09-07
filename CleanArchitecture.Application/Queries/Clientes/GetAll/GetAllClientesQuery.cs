using System;
using CleanArchitecture.Application.ViewModels;
using CleanArchitecture.Application.ViewModels.Clientes;
using CleanArchitecture.Application.ViewModels.Sorting;
using MediatR;

namespace CleanArchitecture.Application.Queries.Clientes.GetAll;

public sealed record GetAllClientesQuery(
    PageQuery Query,
    bool IncludeDeleted,
    string SearchTerm = "",
    string? Estado = null,
    DateTimeOffset? FechaRegistroDesde = null,
    DateTimeOffset? FechaRegistroHasta = null,
    SortQuery? SortQuery = null) :
    IRequest<PagedResult<ClienteViewModel>>;
