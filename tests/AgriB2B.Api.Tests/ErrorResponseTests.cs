using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace AgriB2B.Api.Tests;

[Collection(nameof(ApiCollection))]
public class ErrorResponseTests(ApiFactory factory)
{
    [Fact]
    public async Task UnknownRoute_ReturnsProblemDetails()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/no-such-route");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(404, problem?.Status);
    }
}
