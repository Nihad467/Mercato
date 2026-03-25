namespace Mercato.Domain.Entities;

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ObjectKey { get; set; } = null!;
    public bool IsMain { get; set; }
    public int Order { get; set; }

    public Product Product { get; set; } = null!;
}