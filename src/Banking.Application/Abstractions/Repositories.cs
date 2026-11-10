using Banking.Domain.Entities;

namespace Banking.Application.Abstractions;

public interface ICustomerRepository
{
    Task AddAsync(Customer customer, CancellationToken cancellationToken);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

public interface IAccountRepository
{
    Task AddAsync(Account account, CancellationToken cancellationToken);
    Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken);
    Task<bool> AccountNumberExistsAsync(string accountNumber, CancellationToken cancellationToken);
}

public interface IAccountTransactionRepository
{
    Task AddAsync(AccountTransaction transaction, CancellationToken cancellationToken);

    /// <summary>Devuelve los movimientos de la cuenta en orden cronológico ascendente.</summary>
    Task<IReadOnlyList<AccountTransaction>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken);
}

/// <summary>Confirma como una sola unidad atómica todos los cambios pendientes.</summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
