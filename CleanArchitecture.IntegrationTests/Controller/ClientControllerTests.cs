using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CleanArchitecture.Api.Models;
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
    private static readonly DateTimeOffset s_requestedDeletedAt =
        new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [OneTimeSetUp]
    public async Task Setup() => await _fixture.SeedTestData();

    [Test]
    public async Task Should_Create_Client_Anonymously_And_Ignore_Server_Managed_Fields()
    {
        var requestedId = Guid.NewGuid();
        using var request = new StringContent(
            JsonSerializer.Serialize(new
            {
                name = "New Client",
                email = "new.client@example.test",
                phone = "555-0199",
                id = requestedId,
                deletedAt = s_requestedDeletedAt
            }),
            Encoding.UTF8,
            "application/json");

        try
        {
            using var response = await _fixture.ServerClient.PostAsync("/api/v1/Client", request);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var envelope = await response.Content.ReadAsJsonAsync<Guid>();

            envelope.ShouldNotBeNull();
            envelope!.Success.ShouldBeTrue();
            envelope.Data.ShouldNotBe(Guid.Empty);
            envelope.Data.ShouldNotBe(requestedId);

            using var clientsResponse = await _fixture.ServerClient.GetAsync("/api/v1/Client?searchTerm=new.client");
            var clients = await clientsResponse.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();

            clientsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
            clients?.Data.ShouldNotBeNull();
            var createdClient = clients!.Data!.Items.ShouldHaveSingleItem();
            createdClient.Id.ShouldBe(envelope.Data);
            createdClient.Name.ShouldBe("New Client");
            createdClient.Email.ShouldBe("new.client@example.test");
            createdClient.Phone.ShouldBe("555-0199");
            createdClient.DeletedAt.ShouldBeNull();
        }
        finally
        {
            await _fixture.SeedTestData();
        }
    }

    [Test]
    public async Task Should_Not_Impose_Email_Uniqueness_Without_A_Client_Domain_Rule()
    {
        using var request = new StringContent(
            JsonSerializer.Serialize(new
            {
                name = "Duplicate Email Client",
                email = "alana.rivera@example.test",
                phone = "555-0198"
            }),
            Encoding.UTF8,
            "application/json");

        try
        {
            using var response = await _fixture.ServerClient.PostAsync("/api/v1/Client", request);
            var envelope = await response.Content.ReadAsJsonAsync<Guid>();

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            envelope.ShouldNotBeNull();
            envelope!.Success.ShouldBeTrue();

            using var clientsResponse = await _fixture.ServerClient.GetAsync(
                "/api/v1/Client?searchTerm=alana.rivera%40example.test");
            var clients = await clientsResponse.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();

            clientsResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
            clients?.Data.ShouldNotBeNull();
            clients!.Data!.Count.ShouldBe(2);
        }
        finally
        {
            await _fixture.SeedTestData();
        }
    }

    [TestCase("{\"name\":\"\",\"email\":\"client@example.test\",\"phone\":\"555-0100\"}", "CLIENT_EMPTY_NAME")]
    [TestCase("{\"name\":\"Client\",\"email\":\"\",\"phone\":\"555-0100\"}", "CLIENT_EMPTY_EMAIL")]
    [TestCase("{\"name\":\"Client\",\"email\":\"not-an-email\",\"phone\":\"555-0100\"}", "CLIENT_INVALID_EMAIL")]
    [TestCase("{\"name\":\"Client\",\"email\":\"client@example.test\",\"phone\":\"\"}", "CLIENT_EMPTY_PHONE")]
    public async Task Should_Return_Validation_Envelope_For_Invalid_Client(
        string json,
        string expectedErrorCode)
    {
        using var request = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _fixture.ServerClient.PostAsync("/api/v1/Client", request);
        var envelope = await response.Content.ReadAsJsonAsync<object>();

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        envelope.ShouldNotBeNull();
        envelope!.Success.ShouldBeFalse();
        envelope.Errors.ShouldNotBeNull();
        envelope.Errors.ShouldNotBeEmpty();
        envelope.DetailedErrors.ShouldContain(error => error.Code == expectedErrorCode);
    }

    [Test]
    public async Task Should_List_Clients_Anonymously_With_Default_Paging_And_Only_Public_Fields()
    {
        var response = await _fixture.ServerClient.GetAsync("/api/v1/Client");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var envelope = await response.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();

        envelope.ShouldNotBeNull();
        envelope!.Success.ShouldBeTrue();
        envelope.Data.ShouldNotBeNull();
        envelope.Data!.Count.ShouldBe(3);
        envelope.Data.Page.ShouldBe(1);
        envelope.Data.PageSize.ShouldBe(10);
        envelope.Data.Items.Count.ShouldBe(3);
        envelope.Data.Items.ShouldNotContain(client => client.Id == _fixture.DeletedClientId);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var item = document.RootElement.GetProperty("data").GetProperty("items")[0];
        item.EnumerateObject().Select(property => property.Name)
            .OrderBy(name => name)
            .ShouldBe(new[] { "deletedAt", "email", "id", "name", "phone" });
    }

    [Test]
    public async Task Should_Return_Requested_Page_And_Unpaged_Count()
    {
        var response = await _fixture.ServerClient.GetAsync(
            "/api/v1/Client?page=2&pageSize=2&order_by=name asc");
        var envelope = await response.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        envelope?.Data.ShouldNotBeNull();
        envelope!.Data!.Count.ShouldBe(3);
        envelope.Data.Page.ShouldBe(2);
        envelope.Data.PageSize.ShouldBe(2);
        envelope.Data.Items.ShouldHaveSingleItem();
        envelope.Data.Items.Single().Id.ShouldBe(_fixture.MiloClientId);
    }

    [Test]
    public async Task Should_Return_Empty_Items_For_Page_Beyond_The_End()
    {
        var response = await _fixture.ServerClient.GetAsync("/api/v1/Client?page=100&pageSize=1");
        var envelope = await response.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        envelope?.Data.ShouldNotBeNull();
        envelope!.Data!.Count.ShouldBe(3);
        envelope.Data.Page.ShouldBe(100);
        envelope.Data.PageSize.ShouldBe(1);
        envelope.Data.Items.ShouldBeEmpty();
    }

    [TestCase("lic", "Alicia Stone")]
    [TestCase("chen@", "Milo Chen")]
    [TestCase("0103", "Alicia Stone")]
    public async Task Should_Search_Name_Email_And_Phone(string searchTerm, string expectedName)
    {
        var response = await _fixture.ServerClient.GetAsync(
            $"/api/v1/Client?searchTerm={searchTerm}");
        var envelope = await response.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        envelope?.Data.ShouldNotBeNull();
        envelope!.Data!.Count.ShouldBe(1);
        envelope.Data.Items.Single().Name.ShouldBe(expectedName);
    }

    [Test]
    public async Task Should_Include_Deleted_Clients_Without_Authentication()
    {
        var response = await _fixture.ServerClient.GetAsync("/api/v1/Client?includeDeleted=true");
        var envelope = await response.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        envelope?.Data.ShouldNotBeNull();
        envelope!.Data!.Count.ShouldBe(4);
        envelope.Data.Items.ShouldContain(client => client.Id == _fixture.DeletedClientId);
        envelope.Data.Items.Single(client => client.Id == _fixture.DeletedClientId)
            .DeletedAt.ShouldNotBeNull();
    }

    [Test]
    public async Task Should_Apply_Sorting()
    {
        var response = await _fixture.ServerClient.GetAsync(
            "/api/v1/Client?order_by=name desc,email asc");
        var envelope = await response.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        envelope?.Data.ShouldNotBeNull();
        envelope!.Data!.Items.Select(client => client.Name)
            .ShouldBe(new[] { "Milo Chen", "Alicia Stone", "Alana Rivera" });
    }

    [TestCase("id")]
    [TestCase("name")]
    [TestCase("email")]
    [TestCase("phone")]
    [TestCase("deletedAt")]
    public async Task Should_Accept_Only_Documented_Sort_Fields(string sortField)
    {
        var response = await _fixture.ServerClient.GetAsync($"/api/v1/Client?order_by={sortField}");
        var envelope = await response.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        envelope?.Success.ShouldBeTrue();
        envelope?.Data.ShouldNotBeNull();
    }

    [TestCase(1)]
    [TestCase(100)]
    public async Task Should_Accept_Page_Size_Boundaries(int pageSize)
    {
        var response = await _fixture.ServerClient.GetAsync($"/api/v1/Client?pageSize={pageSize}");
        var envelope = await response.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        envelope?.Data.ShouldNotBeNull();
        envelope!.Data!.PageSize.ShouldBe(pageSize);
    }

    [TestCase("page=0")]
    [TestCase("pageSize=0")]
    [TestCase("pageSize=101")]
    [TestCase("order_by=tenantId")]
    public async Task Should_Return_Validation_Envelope_For_Invalid_Query(string query)
    {
        var response = await _fixture.ServerClient.GetAsync($"/api/v1/Client?{query}");
        var envelope = await response.Content.ReadAsJsonAsync<ResponseMessage<object>>();

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        envelope.ShouldNotBeNull();
        envelope!.Success.ShouldBeFalse();
        envelope.Errors.ShouldNotBeNull();
        envelope.Errors.ShouldNotBeEmpty();
        envelope.DetailedErrors.ShouldNotBeEmpty();
        var body = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);
        document.RootElement.GetProperty("success").GetBoolean().ShouldBeFalse();
    }
}
