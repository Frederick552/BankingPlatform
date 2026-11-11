using Banking.Application.Abstractions;
using Banking.Domain.Entities;

namespace Banking.Tests.Support;

internal sealed class FakeTimeProvider(DateTimeOffset start) : TimeProvider
{
    private DateTimeOffset _now = start;

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan delta) => _now += delta;
}

internal sealed class InMemoryStore :
    ICustomerRepository, IAccountRepository, IAccountTransactionRepository, IUnitOfWork
{
    public List<Customer> Customers { get; } = [];
    public List<Account> Accounts { get; } = [];
    public List<AccountTransaction> Transactions { get; } = [];
    public int SaveCount { get; private set; }

    Task ICustomerRepository.AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        Customers.Add(customer);
        return Task.CompletedTask;
    }

    Task<Customer?> ICustomerRepository.GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Customers.FirstOrDefault(c => c.Id == id));

    Task IAccountRepository.AddAsync(Account account, CancellationToken cancellationToken)
    {
        Accounts.Add(account);
        return Task.CompletedTask;
    }

    Task<Account?> IAccountRepository.GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken) =>
        Task.FromResult(Accounts.FirstOrDefault(a => a.AccountNumber == accountNumber));

    Task<bool> IAccountRepository.AccountNumberExistsAsync(string accountNumber, CancellationToken cancellationToken) =>
        Task.FromResult(Accounts.Any(a => a.AccountNumber == accountNumber));

    Task IAccountTransactionRepository.AddAsync(AccountTransaction transaction, CancellationToken cancellationToken)
    {
        Transactions.Add(transaction);
        return Task.CompletedTask;
    }

    Task<IReadOnlyList<AccountTransaction>> IAccountTransactionRepository.GetByAccountIdAsync(
        Guid accountId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<AccountTransaction>>(
            Transactions
                .Where(t => t.AccountId == accountId)
                .OrderBy(t => t.TimestampUtc)
                .ThenBy(t => t.Sequence)
                .ToList());

    Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}

internal sealed class StubAccountNumberGenerator(params string[] numbers) : IAccountNumberGenerator
{
    private int _index;

    public string Generate() => numbers[_index++];
}
