using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace AgentApi.Services;

public class McpClientService
{
    private readonly McpClient _client;

    public McpClientService(McpClient client)
    {
        _client = client;
    }

    public async Task<IList<McpClientTool>> GetToolsAsync()
    {
        return await _client.ListToolsAsync();
    }

    public async Task<string> CallTool(string name, Dictionary<string, object?> args)
    {
        var result = await _client.CallToolAsync(name, args);

        var text = result.Content?
            .OfType<TextContentBlock>()
            .FirstOrDefault()?
            .Text;

        return text ?? System.Text.Json.JsonSerializer.Serialize(result.Content);
    }
}