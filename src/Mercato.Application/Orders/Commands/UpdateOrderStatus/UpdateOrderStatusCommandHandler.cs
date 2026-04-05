using MediatR;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Application.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateOrderStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.GetOrderByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
            throw new Exception("Order not found.");

        order.Status = request.Status;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}