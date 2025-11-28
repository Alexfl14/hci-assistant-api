using HCI.AIAssistant.API.models.CustomTypes;

namespace HCI.AIAssistant.API.Services;

public interface ISecretsService
{
    public AIAssistantSecrets? AIAssistantSecrets { get; set; }
    public IoTHubSecrets? IoTHubSecrets { get; set; }
}