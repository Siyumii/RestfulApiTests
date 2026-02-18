using System.Net;
using FluentAssertions;
using RestfulApiTests.Helpers;
using RestfulApiTests.Models;
using Xunit;
using Xunit.Abstractions;

namespace RestfulApiTests.Tests;

/// <summary>
/// Integration tests for the public restful-api.dev REST API.
///
/// The tests have been written for below CRUD operations:
///   1. GET  /objects           – list all objects
///   2. POST /objects           – create a new object
///   3. GET  /objects/{id}      – retrieve the newly created object
///   4. PUT  /objects/{id}      – fully update it
///   5. DELETE /objects/{id}    – delete it
///
/// </summary>
public class RestfulApiTests : IDisposable
{

    private readonly ApiClient _client;
    private readonly ITestOutputHelper _output;   // writes to the xUnit test log

    public RestfulApiTests(ITestOutputHelper output)
    {
        _output = output;
        _client = new ApiClient();
    }

    public void Dispose() => _client.Dispose();

    // ── build a new request payload ──────────────────────────────────

    private static CreateObjectRequest BuildSampleRequest(string nameSuffix = "") =>
        new()
        {
            Name = $"Apple MacBook Pro 16{nameSuffix}",
            Data = new ObjectData
            {
                Year = "2023", 
                Price = "2399.99", 
                CpuModel = "Apple M3 Pro",
                HardDiskSize = "512 GB",
                Color = "Space Gray"
            }
        };

    // ══════════════════════════════════════════════════════════════════════════
    // TEST 1 – GET /objects  (list all objects)
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "GET")]
    public async Task GetAllObjects_ShouldReturn_NonEmptyList()
    {
        // Act
        var objects = await _client.GetAllObjectsAsync();

        // Log
        _output.WriteLine($"Total objects returned: {objects?.Count}");

        // Assert
        objects.Should().NotBeNull("the API must return a JSON array");
        objects.Should().NotBeEmpty("the seed data should contain at least one object");

        objects!.ForEach(obj =>
        {
            obj.Id.Should().NotBeNullOrWhiteSpace("every object must have an id");
            obj.Name.Should().NotBeNullOrWhiteSpace("every object must have a name");
        });
    }

    // ══════════════════════════════════════════════════════════════════════════
    // TEST 2 – POST /objects  (create a new object)
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "POST")]
    public async Task CreateObject_ShouldReturn_CreatedObjectWithId()
    {
        // Arrange
        var request = BuildSampleRequest();

        // Act
        var created = await _client.CreateObjectAsync(request);

        // Log
        _output.WriteLine($"Created object id: {created?.Id}");
        _output.WriteLine($"Created object name: {created?.Name}");

        try
        {
            // Assert – the server echoes back the payload + assigns an id
            created.Should().NotBeNull();
            created!.Id.Should().NotBeNullOrWhiteSpace("server must assign a unique id");
            created.Name.Should().Be(request.Name);
            created.Data.Should().NotBeNull();
            created.Data!.Year.Should().Be(request.Data!.Year);
            created.Data!.Price.Should().Be(request.Data!.Price);
            created.Data.CpuModel.Should().Be(request.Data.CpuModel);
            created.CreatedAt.Should().NotBeNull("server should set createdAt timestamp");
        }
        finally
        {
            // Clean up so we don't pollute subsequent test runs
            if (created?.Id is not null)
                await _client.DeleteObjectAsync(created.Id);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // TEST 3 – GET /objects/{id}  (retrieve the object created in step 2)
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "GET")]
    public async Task GetObjectById_AfterCreate_ShouldReturn_CorrectObject()
    {
        // Arrange – create an object first
        var request = BuildSampleRequest(" – GET Test");
        var created = await _client.CreateObjectAsync(request);
        created.Should().NotBeNull();
        var id = created!.Id!;

        try
        {
            // Act
            var fetched = await _client.GetObjectByIdAsync(id);

            // Log
            _output.WriteLine($"Fetched id: {fetched?.Id}, name: {fetched?.Name}");

            // Assert
            fetched.Should().NotBeNull();
            fetched!.Id.Should().Be(id);
            fetched.Name.Should().Be(request.Name);
            fetched.Data.Should().NotBeNull();
            fetched.Data!.Year.Should().Be(request.Data!.Year);
            fetched.Data.CpuModel.Should().Be(request.Data.CpuModel);
        }
        finally
        {
            await _client.DeleteObjectAsync(id);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // TEST 4 – PUT /objects/{id}  (full update of the created object in step 2)
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "PUT")]
    public async Task UpdateObject_ShouldReturn_UpdatedFields()
    {
        // Arrange – create, then prepare an update payload
        var original = BuildSampleRequest(" – PUT Test");
        var created  = await _client.CreateObjectAsync(original);
        created.Should().NotBeNull();
        var id = created!.Id!;

        var updateRequest = new CreateObjectRequest
        {
            Name = "Apple MacBook Air M2 – Updated",
            Data = new ObjectData
            {
                Year         = "2024", 
                Price        = "1299.00", 
                CpuModel     = "Apple M2",
                HardDiskSize = "256 GB",
                Color        = "Midnight"
            }
        };

        try
        {
            // Act
            var updated = await _client.UpdateObjectAsync(id, updateRequest);

            // Log
            _output.WriteLine($"Updated name: {updated?.Name}, Year: {updated?.Data?.Year}");

            // Assert
            updated.Should().NotBeNull();
            updated!.Id.Should().Be(id, "the id must not change after an update");
            updated.Name.Should().Be(updateRequest.Name);
            updated.Data!.Year.Should().Be(updateRequest.Data!.Year);
            updated.Data!.Price.Should().Be(updateRequest.Data!.Price);
            updated.Data.CpuModel.Should().Be(updateRequest.Data.CpuModel);
            updated.Data.Color.Should().Be(updateRequest.Data.Color);
            updated.UpdatedAt.Should().NotBeNull("server should set updatedAt timestamp");
        }
        finally
        {
            await _client.DeleteObjectAsync(id);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // TEST 5 – DELETE /objects/{id}  (delete the created object)
    // ══════════════════════════════════════════════════════════════════════════

    [Fact]
    [Trait("Category", "DELETE")]
    public async Task DeleteObject_ShouldSucceed_AndObjectShouldNotExistAfterwards()
    {
        // Arrange
        var request = BuildSampleRequest(" – DELETE Test");
        var created = await _client.CreateObjectAsync(request);
        created.Should().NotBeNull();
        var id = created!.Id!;

        // Act – delete
        var deleteResponse = await _client.DeleteObjectAsync(id);

        // Assert – 200 OK on delete
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "the API returns 200 on successful deletion");

        // Verify – a subsequent GET must return 404
        var getAfterDelete = await _client.GetObjectByIdRawAsync(id);

        _output.WriteLine($"GET after DELETE status: {(int)getAfterDelete.StatusCode} {getAfterDelete.StatusCode}");

        getAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "a deleted object must no longer be retrievable");
    }
}
