namespace Mercato.Domain.Entities;

public class ProductReview
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}