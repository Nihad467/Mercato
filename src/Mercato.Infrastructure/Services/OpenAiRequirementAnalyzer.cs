using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Mercato.Application.Ai.Models;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Options;
using Microsoft.Extensions.Options;

namespace Mercato.Infrastructure.Services;

public class OpenAiRequirementAnalyzer : IAiRequirementAnalyzer
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;

    public OpenAiRequirementAnalyzer(
        HttpClient httpClient,
        IOptions<OpenAiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<AiProductRequirementDto> AnalyzeAsync(
        string prompt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be empty.", nameof(prompt));

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            return BuildFallbackRequirement(prompt);

        var systemPrompt = """
You are an AI requirement extractor for an electronics e-commerce store called Mercato.

Mercato ONLY sells technology / electronics products.

Supported product categories:
- Phones
- Laptops
- Tablets
- Monitors
- Keyboards
- Mice
- Headsets
- Accessories

Unsupported products:
washing machines, refrigerators, air conditioners, clothes, shoes, food, furniture, cars, cosmetics, perfume, home appliances, kitchen items.

Your job:
Convert the user's natural language request into strict JSON.

Allowed ProductType values:
Phone, Laptop, Tablet, Monitor, Keyboard, Mouse, Headset, Accessory, Unknown

Allowed UsageType values:
Gaming, Programming, Office, Study, Design, Media, General

Allowed PreferredOperatingSystem values:
Windows, macOS, Android, iOS, null

Allowed PreferredConnectionType values:
Wireless, Wired, Bluetooth, USB, null

Rules for product type:
- If user says "komputer", "computer", "comp", "pc", "notebook", "laptop" => ProductType = Laptop.
- If user says "telefon", "phone", "iphone", "android phone" => ProductType = Phone.
- If user says "planset", "planşet", "tablet", "ipad" => ProductType = Tablet.
- If user says "monitor", "screen", "ekran", "144hz", "165hz", "170hz" => ProductType = Monitor.
- If user says "klaviatura", "keyboard" => ProductType = Keyboard.
- If user says "siçan", "sican", "mouse" => ProductType = Mouse.
- If user says "qulaqlıq", "qulaqliq", "headset", "headphone", "earphone" => ProductType = Headset.
- If user says "charger", "adapter", "zaryadka", "cable", "kabel", "ssd", "hub", "router", "webcam", "kamera" => ProductType = Accessory.

Rules for usage type:
- If user mentions games, PUBG, GTA, Valorant, CS, CS2, FPS, gaming, oyun => UsageType = Gaming.
- If user mentions programming, coding, proqramlaşdırma, Visual Studio, backend, frontend, developer => UsageType = Programming.
- If user mentions office, study, dərs, tələbə, məktəb, university => UsageType = Office or Study.
- If user mentions design, Photoshop, video editing, montaj, render => UsageType = Design.
- If user mentions movie, film, Netflix, YouTube, music, media => UsageType = Media.
- If not clear => UsageType = General.

Rules for games:
- PUBG => GameName = PUBG.
- GTA => GameName = GTA.
- Valorant => GameName = Valorant.
- CS, CS2, Counter Strike => GameName = CS.

Rules for FPS / refresh rate:
- If user says 60 FPS or 60Hz => TargetFps = 60.
- If user says 120 FPS or 120Hz => TargetFps = 120.
- If user says 144 FPS or 144Hz => TargetFps = 144.
- If user says 165Hz => TargetFps = 165.
- If user says 170Hz => TargetFps = 170.

Rules for operating system:
- If user asks for Windows, Windows laptop, Windows computer => PreferredOperatingSystem = Windows.
- If user asks for MacBook, macOS, Apple laptop => PreferredOperatingSystem = macOS.
- If user asks for Android, Samsung, Xiaomi, Redmi, Poco, OnePlus, Pixel => PreferredOperatingSystem = Android.
- If user asks for iPhone or iOS => PreferredOperatingSystem = iOS.
- If no OS is mentioned => PreferredOperatingSystem = null.

Rules for brand:
Extract PreferredBrand if user mentions:
Apple, iPhone, MacBook, iPad, Samsung, Xiaomi, Redmi, Poco, OnePlus, Nothing, Google, Pixel,
Lenovo, ASUS, Acer, MSI, Dell, HP, Microsoft, LG, AOC, BenQ, Gigabyte,
Logitech, Razer, SteelSeries, HyperX, Sony, JBL, Corsair, Keychron, Anker, Baseus, UGREEN, TP-Link, Elgato.

Rules for price:
- If user says ucuz, münasib, sərfəli, budget, cheap => WantsCheap = true.
- If user says premium, flagship, güclü, guclu, professional, pro => WantsPremium = true.
- Extract Budget if user writes amount like 500, 1000, 1500, 2000, manat, dollar, usd.
- If amount is clearly FPS or Hz, do not treat it as budget.

Rules for connection:
- If user says wireless, bluetooth, simsiz => WantsWireless = true and PreferredConnectionType = Wireless.
- If user says wired, cable, kabel, USB => PreferredConnectionType = Wired.
- If user says Bluetooth specifically => PreferredConnectionType = Bluetooth.

Rules for keyboard:
- If user asks for mechanical keyboard, mexaniki, switch, red switch, blue switch, brown switch => WantsMechanicalKeyboard = true.

Rules for portability:
- If user asks lightweight, yüngül, yungul, portable, easy to carry, daşımaq rahat => WantsPortable = true.

Rules for phone/camera/battery:
- If user asks good camera, kamera, şəkil, sekil, photo => WantsGoodCamera = true.
- If user asks long battery, batareya, battery, zaryadka saxlasın => WantsLongBattery = true.

Unsupported request rules:
- If the user asks for anything outside electronics, set:
  ProductType = Unknown
  UsageType = General
  IsSupported = false
  UnsupportedReason = "Mercato only sells technology/electronics products."
- Unsupported examples:
  paltaryuyan, washing machine, soyuducu, refrigerator, kondisioner, clothes, paltar, shoes, ayaqqabı, food, yemək, mebel, furniture, car, maşın, cosmetics, perfume.

Return JSON only.
Do not include markdown.
Do not include explanation.

JSON shape:
{
  "intent": "short English intent",
  "productType": "Phone",
  "usageType": "Gaming",
  "gameName": "PUBG",
  "targetFps": 120,
  "budget": null,
  "preferredOperatingSystem": "Android",
  "preferredBrand": "Samsung",
  "preferredConnectionType": "Wireless",
  "wantsCheap": false,
  "wantsPremium": true,
  "wantsWireless": true,
  "wantsMechanicalKeyboard": false,
  "wantsPortable": false,
  "wantsGoodCamera": false,
  "wantsLongBattery": false,
  "isSupported": true,
  "unsupportedReason": null,
  "keywords": ["PUBG", "120 FPS", "gaming phone"]
}
""";

        var payload = new
        {
            model = _options.Model,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = systemPrompt
                },
                new
                {
                    role = "user",
                    content = prompt
                }
            },
            temperature = 0.1,
            response_format = new
            {
                type = "json_object"
            }
        };

        var json = JsonSerializer.Serialize(payload);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            _options.ChatCompletionsUrl);

        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _options.ApiKey);

        request.Content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            return BuildFallbackRequirement(prompt);

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        using var document = JsonDocument.Parse(responseJson);

        var content = document
            .RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(content))
            return BuildFallbackRequirement(prompt);

        AiProductRequirementDto? aiRequirement;

        try
        {
            aiRequirement = JsonSerializer.Deserialize<AiProductRequirementDto>(
                content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch
        {
            return BuildFallbackRequirement(prompt);
        }

        return NormalizeRequirement(aiRequirement, prompt);
    }

    private static AiProductRequirementDto NormalizeRequirement(
        AiProductRequirementDto? requirement,
        string prompt)
    {
        var fallback = BuildFallbackRequirement(prompt);

        if (requirement is null)
            return fallback;

        if (!fallback.IsSupported)
            return fallback;

        if (string.IsNullOrWhiteSpace(requirement.ProductType) ||
            requirement.ProductType.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
        {
            requirement.ProductType = fallback.ProductType;
        }

        if (string.IsNullOrWhiteSpace(requirement.UsageType))
            requirement.UsageType = fallback.UsageType;

        if (string.IsNullOrWhiteSpace(requirement.Intent))
            requirement.Intent = fallback.Intent;

        if (string.IsNullOrWhiteSpace(requirement.GameName))
            requirement.GameName = fallback.GameName;

        if (requirement.TargetFps is null)
            requirement.TargetFps = fallback.TargetFps;

        if (requirement.Budget is null)
            requirement.Budget = fallback.Budget;

        if (string.IsNullOrWhiteSpace(requirement.PreferredOperatingSystem))
            requirement.PreferredOperatingSystem = fallback.PreferredOperatingSystem;

        if (string.IsNullOrWhiteSpace(requirement.PreferredBrand))
            requirement.PreferredBrand = fallback.PreferredBrand;

        if (string.IsNullOrWhiteSpace(requirement.PreferredConnectionType))
            requirement.PreferredConnectionType = fallback.PreferredConnectionType;

        requirement.WantsCheap = requirement.WantsCheap || fallback.WantsCheap;
        requirement.WantsPremium = requirement.WantsPremium || fallback.WantsPremium;
        requirement.WantsWireless = requirement.WantsWireless || fallback.WantsWireless;
        requirement.WantsMechanicalKeyboard = requirement.WantsMechanicalKeyboard || fallback.WantsMechanicalKeyboard;
        requirement.WantsPortable = requirement.WantsPortable || fallback.WantsPortable;
        requirement.WantsGoodCamera = requirement.WantsGoodCamera || fallback.WantsGoodCamera;
        requirement.WantsLongBattery = requirement.WantsLongBattery || fallback.WantsLongBattery;

        if (requirement.Keywords.Count == 0)
            requirement.Keywords = fallback.Keywords;

        requirement = NormalizeFields(requirement);

        if (requirement.ProductType == "Unknown")
        {
            requirement.IsSupported = false;
            requirement.UnsupportedReason = "Mercato only sells technology/electronics products.";
            requirement.Intent = "Unsupported product request";
        }

        return requirement;
    }

    private static AiProductRequirementDto BuildFallbackRequirement(string prompt)
    {
        var lower = prompt.ToLowerInvariant();

        var requirement = new AiProductRequirementDto
        {
            Intent = "Electronics product recommendation",
            ProductType = "Unknown",
            UsageType = "General",
            IsSupported = true,
            UnsupportedReason = null,
            Keywords = prompt
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x.Length >= 2)
                .Take(12)
                .ToList()
        };

        if (IsUnsupported(lower))
        {
            requirement.ProductType = "Unknown";
            requirement.UsageType = "General";
            requirement.GameName = null;
            requirement.TargetFps = null;
            requirement.IsSupported = false;
            requirement.UnsupportedReason = "Mercato only sells technology/electronics products.";
            requirement.Intent = "Unsupported product request";

            return requirement;
        }

        DetectProductType(lower, requirement);
        DetectUsage(lower, requirement);
        DetectGame(lower, requirement);
        DetectOperatingSystem(lower, requirement);
        DetectBrand(lower, requirement);
        DetectConnection(lower, requirement);
        DetectKeyboardPreference(lower, requirement);
        DetectExtraPreferences(lower, requirement);
        DetectBudget(lower, requirement);
        DetectFps(lower, requirement);
        DetectPricePreference(lower, requirement);

        requirement.IsSupported = requirement.ProductType != "Unknown";

        if (!requirement.IsSupported)
        {
            requirement.UnsupportedReason = "Mercato only sells technology/electronics products.";
            requirement.Intent = "Unsupported product request";

            return requirement;
        }

        requirement.Intent = $"{requirement.UsageType} {requirement.ProductType} recommendation";

        return NormalizeFields(requirement);
    }

    private static bool IsUnsupported(string lower)
    {
        var unsupportedWords = new[]
        {
            "paltaryuyan",
            "washing machine",
            "soyuducu",
            "fridge",
            "refrigerator",
            "kondisioner",
            "air conditioner",
            "paltar",
            "clothes",
            "ayaqqabı",
            "ayaqqabi",
            "shoes",
            "qida",
            "yemək",
            "yemek",
            "food",
            "mebel",
            "furniture",
            "divan",
            "sofa",
            "stol",
            "table",
            "stul",
            "chair",
            "maşın",
            "masin",
            "avtomobil",
            "car",
            "kosmetika",
            "cosmetics",
            "ətir",
            "etir",
            "perfume",
            "qab",
            "qazan",
            "mətbəx",
            "metbex",
            "kitchen"
        };

        return unsupportedWords.Any(word => lower.Contains(word));
    }

    private static void DetectProductType(
        string lower,
        AiProductRequirementDto requirement)
    {
        if (ContainsAny(lower,
                "telefon",
                "phone",
                "iphone",
                "android",
                "samsung galaxy",
                "oneplus",
                "xiaomi",
                "redmi",
                "poco",
                "pixel"))
        {
            requirement.ProductType = "Phone";
        }

        if (ContainsAny(lower,
                "laptop",
                "komputer",
                "computer",
                "comp",
                "pc",
                "notebook",
                "macbook"))
        {
            requirement.ProductType = "Laptop";
        }

        if (ContainsAny(lower,
                "planset",
                "planşet",
                "tablet",
                "ipad"))
        {
            requirement.ProductType = "Tablet";
        }

        if (ContainsAny(lower,
                "monitor",
                "screen",
                "ekran",
                "144hz",
                "165hz",
                "170hz"))
        {
            requirement.ProductType = "Monitor";
        }

        if (ContainsAny(lower,
                "klaviatura",
                "keyboard"))
        {
            requirement.ProductType = "Keyboard";
        }

        if (ContainsAny(lower,
                "siçan",
                "sican",
                "mouse"))
        {
            requirement.ProductType = "Mouse";
        }

        if (ContainsAny(lower,
                "qulaqlıq",
                "qulaqliq",
                "headset",
                "headphone",
                "earphone",
                "sound",
                "audio"))
        {
            requirement.ProductType = "Headset";
        }

        if (ContainsAny(lower,
                "charger",
                "adapter",
                "zaryadka",
                "şarj",
                "sarj",
                "kabel",
                "cable",
                "ssd",
                "hub",
                "router",
                "webcam",
                "kamera",
                "camera"))
        {
            requirement.ProductType = "Accessory";
        }

        if (ContainsAny(lower, "pubg"))
            requirement.ProductType = "Phone";

        if (ContainsAny(lower, "gta"))
            requirement.ProductType = "Laptop";

        if (ContainsAny(lower, "valorant", "cs2", "cs go", "counter"))
        {
            if (requirement.ProductType == "Unknown")
                requirement.ProductType = "Laptop";
        }
    }

    private static void DetectUsage(
        string lower,
        AiProductRequirementDto requirement)
    {
        if (ContainsAny(lower,
                "oyun",
                "gaming",
                "pubg",
                "gta",
                "valorant",
                "cs",
                "fps"))
        {
            requirement.UsageType = "Gaming";
        }

        if (ContainsAny(lower,
                "program",
                "coding",
                "proqram",
                "developer",
                "visual studio",
                "backend",
                "frontend",
                "software"))
        {
            requirement.UsageType = "Programming";
        }

        if (ContainsAny(lower,
                "ofis",
                "office",
                "dərs",
                "ders",
                "study",
                "telebe",
                "tələbə",
                "school",
                "university"))
        {
            requirement.UsageType = "Office";
        }

        if (ContainsAny(lower,
                "design",
                "dizayn",
                "photoshop",
                "video montaj",
                "editing",
                "render",
                "graphic"))
        {
            requirement.UsageType = "Design";
        }

        if (ContainsAny(lower,
                "film",
                "youtube",
                "netflix",
                "media",
                "music",
                "kino"))
        {
            requirement.UsageType = "Media";
        }
    }

    private static void DetectGame(
        string lower,
        AiProductRequirementDto requirement)
    {
        if (lower.Contains("pubg"))
            requirement.GameName = "PUBG";

        if (lower.Contains("gta"))
            requirement.GameName = "GTA";

        if (lower.Contains("valorant"))
            requirement.GameName = "Valorant";

        if (ContainsAny(lower, "cs2", "cs go", "counter strike", "counter"))
            requirement.GameName = "CS";

        if (!string.IsNullOrWhiteSpace(requirement.GameName))
            requirement.UsageType = "Gaming";
    }

    private static void DetectOperatingSystem(
        string lower,
        AiProductRequirementDto requirement)
    {
        if (ContainsAny(lower, "windows", "vindovs", "win "))
            requirement.PreferredOperatingSystem = "Windows";

        if (ContainsAny(lower, "macbook", "macos", "mac os", "apple laptop"))
            requirement.PreferredOperatingSystem = "macOS";

        if (ContainsAny(lower,
                "android",
                "samsung",
                "xiaomi",
                "redmi",
                "poco",
                "oneplus",
                "nothing phone",
                "pixel"))
        {
            requirement.PreferredOperatingSystem = "Android";
        }

        if (ContainsAny(lower, "iphone", "ios"))
            requirement.PreferredOperatingSystem = "iOS";
    }

    private static void DetectBrand(
        string lower,
        AiProductRequirementDto requirement)
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
            ["keychron"] = "Keychron",
            ["anker"] = "Anker",
            ["baseus"] = "Baseus",
            ["ugreen"] = "UGREEN",
            ["tp-link"] = "TP-Link",
            ["tplink"] = "TP-Link",
            ["elgato"] = "Elgato"
        };

        foreach (var brand in brands)
        {
            if (lower.Contains(brand.Key))
            {
                requirement.PreferredBrand = brand.Value;
                return;
            }
        }
    }

    private static void DetectConnection(
        string lower,
        AiProductRequirementDto requirement)
    {
        if (ContainsAny(lower, "wireless", "bluetooth", "simsiz"))
        {
            requirement.WantsWireless = true;
            requirement.PreferredConnectionType = "Wireless";
        }

        if (ContainsAny(lower, "wired", "kabel", "cable", "usb"))
            requirement.PreferredConnectionType = "Wired";
    }

    private static void DetectKeyboardPreference(
        string lower,
        AiProductRequirementDto requirement)
    {
        if (ContainsAny(lower,
                "mechanical",
                "mexaniki",
                "switch",
                "red switch",
                "blue switch",
                "brown switch"))
        {
            requirement.WantsMechanicalKeyboard = true;
        }
    }

    private static void DetectExtraPreferences(
        string lower,
        AiProductRequirementDto requirement)
    {
        if (ContainsAny(lower,
                "yüngül",
                "yungul",
                "lightweight",
                "portable",
                "daşımaq",
                "dasimaq",
                "compact"))
        {
            requirement.WantsPortable = true;
        }

        if (ContainsAny(lower,
                "kamera",
                "camera",
                "şəkil",
                "sekil",
                "photo",
                "video"))
        {
            requirement.WantsGoodCamera = true;
        }

        if (ContainsAny(lower,
                "battery",
                "batareya",
                "zaryadka saxlasın",
                "long battery",
                "batareyası yaxşı"))
        {
            requirement.WantsLongBattery = true;
        }
    }

    private static void DetectBudget(
        string lower,
        AiProductRequirementDto requirement)
    {
        var matches = Regex.Matches(lower, @"\b\d{2,5}\b");

        foreach (Match match in matches)
        {
            if (!decimal.TryParse(match.Value, out var number))
                continue;

            if (number is 60 or 90 or 120 or 144 or 165 or 170 or 240)
                continue;

            if (number >= 50)
            {
                requirement.Budget = number;
                return;
            }
        }
    }

    private static void DetectFps(
        string lower,
        AiProductRequirementDto requirement)
    {
        if (lower.Contains("240"))
            requirement.TargetFps = 240;
        else if (lower.Contains("170"))
            requirement.TargetFps = 170;
        else if (lower.Contains("165"))
            requirement.TargetFps = 165;
        else if (lower.Contains("144"))
            requirement.TargetFps = 144;
        else if (lower.Contains("120"))
            requirement.TargetFps = 120;
        else if (lower.Contains("90"))
            requirement.TargetFps = 90;
        else if (lower.Contains("60"))
            requirement.TargetFps = 60;
    }

    private static void DetectPricePreference(
        string lower,
        AiProductRequirementDto requirement)
    {
        if (ContainsAny(lower,
                "ucuz",
                "münasib",
                "munasib",
                "budget",
                "cheap",
                "sərfəli",
                "serfeli"))
        {
            requirement.WantsCheap = true;
        }

        if (ContainsAny(lower,
                "premium",
                "flagship",
                "güclü",
                "guclu",
                "professional",
                "pro "))
        {
            requirement.WantsPremium = true;
        }
    }

    private static AiProductRequirementDto NormalizeFields(
        AiProductRequirementDto requirement)
    {
        requirement.ProductType = NormalizeProductType(requirement.ProductType);
        requirement.UsageType = NormalizeUsageType(requirement.UsageType);
        requirement.PreferredOperatingSystem = NormalizeOperatingSystem(requirement.PreferredOperatingSystem);
        requirement.PreferredConnectionType = NormalizeConnectionType(requirement.PreferredConnectionType);

        requirement.PreferredBrand = string.IsNullOrWhiteSpace(requirement.PreferredBrand)
            ? null
            : requirement.PreferredBrand.Trim();

        requirement.GameName = string.IsNullOrWhiteSpace(requirement.GameName)
            ? null
            : requirement.GameName.Trim();

        requirement.Keywords = requirement.Keywords
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(12)
            .ToList();

        return requirement;
    }

    private static string NormalizeProductType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Unknown";

        var normalized = value.Trim().ToLowerInvariant();

        return normalized switch
        {
            "phone" or "phones" => "Phone",
            "laptop" or "laptops" or "computer" or "pc" => "Laptop",
            "tablet" or "tablets" => "Tablet",
            "monitor" or "monitors" => "Monitor",
            "keyboard" or "keyboards" => "Keyboard",
            "mouse" or "mice" => "Mouse",
            "headset" or "headsets" or "headphone" or "headphones" => "Headset",
            "accessory" or "accessories" => "Accessory",
            _ => "Unknown"
        };
    }

    private static string NormalizeUsageType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "General";

        var normalized = value.Trim().ToLowerInvariant();

        return normalized switch
        {
            "gaming" => "Gaming",
            "programming" => "Programming",
            "office" => "Office",
            "study" => "Study",
            "design" => "Design",
            "media" => "Media",
            "general" => "General",
            _ => "General"
        };
    }

    private static string? NormalizeOperatingSystem(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim().ToLowerInvariant();

        return normalized switch
        {
            "windows" => "Windows",
            "macos" or "mac os" => "macOS",
            "android" => "Android",
            "ios" => "iOS",
            _ => null
        };
    }

    private static string? NormalizeConnectionType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim().ToLowerInvariant();

        return normalized switch
        {
            "wireless" => "Wireless",
            "bluetooth" => "Bluetooth",
            "wired" => "Wired",
            "usb" => "USB",
            _ => null
        };
    }

    private static bool ContainsAny(string text, params string[] values)
    {
        return values.Any(text.Contains);
    }
}