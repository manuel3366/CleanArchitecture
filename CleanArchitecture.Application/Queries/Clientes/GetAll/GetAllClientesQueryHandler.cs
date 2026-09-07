using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Application.Extensions;
using CleanArchitecture.Application.ViewModels;
using CleanArchitecture.Application.ViewModels.Clientes;
using CleanArchitecture.Application.ViewModels.Sorting;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Queries.Clientes.GetAll;

public sealed class GetAllClientesQueryHandler :
    IRequestHandler<GetAllClientesQuery, PagedResult<ClienteViewModel>>
{
    private readonly IClienteRepository _clienteRepository;
    private readonly ISortingExpressionProvider<ClienteViewModel, Cliente> _sortingExpressionProvider;

    public GetAllClientesQueryHandler(
        IClienteRepository clienteRepository,
        ISortingExpressionProvider<ClienteViewModel, Cliente> sortingExpressionProvider)
    {
        _clienteRepository = clienteRepository;
        _sortingExpressionProvider = sortingExpressionProvider;
    }

    public async Task<PagedResult<ClienteViewModel>> Handle(
        GetAllClientesQuery request,
        CancellationToken cancellationToken)
    {
        var clientesQuery = _clienteRepository
            .GetAllNoTracking()
            .IgnoreQueryFilters()
            .Where(cliente => request.IncludeDeleted || cliente.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = NormalizeText(request.SearchTerm.Trim());
            clientesQuery = clientesQuery.Where(cliente =>
                ContainsNormalized(cliente.Nombres, searchTerm) ||
                ContainsNormalized(cliente.Apellidos, searchTerm) ||
                ContainsNormalized(cliente.DocumentoIdentidad, searchTerm) ||
                ContainsNormalized(cliente.Email, searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(request.Estado))
        {
            clientesQuery = clientesQuery.Where(cliente => cliente.Estado == request.Estado);
        }

        if (request.FechaRegistroDesde is not null)
        {
            clientesQuery = clientesQuery.Where(cliente => cliente.FechaRegistro >= request.FechaRegistroDesde.Value);
        }

        if (request.FechaRegistroHasta is not null)
        {
            clientesQuery = clientesQuery.Where(cliente => cliente.FechaRegistro <= request.FechaRegistroHasta.Value);
        }

        var totalCount = await clientesQuery.CountAsync(cancellationToken);

        if (request.SortQuery is null || !request.SortQuery.Parameters.Any())
        {
            clientesQuery = clientesQuery.OrderByDescending(cliente => cliente.FechaRegistro)
                .ThenBy(cliente => cliente.Id);
        }
        else
        {
            clientesQuery = clientesQuery.GetOrderedQueryable(request.SortQuery, _sortingExpressionProvider);
        }

        var clientes = await clientesQuery
            .Skip((request.Query.Page - 1) * request.Query.PageSize)
            .Take(request.Query.PageSize)
            .Select(cliente => ClienteViewModel.FromCliente(cliente))
            .ToListAsync(cancellationToken);

        return new PagedResult<ClienteViewModel>(
            totalCount,
            clientes,
            request.Query.Page,
            request.Query.PageSize);
    }

    private static string NormalizeText(string value)
    {
        var normalized = value.Trim();
        var builder = new StringBuilder();

        foreach (var character in normalized.Normalize(NormalizationForm.FormD))
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        return builder.ToString();
    }

    private static bool ContainsNormalized(string? value, string normalizedSearchTerm)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return NormalizeText(value).Contains(normalizedSearchTerm, StringComparison.Ordinal);
    }
}
