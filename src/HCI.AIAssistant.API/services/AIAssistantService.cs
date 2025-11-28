using Azure;
using Azure.AI.OpenAI.Assistants;

namespace HCI.AIAssistant.API.Services;

public class AIAssistantService : IAIAssistantService
{
    private const int _DELAY_IN_MS = 500;

    private readonly ISecretsService _secretsService;
    private readonly AssistantsClient? _assistantsClient;
    private readonly string? _id;

    public AIAssistantService(ISecretsService secretsService)
    {
        _secretsService = secretsService;

        var endPoint = _secretsService.AIAssistantSecrets?.EndPoint?.Trim();
        var key = _secretsService.AIAssistantSecrets?.Key?.Trim();
        _id = _secretsService.AIAssistantSecrets?.Id?.Trim();
        
        // If any required config is missing, set client to null
        if (string.IsNullOrWhiteSpace(endPoint) || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(_id))
        {
            Console.WriteLine("AI Assistant is not configured - using test mode");
            _assistantsClient = null;
            return;
        }
        
        Console.WriteLine($"ENDPOINT: '{endPoint}'");
        Console.WriteLine($"KEY: '{key}'");
        Console.WriteLine($"ID: '{_id}'");
        
        // Validate URI before using it
        if (!Uri.TryCreate(endPoint, UriKind.Absolute, out var uri))
        {
            Console.WriteLine($"ERROR: Invalid endpoint URI format: '{endPoint}'");
            _assistantsClient = null;
            return;
        }
        
        try
        {
            _assistantsClient = new AssistantsClient(
                uri,
                new AzureKeyCredential(key)
            );
            Console.WriteLine("AssistantsClient initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR initializing AssistantsClient: {ex.Message}");
            _assistantsClient = null;
        }
    }

    public async Task<string> SendMessageAndGetResponseAsync(string message)
    {
        // If client is not initialized, return a test response
        if (_assistantsClient == null || _id == null)
        {
            // Return an echo response for testing
            return $"Echo (test mode): {message}";
        }

        try
        {
            AssistantThread assistantThread = await _assistantsClient.CreateThreadAsync();
            ThreadMessage threadMessage = await _assistantsClient.CreateMessageAsync(assistantThread.Id, MessageRole.User, message);
            ThreadRun threadRun = await _assistantsClient.CreateRunAsync(assistantThread.Id, new CreateRunOptions(_id));

            do
            {
                threadRun = await _assistantsClient.GetRunAsync(assistantThread.Id, threadRun.Id);
                await Task.Delay(TimeSpan.FromMilliseconds(_DELAY_IN_MS));
            }
            while (threadRun.Status == RunStatus.Queued || threadRun.Status == RunStatus.InProgress);

            if (threadRun.Status != RunStatus.Completed)
            {
                _ = _assistantsClient.DeleteThreadAsync(assistantThread.Id);
                return "Error: Assistant run did not complete successfully!";
            }

            PageableList<ThreadMessage> messagesList = await _assistantsClient.GetMessagesAsync(assistantThread.Id);
            ThreadMessage? lastAssistantMessage = messagesList.FirstOrDefault(
                m => m.Role == MessageRole.Assistant
            );
            if (lastAssistantMessage?.ContentItems?.FirstOrDefault() is not MessageTextContent messageTextContent)
            {
                _ = _assistantsClient.DeleteThreadAsync(assistantThread.Id);
                return "Error: No valid response from assistant!";
            }

            _ = _assistantsClient.DeleteThreadAsync(assistantThread.Id);
            return messageTextContent.Text;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in SendMessageAndGetResponseAsync: {ex.Message}");
            return $"Error: {ex.Message}";
        }
    }
}
