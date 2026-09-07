using System;
using System.Threading.Tasks;
using CleanArchitecture.Application.ViewModels;
using CleanArchitecture.Application.ViewModels.Clientes;
using CleanArchitecture.Application.ViewModels.Sorting;

namespace CleanArchitecture.Application.Interfaces;

public interface IClienteService
{
    public Task<PagedResult<ClienteViewModel>> GetAllClientesAsync(
        PageQuery query,
        bool includeDeleted,
        string searchTerm = "",
        string? estado = null,
        DateTimeOffset? fechaRegistroDesde = null,
        DateTimeOffset? fechaRegistroHasta = null,
        SortQuery? sortQuery = null);
}
