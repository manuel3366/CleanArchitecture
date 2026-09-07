using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CleanArchitecture.Application.ViewModels;
using CleanArchitecture.Application.ViewModels.Clients;
using CleanArchitecture.IntegrationTests.Extensions;
using CleanArchitecture.IntegrationTests.Fixtures;
using Shouldly;

namespace CleanArchitecture.IntegrationTests.Controller;

[NonParallelizable]
public sealed class ClientControllerTests
{
    private readonly ClientTestFixture _fixture = new();

    [OneTimeSetUp]
    public async Task Setup() => await _fixture.SeedTestData();

    [Test]
    public async Task Should_Get_All_Clients_With_Filters_And_Pagination()
    {
        var response = await _fixture.ServerClient.GetAsync(
            $"/api/v1/Client?searchTerm=ana&status=Activo&fechaRegistroDesde=2024-01-01&fechaRegistroHasta=2024-12-31&page=1&pageSize=10");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var message = await response.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();

        message.ShouldNotBeNull();
        message!.Data.ShouldNotBeNull();
        message.Data!.Items.ShouldNotBeEmpty();
        message.Data.Items.Count.ShouldBe(1);
        message.Data.Items.First().Nombres.ShouldBe("Ana");
        message.Data.Items.First().Estado.ShouldBe("Activo");
    }

    [Test]
    public async Task Should_Get_Paged_Clients_Without_Filters()
    {
        var response = await _fixture.ServerClient.GetAsync("/api/v1/Client?page=1&pageSize=2");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var message = await response.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();

        message.ShouldNotBeNull();
        message!.Data.ShouldNotBeNull();
        message.Data!.Items.Count.ShouldBe(2);
        message.Data.Count.ShouldBeGreaterThanOrEqualTo(2);
        message.Data.Page.ShouldBe(1);
        message.Data.PageSize.ShouldBe(2);
    }

    [Test]
    public async Task Should_Return_Empty_When_No_Client_Matches_Filters()
    {
        var response = await _fixture.ServerClient.GetAsync(
            "/api/v1/Client?searchTerm=NoExiste&status=Activo&page=1&pageSize=10");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var message = await response.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();

        message.ShouldNotBeNull();
        message!.Data.ShouldNotBeNull();
        message.Data!.Items.ShouldBeEmpty();
        message.Data.Count.ShouldBe(0);
    }

    [Test]
    public async Task Should_Create_Client_When_Request_Is_Valid()
    {
        var request = new CreateClientViewModel(
            "Carlos",
            "Pérez",
            "99988777",
            "carlos.perez@test.com",
            "04145556600",
            "Activo");

        var response = await _fixture.ServerClient.PostAsJsonAsync("/api/v1/Client", request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var message = await response.Content.ReadAsJsonAsync<ClientViewModel>();

        message.ShouldNotBeNull();
        message!.Data.ShouldNotBeNull();
        message.Data!.Nombres.ShouldBe("Carlos");
        message.Data.Apellidos.ShouldBe("Pérez");
        message.Data.DocumentoIdentidad.ShouldBe("99988777");
        message.Data.Email.ShouldBe("carlos.perez@test.com");
        message.Data.Estado.ShouldBe("Activo");
        message.Data.FechaRegistro.ShouldNotBe(default);
        message.Data.FechaActualizacion.ShouldNotBe(default);
    }

    [Test]
    public async Task Should_Not_Create_Client_When_Email_Is_Invalid()
    {
        var request = new CreateClientViewModel(
            "Pedro",
            "Gómez",
            "12398765",
            "correo-no-valido",
            null,
            "Activo");

        var response = await _fixture.ServerClient.PostAsJsonAsync("/api/v1/Client", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var message = await response.Content.ReadAsJsonAsync<ClientViewModel>();
        message.ShouldNotBeNull();
        message!.Data.ShouldBeNull();
    }
}

