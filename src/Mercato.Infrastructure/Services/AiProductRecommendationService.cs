using Mercato.Application.Ai.Models;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Infrastructure.Services;

public class AiProductRecommendationService : IAiProductRecommendationService
{
    private readonly IAiRequirementAnalyzer _requirementAnalyzer;
    private readonly IApplicationDbContext _context;

    public AiProductRecommendationService(
        IAiRequirementAnalyzer requirementAnalyzer,
        IApplicationDbContext context)
    {
        _requirementAnalyzer = requirementAnalyzer;
        _context = context;
    }

    public async Task<AiProductRecommendationResponse> RecommendAsync(
        AiProductRecommendationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            throw new ArgumentException("Prompt cannot be empty.", nameof(request.Prompt));

        var requirement = await _requirementAnalyzer.AnalyzeAsync(
            request.Prompt,
            cancellationToken);

        requirement = EnrichRequirementFromPrompt(requirement, request.Prompt);

        if (!requirement.IsSupported || requirement.ProductType == "Unknown")
        {
            return new AiProductRecommendationResponse
            {
                Prompt = request.Prompt,
                Intent = "Unsupported product request",
                Message = "Bağışlayın, dəyərli müştəri. Mercato hazırda yalnız texnologiya məhsulları satır.",
                Requirements = requirement,
                Products = new List<RecommendedProductDto>()
            };
        }

        var candidates = await _context.GetAiProductCandidatesAsync(cancellationToken);

        var expectedCategory = MapProductTypeToCategory(requirement.ProductType);

        var products = candidates
            .Where(x => x.Stock > 0)
            .Where(x => string.IsNullOrWhiteSpace(expectedCategory) ||
                        x.CategoryName.Equals(expectedCategory, StringComparison.OrdinalIgnoreCase))
            .Where(x => requirement.Budget is null || x.Price <= requirement.Budget.Value)
            .Where(x => IsHardCompatible(x, requirement))
            .Select(x => new
            {
                Product = x,
                Score = CalculateScore(x, requirement, request.Prompt)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Product.Price)
            .Take(5)
            .Select(x => new RecommendedProductDto
            {
                ProductId = x.Product.ProductId,
                Name = x.Product.Name,
                CategoryName = x.Product.CategoryName,
                Price = x.Product.Price,
                Stock = x.Product.Stock,
                MainImageObjectKey = x.Product.MainImageObjectKey,
                Score = x.Score,
                Reason = BuildReason(x.Product, requirement)
            })
            .ToList();

        return new AiProductRecommendationResponse
        {
            Prompt = request.Prompt,
            Intent = requirement.Intent,
            Message = products.Any()
                ? "Sənin istəyinə uyğun məhsullar tapıldı."
                : "Təəssüf ki, bu istəyə uyğun məhsul tapılmadı.",
            Requirements = requirement,
            Products = products
        };
    }

    private static AiProductRequirementDto EnrichRequirementFromPrompt(
        AiProductRequirementDto requirement,
        string prompt)
    {
        var lower = prompt.ToLowerInvariant();

        var unsupportedWords = new[]
        {
            "paltaryuyan", "washing machine", "soyuducu", "fridge", "refrigerator",
            "kondisioner", "conditioner", "paltar", "clothes", "ayaqqabı", "ayaqqabi", "shoes",
            "qida", "yemək", "yemek", "food", "mebel", "furniture", "divan", "sofa",
            "stol", "table", "stul", "chair", "maşın", "masin", "avtomobil", "car",
            "kosmetika", "cosmetics", "ətir", "etir", "perfume"
        };

        if (unsupportedWords.Any(x => lower.Contains(x)))
        {
            requirement.ProductType = "Unknown";
            requirement.UsageType = "General";
            requirement.IsSupported = false;
            requirement.UnsupportedReason = "Mercato only sells technology/electronics products.";
            requirement.Intent = "Unsupported product request";
            requirement.ProductsSafeNormalize();

            return requirement;
        }

        if (ContainsAny(lower, "telefon", "phone", "iphone", "android", "samsung galaxy", "oneplus", "xiaomi", "redmi", "poco", "pixel"))
            requirement.ProductType = "Phone";

        if (ContainsAny(lower, "laptop", "komputer", "computer", "comp", "pc", "notebook", "macbook"))
            requirement.ProductType = "Laptop";

        if (ContainsAny(lower, "planset", "planşet", "tablet", "ipad"))
            requirement.ProductType = "Tablet";

        if (ContainsAny(lower, "monitor", "144hz", "165hz", "170hz", "ekran"))
            requirement.ProductType = "Monitor";

        if (ContainsAny(lower, "klaviatura", "keyboard"))
            requirement.ProductType = "Keyboard";

        if (ContainsAny(lower, "siçan", "sican", "mouse"))
            requirement.ProductType = "Mouse";

        if (ContainsAny(lower, "qulaqlıq", "qulaqliq", "headset", "headphone", "earphone", "sound"))
            requirement.ProductType = "Headset";

        if (ContainsAny(lower, "charger", "adapter", "zaryadka", "kabel", "cable", "ssd", "hub", "router", "webcam", "kamera"))
            requirement.ProductType = "Accessory";

        if (ContainsAny(lower, "oyun", "gaming", "pubg", "gta", "valorant", "cs", "fps"))
            requirement.UsageType = "Gaming";

        if (ContainsAny(lower, "program", "coding", "proqram", "developer", "visual studio", "backend", "frontend"))
            requirement.UsageType = "Programming";

        if (ContainsAny(lower, "ofis", "office", "dərs", "ders", "study", "telebe", "tələbə"))
            requirement.UsageType = "Office";

        if (ContainsAny(lower, "design", "dizayn", "photoshop", "video montaj", "editing", "render"))
            requirement.UsageType = "Design";

        if (ContainsAny(lower, "film", "youtube", "netflix", "media", "music"))
            requirement.UsageType = "Media";

        if (lower.Contains("pubg"))
            requirement.GameName = "PUBG";

        if (lower.Contains("gta"))
            requirement.GameName = "GTA";

        if (lower.Contains("valorant"))
            requirement.GameName = "Valorant";

        if (ContainsAny(lower, "cs2", "cs go", "counter"))
            requirement.GameName = "CS";

        if (!string.IsNullOrWhiteSpace(requirement.GameName))
            requirement.UsageType = "Gaming";

        if (ContainsAny(lower, "windows", "vindovs", "win "))
            requirement.PreferredOperatingSystem = "Windows";

        if (ContainsAny(lower, "macbook", "macos", "mac os", "apple laptop"))
            requirement.PreferredOperatingSystem = "macOS";

        if (ContainsAny(lower, "android", "samsung", "xiaomi", "redmi", "poco", "oneplus", "nothing phone", "pixel"))
            requirement.PreferredOperatingSystem = "Android";

        if (ContainsAny(lower, "iphone", "ios"))
            requirement.PreferredOperatingSystem = "iOS";

        requirement.PreferredBrand ??= DetectBrand(lower);

        if (ContainsAny(lower, "wireless", "bluetooth", "simsiz"))
        {
            requirement.WantsWireless = true;
            requirement.PreferredConnectionType = "Wireless";
        }

        if (ContainsAny(lower, "wired", "kabel", "usb"))
            requirement.PreferredConnectionType = "Wired";

        if (ContainsAny(lower, "mechanical", "mexaniki", "red switch", "blue switch", "brown switch"))
            requirement.WantsMechanicalKeyboard = true;

        if (ContainsAny(lower, "ucuz", "münasib", "munasib", "budget", "cheap", "sərfəli", "serfeli"))
            requirement.WantsCheap = true;

        if (ContainsAny(lower, "premium", "flagship", "güclü", "guclu", "professional", "pro "))
            requirement.WantsPremium = true;

        if (ContainsAny(lower, "yüngül", "yungul", "lightweight", "portable", "daşımaq", "dasimaq"))
            requirement.WantsPortable = true;

        if (ContainsAny(lower, "kamera", "camera", "şəkil", "sekil", "photo"))
            requirement.WantsGoodCamera = true;

        if (ContainsAny(lower, "battery", "batareya", "zaryadka saxlasın", "long battery"))
            requirement.WantsLongBattery = true;

        if (lower.Contains("170"))
            requirement.TargetFps = 170;
        else if (lower.Contains("165"))
            requirement.TargetFps = 165;
        else if (lower.Contains("144"))
            requirement.TargetFps = 144;
        else if (lower.Contains("120"))
            requirement.TargetFps = 120;
        else if (lower.Contains("60"))
            requirement.TargetFps = 60;

        requirement.IsSupported = requirement.ProductType != "Unknown";

        if (!requirement.IsSupported)
        {
            requirement.UnsupportedReason = "Mercato only sells technology/electronics products.";
            requirement.Intent = "Unsupported product request";
        }
        else
        {
            requirement.Intent = $"{requirement.UsageType} {requirement.ProductType} recommendation";
        }

        requirement.ProductsSafeNormalize();

        return requirement;
    }

    private static int CalculateScore(
        AiProductCandidateDto product,
        AiProductRequirementDto requirement,
        string prompt)
    {
        var score = 0;

        var text = $"{product.Name} {product.Description} {product.CategoryName}".ToLowerInvariant();
        var promptLower = prompt.ToLowerInvariant();

        score += 50;

        ApplyOperatingSystemScore(product, requirement, text, ref score);
        ApplyBrandScore(requirement, text, ref score);
        ApplyUsageScore(product, requirement, text, ref score);
        ApplyGameScore(product, requirement, text, ref score);
        ApplyFpsScore(requirement, text, ref score);
        ApplyConnectionScore(requirement, text, ref score);
        ApplyKeyboardMouseScore(product, requirement, text, ref score);
        ApplyCameraBatteryPortableScore(requirement, text, ref score);
        ApplyKeywordScore(requirement, text, ref score);
        ApplyPricePreferenceScore(product, requirement, promptLower, ref score);

        if (product.Stock <= 3)
            score -= 5;

        return score;
    }

    private static bool IsHardCompatible(
        AiProductCandidateDto product,
        AiProductRequirementDto requirement)
    {
        var text = $"{product.Name} {product.Description} {product.CategoryName}".ToLowerInvariant();

        if (requirement.PreferredOperatingSystem == "Windows" &&
            product.CategoryName.Equals("Laptops", StringComparison.OrdinalIgnoreCase) &&
            IsAppleProduct(text))
        {
            return false;
        }

        if (requirement.PreferredOperatingSystem == "Android" &&
            (product.CategoryName.Equals("Phones", StringComparison.OrdinalIgnoreCase) ||
             product.CategoryName.Equals("Tablets", StringComparison.OrdinalIgnoreCase)) &&
            IsAppleProduct(text))
        {
            return false;
        }

        if (requirement.PreferredOperatingSystem == "iOS" &&
            (product.CategoryName.Equals("Phones", StringComparison.OrdinalIgnoreCase) ||
             product.CategoryName.Equals("Tablets", StringComparison.OrdinalIgnoreCase)) &&
            !IsAppleProduct(text))
        {
            return false;
        }

        if (requirement.PreferredOperatingSystem == "macOS" &&
            product.CategoryName.Equals("Laptops", StringComparison.OrdinalIgnoreCase) &&
            !IsAppleProduct(text))
        {
            return false;
        }

        return true;
    }

    private static void ApplyOperatingSystemScore(
        AiProductCandidateDto product,
        AiProductRequirementDto requirement,
        string text,
        ref int score)
    {
        if (string.IsNullOrWhiteSpace(requirement.PreferredOperatingSystem))
            return;

        var os = requirement.PreferredOperatingSystem.Trim().ToLowerInvariant();

        if (os == "windows")
        {
            if (product.CategoryName.Equals("Laptops", StringComparison.OrdinalIgnoreCase))
                score += IsAppleProduct(text) ? -150 : 45;
        }

        if (os == "macos")
        {
            if (IsAppleProduct(text))
                score += 70;
            else if (product.CategoryName.Equals("Laptops", StringComparison.OrdinalIgnoreCase))
                score -= 80;
        }

        if (os == "android")
        {
            if (product.CategoryName.Equals("Phones", StringComparison.OrdinalIgnoreCase) ||
                product.CategoryName.Equals("Tablets", StringComparison.OrdinalIgnoreCase))
            {
                score += IsAppleProduct(text) ? -150 : 45;
            }
        }

        if (os == "ios")
        {
            if (IsAppleProduct(text))
                score += 70;
            else if (product.CategoryName.Equals("Phones", StringComparison.OrdinalIgnoreCase) ||
                     product.CategoryName.Equals("Tablets", StringComparison.OrdinalIgnoreCase))
                score -= 100;
        }
    }

    private static void ApplyBrandScore(
        AiProductRequirementDto requirement,
        string text,
        ref int score)
    {
        if (string.IsNullOrWhiteSpace(requirement.PreferredBrand))
            return;

        var brand = requirement.PreferredBrand.Trim().ToLowerInvariant();

        if (text.Contains(brand))
            score += 60;
        else
            score -= 15;
    }

    private static void ApplyUsageScore(
        AiProductCandidateDto product,
        AiProductRequirementDto requirement,
        string text,
        ref int score)
    {
        if (requirement.UsageType.Equals("Gaming", StringComparison.OrdinalIgnoreCase))
        {
            if (text.Contains("gaming"))
                score += 35;

            if (ContainsAny(text, "rtx", "snapdragon", "high-performance", "performance-focused", "flagship", "pro chip"))
                score += 35;

            if (ContainsAny(text, "120hz", "144hz", "165hz", "170hz", "high refresh", "promotion"))
                score += 30;
        }

        if (requirement.UsageType.Equals("Programming", StringComparison.OrdinalIgnoreCase))
        {
            if (product.CategoryName.Equals("Laptops", StringComparison.OrdinalIgnoreCase))
                score += 40;

            if (ContainsAny(text, "16gb", "m2", "m3", "ssd", "ultrabook", "productivity", "xps", "zenbook", "ideapad", "surface"))
                score += 35;
        }

        if (requirement.UsageType.Equals("Office", StringComparison.OrdinalIgnoreCase) ||
            requirement.UsageType.Equals("Study", StringComparison.OrdinalIgnoreCase))
        {
            if (ContainsAny(text, "productivity", "office", "study", "lightweight", "wireless", "slim", "portable"))
                score += 30;
        }

        if (requirement.UsageType.Equals("Design", StringComparison.OrdinalIgnoreCase))
        {
            if (ContainsAny(text, "oled", "retina", "ultrasharp", "professional", "pro", "amoled", "qhd"))
                score += 35;
        }

        if (requirement.UsageType.Equals("Media", StringComparison.OrdinalIgnoreCase))
        {
            if (ContainsAny(text, "amoled", "large screen", "smart monitor", "sound", "headphones", "wireless", "noise cancellation"))
                score += 30;
        }
    }

    private static void ApplyGameScore(
        AiProductCandidateDto product,
        AiProductRequirementDto requirement,
        string text,
        ref int score)
    {
        if (string.IsNullOrWhiteSpace(requirement.GameName))
            return;

        var game = requirement.GameName.ToLowerInvariant();

        if (game == "pubg")
        {
            if (product.CategoryName.Equals("Phones", StringComparison.OrdinalIgnoreCase))
                score += 35;

            if (ContainsAny(text, "snapdragon", "flagship", "performance", "120hz", "pro chip"))
                score += 35;
        }

        if (game == "gta")
        {
            if (product.CategoryName.Equals("Laptops", StringComparison.OrdinalIgnoreCase))
                score += 35;

            if (ContainsAny(text, "rtx", "gaming", "dedicated", "gpu", "tuf", "legion", "nitro", "katana", "victus"))
                score += 40;

            if (IsAppleProduct(text))
                score -= 70;
        }

        if (game == "valorant" || game == "cs")
        {
            if (product.CategoryName.Equals("Laptops", StringComparison.OrdinalIgnoreCase) ||
                product.CategoryName.Equals("Mice", StringComparison.OrdinalIgnoreCase) ||
                product.CategoryName.Equals("Monitors", StringComparison.OrdinalIgnoreCase) ||
                product.CategoryName.Equals("Keyboards", StringComparison.OrdinalIgnoreCase) ||
                product.CategoryName.Equals("Headsets", StringComparison.OrdinalIgnoreCase))
            {
                score += 30;
            }

            if (ContainsAny(text, "144hz", "165hz", "170hz", "mechanical", "gaming mouse", "headset", "low latency"))
                score += 25;
        }
    }

    private static void ApplyFpsScore(
        AiProductRequirementDto requirement,
        string text,
        ref int score)
    {
        if (requirement.TargetFps is null)
            return;

        if (requirement.TargetFps >= 120)
        {
            if (ContainsAny(text, "120hz", "144hz", "165hz", "170hz", "promotion", "high refresh"))
                score += 40;
            else
                score -= 20;
        }

        if (requirement.TargetFps >= 144)
        {
            if (ContainsAny(text, "144hz", "165hz", "170hz"))
                score += 30;
            else if (text.Contains("120hz"))
                score -= 10;
        }
    }

    private static void ApplyConnectionScore(
        AiProductRequirementDto requirement,
        string text,
        ref int score)
    {
        if (string.IsNullOrWhiteSpace(requirement.PreferredConnectionType))
            return;

        var connection = requirement.PreferredConnectionType.ToLowerInvariant();

        if (connection == "wireless")
        {
            if (ContainsAny(text, "wireless", "bluetooth"))
                score += 35;
            else
                score -= 15;
        }

        if (connection == "wired")
        {
            if (ContainsAny(text, "wired", "usb", "cable"))
                score += 25;
        }
    }

    private static void ApplyKeyboardMouseScore(
        AiProductCandidateDto product,
        AiProductRequirementDto requirement,
        string text,
        ref int score)
    {
        if (product.CategoryName.Equals("Keyboards", StringComparison.OrdinalIgnoreCase))
        {
            if (requirement.WantsMechanicalKeyboard && ContainsAny(text, "mechanical", "switch", "hot-swappable"))
                score += 45;

            if (ContainsAny(text, "compact", "60 percent", "rgb", "gaming"))
                score += 15;
        }

        if (product.CategoryName.Equals("Mice", StringComparison.OrdinalIgnoreCase))
        {
            if (ContainsAny(text, "high dpi", "dpi", "lightweight", "gaming", "sensor", "programmable"))
                score += 25;

            if (requirement.WantsWireless && text.Contains("wireless"))
                score += 30;
        }
    }

    private static void ApplyCameraBatteryPortableScore(
        AiProductRequirementDto requirement,
        string text,
        ref int score)
    {
        if (requirement.WantsGoodCamera && ContainsAny(text, "camera", "pixel", "iphone", "galaxy", "pro"))
            score += 25;

        if (requirement.WantsLongBattery && ContainsAny(text, "battery", "large battery", "long battery", "5000"))
            score += 25;

        if (requirement.WantsPortable && ContainsAny(text, "lightweight", "slim", "portable", "compact"))
            score += 25;
    }

    private static void ApplyKeywordScore(
        AiProductRequirementDto requirement,
        string text,
        ref int score)
    {
        foreach (var keyword in requirement.Keywords)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                continue;

            var normalizedKeyword = keyword.ToLowerInvariant();

            if (normalizedKeyword.Length < 3)
                continue;

            if (text.Contains(normalizedKeyword))
                score += 5;
        }
    }

    private static void ApplyPricePreferenceScore(
        AiProductCandidateDto product,
        AiProductRequirementDto requirement,
        string promptLower,
        ref int score)
    {
        var wantsCheap = requirement.WantsCheap ||
                         ContainsAny(promptLower, "ucuz", "budget", "cheap", "münasib", "munasib", "sərfəli", "serfeli");

        var wantsPremium = requirement.WantsPremium ||
                           ContainsAny(promptLower, "premium", "güclü", "guclu", "flagship", "professional");

        if (wantsCheap)
        {
            if (product.Price <= 500)
                score += 40;
            else if (product.Price <= 1000)
                score += 25;
            else
                score -= 20;
        }

        if (wantsPremium)
        {
            if (product.Price >= 900)
                score += 25;
            else
                score -= 5;
        }
    }

    private static string? MapProductTypeToCategory(string productType)
    {
        return productType switch
        {
            "Phone" => "Phones",
            "Laptop" => "Laptops",
            "Tablet" => "Tablets",
            "Monitor" => "Monitors",
            "Keyboard" => "Keyboards",
            "Mouse" => "Mice",
            "Headset" => "Headsets",
            "Accessory" => "Accessories",
            _ => null
        };
    }

    private static string? DetectBrand(string lower)
    {
        var brands = new Dictionary<string, string>
        {
            ["apple"] = "Apple",
            ["iphone"] = "Apple",
            ["macbook"] = "Apple",
            ["ipad"] = "Apple",
            ["samsung"] = "Samsung",
            ["xiaomi"] = "Xiaomi",
            ["redmi"] = "Xiaomi",
            ["poco"] = "Poco",
            ["oneplus"] = "OnePlus",
            ["nothing"] = "Nothing",
            ["google"] = "Google",
            ["pixel"] = "Google",
            ["lenovo"] = "Lenovo",
            ["asus"] = "ASUS",
            ["acer"] = "Acer",
            ["msi"] = "MSI",
            ["dell"] = "Dell",
            ["hp"] = "HP",
            ["microsoft"] = "Microsoft",
            ["lg"] = "LG",
            ["aoc"] = "AOC",
            ["benq"] = "BenQ",
            ["gigabyte"] = "Gigabyte",
            ["logitech"] = "Logitech",
            ["razer"] = "Razer",
            ["steelseries"] = "SteelSeries",
            ["hyperx"] = "HyperX",
            ["sony"] = "Sony",
            ["jbl"] = "JBL",
            ["corsair"] = "Corsair",
            ["keychron"] = "Keychron"
        };

        foreach (var brand in brands)
        {
            if (lower.Contains(brand.Key))
                return brand.Value;
        }

        return null;
    }

    private static bool IsAppleProduct(string text)
    {
        return ContainsAny(text, "apple", "iphone", "ipad", "macbook", "m2 chip", "m3 chip", "retina");
    }

    private static bool ContainsAny(string text, params string[] values)
    {
        return values.Any(text.Contains);
    }

    private static string BuildReason(
        AiProductCandidateDto product,
        AiProductRequirementDto requirement)
    {
        var osPart = string.IsNullOrWhiteSpace(requirement.PreferredOperatingSystem)
            ? string.Empty
            : $" İstənilən sistem: {requirement.PreferredOperatingSystem}.";

        var brandPart = string.IsNullOrWhiteSpace(requirement.PreferredBrand)
            ? string.Empty
            : $" Brand istəyi: {requirement.PreferredBrand}.";

        if (requirement.UsageType.Equals("Gaming", StringComparison.OrdinalIgnoreCase))
        {
            return product.CategoryName switch
            {
                "Phones" => $"Bu telefon gaming üçün uyğundur.{osPart}{brandPart} {product.Description}",
                "Laptops" => $"Bu laptop oyun üçün uyğundur.{osPart}{brandPart} {product.Description}",
                "Monitors" => $"Bu monitor gaming üçün uyğundur.{osPart}{brandPart} {product.Description}",
                "Mice" => $"Bu mouse gaming üçün uyğundur.{osPart}{brandPart} {product.Description}",
                "Keyboards" => $"Bu klaviatura gaming setup üçün uyğundur.{osPart}{brandPart} {product.Description}",
                "Headsets" => $"Bu headset gaming üçün uyğundur.{osPart}{brandPart} {product.Description}",
                _ => $"Bu məhsul gaming setup üçün uyğun ola bilər.{osPart}{brandPart} {product.Description}"
            };
        }

        if (requirement.UsageType.Equals("Programming", StringComparison.OrdinalIgnoreCase))
            return $"Bu məhsul proqramlaşdırma və produktiv işlər üçün uyğun ola bilər.{osPart}{brandPart} {product.Description}";

        if (requirement.UsageType.Equals("Office", StringComparison.OrdinalIgnoreCase) ||
            requirement.UsageType.Equals("Study", StringComparison.OrdinalIgnoreCase))
            return $"Bu məhsul gündəlik istifadə, ofis və dərs üçün uyğun ola bilər.{osPart}{brandPart} {product.Description}";

        if (requirement.UsageType.Equals("Design", StringComparison.OrdinalIgnoreCase))
            return $"Bu məhsul dizayn və vizual işlər üçün uyğun ola bilər.{osPart}{brandPart} {product.Description}";

        if (requirement.UsageType.Equals("Media", StringComparison.OrdinalIgnoreCase))
            return $"Bu məhsul media istifadəsi üçün uyğun ola bilər.{osPart}{brandPart} {product.Description}";

        return $"Bu məhsul sənin istəyinə uyğun ola bilər.{osPart}{brandPart} {product.Description}";
    }
}

public static class AiProductRequirementExtensions
{
    public static void ProductsSafeNormalize(this AiProductRequirementDto requirement)
    {
        requirement.ProductType = Normalize(requirement.ProductType);
        requirement.UsageType = Normalize(requirement.UsageType);
        requirement.PreferredOperatingSystem = NormalizeNullable(requirement.PreferredOperatingSystem);
        requirement.PreferredBrand = NormalizeNullable(requirement.PreferredBrand);
        requirement.PreferredConnectionType = NormalizeNullable(requirement.PreferredConnectionType);

        requirement.Keywords = requirement.Keywords
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(12)
            .ToList();
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "Unknown"
            : value.Trim();
    }

    private static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}