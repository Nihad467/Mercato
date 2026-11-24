namespace Mercato.Application.Options;

public class StripeOptions
{
    public const string SectionName = "Stripe";

    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public string Currency { get; set; } = "azn";

    public bool HasValidSecretKey =>
        SecretKey?.StartsWith("sk_test_", StringComparison.Ordinal) == true
        || SecretKey?.StartsWith("sk_live_", StringComparison.Ordinal) == true;

    public bool HasValidWebhookSecret =>
        WebhookSecret?.StartsWith("whsec_", StringComparison.Ordinal) == true
        && WebhookSecret.Length > 12;

    public bool HasValidReturnUrls => IsValidReturnUrl(SuccessUrl) && IsValidReturnUrl(CancelUrl);

    private static bool IsValidReturnUrl(string? template)
    {
        if (string.IsNullOrWhiteSpace(template))
            return false;

        var sample = template
            .Replace("{ORDER_ID}", "1", StringComparison.OrdinalIgnoreCase)
            .Replace("{CHECKOUT_SESSION_ID}", "session", StringComparison.Ordinal);

        return Uri.TryCreate(sample, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttps || (uri.Scheme == Uri.UriSchemeHttp && uri.Host == "localhost"));
    }
}
