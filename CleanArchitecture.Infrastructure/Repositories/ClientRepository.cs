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

    public async Task<Client?> GetByEmailAsync(string email)
    {
        return await DbSet.SingleOrDefaultAsync(client => client.Email == email);
    }

    public async Task<Client?> GetByDocumentoIdentidadAsync(string documentoIdentidad)
    {
        return await DbSet.SingleOrDefaultAsync(client => client.DocumentoIdentidad == documentoIdentidad);
    }
}
