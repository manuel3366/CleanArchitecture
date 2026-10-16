using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using CleanArchitecture.Application.ViewModels;
using CleanArchitecture.Application.ViewModels.Clients;
using CleanArchitecture.IntegrationTests.Extensions;
using CleanArchitecture.IntegrationTests.Fixtures;
using Shouldly;

namespace CleanArchitecture.IntegrationTests.Controller;

[NonParallelizable]
public sealed class ClientDeletionControllerTests
{
    private readonly ClientTestFixture _fixture = new();

    [SetUp]
    public async Task Setup() => await _fixture.SeedTestData();

    [Test]
    public async Task Should_Soft_Delete_Client_Anonymously_And_Preserve_Its_Public_Data()
    {
        var response = await _fixture.ServerClient.DeleteAsync(
            $"/api/v1/Client/{_fixture.AlphaClientId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var message = await response.Content.ReadAsJsonAsync<Guid>();
        message?.Success.ShouldBeTrue();
        message!.Data.ShouldBe(_fixture.AlphaClientId);

        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.ShouldNotContain("Alpha Client");
        responseBody.ShouldNotContain("alpha@example.test");
        responseBody.ShouldNotContain("555-1111");

        var activeClientsResponse = await _fixture.ServerClient.GetAsync(
            "/api/v1/Client?pageSize=100");
        activeClientsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var activeClients = await activeClientsResponse.Content
            .ReadAsJsonAsync<PagedResult<ClientViewModel>>();
        activeClients?.Data.ShouldNotBeNull();
        activeClients!.Data!.Items.Select(client => client.Id)
            .ShouldNotContain(_fixture.AlphaClientId);

        var allClientsResponse = await _fixture.ServerClient.GetAsync(
            "/api/v1/Client?includeDeleted=true&pageSize=100");
        allClientsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var allClients = await allClientsResponse.Content
            .ReadAsJsonAsync<PagedResult<ClientViewModel>>();
        allClients?.Data.ShouldNotBeNull();
        var deletedClient = allClients!.Data!.Items
            .Single(client => client.Id == _fixture.AlphaClientId);
        deletedClient.Name.ShouldBe("Alpha Client");
        deletedClient.Email.ShouldBe("alpha@example.test");
        deletedClient.Phone.ShouldBe("555-1111");
        deletedClient.DeletedAt.ShouldNotBeNull();
        deletedClient.DeletedAt!.Value.Offset.ShouldBe(TimeSpan.Zero);

        using var document = JsonDocument.Parse(responseBody);
        document.RootElement.GetProperty("data").GetGuid()
            .ShouldBe(_fixture.AlphaClientId);
    }

    [Test]
    public async Task Should_Return_Not_Found_When_Deleting_An_Unknown_Client()
    {
        var response = await _fixture.ServerClient.DeleteAsync(
            $"/api/v1/Client/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var body = document.RootElement;
        body.GetProperty("success").GetBoolean().ShouldBeFalse();
        body.GetProperty("errors").GetArrayLength().ShouldBeGreaterThan(0);
        body.GetProperty("detailedErrors")[0].GetProperty("code").GetString()
            .ShouldBe("OBJECT_NOT_FOUND");
    }

    [Test]
    public async Task Should_Reject_A_Client_Id_That_Is_Not_A_Guid()
    {
        var response = await _fixture.ServerClient.DeleteAsync("/api/v1/Client/not-a-guid");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldNotContain("Alpha Client");
        body.ShouldNotContain("alpha@example.test");
        body.ShouldNotContain("555-1111");
    }

    [Test]
    public async Task Should_Return_Not_Found_When_Deleting_The_Same_Client_Again()
    {
        var firstResponse = await _fixture.ServerClient.DeleteAsync(
            $"/api/v1/Client/{_fixture.AlphaClientId}");
        var secondResponse = await _fixture.ServerClient.DeleteAsync(
            $"/api/v1/Client/{_fixture.AlphaClientId}");

        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var responseBody = await secondResponse.Content.ReadAsStringAsync();
        responseBody.ShouldNotContain("Alpha Client");
        responseBody.ShouldNotContain("alpha@example.test");
        responseBody.ShouldNotContain("555-1111");
    }
}
