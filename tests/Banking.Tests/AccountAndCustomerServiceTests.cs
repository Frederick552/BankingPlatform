using Banking.Application.Dtos;
using Banking.Application.Services;
using Banking.Domain.Entities;
using Banking.Domain.Enums;
using Banking.Domain.Exceptions;
using Banking.Tests.Support;

namespace Banking.Tests;

public class AccountAndCustomerServiceTests
{
    private readonly InMemoryStore _store = new();
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 6, 18, 10, 0, 0, TimeSpan.Zero));

    private CustomerService CreateCustomerService() => new(_store, _store, _clock);

    private AccountService CreateAccountService(params string[] generatedNumbers) =>
        new(_store, _store, new StubAccountNumberGenerator(generatedNumbers), _store, _clock);

    private async Task<CustomerDto> SeedCustomerAsync() =>
        await CreateCustomerService().CreateAsync(
            new CreateCustomerRequest("Ana Pérez", new DateOnly(1990, 5, 20), Gender.Female, 1500m),
            CancellationToken.None);

    [Fact]
    public async Task CreateCustomer_WithValidData_IsPersisted()
    {
        var customer = await SeedCustomerAsync();

        Assert.Single(_store.Customers);
        Assert.Equal("Ana Pérez", customer.FullName);
        Assert.Equal(Gender.Female, customer.Gender);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateCustomer_WithoutName_ThrowsValidationException(string name)
    {
        await Assert.ThrowsAsync<DomainValidationException>(() =>
            CreateCustomerService().CreateAsync(
                new CreateCustomerRequest(name, new DateOnly(1990, 5, 20), Gender.Male, 1000m),
                CancellationToken.None));
    }

    [Fact]
    public async Task CreateCustomer_WithFutureBirthDate_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<DomainValidationException>(() =>
            CreateCustomerService().CreateAsync(
                new CreateCustomerRequest("Ana", new DateOnly(2030, 1, 1), Gender.Female, 1000m),
                CancellationToken.None));
    }

    [Fact]
    public async Task CreateAccount_ForExistingCustomer_UsesGeneratedNumberAndInitialBalance()
    {
        var customer = await SeedCustomerAsync();

        var account = await CreateAccountService("ACC-20260618-0042")
            .CreateAsync(new CreateAccountRequest(customer.Id, 250m), CancellationToken.None);

        Assert.Equal("ACC-20260618-0042", account.AccountNumber);
        Assert.Equal(250m, account.Balance);
        Assert.Equal(customer.Id, account.CustomerId);
    }

    [Fact]
    public async Task CreateAccount_ForUnknownCustomer_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateAccountService("ACC-20260618-0001")
                .CreateAsync(new CreateAccountRequest(Guid.NewGuid(), 10m), CancellationToken.None));
    }

    [Fact]
    public async Task CreateAccount_RetriesWithNewNumber_WhenGeneratedNumberAlreadyExists()
    {
        var customer = await SeedCustomerAsync();
        _store.Accounts.Add(Account.Open(customer.Id, "ACC-20260618-0001", 0m, _clock.GetUtcNow().UtcDateTime));

        var account = await CreateAccountService("ACC-20260618-0001", "ACC-20260618-0002")
            .CreateAsync(new CreateAccountRequest(customer.Id, 10m), CancellationToken.None);

        Assert.Equal("ACC-20260618-0002", account.AccountNumber);
    }

    [Fact]
    public async Task GetBalance_ReturnsCurrentBalance()
    {
        var customer = await SeedCustomerAsync();
        var service = CreateAccountService("ACC-20260618-0007");
        await service.CreateAsync(new CreateAccountRequest(customer.Id, 321.45m), CancellationToken.None);

        var balance = await service.GetBalanceAsync("ACC-20260618-0007", CancellationToken.None);

        Assert.Equal(321.45m, balance.Balance);
    }

    [Fact]
    public async Task GetBalance_OfUnknownAccount_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateAccountService().GetBalanceAsync("ACC-20260618-9999", CancellationToken.None));
    }
}
