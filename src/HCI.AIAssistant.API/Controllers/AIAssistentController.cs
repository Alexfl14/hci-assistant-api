using Microsoft.AspNetCore.Mvc;

using HCI.AIAssistant.API.models.DTOs.AIAssistantController;

namespace HCI.AIAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIAssistantController : ControllerBase
{
        [HttpPost("/message")]
        public async Task<ActionResult<AIAssistantControllerPostMessageResponseDTO>> PostMessage([FromBody] AIAssistantControllerPostMessageRequestDTO request)
        {
            AIAssistantControllerPostMessageResponseDTO response = new()
            {
                TextMessage = "HI!" + request.TextMessage
            };

            return Ok(response);
        }
}