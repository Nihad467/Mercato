using Mercato.Application.Ai.Models;

namespace Mercato.Application.Common.Interfaces;

public interface IAiProductRecommendationService
{
    Task<AiProductRecommendationResponse> RecommendAsync(
        AiProductRecommendationRequest request,
        CancellationToken cancellationToken = default);
}