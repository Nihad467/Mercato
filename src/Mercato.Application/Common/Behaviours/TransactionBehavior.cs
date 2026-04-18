using MediatR;
using Mercato.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Mercato.Application.Common.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        IApplicationDbContext context,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        if (requestName.EndsWith("Query"))
        {
            return await next();
        }

        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next();

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Transaction committed for {RequestName}",
                requestName);

            return response;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);

            _logger.LogWarning(
                "Transaction rolled back for {RequestName}",
                requestName);

            throw;
        }
    }
}