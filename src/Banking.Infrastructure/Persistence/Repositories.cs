using Banking.Application.Abstractions;
using Banking.Domain.Entities;
using Banking.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Persistence;

internal sealed class CustomerRepository(BankingDbContext db) : ICustomerRepository
{
    public async Task AddAsync(Customer customer, CancellationToken cancellationToken) =>
        await db.Customers.AddAsync(customer, cancellationToken);

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
}

internal sealed class AccountRepository(BankingDbContext db) : IAccountRepository
{
    public async Task AddAsync(Account account, CancellationToken cancellationToken) =>
        await db.Accounts.AddAsync(account, cancellationToken);

    // Se devuelve rastreada: el servicio la modifica y UnitOfWork confirma el cambio.
    public Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken) =>
        db.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, cancellationToken);

    public Task<bool> AccountNumberExistsAsync(string accountNumber, CancellationToken cancellationToken) =>
        db.Accounts.AnyAsync(a => a.AccountNumber == accountNumber, cancellationToken);
}

internal sealed class AccountTransactionRepository(BankingDbContext db) : IAccountTransactionRepository
{
    public async Task AddAsync(AccountTransaction transaction, CancellationToken cancellationToken) =>
        await db.Transactions.AddAsync(transaction, cancellationToken);

    public async Task<IReadOnlyList<AccountTransaction>> GetByAccountIdAsync(
        Guid accountId, CancellationToken cancellationToken) =>
        await db.Transactions
            .AsNoTracking()
            .Where(t => t.AccountId == accountId)
            .OrderBy(t => t.TimestampUtc)
            .ThenBy(t => t.Sequence)
            .ToListAsync(cancellationToken);
}

internal sealed class UnitOfWork(BankingDbContext db) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Se traduce a una excepción de negocio para que la capa de aplicación/API no conozca EF Core.
            throw new ConcurrencyConflictException();
        }
    }
}
