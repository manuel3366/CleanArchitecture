using System;

namespace CleanArchitecture.Application.ViewModels.Clients;

public sealed record UpdateClientViewModel(
    Guid Id,
    string? Name,
    string? Email,
    string? Phone);
