using System;
using System.Linq;
using System.Net.Http;
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
public sealed class ClientControllerTests
{
    private readonly ClientTestFixture _fixture = new();

    [SetUp]
    public async Task Setup() => await _fixture.SeedTestData();

    [Test]
    public async Task Should_Create_Client_Anonymously_And_Ignore_Server_Managed_Fields()
    {
        var suppliedId = Guid.NewGuid();
        var deletedAt = DateTimeOffset.UtcNow;
        var request = new
        {
            name = "New client",
            email = "new-client@example.com",
            phone = "not constrained",
            id = suppliedId,
            deletedAt
        };

        var response = await _fixture.ServerClient.PostAsJsonAsync("/api/v1/Client", request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var message = await response.Content.ReadAsJsonAsync<Guid?>();
        message.ShouldNotBeNull();
        message!.Success.ShouldBeTrue();
        message.Data.ShouldNotBeNull();
        var createdClientId = message.Data!.Value;
        createdClientId.ShouldNotBe(Guid.Empty);
        createdClientId.ShouldNotBe(suppliedId);

        var listResponse = await _fixture.ServerClient.GetAsync(
            $"/api/v1/Client?searchTerm={Uri.EscapeDataString(request.name)}");
        listResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var list = await listResponse.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();
        var createdClient = list!.Data!.Items.ShouldHaveSingleItem();
        createdClient.Id.ShouldBe(createdClientId);
        createdClient.Name.ShouldBe(request.name);
        createdClient.Email.ShouldBe(request.email);
        createdClient.Phone.ShouldBe(request.phone);
        createdClient.DeletedAt.ShouldBeNull();
    }

    [TestCase("", "email", "phone", "CLIENT_EMPTY_NAME")]
    [TestCase("name", null, "phone", "CLIENT_EMPTY_EMAIL")]
    [TestCase("name", "email", " ", "CLIENT_EMPTY_PHONE")]
    public async Task Should_Return_Standard_Validation_Envelope_For_Invalid_Fields(
        string? name,
        string? email,
        string? phone,
        string errorCode)
    {
        var response = await _fixture.ServerClient.PostAsJsonAsync(
            "/api/v1/Client",
            new CreateClientViewModel
            {
                Name = name,
                Email = email,
                Phone = phone
            });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var message = await response.Content.ReadAsJsonAsync<Guid?>();
        message.ShouldNotBeNull();
        message!.Success.ShouldBeFalse();
        message.Errors.ShouldNotBeNull();
        message.Errors.ShouldNotBeEmpty();
        message.DetailedErrors.ShouldContain(error => error.Code == errorCode);
    }

    [Test]
    public async Task Should_Return_Standard_Validation_Envelope_For_Empty_Request_Body()
    {
        using var content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");
        var response = await _fixture.ServerClient.PostAsync("/api/v1/Client", content);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var message = await response.Content.ReadAsJsonAsync<Guid?>();
        message.ShouldNotBeNull();
        message!.Success.ShouldBeFalse();
        message.Errors.ShouldNotBeNull();
        message.Errors!.ShouldContain("Request body is required.");
        message.DetailedErrors.ShouldContain(error => error.Code == "VALIDATION_ERROR");
    }

    [Test]
    public async Task Should_List_Clients_Anonymously_With_Default_Paging_And_Only_Public_Fields()
    {
        var response = await _fixture.ServerClient.GetAsync("/api/v1/Client");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var message = await response.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();
        message.ShouldNotBeNull();
        message!.Success.ShouldBeTrue();
        message.Data.ShouldNotBeNull();
        message.Data!.Count.ShouldBe(3);
        message.Data.Page.ShouldBe(1);
        message.Data.PageSize.ShouldBe(10);
        message.Data.Items.Count.ShouldBe(3);
        message.Data.Items.Select(client => client.Name).ShouldBe(new[] { "Alpha", "Bravo", "Charlie" });
        message.Data.Items.ShouldAllBe(client => client.DeletedAt == null);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var fields = document.RootElement
            .GetProperty("data")
            .GetProperty("items")[0]
            .EnumerateObject()
            .Select(property => property.Name)
            .OrderBy(name => name)
            .ToArray();
        fields.ShouldBe(new[] { "deletedAt", "email", "id", "name", "phone" });
    }

    [Test]
    public async Task Should_Filter_By_Name_Email_Or_Phone()
    {
        foreach (var term in new[] { "Alpha", "bravo@example.com", "0300" })
        {
            var response = await _fixture.ServerClient.GetAsync(
                $"/api/v1/Client?searchTerm={Uri.EscapeDataString(term)}");

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var message = await response.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();
            message!.Data!.Count.ShouldBe(1);
            message.Data.Items.ShouldHaveSingleItem();
        }
    }

    [Test]
    public async Task Should_Apply_IncludeDeleted_Order_And_Pagination_Before_Returning_Items()
    {
        var activeResponse = await _fixture.ServerClient.GetAsync(
            "/api/v1/Client?page=2&pageSize=2&order_by=name%20desc");
        activeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var active = await activeResponse.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();
        active!.Data!.Count.ShouldBe(3);
        active.Data.Page.ShouldBe(2);
        active.Data.PageSize.ShouldBe(2);
        active.Data.Items.Select(client => client.Name).ShouldBe(new[] { "Alpha" });

        var allResponse = await _fixture.ServerClient.GetAsync(
            "/api/v1/Client?includeDeleted=true&order_by=name%20asc,email%20desc");
        allResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var all = await allResponse.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();
        all!.Data!.Count.ShouldBe(4);
        all.Data.Items.ShouldContain(client => client.Id == _fixture.DeletedClientId && client.DeletedAt != null);

        var outOfRangeResponse = await _fixture.ServerClient.GetAsync("/api/v1/Client?page=10&pageSize=2");
        var outOfRange = await outOfRangeResponse.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();
        outOfRange!.Data!.Count.ShouldBe(3);
        outOfRange.Data.Items.ShouldBeEmpty();
        outOfRange.Data.Page.ShouldBe(10);
        outOfRange.Data.PageSize.ShouldBe(2);
    }

    [Test]
    public async Task Should_Accept_All_Allowed_Sort_Fields()
    {
        foreach (var field in new[] { "id", "name", "email", "phone", "deletedAt" })
        {
            var response = await _fixture.ServerClient.GetAsync(
                $"/api/v1/Client?includeDeleted=true&order_by={field}%20asc");

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var message = await response.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();
            message!.Success.ShouldBeTrue();
        }
    }

    [Test]
    public async Task Should_Accept_Page_Size_Bounds()
    {
        foreach (var pageSize in new[] { 1, 100 })
        {
            var response = await _fixture.ServerClient.GetAsync($"/api/v1/Client?pageSize={pageSize}");

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var message = await response.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();
            message!.Data!.PageSize.ShouldBe(pageSize);
        }
    }

    [TestCase(0)]
    [TestCase(101)]
    public async Task Should_Reject_Page_Size_Outside_Allowed_Range(int pageSize)
    {
        var response = await _fixture.ServerClient.GetAsync($"/api/v1/Client?pageSize={pageSize}");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var message = await response.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();
        message!.Success.ShouldBeFalse();
        message.Errors.ShouldNotBeNull();
        message.Errors.ShouldContain("Page size must be between 1 and 100.");
        message.DetailedErrors.ShouldContain(error => error.Code == "VALIDATION_ERROR");
    }

    [Test]
    public async Task Should_Reject_Invalid_Page_And_Unsupported_Sort_Field_With_Standard_Error_Envelope()
    {
        var invalidPage = await _fixture.ServerClient.GetAsync("/api/v1/Client?page=0");
        invalidPage.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var pageMessage = await invalidPage.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();
        pageMessage!.Success.ShouldBeFalse();
        pageMessage.Errors.ShouldNotBeNull();
        pageMessage.Errors.ShouldContain("Page must be at least 1.");

        var invalidSort = await _fixture.ServerClient.GetAsync("/api/v1/Client?order_by=password");
        invalidSort.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var sortMessage = await invalidSort.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();
        sortMessage!.Success.ShouldBeFalse();
        sortMessage.Errors.ShouldNotBeNull();
        sortMessage.Errors.ShouldContain(error => error.Contains("Sortable fields are limited to"));
        sortMessage.DetailedErrors.ShouldContain(error => error.Code == "VALIDATION_ERROR");
    }
}
