using System;
using System.Threading.Tasks;
using CleanArchitecture.Api.Models;
using CleanArchitecture.Api.Swagger;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.SortProviders;
using CleanArchitecture.Application.ViewModels;
using CleanArchitecture.Application.ViewModels.Clientes;
using CleanArchitecture.Application.ViewModels.Sorting;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Notifications;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CleanArchitecture.Api.Controllers;

[ApiController]
[Route("/api/v1/[controller]")]
public sealed class ClienteController : ApiController
{
    private readonly IClienteService _clienteService;

    public ClienteController(
        INotificationHandler<DomainNotification> notifications,
        IClienteService clienteService) : base(notifications)
    {
        _clienteService = clienteService;
    }

    [HttpGet]
    [SwaggerOperation("Get a paginated list of clients")]
    [SwaggerResponse(200, "Request successful", typeof(ResponseMessage<PagedResult<ClienteViewModel>>))]
    public async Task<IActionResult> GetAllClientesAsync(
        [FromQuery] PageQuery query,
        [FromQuery] string searchTerm = "",
        [FromQuery] string? estado = null,
        [FromQuery] DateTimeOffset? fechaRegistroDesde = null,
        [FromQuery] DateTimeOffset? fechaRegistroHasta = null,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] [SortableFieldsAttribute<ClienteViewModelSortProvider, ClienteViewModel, Cliente>]
        SortQuery? sortQuery = null)
    {
        var clientes = await _clienteService.GetAllClientesAsync(
            query,
            includeDeleted,
            searchTerm,
            estado,
            fechaRegistroDesde,
            fechaRegistroHasta,
            sortQuery);

        return Response(clientes);
    }
}
