namespace Mercato.Application.Ai.Models;

public class AiProductRecommendationResponse
{
    public string Prompt { get; set; } = string.Empty;

    public string Intent { get; set; } = string.Empty;

    public string? Message { get; set; }

    public AiProductRequirementDto Requirements { get; set; } = new();

    public List<RecommendedProductDto> Products { get; set; } = new();
}