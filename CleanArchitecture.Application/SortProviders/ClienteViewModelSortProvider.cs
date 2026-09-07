using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CleanArchitecture.Application.ViewModels.Clientes;
using CleanArchitecture.Application.ViewModels.Sorting;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.SortProviders;

public sealed class ClienteViewModelSortProvider : ISortingExpressionProvider<ClienteViewModel, Cliente>
{
    private static readonly Dictionary<string, Expression<Func<Cliente, object>>> s_expressions = new()
    {
        { "id", cliente => cliente.Id },
        { "nombres", cliente => cliente.Nombres },
        { "apellidos", cliente => cliente.Apellidos },
        { "documentoidentidad", cliente => cliente.DocumentoIdentidad },
        { "email", cliente => cliente.Email },
        { "estado", cliente => cliente.Estado },
        { "fecharegistro", cliente => cliente.FechaRegistro },
        { "fechaactualizacion", cliente => cliente.FechaActualizacion }
    };

    public Dictionary<string, Expression<Func<Cliente, object>>> GetSortingExpressions()
    {
        return s_expressions;
    }
}
