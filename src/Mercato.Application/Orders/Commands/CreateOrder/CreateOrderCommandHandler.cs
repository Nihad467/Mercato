using MediatR;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Options;
using Mercato.Domain.Entities;
using Mercato.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Text;

namespace Mercato.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;
    private readonly UserManager<AppUser> _userManager;
    private readonly EmailOptions _emailOptions;

    public CreateOrderCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IEmailService emailService,
        UserManager<AppUser> userManager,
        IOptions<EmailOptions> emailOptions)
    {
        _context = context;
        _currentUserService = currentUserService;
        _emailService = emailService;
        _userManager = userManager;
        _emailOptions = emailOptions.Value;
    }

    public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var cartItems = await _context.GetUserCartItemsAsync(userId, cancellationToken);

        if (!cartItems.Any())
            throw new Exception("Cart is empty.");

        foreach (var cartItem in cartItems)
        {
            if (cartItem.Product is null)
                throw new Exception("Product not found in cart.");

            if (cartItem.Product.Stock < cartItem.Quantity)
                throw new Exception($"Not enough stock for product: {cartItem.Product.Name}");
        }

        var subTotalPrice = cartItems.Sum(x => x.Product!.Price * x.Quantity);

        decimal discountAmount = 0;
        string? appliedCouponCode = null;

        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var couponCode = request.CouponCode.Trim().ToUpper();

            var coupon = await _context.GetCouponByCodeAsync(couponCode, cancellationToken);

            if (coupon is null)
                throw new Exception("Coupon not found.");

            if (!coupon.IsActive)
                throw new Exception("Coupon is not active.");

            if (coupon.ExpireDate <= DateTime.UtcNow)
                throw new Exception("Coupon has expired.");

            if (coupon.UsedCount >= coupon.UsageLimit)
                throw new Exception("Coupon usage limit has been reached.");

            if (coupon.DiscountType == DiscountType.Percentage)
            {
                discountAmount = subTotalPrice * coupon.DiscountValue / 100;
            }
            else if (coupon.DiscountType == DiscountType.FixedAmount)
            {
                discountAmount = coupon.DiscountValue;
            }

            if (discountAmount > subTotalPrice)
                discountAmount = subTotalPrice;

            coupon.UsedCount += 1;
            appliedCouponCode = coupon.Code;
        }

        var totalPrice = subTotalPrice - discountAmount;

        var lowStockProducts = new List<Mercato.Domain.Entities.Product>();

        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.Pending,
            SubTotalPrice = subTotalPrice,
            DiscountAmount = discountAmount,
            TotalPrice = totalPrice,
            CouponCode = appliedCouponCode,
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var cartItem in cartItems)
        {
            var product = cartItem.Product!;

            var unitPrice = product.Price;
            var itemTotalPrice = unitPrice * cartItem.Quantity;

            var orderItem = new OrderItem
            {
                ProductId = cartItem.ProductId,
                ProductName = product.Name,
                UnitPrice = unitPrice,
                Quantity = cartItem.Quantity,
                TotalPrice = itemTotalPrice
            };

            order.OrderItems.Add(orderItem);

            product.Stock -= cartItem.Quantity;

            if (product.Stock <= _emailOptions.LowStockThreshold)
            {
                lowStockProducts.Add(product);
            }

            _context.RemoveCartItem(cartItem);
        }

        await _context.AddOrderAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is not null && !string.IsNullOrWhiteSpace(user.Email))
        {
            var emailBody = BuildOrderPlacedEmailBody(order);

            await _emailService.SendEmailAsync(
                user.Email,
                "Your Mercato order has been placed successfully",
                emailBody,
                cancellationToken);
        }

        if (lowStockProducts.Count > 0 &&
            !string.IsNullOrWhiteSpace(_emailOptions.AdminEmail))
        {
            var stockAlertBody = BuildLowStockAlertEmailBody(lowStockProducts);

            await _emailService.SendEmailAsync(
                _emailOptions.AdminEmail,
                "Mercato low stock alert",
                stockAlertBody,
                cancellationToken);
        }

        return new CreateOrderResult
        {
            OrderId = order.Id,
            SubTotalPrice = order.SubTotalPrice,
            DiscountAmount = order.DiscountAmount,
            TotalPrice = order.TotalPrice,
            CouponCode = order.CouponCode,
            CreatedAtUtc = order.CreatedAtUtc
        };
    }

    private static string BuildOrderPlacedEmailBody(Order order)
    {
        var rowsBuilder = new StringBuilder();

        foreach (var item in order.OrderItems)
        {
            rowsBuilder.AppendLine($@"
                <tr>
                    <td style='padding: 14px 12px; border-bottom: 1px solid #edf2f7; color: #1a202c;'>
                        <div style='font-weight: 600;'>{item.ProductName}</div>
                        <div style='font-size: 12px; color: #718096;'>Product ID: {item.ProductId}</div>
                    </td>
                    <td style='padding: 14px 12px; border-bottom: 1px solid #edf2f7; color: #4a5568; text-align: center;'>
                        {item.Quantity}
                    </td>
                    <td style='padding: 14px 12px; border-bottom: 1px solid #edf2f7; color: #4a5568; text-align: right;'>
                        {item.UnitPrice:0.00} $
                    </td>
                    <td style='padding: 14px 12px; border-bottom: 1px solid #edf2f7; color: #1a202c; text-align: right; font-weight: 600;'>
                        {item.TotalPrice:0.00} $
                    </td>
                </tr>");
        }

        var discountRow = order.DiscountAmount > 0
            ? $@"
                        <tr>
                            <td style='font-size: 14px; color: #d1d5db;'>Discount ({order.CouponCode})</td>
                            <td style='font-size: 16px; font-weight: 700; text-align: right; color: #bbf7d0;'>
                                -{order.DiscountAmount:0.00} $
                            </td>
                        </tr>"
            : string.Empty;

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Order Placed</title>
</head>
<body style='margin: 0; padding: 0; background-color: #f4f7fb; font-family: Arial, Helvetica, sans-serif;'>

    <div style='width: 100%; background-color: #f4f7fb; padding: 32px 0;'>
        <div style='max-width: 680px; margin: 0 auto; background-color: #ffffff; border-radius: 18px; overflow: hidden; box-shadow: 0 10px 30px rgba(15, 23, 42, 0.08);'>

            <div style='background: linear-gradient(135deg, #111827, #374151); padding: 30px 36px; color: #ffffff;'>
                <h1 style='margin: 0; font-size: 28px; letter-spacing: 0.3px;'>Mercato</h1>
                <p style='margin: 8px 0 0; font-size: 15px; color: #d1d5db;'>
                    Your order has been received successfully.
                </p>
            </div>

            <div style='padding: 34px 36px 20px;'>
                <div style='display: inline-block; padding: 8px 14px; background-color: #ecfdf5; color: #047857; border-radius: 999px; font-size: 13px; font-weight: 700;'>
                    Order confirmed
                </div>

                <h2 style='margin: 22px 0 10px; font-size: 24px; color: #111827;'>
                    Thanks for your order!
                </h2>

                <p style='margin: 0; font-size: 15px; line-height: 1.7; color: #4b5563;'>
                    We have received your order and it is now being processed. Below are your order details.
                </p>
            </div>

            <div style='padding: 10px 36px 24px;'>
                <table style='width: 100%; border-collapse: collapse; background-color: #f9fafb; border-radius: 14px; overflow: hidden;'>
                    <tr>
                        <td style='padding: 16px; color: #6b7280; font-size: 13px;'>Order ID</td>
                        <td style='padding: 16px; color: #111827; font-size: 13px; font-weight: 700; text-align: right;'>
                            #{order.Id}
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 16px; color: #6b7280; font-size: 13px;'>Status</td>
                        <td style='padding: 16px; color: #111827; font-size: 13px; font-weight: 700; text-align: right;'>
                            {order.Status}
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 16px; color: #6b7280; font-size: 13px;'>Order date</td>
                        <td style='padding: 16px; color: #111827; font-size: 13px; font-weight: 700; text-align: right;'>
                            {order.CreatedAtUtc:dd MMM yyyy, HH:mm}
                        </td>
                    </tr>
                </table>
            </div>

            <div style='padding: 0 36px 30px;'>
                <h3 style='margin: 0 0 14px; color: #111827; font-size: 18px;'>
                    Order items
                </h3>

                <table style='width: 100%; border-collapse: collapse; border: 1px solid #edf2f7; border-radius: 14px; overflow: hidden;'>
                    <thead>
                        <tr style='background-color: #f8fafc;'>
                            <th style='padding: 13px 12px; color: #64748b; font-size: 12px; text-align: left; text-transform: uppercase;'>Product</th>
                            <th style='padding: 13px 12px; color: #64748b; font-size: 12px; text-align: center; text-transform: uppercase;'>Qty</th>
                            <th style='padding: 13px 12px; color: #64748b; font-size: 12px; text-align: right; text-transform: uppercase;'>Price</th>
                            <th style='padding: 13px 12px; color: #64748b; font-size: 12px; text-align: right; text-transform: uppercase;'>Total</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rowsBuilder}
                    </tbody>
                </table>

                <div style='margin-top: 22px; padding: 18px 20px; background-color: #111827; border-radius: 14px; color: #ffffff;'>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr>
                            <td style='font-size: 14px; color: #d1d5db;'>Subtotal</td>
                            <td style='font-size: 16px; font-weight: 700; text-align: right;'>
                                {order.SubTotalPrice:0.00} $
                            </td>
                        </tr>
                        {discountRow}
                        <tr>
                            <td style='font-size: 16px; font-weight: 600; padding-top: 10px;'>Total price</td>
                            <td style='font-size: 22px; font-weight: 800; text-align: right; padding-top: 10px;'>
                                {order.TotalPrice:0.00} $
                            </td>
                        </tr>
                    </table>
                </div>
            </div>

            <div style='padding: 24px 36px; background-color: #f9fafb; border-top: 1px solid #edf2f7;'>
                <p style='margin: 0 0 8px; color: #4b5563; font-size: 14px; line-height: 1.6;'>
                    If you have any questions about your order, please contact Mercato support.
                </p>
                <p style='margin: 0; color: #9ca3af; font-size: 12px;'>
                    © {DateTime.UtcNow.Year} Mercato. All rights reserved.
                </p>
            </div>

        </div>
    </div>

</body>
</html>";
    }

    private static string BuildLowStockAlertEmailBody(List<Mercato.Domain.Entities.Product> products)
    {
        var rowsBuilder = new StringBuilder();

        foreach (var product in products)
        {
            rowsBuilder.AppendLine($@"
                <tr>
                    <td style='padding: 14px 12px; border-bottom: 1px solid #fee2e2; color: #1f2937;'>
                        <div style='font-weight: 700;'>{product.Name}</div>
                        <div style='font-size: 12px; color: #6b7280;'>Product ID: {product.Id}</div>
                    </td>
                    <td style='padding: 14px 12px; border-bottom: 1px solid #fee2e2; color: #991b1b; text-align: right; font-size: 18px; font-weight: 800;'>
                        {product.Stock}
                    </td>
                </tr>");
        }

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Low Stock Alert</title>
</head>
<body style='margin: 0; padding: 0; background-color: #f4f7fb; font-family: Arial, Helvetica, sans-serif;'>

    <div style='width: 100%; background-color: #f4f7fb; padding: 32px 0;'>
        <div style='max-width: 650px; margin: 0 auto; background-color: #ffffff; border-radius: 18px; overflow: hidden; box-shadow: 0 10px 30px rgba(15, 23, 42, 0.08);'>

            <div style='background: linear-gradient(135deg, #7f1d1d, #dc2626); padding: 30px 36px; color: #ffffff;'>
                <h1 style='margin: 0; font-size: 28px; letter-spacing: 0.3px;'>Mercato Admin</h1>
                <p style='margin: 8px 0 0; font-size: 15px; color: #fee2e2;'>
                    Low stock alert
                </p>
            </div>

            <div style='padding: 34px 36px 22px;'>
                <div style='display: inline-block; padding: 8px 14px; background-color: #fef2f2; color: #dc2626; border-radius: 999px; font-size: 13px; font-weight: 700;'>
                    Action required
                </div>

                <h2 style='margin: 22px 0 10px; font-size: 24px; color: #111827;'>
                    Some products are running low
                </h2>

                <p style='margin: 0; font-size: 15px; line-height: 1.7; color: #4b5563;'>
                    The following products reached the low stock threshold. Please review inventory and restock if needed.
                </p>
            </div>

            <div style='padding: 0 36px 30px;'>
                <table style='width: 100%; border-collapse: collapse; border: 1px solid #fee2e2; border-radius: 14px; overflow: hidden;'>
                    <thead>
                        <tr style='background-color: #fef2f2;'>
                            <th style='padding: 13px 12px; color: #991b1b; font-size: 12px; text-align: left; text-transform: uppercase;'>Product</th>
                            <th style='padding: 13px 12px; color: #991b1b; font-size: 12px; text-align: right; text-transform: uppercase;'>Remaining Stock</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rowsBuilder}
                    </tbody>
                </table>

                <div style='margin-top: 22px; padding: 18px 20px; background-color: #fff7ed; border: 1px solid #fed7aa; border-radius: 14px;'>
                    <p style='margin: 0; color: #9a3412; font-size: 14px; line-height: 1.6;'>
                        Recommended action: update stock quantities or temporarily hide unavailable products.
                    </p>
                </div>
            </div>

            <div style='padding: 24px 36px; background-color: #f9fafb; border-top: 1px solid #edf2f7;'>
                <p style='margin: 0; color: #9ca3af; font-size: 12px;'>
                    © {DateTime.UtcNow.Year} Mercato. Admin notification.
                </p>
            </div>

        </div>
    </div>

</body>
</html>";
    }
}