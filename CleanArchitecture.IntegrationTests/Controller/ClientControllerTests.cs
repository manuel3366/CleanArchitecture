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

    [OneTimeSetUp]
    public async Task Setup() => await _fixture.SeedTestData();

    [Test]
    public async Task Should_List_Clients_With_Search_Sorting_Pagination_And_Only_Public_Fields_Anonymously()
    {
        var response = await _fixture.ServerClient.GetAsync(
            "/api/v1/Client?searchTerm=example.test&page=2&pageSize=1&order_by=name%20desc,email%20asc");

        response.StatusCode.ShouldBe(
            HttpStatusCode.OK,
            await response.Content.ReadAsStringAsync());
        var message = await response.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();

        message?.Success.ShouldBeTrue();
        message!.Data.ShouldNotBeNull();
        message.Data!.Count.ShouldBe(2);
        message.Data.Page.ShouldBe(2);
        message.Data.PageSize.ShouldBe(1);
        message.Data.Items.ShouldHaveSingleItem();
        message.Data.Items.Single().Id.ShouldBe(_fixture.AlphaClientId);
        message.Data.Items.Single().Name.ShouldBe("Alpha Client");
        message.Data.Items.Single().Email.ShouldBe("alpha@example.test");
        message.Data.Items.Single().Phone.ShouldBe("555-1111");
        message.Data.Items.Single().DeletedAt.ShouldBeNull();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var item = document.RootElement.GetProperty("data").GetProperty("items")[0];
        item.EnumerateObject().Select(property => property.Name)
            .OrderBy(name => name)
            .ShouldBe(new[] { "deletedAt", "email", "id", "name", "phone" });
    }

    [Test]
    public async Task Should_Include_Deleted_Without_Authentication_Or_Additional_Permission()
    {
        var response = await _fixture.ServerClient.GetAsync(
            "/api/v1/Client?includeDeleted=true&pageSize=100&order_by=deletedAt%20desc");

        response.StatusCode.ShouldBe(
            HttpStatusCode.OK,
            await response.Content.ReadAsStringAsync());
        var message = await response.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();

        message?.Data.ShouldNotBeNull();
        message!.Data!.Count.ShouldBe(3);
        message.Data.Page.ShouldBe(1);
        message.Data.PageSize.ShouldBe(100);
        message.Data.Items.Select(client => client.Id).ShouldContain(_fixture.DeletedClientId);
    }

    [TestCase("Alpha Client", "Alpha Client")]
    [TestCase("alpha@example.test", "Alpha Client")]
    [TestCase("555-2222", "Beta Client")]
    public async Task Should_Search_By_Name_Email_And_Phone(string searchTerm, string expectedName)
    {
        var response = await _fixture.ServerClient.GetAsync(
            $"/api/v1/Client?searchTerm={Uri.EscapeDataString(searchTerm)}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var message = await response.Content.ReadAsJsonAsync<PagedResult<ClientViewModel>>();

        message?.Data.ShouldNotBeNull();
        message!.Data!.Count.ShouldBe(1);
        message.Data.Items.ShouldHaveSingleItem();
        message.Data.Items.Single().Name.ShouldBe(expectedName);
    }

    [TestCase("/api/v1/Client?page=0", "page")]
    [TestCase("/api/v1/Client?pageSize=0", "pageSize")]
    [TestCase("/api/v1/Client?pageSize=101", "pageSize")]
    [TestCase("/api/v1/Client?order_by=password", "order_by")]
    public async Task Should_Return_Standard_Error_Envelope_For_Invalid_Query_Parameters(
        string requestUri,
        string expectedErrorKey)
    {
        var response = await _fixture.ServerClient.GetAsync(requestUri);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var body = document.RootElement;
        body.GetProperty("success").GetBoolean().ShouldBeFalse();
        body.GetProperty("errors").GetArrayLength().ShouldBeGreaterThan(0);
        body.GetProperty("detailedErrors")[0].GetProperty("code").GetString()
            .ShouldBe("VALIDATION_ERROR");
        body.GetProperty("detailedErrors")[0].GetProperty("data").GetProperty("key").GetString()
            .ShouldBe(expectedErrorKey);
    }
}
