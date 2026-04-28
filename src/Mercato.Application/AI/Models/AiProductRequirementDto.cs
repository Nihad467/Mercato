namespace Mercato.Application.Ai.Models;

public class AiProductRequirementDto
{
    public string Intent { get; set; } = string.Empty;

    public string ProductType { get; set; } = "Unknown";

    public string UsageType { get; set; } = "General";

    public string? GameName { get; set; }

    public int? TargetFps { get; set; }

    public decimal? Budget { get; set; }

    public string? PreferredOperatingSystem { get; set; }

    public string? PreferredBrand { get; set; }

    public string? PreferredConnectionType { get; set; }

    public bool WantsCheap { get; set; }

    public bool WantsPremium { get; set; }

    public bool WantsWireless { get; set; }

    public bool WantsMechanicalKeyboard { get; set; }

    public bool WantsPortable { get; set; }

    public bool WantsGoodCamera { get; set; }

    public bool WantsLongBattery { get; set; }

    public bool IsSupported { get; set; } = true;

    public string? UnsupportedReason { get; set; }

    public List<string> Keywords { get; set; } = new();
}