using Mercato.Application.Ai.Models;

namespace Mercato.Application.Common.Interfaces;

public interface IAiRequirementAnalyzer
{
    Task<AiProductRequirementDto> AnalyzeAsync(
        string prompt,
        CancellationToken cancellationToken = default);
}