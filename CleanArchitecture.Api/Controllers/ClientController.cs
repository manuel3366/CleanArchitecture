using System;
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
using Swashbuckle.AspNetCore.Annotations;

namespace CleanArchitecture.Api.Controllers;

[ApiController]
[Route("/api/v1/[controller]")]
public sealed class ClientController : ApiController
{
    private readonly IClientService _clientService;
    private readonly ISortingExpressionProvider<ClientViewModel, Client> _sortingExpressionProvider;

    public ClientController(
        INotificationHandler<DomainNotification> notifications,
        IClientService clientService,
        ISortingExpressionProvider<ClientViewModel, Client> sortingExpressionProvider) : base(notifications)
    {
        _clientService = clientService;
        _sortingExpressionProvider = sortingExpressionProvider;
    }

    [HttpPost]
    [SwaggerOperation("Create a client")]
    [SwaggerResponse(200, "Request successful", typeof(ResponseMessage<System.Guid>))]
    [SwaggerResponse(400, "Invalid client data", typeof(ResponseMessage<object>))]
    public async Task<IActionResult> CreateClientAsync([FromBody] CreateClientViewModel? viewModel)
    {
        if (viewModel is null)
        {
            return ValidationError("body", "A client request body is required.");
        }

        var clientId = await _clientService.CreateClientAsync(viewModel);
        return Response(clientId);
    }

    [HttpDelete("{id}")]
    [SwaggerOperation("Soft delete a client by id")]
    [SwaggerResponse(200, "Request successful", typeof(ResponseMessage<System.Guid>))]
    [SwaggerResponse(400, "Invalid client id", typeof(ResponseMessage<object>))]
    [SwaggerResponse(404, "Client not found", typeof(ResponseMessage<object>))]
    public async Task<IActionResult> DeleteClientAsync([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var clientId))
        {
            return ValidationError("id", "id must be a valid GUID.");
        }

        await _clientService.DeleteClientAsync(clientId);
        return Response(clientId);
    }

    [HttpGet]
    [SwaggerOperation(
        "Get a paginated list of clients",
        Description = "Supports page (default 1), pageSize (default 10, range 1-100), " +
                      "searchTerm (partial match on name, email, and phone), includeDeleted (default false), " +
                      "and order_by (id, name, email, phone, deletedAt; e.g. name asc or -name; " +
                      "multiple fields may be comma-separated).")]
    [SwaggerResponse(200, "Request successful", typeof(ResponseMessage<PagedResult<ClientViewModel>>))]
    [SwaggerResponse(400, "Invalid query parameter", typeof(ResponseMessage<object>))]
    public async Task<IActionResult> GetAllClientsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string searchTerm = "",
        [FromQuery] bool includeDeleted = false,
        [FromQuery(Name = "order_by")]
        [SortableFieldsAttribute<ClientViewModelSortProvider, ClientViewModel, Client>]
        string? orderBy = null)
    {
        if (page < 1)
        {
            return ValidationError(nameof(page), "page must be greater than or equal to 1.");
        }

        if (pageSize is < 1 or > 100)
        {
            return ValidationError(nameof(pageSize), "pageSize must be between 1 and 100.");
        }

        SortQuery? sortQuery = null;
        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            sortQuery = new SortQuery();
            try
            {
                sortQuery.Query = orderBy;
            }
            catch (ArgumentException exception)
            {
                return ValidationError("order_by", exception.Message);
            }
            catch (FormatException exception)
            {
                return ValidationError("order_by", exception.Message);
            }

            var sortableFields = _sortingExpressionProvider.GetSortingExpressions();
            var invalidField = sortQuery.Parameters
                .Select(parameter => parameter.ParameterName)
                .FirstOrDefault(parameterName => !sortableFields.Keys.Any(
                    field => string.Equals(field, parameterName, StringComparison.OrdinalIgnoreCase)));

            if (invalidField is not null)
            {
                return ValidationError(
                    "order_by",
                    $"'{invalidField}' is not sortable. Allowed fields: " +
                    string.Join(", ", sortableFields.Keys.OrderBy(field => field)) + ".");
            }
        }

        var result = await _clientService.GetAllClientsAsync(
            new PageQuery { Page = page, PageSize = pageSize },
            includeDeleted,
            searchTerm,
            sortQuery);
        return Response(result);
    }
}
