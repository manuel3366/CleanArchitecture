using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CleanArchitecture.Application.ViewModels.Clients;
using CleanArchitecture.Application.ViewModels.Sorting;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.SortProviders;

public sealed class ClientViewModelSortProvider : ISortingExpressionProvider<ClientViewModel, Client>
{
    private static readonly Dictionary<string, Expression<Func<Client, object>>> s_expressions =
        new(StringComparer.OrdinalIgnoreCase)
    {
        { "id", client => client.Id },
        { "name", client => client.Name },
        { "email", client => client.Email },
        { "phone", client => client.Phone },
        { "deletedAt", client => client.DeletedAt ?? DateTimeOffset.MinValue }
    };

    public Dictionary<string, Expression<Func<Client, object>>> GetSortingExpressions()
    {
        return s_expressions;
    }
}
