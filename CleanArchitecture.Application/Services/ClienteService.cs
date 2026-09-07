using System;
using System.Threading.Tasks;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Queries.Clientes.GetAll;
using CleanArchitecture.Application.ViewModels;
using CleanArchitecture.Application.ViewModels.Clientes;
using CleanArchitecture.Application.ViewModels.Sorting;
using CleanArchitecture.Domain.Interfaces;

namespace CleanArchitecture.Application.Services;

public sealed class ClienteService : IClienteService
{
    private readonly IMediatorHandler _bus;

    public ClienteService(IMediatorHandler bus)
    {
        _bus = bus;
    }

    public async Task<PagedResult<ClienteViewModel>> GetAllClientesAsync(
        PageQuery query,
        bool includeDeleted,
        string searchTerm = "",
        string? estado = null,
        DateTimeOffset? fechaRegistroDesde = null,
        DateTimeOffset? fechaRegistroHasta = null,
        SortQuery? sortQuery = null)
    {
        return await _bus.QueryAsync(new GetAllClientesQuery(
            query,
            includeDeleted,
            searchTerm,
            estado,
            fechaRegistroDesde,
            fechaRegistroHasta,
            sortQuery));
    }
}
