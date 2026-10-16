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
public sealed class ClientUpdateControllerTests
{
    private readonly ClientTestFixture _fixture = new();

    [SetUp]
    public async Task Setup() => await _fixture.SeedTestData();

    [Test]
    public async Task Should_Update_Active_Client_Anonymously_And_Return_Only_Public_Fields()
    {
        var response = await _fixture.ServerClient.PutAsJsonAsync(
            "/api/v1/Client",
            new
            {
                id = _fixture.AlphaClientId,
                name = "Updated Client",
                email = "updated@example.test",
                phone = "555-9999",
                deletedAt = "2025-01-01T00:00:00Z",
                tenantId = Guid.NewGuid()
            });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var message = await response.Content.ReadAsJsonAsync<ClientViewModel>();
        message?.Success.ShouldBeTrue();
        message!.Data.ShouldNotBeNull();
        message.Data!.Id.ShouldBe(_fixture.AlphaClientId);
        message.Data.Name.ShouldBe("Updated Client");
        message.Data.Email.ShouldBe("updated@example.test");
        message.Data.Phone.ShouldBe("555-9999");
        message.Data.DeletedAt.ShouldBeNull();

        using var responseDocument = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var returnedClient = responseDocument.RootElement.GetProperty("data");
        returnedClient.EnumerateObject().Select(property => property.Name)
            .OrderBy(name => name)
            .ShouldBe(new[] { "deletedAt", "email", "id", "name", "phone" });

        var listResponse = await _fixture.ServerClient.GetAsync("/api/v1/Client?pageSize=100");
        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var clients = await listResponse.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();
        clients?.Data.ShouldNotBeNull();
        var persistedClient = clients!.Data!.Items.Single(client => client.Id == _fixture.AlphaClientId);
        persistedClient.Name.ShouldBe("Updated Client");
        persistedClient.Email.ShouldBe("updated@example.test");
        persistedClient.Phone.ShouldBe("555-9999");
        persistedClient.DeletedAt.ShouldBeNull();
    }

    [TestCase(null, "valid@example.test", "555-1111", "CLIENT_EMPTY_NAME")]
    [TestCase("Updated", null, "555-1111", "CLIENT_EMPTY_EMAIL")]
    [TestCase("Updated", "valid@example.test", null, "CLIENT_EMPTY_PHONE")]
    [TestCase("", "valid@example.test", "555-1111", "CLIENT_EMPTY_NAME")]
    public async Task Should_Return_Standard_Error_Envelope_And_Not_Persist_Invalid_Fields(
        string? name,
        string? email,
        string? phone,
        string expectedErrorCode)
    {
        var response = await _fixture.ServerClient.PutAsJsonAsync(
            "/api/v1/Client",
            new UpdateClientViewModel(_fixture.AlphaClientId, name, email, phone));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var body = document.RootElement;
        body.GetProperty("success").GetBoolean().ShouldBeFalse();
        body.GetProperty("errors").GetArrayLength().ShouldBeGreaterThan(0);
        body.GetProperty("detailedErrors")[0].GetProperty("code").GetString()
            .ShouldBe(expectedErrorCode);

        var listResponse = await _fixture.ServerClient.GetAsync("/api/v1/Client?pageSize=100");
        var clients = await listResponse.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();
        var client = clients!.Data!.Items.Single(item => item.Id == _fixture.AlphaClientId);
        client.Name.ShouldBe("Alpha Client");
        client.Email.ShouldBe("alpha@example.test");
        client.Phone.ShouldBe("555-1111");
    }

    [Test]
    public async Task Should_Return_Validation_Error_For_Empty_Id()
    {
        var response = await _fixture.ServerClient.PutAsJsonAsync(
            "/api/v1/Client",
            new UpdateClientViewModel(Guid.Empty, "Updated", "updated@example.test", "555-9999"));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.GetProperty("success").GetBoolean().ShouldBeFalse();
        document.RootElement.GetProperty("detailedErrors")[0].GetProperty("code").GetString()
            .ShouldBe("CLIENT_EMPTY_ID");
    }

    [Test]
    public async Task Should_Return_Not_Found_Without_Updating_Unknown_Or_Deleted_Clients()
    {
        foreach (var clientId in new[] { Guid.NewGuid(), _fixture.DeletedClientId })
        {
            var response = await _fixture.ServerClient.PutAsJsonAsync(
                "/api/v1/Client",
                new UpdateClientViewModel(clientId, "Updated", "updated@example.test", "555-9999"));

            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var body = document.RootElement;
            body.GetProperty("success").GetBoolean().ShouldBeFalse();
            body.GetProperty("detailedErrors")[0].GetProperty("code").GetString()
                .ShouldBe("OBJECT_NOT_FOUND");

            var listResponse = await _fixture.ServerClient.GetAsync(
                "/api/v1/Client?includeDeleted=true&pageSize=100");
            var clients = await listResponse.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();
            var client = clients!.Data!.Items.SingleOrDefault(item => item.Id == clientId);
            if (clientId == _fixture.DeletedClientId)
            {
                client.ShouldNotBeNull();
                client!.Name.ShouldBe("Deleted Client");
                client.Email.ShouldBe("deleted@example.test");
                client.Phone.ShouldBe("555-3333");
                client.DeletedAt.ShouldNotBeNull();
            }
            else
            {
                client.ShouldBeNull();
            }
        }
    }
}
