using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace Banking.Tests;

public sealed class BankingApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"banking-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseSetting("ConnectionStrings:Banking", $"Data Source={_dbPath}");

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing) return;

        SqliteConnection.ClearAllPools();
        try { File.Delete(_dbPath); } catch (IOException) { /* limpieza best-effort */ }
    }
}

public class ApiIntegrationTests(BankingApiFactory factory) : IClassFixture<BankingApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<string> CreateAccountAsync(decimal initialBalance)
    {
        var customerResponse = await _client.PostAsJsonAsync("/api/customers", new
        {
            fullName = "María López",
            dateOfBirth = "1990-05-20",
            gender = "Female",
            monthlyIncome = 2500.50m
        });
        Assert.Equal(HttpStatusCode.Created, customerResponse.StatusCode);
        var customerId = (await customerResponse.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("id").GetGuid();

        var accountResponse = await _client.PostAsJsonAsync("/api/accounts", new { customerId, initialBalance });
        Assert.Equal(HttpStatusCode.Created, accountResponse.StatusCode);

        return (await accountResponse.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("accountNumber").GetString()!;
    }

    [Fact]
    public async Task FullFlow_CreateAccount_DepositWithdraw_BalanceAndHistory()
    {
        var accountNumber = await CreateAccountAsync(100m);
        Assert.Matches(@"^ACC-\d{8}-\d{4}$", accountNumber);

        Assert.Equal(HttpStatusCode.OK,
            (await _client.PostAsJsonAsync($"/api/accounts/{accountNumber}/deposits", new { amount = 50m })).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await _client.PostAsJsonAsync($"/api/accounts/{accountNumber}/withdrawals", new { amount = 30m })).StatusCode);

        var balance = await _client.GetFromJsonAsync<JsonElement>($"/api/accounts/{accountNumber}/balance");
        Assert.Equal(120m, balance.GetProperty("balance").GetDecimal());

        var history = await _client.GetFromJsonAsync<JsonElement>($"/api/accounts/{accountNumber}/transactions");
        var movements = history.GetProperty("transactions").EnumerateArray().ToList();

        Assert.Equal(2, movements.Count);
        Assert.Equal("Deposit", movements[0].GetProperty("type").GetString());
        Assert.Equal(150m, movements[0].GetProperty("balanceAfter").GetDecimal());
        Assert.Equal("Withdrawal", movements[1].GetProperty("type").GetString());
        Assert.Equal(120m, movements[1].GetProperty("balanceAfter").GetDecimal());
    }

    [Fact]
    public async Task Withdraw_WithInsufficientFunds_Returns400ProblemDetails_WithoutLeakingInternals()
    {
        var accountNumber = await CreateAccountAsync(10m);

        var response = await _client.PostAsJsonAsync($"/api/accounts/{accountNumber}/withdrawals", new { amount = 999m });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("Fondos insuficientes", body);
        Assert.DoesNotContain("Exception", body);
        Assert.DoesNotContain("at Banking.", body); // sin stack trace
    }

    [Fact]
    public async Task GetBalance_OfUnknownAccount_Returns404()
    {
        var response = await _client.GetAsync("/api/accounts/ACC-20000101-0000/balance");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateAccount_ForUnknownCustomer_Returns404()
    {
        var response = await _client.PostAsJsonAsync("/api/accounts",
            new { customerId = Guid.NewGuid(), initialBalance = 10m });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Deposit_WithNegativeAmount_Returns400()
    {
        var accountNumber = await CreateAccountAsync(10m);

        var response = await _client.PostAsJsonAsync($"/api/accounts/{accountNumber}/deposits", new { amount = -5m });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
