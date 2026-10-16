using System;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Domain.Interfaces.Repositories;

public interface IClientRepository : IRepository<Client>
{
    Task<Client?> GetByIdIncludingDeletedAsync(Guid id);
}
