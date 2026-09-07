using System;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Application.Queries.Tenants.GetAll;
using CleanArchitecture.Application.Tests.Fixtures.Queries.Tenants;
using CleanArchitecture.Application.ViewModels;
using CleanArchitecture.Domain.Entities;
using Shouldly;
using Xunit;

namespace CleanArchitecture.Application.Tests.Queries.Tenants;

public sealed class GetAllTenantsQueryHandlerTests
{
    private readonly GetAllTenantsTestFixture _fixture = new();

    [Fact]
    public async Task Should_Get_Existing_Tenant()
    {
        var tenant = _fixture.SetupTenant();

        var query = new PageQuery
        {
            PageSize = 10,
            Page = 1
        };

        var result = await _fixture.QueryHandler.Handle(
            new GetAllTenantsQuery(query, false),
            default);

        _fixture.VerifyNoDomainNotification();

        result.PageSize.ShouldBe(query.PageSize);
        result.Page.ShouldBe(query.Page);
        result.Count.ShouldBe(1);

        var checkingTenant = result.Items.First();

        tenant.Id.ShouldBe(checkingTenant.Id);
        tenant.Name.ShouldBe(checkingTenant.Name);
        tenant.Users.Count.ShouldBe(checkingTenant.Users.Count());
    }

    [Fact]
    public async Task Should_Not_Get_Deleted_Tenant()
    {
        _fixture.SetupTenant(true);

        var query = new PageQuery
        {
            PageSize = 10,
            Page = 1
        };

        var result = await _fixture.QueryHandler.Handle(
            new GetAllTenantsQuery(query, false),
            default);

        result.PageSize.ShouldBe(query.PageSize);
        result.Page.ShouldBe(query.Page);
        result.Count.ShouldBe(0);

        result.Items.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Should_Order_Tenants_By_Id_When_No_Sort_Is_Provided()
    {
        var tenantA = new Tenant(Guid.Parse("00000000-0000-0000-0000-000000000002"), "Tenant B");
        var tenantB = new Tenant(Guid.Parse("00000000-0000-0000-0000-000000000001"), "Tenant A");
        _fixture.SetupTenants(tenantA, tenantB);

        var query = new PageQuery
        {
            PageSize = 10,
            Page = 1
        };

        var result = await _fixture.QueryHandler.Handle(
            new GetAllTenantsQuery(query, false),
            default);

        result.Count.ShouldBe(2);
        result.Items.Select(item => item.Id).ShouldBe(new[] { tenantB.Id, tenantA.Id });
    }
}