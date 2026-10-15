using System;
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
using Swashbuckle.AspNetCore.Annotations;

namespace CleanArchitecture.Api.Controllers;

[ApiController]
[Route("/api/v1/[controller]")]
public sealed class ClientController : ApiController
{
    private readonly IClientService _clientService;

    public ClientController(
        INotificationHandler<DomainNotification> notifications,
        IClientService clientService) : base(notifications)
    {
        _clientService = clientService;
    }

    [HttpGet]
    [SwaggerOperation("Get a paginated list of clients")]
    [SwaggerResponse(200, "Request successful", typeof(ResponseMessage<PagedResult<ClientViewModel>>))]
    public async Task<IActionResult> GetAllClientsAsync(
        [FromQuery] [SwaggerParameter(Description = "Page number, starting at 1 (default: 1).")]
        int page = 1,
        [FromQuery] [SwaggerParameter(Description = "Number of clients per page, from 1 to 100 (default: 10).")]
        int pageSize = 10,
        [FromQuery] [SwaggerParameter(Description = "Partial match against name, email, or phone.")]
        string searchTerm = "",
        [FromQuery] [SwaggerParameter(Description = "Include soft-deleted clients (default: false).")]
        bool includeDeleted = false,
        [FromQuery]
        [SwaggerParameter(Description = "Sort fields using name asc or -name; multiple comma-separated fields are supported.")]
        [SortableFieldsAttribute<ClientViewModelSortProvider, ClientViewModel, Client>]
        SortQuery? sortQuery = null)
    {
        var clients = await _clientService.GetAllClientsAsync(
            page,
            pageSize,
            includeDeleted,
            searchTerm,
            sortQuery);
        return Response(clients);
    }

    [HttpPost]
    [SwaggerOperation("Create a new client")]
    [SwaggerResponse(200, "Request successful", typeof(ResponseMessage<Guid>))]
    [SwaggerResponse(400, "Client data is invalid", typeof(ResponseMessage<object>))]
    public async Task<IActionResult> CreateClientAsync([FromBody] CreateClientViewModel viewModel)
    {
        var clientId = await _clientService.CreateClientAsync(viewModel);
        return Response(clientId);
    }
}
