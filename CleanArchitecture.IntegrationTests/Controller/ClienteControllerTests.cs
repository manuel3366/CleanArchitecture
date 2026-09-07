using System.Net;
using System.Threading.Tasks;
using CleanArchitecture.Application.ViewModels;
using CleanArchitecture.Application.ViewModels.Clientes;
using CleanArchitecture.IntegrationTests.Extensions;
using CleanArchitecture.IntegrationTests.Fixtures;
using Shouldly;

namespace CleanArchitecture.IntegrationTests.Controller;

[NonParallelizable]
public sealed class ClienteControllerTests
{
    private readonly ClienteTestFixture _fixture = new();

    [OneTimeSetUp]
    public async Task Setup() => await _fixture.SeedTestData();

    [Test, Order(0)]
    public async Task Should_Get_All_Clientes_With_Pagination()
    {
        var response = await _fixture.ServerClient.GetAsync("/api/v1/Cliente?page=1&pageSize=2");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var message = await response.Content.ReadAsJsonAsync<PagedResult<ClienteViewModel>>();

        message?.Data.ShouldNotBeNull();
        message!.Data!.Count.ShouldBe(3);
        message.Data.Items.Count.ShouldBe(2);
        message.Data.Page.ShouldBe(1);
        message.Data.PageSize.ShouldBe(2);
    }

    [Test, Order(1)]
    public async Task Should_Filter_Clientes_By_Search_Term_And_State()
    {
        var response = await _fixture.ServerClient.GetAsync("/api/v1/Cliente?searchTerm=Garcia&estado=Activo&page=1&pageSize=10");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var message = await response.Content.ReadAsJsonAsync<PagedResult<ClienteViewModel>>();

        message?.Data.ShouldNotBeNull();
        message!.Data!.Count.ShouldBe(1);
        message.Data.Items.ShouldHaveSingleItem();
        message.Data.Items[0].Email.ShouldBe("ana.garcia@correo.com");
    }

    [Test, Order(2)]
    public async Task Should_Filter_Clientes_By_Date_Range_And_State()
    {
        var response = await _fixture.ServerClient.GetAsync(
            "/api/v1/Cliente?estado=Activo&fechaRegistroDesde=2026-02-01T00:00:00Z&fechaRegistroHasta=2026-03-31T23:59:59Z&page=1&pageSize=10");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var message = await response.Content.ReadAsJsonAsync<PagedResult<ClienteViewModel>>();

        message?.Data.ShouldNotBeNull();
        message!.Data!.Count.ShouldBe(1);
        message.Data.Items[0].Email.ShouldBe("maria.lopez@correo.com");
    }

    [Test, Order(3)]
    public async Task Should_Return_Empty_When_No_Clientes_Match_Filter()
    {
        var response = await _fixture.ServerClient.GetAsync("/api/v1/Cliente?searchTerm=NoExiste&page=1&pageSize=10");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var message = await response.Content.ReadAsJsonAsync<PagedResult<ClienteViewModel>>();

        message?.Data.ShouldNotBeNull();
        message!.Data!.Count.ShouldBe(0);
        message.Data.Items.ShouldBeEmpty();
    }
}
