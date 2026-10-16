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
using Microsoft.AspNetCore.Mvc.ModelBinding;
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

    [HttpPost]
    [SwaggerOperation("Create a new client")]
    [SwaggerResponse(200, "Request successful", typeof(ResponseMessage<Guid>))]
    [SwaggerResponse(400, "The client data is invalid", typeof(ResponseMessage<object>))]
    public async Task<IActionResult> CreateClientAsync(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CreateClientViewModel? client)
    {
        var clientId = await _clientService.CreateClientAsync(client);
        return Response(clientId);
    }

    [HttpPut]
    [SwaggerOperation("Update an existing client")]
    [SwaggerResponse(200, "Request successful", typeof(ResponseMessage<ClientViewModel>))]
    [SwaggerResponse(400, "The client data is invalid", typeof(ResponseMessage<object>))]
    [SwaggerResponse(404, "The client was not found", typeof(ResponseMessage<object>))]
    public async Task<IActionResult> UpdateClientAsync(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] UpdateClientViewModel? client)
    {
        var updatedClient = await _clientService.UpdateClientAsync(client);
        return Response(updatedClient);
    }

    [HttpDelete("{id}")]
    [SwaggerOperation("Delete an existing client")]
    [SwaggerResponse(200, "Request successful", typeof(ResponseMessage<Guid>))]
    [SwaggerResponse(404, "The client was not found", typeof(ResponseMessage<object>))]
    public async Task<IActionResult> DeleteClientAsync([FromRoute] Guid id)
    {
        await _clientService.DeleteClientAsync(id);
        return Response(id);
    }

    [HttpGet]
    [SwaggerOperation("Get a paginated list of clients")]
    [SwaggerResponse(200, "Request successful", typeof(ResponseMessage<PagedResult<ClientViewModel>>))]
    public async Task<IActionResult> GetAllClientsAsync(
        [SwaggerParameter("Page number starting at 1. Defaults to 1; values must be at least 1.")]
        [FromQuery] int page = 1,
        [SwaggerParameter("Number of items per page. Defaults to 10; accepted range is 1 to 100.")]
        [FromQuery] int pageSize = 10,
        [SwaggerParameter("Optional partial match against client name, email, or phone.")]
        [FromQuery] string searchTerm = "",
        [SwaggerParameter("Include soft-deleted clients when true. Defaults to false.")]
        [FromQuery] bool includeDeleted = false,
        [SwaggerParameter("Sort fields using order_by, for example 'name asc' or '-name'.")]
        [FromQuery]
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
}
