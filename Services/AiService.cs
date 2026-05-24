using LineNoteBot.Models.Dtos;
using LineNoteBot.Models.Entities;
using LineNoteBot.Services.Interfaces;

namespace LineNoteBot.Services;

public class AiService(HttpClient http, IConfiguration config, ILogger<AiService> logger) : IAiService
{
    public async Task<string?> AskAsync(string question, IEnumerable<string> content)
    {
        if (IsSuspiciousQuestion(question))
        {
            logger.LogWarning("Suspicious question input blocked");
            return null;
        }

        try
        {
            var model = config["Groq:Model"];
            var apiKey = config["Groq:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                logger.LogError("Groq API key is missing");
                return null;
            }

            const string endpoint = "openai/v1/chat/completions";
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var formattedNotes = content.Select((ct, index) => $"[NOTE {index + 1}]\n{ct}");
            var notesText = string.Join("\n\n", formattedNotes);

            var systemPrompt = $"""
                You are a note retrieval assistant.

                Your task:
                - Find the notes most relevant to the user's question
                - Return ONLY the exact original note contents
                - Do NOT summarize
                - Do NOT explain
                - Do NOT rewrite
                - Do NOT add extra text
                - Do NOT answer using external knowledge
                - If no relevant notes are found, reply exactly:
                No matching notes found.

                Notes:
                ---
                {notesText}
                ---
            """;

            var requestBody = new
            {
                model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = question }
                },
                temperature = 0,
                max_tokens = 500
            };

            var response = await http.PostAsJsonAsync(endpoint, requestBody);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                logger.LogWarning("Groq API failed. Status: {StatusCode}, Response: {Response}", response.StatusCode, error);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<GroqResponse>();

            return result?.Choices
                ?.FirstOrDefault()
                ?.Message
                ?.Content
                ?.Trim();
        }
        catch (TaskCanceledException ex)
        {
            logger.LogWarning(ex, "Groq API timeout");
            return null;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Groq API request failed");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while calling Groq API");
            return null;
        }
    }

    private bool IsSuspiciousQuestion(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;
        var patterns = new[]
        {
            "ignore previous",
            "reveal",
            "system prompt",
            "api key",
            "authorization",
            "bypass",
            "developer mode",
            "ignore instructions",
            "jailbreak",
            "do anything now"
        };

        return patterns.Any(p => input.Contains(p, StringComparison.OrdinalIgnoreCase));
    }
}
