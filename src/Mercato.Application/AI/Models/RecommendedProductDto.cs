namespace Mercato.Application.Ai.Models;

public class RecommendedProductDto
{
    public int ProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public string? MainImageObjectKey { get; set; }

    public int Score { get; set; }

    public string Reason { get; set; } = string.Empty;
}