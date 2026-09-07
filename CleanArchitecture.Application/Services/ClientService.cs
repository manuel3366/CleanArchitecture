using System;
using System.Threading.Tasks;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Queries.Clients.GetAll;
using CleanArchitecture.Application.ViewModels;
using CleanArchitecture.Application.ViewModels.Clients;
using CleanArchitecture.Domain;
using CleanArchitecture.Domain.Commands.Clients.CreateClient;
using CleanArchitecture.Domain.Interfaces;

namespace CleanArchitecture.Application.Services;

public sealed class ClientService : IClientService
{
    private readonly IMediatorHandler _bus;

    public ClientService(IMediatorHandler bus)
    {
        _bus = bus;
    }

    public async Task<PagedResult<ClientViewModel>> GetAllClientsAsync(
        PageQuery query,
        string searchTerm = "",
        string? status = null,
        DateTimeOffset? fechaRegistroDesde = null,
        DateTimeOffset? fechaRegistroHasta = null)
    {
        return await _bus.QueryAsync(new GetAllClientsQuery(
            query,
            searchTerm,
            status,
            fechaRegistroDesde,
            fechaRegistroHasta));
    }

    public async Task<ClientViewModel> CreateClientAsync(CreateClientViewModel client)
    {
        var clientId = Guid.NewGuid();
        var fechaRegistro = client.FechaRegistro ?? DateTimeOffset.UtcNow;
        var fechaActualizacion = client.FechaActualizacion ?? fechaRegistro;

        await _bus.SendCommandAsync(new CreateClientCommand(
            clientId,
            client.Nombres,
            client.Apellidos,
            client.DocumentoIdentidad,
            client.Email,
            client.Telefono,
            client.Estado,
            fechaRegistro,
            fechaActualizacion));

        return new ClientViewModel
        {
            Id = clientId,
            Nombres = client.Nombres,
            Apellidos = client.Apellidos,
            DocumentoIdentidad = client.DocumentoIdentidad,
            Email = client.Email,
            Telefono = client.Telefono,
            Estado = client.Estado,
            FechaRegistro = fechaRegistro,
            FechaActualizacion = fechaActualizacion
        };
    }
}
