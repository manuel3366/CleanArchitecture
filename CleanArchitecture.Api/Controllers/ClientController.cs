using System;
using System.Threading.Tasks;
using CleanArchitecture.Api.Models;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.ViewModels;
using CleanArchitecture.Application.ViewModels.Clients;
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
    [SwaggerOperation("Get a paginated and filtered list of clients")]
    [SwaggerResponse(200, "Request successful", typeof(ResponseMessage<PagedResult<ClientViewModel>>))]
    public async Task<IActionResult> GetAllClientsAsync(
        [FromQuery] PageQuery query,
        [FromQuery] string searchTerm = "",
        [FromQuery] string? status = null,
        [FromQuery] string? estado = null,
        [FromQuery] DateTimeOffset? fechaRegistroDesde = null,
        [FromQuery] DateTimeOffset? fechaRegistroHasta = null,
        [FromQuery] DateTimeOffset? registeredFrom = null,
        [FromQuery] DateTimeOffset? registeredTo = null,
        [FromQuery] string? textoBusqueda = null)
    {
        var effectiveSearchTerm = !string.IsNullOrWhiteSpace(searchTerm) ? searchTerm : textoBusqueda;
        var effectiveStatus = !string.IsNullOrWhiteSpace(status) ? status : estado;
        var effectiveFrom = fechaRegistroDesde ?? registeredFrom;
        var effectiveTo = fechaRegistroHasta ?? registeredTo;

        var clients = await _clientService.GetAllClientsAsync(
            query,
            effectiveSearchTerm ?? string.Empty,
            effectiveStatus,
            effectiveFrom,
            effectiveTo);

        return Response(clients);
    }

    [HttpPost]
    [SwaggerOperation("Create a new client")]
    [SwaggerResponse(200, "Request successful", typeof(ResponseMessage<ClientViewModel>))]
    public async Task<IActionResult> CreateClientAsync([FromBody] CreateClientViewModel viewModel)
    {
        var createdClient = await _clientService.CreateClientAsync(viewModel);
        return Response(createdClient);
    }
}
