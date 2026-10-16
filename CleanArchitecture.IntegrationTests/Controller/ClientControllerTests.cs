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
public sealed class ClientControllerTests
{
    private readonly ClientTestFixture _fixture = new();

    [SetUp]
    public async Task Setup() => await _fixture.SeedTestData();

    [Test]
    public async Task Should_List_Clients_Without_Authentication_And_Expose_Only_ViewModel_Fields()
    {
        var response = await _fixture.ServerClient.GetAsync("/api/v1/Client");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var message = await response.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();

        message!.Success.ShouldBeTrue();
        message.Data!.Count.ShouldBe(3);
        message.Data.Page.ShouldBe(1);
        message.Data.PageSize.ShouldBe(10);
        message.Data.Items.Count.ShouldBe(3);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var item = json.RootElement.GetProperty("data").GetProperty("items")[0];
        item.EnumerateObject().Select(property => property.Name)
            .OrderBy(name => name)
            .ShouldBe(new[] { "deletedAt", "email", "id", "name", "phone" });
    }

    [Test]
    public async Task Should_Filter_Clients_By_Name_Email_Or_Phone_And_Include_Deleted_On_Request()
    {
        var searchResponse = await _fixture.ServerClient.GetAsync(
            "/api/v1/Client?searchTerm=555-0102");
        var searchMessage = await searchResponse.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();

        searchResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        searchMessage!.Data!.Items.ShouldHaveSingleItem()
            .Name.ShouldBe("Bruno Jones");

        var emailResponse = await _fixture.ServerClient.GetAsync(
            "/api/v1/Client?searchTerm=charlie@demo");
        var emailMessage = await emailResponse.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();
        emailMessage!.Data!.Items.ShouldHaveSingleItem().Name.ShouldBe("Charlie Brown");

        var deletedResponse = await _fixture.ServerClient.GetAsync(
            "/api/v1/Client?includeDeleted=true");
        var deletedMessage = await deletedResponse.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();

        deletedMessage!.Data!.Count.ShouldBe(4);
        deletedMessage.Data.Items.ShouldContain(client => client.Id == _fixture.DeletedClientId);
    }

    [Test]
    public async Task Should_Sort_And_Page_Filtered_Clients_And_Return_Empty_Out_Of_Range()
    {
        var response = await _fixture.ServerClient.GetAsync(
            "/api/v1/Client?order_by=name%20asc&page=2&pageSize=1");
        var message = await response.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        message!.Data!.Count.ShouldBe(3);
        message.Data.Page.ShouldBe(2);
        message.Data.PageSize.ShouldBe(1);
        message.Data.Items.ShouldHaveSingleItem().Name.ShouldBe("Bruno Jones");

        var outOfRangeResponse = await _fixture.ServerClient.GetAsync(
            "/api/v1/Client?page=10&pageSize=1");
        var outOfRangeMessage = await outOfRangeResponse.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();

        outOfRangeMessage!.Data!.Count.ShouldBe(3);
        outOfRangeMessage.Data.Page.ShouldBe(10);
        outOfRangeMessage.Data.PageSize.ShouldBe(1);
        outOfRangeMessage.Data.Items.ShouldBeEmpty();
    }

    [TestCase("id")]
    [TestCase("name")]
    [TestCase("email")]
    [TestCase("phone")]
    [TestCase("deletedAt")]
    [TestCase("name asc,email desc")]
    public async Task Should_Accept_Only_Documented_Sort_Fields(string orderBy)
    {
        var response = await _fixture.ServerClient.GetAsync(
            $"/api/v1/Client?order_by={Uri.EscapeDataString(orderBy)}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task Should_Create_Client_Anonymously_And_Return_Generated_Id()
    {
        var response = await _fixture.ServerClient.PostAsJsonAsync(
            "/api/v1/Client",
            new CreateClientViewModel("New Client", "not-an-email", "phone/ext 12"));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var message = await response.Content.ReadAsJsonAsync<System.Guid>();
        message!.Success.ShouldBeTrue();
        message.Data.ShouldNotBe(Guid.Empty);

        var createdResponse = await _fixture.ServerClient.GetAsync(
            $"/api/v1/Client?searchTerm={Uri.EscapeDataString("New Client")}");
        var createdMessage = await createdResponse.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();

        createdMessage!.Data!.Items.ShouldHaveSingleItem();
        var createdClient = createdMessage.Data.Items[0];
        createdClient.Id.ShouldBe(message.Data);
        createdClient.Name.ShouldBe("New Client");
        createdClient.Email.ShouldBe("not-an-email");
        createdClient.Phone.ShouldBe("phone/ext 12");
        createdClient.DeletedAt.ShouldBeNull();
    }

    [Test]
    public async Task Should_Ignore_Client_Provided_Id_And_DeletedAt()
    {
        var suppliedId = Guid.NewGuid();

        var response = await _fixture.ServerClient.PostAsJsonAsync(
            "/api/v1/Client",
            new
            {
                id = suppliedId,
                name = "Server Generated Id",
                email = "generated@example.test",
                phone = "123",
                deletedAt = "2025-01-01T00:00:00Z"
            });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var message = await response.Content.ReadAsJsonAsync<System.Guid>();
        message!.Data.ShouldNotBe(suppliedId);

        var createdResponse = await _fixture.ServerClient.GetAsync(
            "/api/v1/Client?searchTerm=Server%20Generated%20Id");
        var createdMessage = await createdResponse.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();
        var createdClient = createdMessage!.Data!.Items.ShouldHaveSingleItem();

        createdClient.Id.ShouldBe(message.Data);
        createdClient.DeletedAt.ShouldBeNull();
    }

    [Test]
    public async Task Should_Return_Validation_Envelope_And_Not_Create_Client_For_Empty_Fields()
    {
        var response = await _fixture.ServerClient.PostAsJsonAsync(
            "/api/v1/Client",
            new CreateClientViewModel("", "", ""));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;
        root.GetProperty("success").GetBoolean().ShouldBeFalse();
        root.GetProperty("errors").GetArrayLength().ShouldBe(3);
        root.GetProperty("detailedErrors").GetArrayLength().ShouldBe(3);

        var clientsResponse = await _fixture.ServerClient.GetAsync("/api/v1/Client");
        var clientsMessage = await clientsResponse.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();
        clientsMessage!.Data!.Count.ShouldBe(3);
    }

    [Test]
    public async Task Should_Soft_Delete_Client_Without_Authentication_And_Preserve_Its_Data()
    {
        var startedAt = DateTimeOffset.UtcNow;
        var response = await _fixture.ServerClient.DeleteAsync(
            $"/api/v1/Client/{_fixture.FirstClientId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var message = await response.Content.ReadAsJsonAsync<Guid>();
        message!.Success.ShouldBeTrue();
        message.Data.ShouldBe(_fixture.FirstClientId);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = json.RootElement.GetProperty("data");
        data.ValueKind.ShouldBe(JsonValueKind.String);
        data.GetString().ShouldBe(_fixture.FirstClientId.ToString());
        json.RootElement.ToString().ShouldNotContain("amina@example.com");

        var activeResponse = await _fixture.ServerClient.GetAsync("/api/v1/Client");
        var activeMessage = await activeResponse.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();
        activeMessage!.Data!.Count.ShouldBe(2);
        activeMessage.Data.Items.ShouldNotContain(client => client.Id == _fixture.FirstClientId);

        var includedResponse = await _fixture.ServerClient.GetAsync("/api/v1/Client?includeDeleted=true");
        var includedMessage = await includedResponse.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();
        var deleted = includedMessage!.Data!.Items.Single(client => client.Id == _fixture.FirstClientId);
        deleted.DeletedAt.ShouldNotBeNull();
        deleted.DeletedAt.Value.ShouldBeGreaterThanOrEqualTo(startedAt);
        deleted.DeletedAt.Value.ShouldBeLessThanOrEqualTo(DateTimeOffset.UtcNow);
        deleted.Name.ShouldBe("Amina Smith");
        deleted.Email.ShouldBe("amina@example.com");
        deleted.Phone.ShouldBe("555-0101");
    }

    [Test]
    public async Task Should_Return_NotFound_Without_Exposing_Client_Data_When_Deleting_Missing_Client()
    {
        var response = await _fixture.ServerClient.DeleteAsync($"/api/v1/Client/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;
        root.GetProperty("success").GetBoolean().ShouldBeFalse();
        root.GetProperty("errors").GetArrayLength().ShouldBeGreaterThan(0);
        root.GetRawText().ShouldNotContain("@");
        root.GetProperty("detailedErrors")[0].GetProperty("code").GetString()
            .ShouldBe("OBJECT_NOT_FOUND");
    }

    [Test]
    public async Task Should_Return_Validation_Envelope_For_Invalid_Client_Id()
    {
        var response = await _fixture.ServerClient.DeleteAsync("/api/v1/Client/not-a-guid");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;
        root.GetProperty("success").GetBoolean().ShouldBeFalse();
        root.GetProperty("detailedErrors")[0].GetProperty("code").GetString()
            .ShouldBe("VALIDATION_ERROR");
        root.GetRawText().ShouldNotContain("@");
    }

    [Test]
    public async Task Should_Return_NotFound_When_Deleting_A_Client_More_Than_Once()
    {
        var firstResponse = await _fixture.ServerClient.DeleteAsync(
            $"/api/v1/Client/{_fixture.FirstClientId}");
        var secondResponse = await _fixture.ServerClient.DeleteAsync(
            $"/api/v1/Client/{_fixture.FirstClientId}");

        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [TestCase("/api/v1/Client?page=0")]
    [TestCase("/api/v1/Client?pageSize=101")]
    [TestCase("/api/v1/Client?order_by=tenantId")]
    [TestCase("/api/v1/Client?order_by=name%20asc,")]
    public async Task Should_Return_Envelope_For_Invalid_Query_Parameters(string url)
    {
        var response = await _fixture.ServerClient.GetAsync(url);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;
        root.GetProperty("success").GetBoolean().ShouldBeFalse();
        root.GetProperty("errors").GetArrayLength().ShouldBeGreaterThan(0);
        root.GetProperty("detailedErrors")[0].GetProperty("code").GetString()
            .ShouldBe("VALIDATION_ERROR");
    }
}
