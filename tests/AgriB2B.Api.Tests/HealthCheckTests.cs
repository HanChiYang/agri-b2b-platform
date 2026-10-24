namespace AgriB2B.Api.Tests;

[Collection(nameof(ApiCollection))]
public class HealthCheckTests(ApiFactory factory)
{
    [Fact]
    public async Task Health_ReturnsHealthy_WhenDatabaseIsReachable()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
