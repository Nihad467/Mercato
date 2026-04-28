namespace Mercato.Application.Ai.Models;

public class AiProductCandidateDto
{
    public int ProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string? MainImageObjectKey { get; set; }
}