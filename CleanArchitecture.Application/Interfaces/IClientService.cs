using System;
using System.Threading.Tasks;
using CleanArchitecture.Application.ViewModels;
using CleanArchitecture.Application.ViewModels.Clients;
using CleanArchitecture.Application.ViewModels.Sorting;

namespace CleanArchitecture.Application.Interfaces;

public interface IClientService
{
    Task<Guid> CreateClientAsync(CreateClientViewModel client);

    Task<PagedResult<ClientViewModel>> GetAllClientsAsync(
        PageQuery query,
        bool includeDeleted,
        string searchTerm = "",
        SortQuery? sortQuery = null);
}
