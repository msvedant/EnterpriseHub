using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmployeeManagement.UnitTests.Integration;

[Collection(ApiCollection.Name)]
public class EmployeeApiTests(ApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private async Task<HttpClient> AuthenticatedClientAsync()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email = ApiFactory.AdminEmail, password = ApiFactory.AdminPassword });
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var token = doc.RootElement.GetProperty("data").GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<JsonElement> DataAsync(HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(text);
        return doc.RootElement.GetProperty("data").Clone();
    }

    [Fact]
    public async Task Employees_endpoint_requires_authentication()
    {
        var response = await factory.CreateClient().GetAsync("/api/employees");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_401_in_standard_format()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/auth/login",
            new { email = ApiFactory.AdminEmail, password = "definitely-wrong" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(doc.RootElement.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task Invalid_login_payload_returns_400_with_field_errors()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/auth/login", new { email = "nope", password = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(doc.RootElement.GetProperty("errors").TryGetProperty("email", out _));
    }

    [Fact]
    public async Task Seeded_masters_are_available()
    {
        var client = await AuthenticatedClientAsync();
        var response = await client.GetAsync("/api/masters/departments?pageSize=100");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var data = await DataAsync(response);
        Assert.True(data.GetProperty("totalCount").GetInt32() >= 5);
    }

    [Fact]
    public async Task Unknown_master_returns_404()
    {
        var client = await AuthenticatedClientAsync();
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/masters/payroll")).StatusCode);
    }

    [Fact]
    public async Task Employee_lifecycle_create_get_update_list_deactivate_delete()
    {
        var client = await AuthenticatedClientAsync();

        int FirstId(JsonElement page) => page.GetProperty("items")[0].GetProperty("id").GetInt32();

        var departmentId = FirstId(await DataAsync(await client.GetAsync("/api/masters/departments")));
        var designationId = FirstId(await DataAsync(await client.GetAsync("/api/masters/designations")));
        var typeId = FirstId(await DataAsync(await client.GetAsync("/api/masters/employee-types")));
        var relationshipId = FirstId(await DataAsync(await client.GetAsync("/api/masters/relationships")));
        var countryId = FirstId(await DataAsync(await client.GetAsync("/api/masters/countries?search=India")));
        var stateId = FirstId(await DataAsync(await client.GetAsync($"/api/masters/states?parentId={countryId}")));
        var cityId = FirstId(await DataAsync(await client.GetAsync($"/api/masters/cities?parentId={stateId}")));

        var create = new
        {
            employeeCode = "IT-001",
            firstName = "Asha",
            lastName = "Patil",
            dateOfBirth = "1992-03-14",
            gender = "Female",
            email = "asha.patil@example.com",
            mobile = "+919800000001",
            joiningDate = "2023-04-03",
            departmentId, designationId, employeeTypeId = typeId,
            families = new[] { new { relationshipId, fullName = "Ravi Patil", isDependent = false, isEmergencyContact = true } },
            addresses = new[] { new { addressType = "Permanent", line1 = "21 Lake View", countryId, stateId, cityId, postalCode = "411045", isPrimary = true } },
            educations = new[] { new { degree = "MCA", institution = "Pune University", yearOfPassing = 2015 } },
            experiences = new[] { new { companyName = "Initech", jobTitle = "Analyst", startDate = "2015-08-01", endDate = "2023-03-01" } }
        };

        var created = await client.PostAsJsonAsync("/api/employees", create);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(created.Headers.Location);
        var createdData = await DataAsync(created);
        var id = createdData.GetProperty("id").GetInt32();
        Assert.Equal(1, createdData.GetProperty("families").GetArrayLength());

        // duplicate code → 409
        var duplicate = await client.PostAsJsonAsync("/api/employees", create);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        // get by id
        var fetched = await DataAsync(await client.GetAsync($"/api/employees/{id}"));
        Assert.Equal("Asha Patil", fetched.GetProperty("fullName").GetString());
        Assert.Equal(1, fetched.GetProperty("addresses").GetArrayLength());

        // update: change name, drop experience, keep others by id
        var update = new
        {
            employeeCode = "IT-001", firstName = "Asha", lastName = "Patil-Kulkarni",
            dateOfBirth = "1992-03-14", gender = "Female", email = "asha.patil@example.com",
            mobile = "+919800000001", joiningDate = "2023-04-03",
            departmentId, designationId, employeeTypeId = typeId,
            rowVersion = fetched.GetProperty("rowVersion").GetString(),
            families = new[] { new { id = fetched.GetProperty("families")[0].GetProperty("id").GetInt32(), relationshipId, fullName = "Ravi Patil", isDependent = false, isEmergencyContact = true } },
            addresses = new[] { new { id = fetched.GetProperty("addresses")[0].GetProperty("id").GetInt32(), addressType = "Permanent", line1 = "21 Lake View", countryId, stateId, cityId, postalCode = "411045", isPrimary = true } },
            educations = new[] { new { id = fetched.GetProperty("educations")[0].GetProperty("id").GetInt32(), degree = "MCA", institution = "Pune University", yearOfPassing = 2015 } },
            experiences = Array.Empty<object>()
        };
        var updated = await client.PutAsJsonAsync($"/api/employees/{id}", update);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        var updatedData = await DataAsync(updated);
        Assert.Equal("Patil-Kulkarni", updatedData.GetProperty("lastName").GetString());
        Assert.Equal(0, updatedData.GetProperty("experiences").GetArrayLength());

        // list with filter and sorting
        var list = await DataAsync(await client.GetAsync("/api/employees?search=asha&sortBy=firstName&sortDirection=desc&pageSize=5"));
        Assert.Equal(1, list.GetProperty("totalCount").GetInt32());

        // invalid sort column → 400
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/employees?sortBy=password")).StatusCode);

        // deactivate
        var status = await client.PatchAsJsonAsync($"/api/employees/{id}/status", new { isActive = false });
        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        var inactive = await DataAsync(await client.GetAsync($"/api/employees/{id}"));
        Assert.False(inactive.GetProperty("isActive").GetBoolean());

        // soft delete
        Assert.Equal(HttpStatusCode.OK, (await client.DeleteAsync($"/api/employees/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/employees/{id}")).StatusCode);
    }

    [Fact]
    public async Task Invalid_employee_payload_returns_field_level_errors()
    {
        var client = await AuthenticatedClientAsync();
        var response = await client.PostAsJsonAsync("/api/employees", new { employeeCode = "", firstName = "", email = "bad" }, Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var errors = doc.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("employeeCode", out _));
        Assert.True(errors.TryGetProperty("email", out _));
    }
}
