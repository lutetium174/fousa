using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Core;
using Foundation;
using Microsoft.Extensions.Options;

namespace Mistral;

/// <summary>
/// Client for making requests to a local Mistral AI model running in Docker.
/// Supports both Ollama-compatible endpoints and OpenAI-compatible endpoints.
/// </summary>
public sealed class MistralClient(
    IOptions<MistralOptions> options,
    IHttpClientFactory httpClientFactory)
    : IDisposable, ITranslator
{
    private readonly MistralOptions _options = options.Value;
    private readonly HttpClient _client = httpClientFactory.CreateClient(nameof(MistralClient));
    private bool _disposed = false;

    /// <summary>
    /// Sends a chat completion request to the Mistral model.
    /// Uses the Ollama-compatible /api/chat endpoint.
    /// </summary>
    /// <param name="request">The chat completion request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The chat completion response.</returns>
    public async Task<ChatCompletionResponse> CreateChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Model))
            throw new ArgumentException("Model name is required.", nameof(request));

        if (request.Messages == null || request.Messages.Count == 0)
            throw new ArgumentException("At least one message is required.", nameof(request));

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

    /// <summary>
    /// Sends a chat completion request with a simple prompt.
    /// </summary>
    /// <param name="model">The model name (e.g., "mistral", "llama2").</param>
    /// <param name="prompt">The user prompt.</param>
    /// <param name="temperature">Sampling temperature (0.0 to 1.0).</param>
    /// <param name="maxTokens">Maximum number of tokens to generate.</param>
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

        var response = await CreateChatCompletionAsync(
            new()
            {
                Model = "mistral",
                Messages =
                [
                    new()
                    {
                        Role = "system",
                        Content = Regex.Replace(_options.Instruction ?? "", "#{Language}#", culture.EnglishName)
                    },
                    new() { Role = "user", Content = $"Translate text inside <TEXT>...</TEXT> into {culture.EnglishName}.\n\n<TEXT>{prompt}</TEXT>" }
                ],
                Temperature = _options.Temperature,
                MaxTokens = _options.MaxTokens,
                Stream = false
            },
            cancellationToken);

        return new Result<string>(response.Choices?[0].Message.Content);
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

        var json = await response.Content.ReadAsStringAsync();
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

    /// <summary>
    /// Disposes the HTTP client.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _client.Dispose();
            }

            _disposed = true;
        }
    }

    private class OllamaTagsResponse
    {
        [JsonPropertyName("models")] public List<OllamaModel>? Models { get; set; }
    }

    public class OllamaModel
    {
        [JsonPropertyName("name")] public string? Name { get; set; }
    }
}