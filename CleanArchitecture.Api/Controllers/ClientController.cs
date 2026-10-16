using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Api.Models;
using CleanArchitecture.Api.Swagger;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.SortProviders;
using CleanArchitecture.Application.ViewModels;
using CleanArchitecture.Application.ViewModels.Clients;
using CleanArchitecture.Application.ViewModels.Sorting;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Notifications;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanArchitecture.Api.Controllers;

[ApiController]
[Route("/api/v1/[controller]")]
public sealed class ClientController : ApiController
{
    private static readonly HashSet<string> s_sortableFields =
        new(new ClientViewModelSortProvider().GetSortingExpressions().Keys);

    private readonly IClientService _clientService;

    public ClientController(
        INotificationHandler<DomainNotification> notifications,
        IClientService clientService) : base(notifications)
    {
        _clientService = clientService;
    }

    [HttpGet]
    [SwaggerOperation(
        "Get a paginated list of clients",
        Description = "Query parameters: page (default 1, minimum 1), pageSize (default 10, range 1-100), " +
                      "searchTerm (partial match on name, email, or phone), includeDeleted (default false), " +
                      "and order_by (id, name, email, phone, or deletedAt; supports the existing SortQuery syntax).")]
    [SwaggerResponse(200, "Request successful", typeof(ResponseMessage<PagedResult<ClientViewModel>>))]
    public async Task<IActionResult> GetAllClientsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string searchTerm = "",
        [FromQuery] bool includeDeleted = false,
        [FromQuery]
        [SortableFieldsAttribute<ClientViewModelSortProvider, ClientViewModel, Client>]
        SortQuery? sortQuery = null)
    {
        if (page < 1)
        {
            return await ValidationErrorAsync("page", "Page must be at least 1.");
        }

        if (pageSize is < 1 or > 100)
        {
            return await ValidationErrorAsync("pageSize", "Page size must be between 1 and 100.");
        }

        if (sortQuery is not null &&
            sortQuery.Parameters.Any(parameter => !s_sortableFields.Contains(parameter.ParameterName)))
        {
            return await ValidationErrorAsync(
                "order_by",
                $"Sortable fields are limited to: {string.Join(", ", s_sortableFields.OrderBy(field => field))}.");
        }

        var query = new PageQuery
        {
            Page = page,
            PageSize = pageSize
        };

        var clients = await _clientService.GetAllClientsAsync(
            query,
            includeDeleted,
            searchTerm,
            sortQuery);

        return Response(clients);
    }

    [HttpPost]
    [SwaggerOperation("Create a new client")]
    [SwaggerResponse(200, "Request successful", typeof(ResponseMessage<Guid>))]
    public async Task<IActionResult> CreateClientAsync(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CreateClientViewModel? viewModel)
    {
        if (viewModel is null)
        {
            return await ValidationErrorAsync("body", "Request body is required.");
        }

        var clientId = await _clientService.CreateClientAsync(viewModel);
        return Response(clientId);
    }
}
