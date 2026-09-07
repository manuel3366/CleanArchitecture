using System;
using System.Threading.Tasks;
using CleanArchitecture.Application.ViewModels;
using CleanArchitecture.Application.ViewModels.Clients;

namespace CleanArchitecture.Application.Interfaces;

public interface IClientService
{
    Task<PagedResult<ClientViewModel>> GetAllClientsAsync(
        PageQuery query,
        string searchTerm = "",
        string? status = null,
        DateTimeOffset? fechaRegistroDesde = null,
        DateTimeOffset? fechaRegistroHasta = null);

    Task<ClientViewModel> CreateClientAsync(CreateClientViewModel client);
}
