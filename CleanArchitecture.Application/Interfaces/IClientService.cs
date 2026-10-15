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
        int page,
        int pageSize,
        bool includeDeleted,
        string searchTerm = "",
        SortQuery? sortQuery = null);
}
