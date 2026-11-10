using Banking.Application.Dtos;

namespace Banking.Application.Services;

public interface ICustomerService
{
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken);
    Task<CustomerDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

public interface IAccountService
{
    Task<AccountDto> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken);
    Task<BalanceDto> GetBalanceAsync(string accountNumber, CancellationToken cancellationToken);
}

public interface ITransactionService
{
    Task<TransactionDto> DepositAsync(string accountNumber, decimal amount, CancellationToken cancellationToken);
    Task<TransactionDto> WithdrawAsync(string accountNumber, decimal amount, CancellationToken cancellationToken);
    Task<TransactionHistoryDto> GetHistoryAsync(string accountNumber, CancellationToken cancellationToken);
}
