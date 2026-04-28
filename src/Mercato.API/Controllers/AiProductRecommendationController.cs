using Mercato.Application.Ai.Models;
using Mercato.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Mercato.API.Controllers;

[ApiController]
[Route("api/ai/product-recommendations")]
public class AiProductRecommendationController : ControllerBase
{
    private readonly IAiProductRecommendationService _recommendationService;

    public AiProductRecommendationController(
        IAiProductRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpPost]
    public async Task<IActionResult> Recommend(
        [FromBody] AiProductRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _recommendationService.RecommendAsync(
            request,
            cancellationToken);

        return Ok(result);
    }
}