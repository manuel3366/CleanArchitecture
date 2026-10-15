using System;

namespace CleanArchitecture.Domain.Entities;

public class Client : Entity
{
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }

    public Client(Guid id, string name, string email, string phone) : base(id)
    {
        Name = name;
        Email = email;
        Phone = phone;
    }
}
