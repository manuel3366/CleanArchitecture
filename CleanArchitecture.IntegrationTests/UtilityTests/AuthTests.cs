using System.Net;
using System.Threading.Tasks;
using CleanArchitecture.IntegrationTests.Fixtures;
using Shouldly;

namespace CleanArchitecture.IntegrationTests.UtilityTests;

[NonParallelizable]
public sealed class AuthenticationTests
{
    private readonly AuthTestFixure _fixture = new();

    [OneTimeSetUp]
    public async Task Setup() => await _fixture.ResetDatabaseAsync();

    [Test]
    [TestCase("/api/v1/user")]
    [TestCase("/api/v1/user/me")]
    [TestCase("/api/v1/user/d74b112a-ece0-443d-9b4f-85bc418822ca")]
    [TestCase("/api/v1/tenant")]
    [TestCase("/api/v1/tenant/d74b112a-ece0-443d-9b4f-85bc418822ca")]
    public async Task Should_Not_Require_A_Token_To_Call_Endpoint(string url)
    {
        var response = await _fixture.ServerClient.GetAsync(url);
        response.StatusCode.ShouldNotBe(HttpStatusCode.Unauthorized);
    }
}