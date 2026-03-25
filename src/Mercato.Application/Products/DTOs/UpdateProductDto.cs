using Microsoft.AspNetCore.Http;

public class UpdateProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public List<IFormFile>? Images { get; set; }
}