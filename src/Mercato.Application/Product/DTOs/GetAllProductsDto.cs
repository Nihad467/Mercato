namespace Mercato.Application.Product.DTOs;

public class GetAllProductsDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Stock { get; set; }
}