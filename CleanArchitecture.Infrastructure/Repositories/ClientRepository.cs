using System;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Interfaces.Repositories;
using CleanArchitecture.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Repositories;

public sealed class ClientRepository : BaseRepository<Client>, IClientRepository
{
    public ClientRepository(ApplicationDbContext context) : base(context)
    {
    }

    public Task<Client?> GetByIdIncludingDeletedAsync(Guid id)
    {
        return DbSet
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(client => client.Id == id);
    }
}
