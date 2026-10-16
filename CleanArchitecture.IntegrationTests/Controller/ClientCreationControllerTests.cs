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
public sealed class ClientCreationControllerTests
{
    private readonly ClientTestFixture _fixture = new();

    [SetUp]
    public async Task Setup() => await _fixture.ResetDatabaseAsync();

    [Test]
    public async Task Should_Create_Client_Anonymously_With_Server_Generated_Id_And_Null_DeletedAt()
    {
        var suppliedId = Guid.NewGuid();
        var response = await _fixture.ServerClient.PostAsJsonAsync(
            "/api/v1/Client",
            new
            {
                id = suppliedId,
                name = "New Client",
                email = "email text",
                phone = "phone text",
                deletedAt = "2025-01-01T00:00:00Z"
            });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var message = await response.Content.ReadAsJsonAsync<Guid>();
        message?.Success.ShouldBeTrue();
        message!.Data.ShouldNotBe(Guid.Empty);
        message.Data.ShouldNotBe(suppliedId);

        var listResponse = await _fixture.ServerClient.GetAsync("/api/v1/Client");
        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var clients = await listResponse.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();
        clients?.Data.ShouldNotBeNull();
        clients!.Data!.Items.ShouldHaveSingleItem();
        var createdClient = clients.Data.Items[0];
        createdClient.Id.ShouldBe(message.Data);
        createdClient.Name.ShouldBe("New Client");
        createdClient.Email.ShouldBe("email text");
        createdClient.Phone.ShouldBe("phone text");
        createdClient.DeletedAt.ShouldBeNull();

        using var document = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        var item = document.RootElement.GetProperty("data").GetProperty("items")[0];
        item.EnumerateObject().Select(property => property.Name)
            .OrderBy(name => name)
            .ShouldBe(new[] { "deletedAt", "email", "id", "name", "phone" });
    }

    [Test]
    public async Task Should_Allow_Duplicate_Email_When_No_Client_Uniqueness_Rule_Exists()
    {
        var request = new CreateClientViewModel("First", "shared@example.test", "111");
        var firstResponse = await _fixture.ServerClient.PostAsJsonAsync("/api/v1/Client", request);
        var secondResponse = await _fixture.ServerClient.PostAsJsonAsync(
            "/api/v1/Client",
            request with { Name = "Second" });

        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var first = await firstResponse.Content.ReadAsJsonAsync<Guid>();
        var second = await secondResponse.Content.ReadAsJsonAsync<Guid>();
        first?.Success.ShouldBeTrue();
        second?.Success.ShouldBeTrue();
        first!.Data.ShouldNotBe(Guid.Empty);
        second!.Data.ShouldNotBe(Guid.Empty);
        second.Data.ShouldNotBe(first.Data);

        var listResponse = await _fixture.ServerClient.GetAsync("/api/v1/Client");
        var clients = await listResponse.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();
        clients?.Data.ShouldNotBeNull();
        clients!.Data!.Count.ShouldBe(2);
    }

    [TestCase("", "valid@example.test", "555-1111", "CLIENT_EMPTY_NAME")]
    [TestCase("Client", "", "555-1111", "CLIENT_EMPTY_EMAIL")]
    [TestCase("Client", "valid@example.test", "", "CLIENT_EMPTY_PHONE")]
    [TestCase(null, "valid@example.test", "555-1111", "CLIENT_EMPTY_NAME")]
    public async Task Should_Return_Standard_Error_Envelope_For_Invalid_Fields(
        string? name,
        string? email,
        string? phone,
        string expectedErrorCode)
    {
        var response = await _fixture.ServerClient.PostAsJsonAsync(
            "/api/v1/Client",
            new CreateClientViewModel(name, email, phone));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var body = document.RootElement;
        body.GetProperty("success").GetBoolean().ShouldBeFalse();
        body.GetProperty("errors").GetArrayLength().ShouldBeGreaterThan(0);
        body.GetProperty("detailedErrors")[0].GetProperty("code").GetString()
            .ShouldBe(expectedErrorCode);

        var listResponse = await _fixture.ServerClient.GetAsync("/api/v1/Client");
        var clients = await listResponse.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();
        clients?.Data.ShouldNotBeNull();
        clients!.Data!.Count.ShouldBe(0);
    }

    [Test]
    public async Task Should_Return_Standard_Error_Envelope_For_Null_Request_Body()
    {
        var response = await _fixture.ServerClient.PostAsJsonAsync(
            "/api/v1/Client",
            (object?)null);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var body = document.RootElement;
        body.GetProperty("success").GetBoolean().ShouldBeFalse();
        body.GetProperty("errors").GetArrayLength().ShouldBeGreaterThan(0);
        body.GetProperty("detailedErrors")[0].GetProperty("code").GetString()
            .ShouldBe("CLIENT_EMPTY_NAME");
    }
}
