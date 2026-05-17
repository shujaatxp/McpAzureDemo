using Microsoft.AspNetCore.Mvc;
using AgentApi.Services;

namespace AgentApi.Controllers;

[ApiController]
[Route("api/tools")]
public class ToolsController : ControllerBase
{
    private readonly AzureOpenAiAgent _agent;

    public ToolsController(AzureOpenAiAgent agent)
    {
        _agent = agent;
    }

    [HttpGet]
    public async Task<IActionResult> GetTools()
    {
        var tools = await _agent.GetToolsAsync();
        return Ok(tools);
    }
}