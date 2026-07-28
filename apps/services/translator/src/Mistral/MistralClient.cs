using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Core;
using Foundation;
using Microsoft.Extensions.Options;

namespace Mistral;

/// <summary>
/// Client for making requests to a local Mistral AI model running in Docker.
/// Supports both Ollama-compatible endpoints and OpenAI-compatible endpoints.
/// </summary>
public sealed partial class MistralClient(
    IOptions<MistralOptions> options,
    IHttpClientFactory httpClientFactory)
    : ITranslator
{
    private readonly MistralOptions _options = options.Value;
    private readonly HttpClient _client = httpClientFactory.CreateClient(nameof(MistralClient));

    /// <summary>
    /// Sends a chat completion request with a simple prompt.
    /// </summary>
    /// <param name="prompt">The user prompt.</param>
    /// <param name="culture">The language to translate to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The chat completion response.</returns>
    public async Task<Result<string>> Translate(
        string? prompt,
        CultureInfo culture,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return new Error<string>("Prompt is required.");

        var response = await ExecuteChatCompletionAsync(
            new()
            {
                Model = "mistral",
                Messages =
                [
                    new()
                    {
                        Role = "system",
                        Content = ReplaceLanguageRegex()
                            .Replace(_options.Instruction ?? "", culture.EnglishName)
                    },
                    new()
                    {
                        Role = "user",
                        Content = $"Translate text inside <TEXT>...</TEXT> into {culture.EnglishName}.\n\n<TEXT>{prompt}</TEXT>"
                    }
                ],
                Temperature = _options.Temperature,
                MaxTokens = _options.MaxTokens,
                Stream = false
            },
            cancellationToken);

        return new(response.Choices?[0].Message.Content);
    }

    /// <summary>
    /// Lists available models from the local Mistral API.
    /// Uses the /api/tags endpoint (Ollama-compatible).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of available model names.</returns>
    public async Task<IEnumerable<OllamaModel>> ListModelsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync("/api/tags", cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        try
        {
            var result = JsonSerializer.Deserialize<OllamaTagsResponse>(json, options);
            return result?.Models ?? Enumerable.Empty<OllamaModel>();
        }
        catch
        {
            var models = JsonSerializer.Deserialize<List<string>>(json, options);
            return models?.Select(m => new OllamaModel { Name = m }) ?? Enumerable.Empty<OllamaModel>();
        }
    }

    /// <summary>
    /// Checks if the local Mistral API is available.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the API is available, false otherwise.</returns>
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.GetAsync("/api/tags", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    
    private async Task<ChatCompletionResponse> ExecuteChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var response = await _client.PostAsJsonAsync(
            "/engines/v1/chat/completions",
            request,
            cancellationToken: cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                Converters = { new UnixTimeConverter() }
            },
            cancellationToken);
        
        return result ?? throw new JsonException("Failed to deserialize response.");
    }

    private static void ValidateRequest(ChatCompletionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Model))
            throw new ArgumentException("Model name is required.", nameof(request));

        if (request.Messages == null || request.Messages.Count == 0)
            throw new ArgumentException("At least one message is required.", nameof(request));
    }

    private record OllamaTagsResponse
    {
        public List<OllamaModel>? Models { get; init; }
    }

    public record OllamaModel
    {
        public string? Name { get; set; }
    }

    [GeneratedRegex("#{Language}#")]
    private static partial Regex ReplaceLanguageRegex();
}