using System;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.ViewModels.Clients;

public sealed class ClientViewModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public DateTimeOffset? DeletedAt { get; init; }

    public static ClientViewModel FromClient(Client client)
    {
        return new ClientViewModel
        {
            Id = client.Id,
            Name = client.Name,
            Email = client.Email,
            Phone = client.Phone,
            DeletedAt = client.DeletedAt
        };
    }
}
