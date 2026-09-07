using System;
using CleanArchitecture.Application.ViewModels;
using CleanArchitecture.Application.ViewModels.Clients;
using MediatR;

namespace CleanArchitecture.Application.Queries.Clients.GetAll;

public sealed record GetAllClientsQuery(
    PageQuery Query,
    string SearchTerm = "",
    string? Status = null,
    DateTimeOffset? FechaRegistroDesde = null,
    DateTimeOffset? FechaRegistroHasta = null) :
    IRequest<PagedResult<ClientViewModel>>;
