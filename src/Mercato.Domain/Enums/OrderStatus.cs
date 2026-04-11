namespace Mercato.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    PaymentPending = 2,
    Paid = 3,
    PaymentFailed = 4,
    Cancelled = 5,
    Shipped = 6,
    Completed = 7
}