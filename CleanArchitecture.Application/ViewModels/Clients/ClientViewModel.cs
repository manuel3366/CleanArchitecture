using System;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.ViewModels.Clients;

public sealed class ClientViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTimeOffset? DeletedAt { get; set; }

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
