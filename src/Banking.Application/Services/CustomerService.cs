using Banking.Application.Abstractions;
using Banking.Application.Dtos;
using Banking.Domain.Entities;
using Banking.Domain.Exceptions;

namespace Banking.Application.Services;

public sealed class CustomerService(
    ICustomerRepository customers,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICustomerService
{
    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = Customer.Create(
            request.FullName,
            request.DateOfBirth,
            request.Gender,
            request.MonthlyIncome,
            timeProvider.GetUtcNow().UtcDateTime);

        await customers.AddAsync(customer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return customer.ToDto();
    }

    public async Task<CustomerDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await customers.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("El cliente indicado no existe.");

        return customer.ToDto();
    }
}
