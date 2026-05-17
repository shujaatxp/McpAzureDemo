using Microsoft.AspNetCore.Mvc;
using AgentApi.Services;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly AzureOpenAiAgent _agent;

    public ChatController(AzureOpenAiAgent agent)
    {
        _agent = agent;
    }

    [HttpGet]
    public async Task<string> Ask(string q)
        => await _agent.AskAsync(q);
}