namespace Mercato.Application.Options;

public class OpenAiOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";
    public string ChatCompletionsUrl { get; set; } = "https://api.openai.com/v1/chat/completions";
}