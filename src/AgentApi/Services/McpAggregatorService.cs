using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace AgentApi.Services;

public class McpAggregatorService
{
    private readonly List<McpClient> _clients = new();

    public McpAggregatorService(IConfiguration config)
    {
        var servers = config.GetSection("MCP:Servers").Get<string[]>() ?? [];

        foreach (var server in servers)
        {
            var transport = new HttpClientTransport(new HttpClientTransportOptions
            {
                Endpoint = new Uri(server)
            });

            var client = McpClient.CreateAsync(transport)
                .GetAwaiter()
                .GetResult();

            _clients.Add(client);
        }
    }
    // =====================================================
    // GET ALL TOOLS (THIS FIXES YOUR "LIST TOOLS ISSUE")
    // =====================================================
    public async Task<List<McpClientTool>> GetAllToolsAsync()
    {
        var result = new List<McpClientTool>();

        foreach (var client in _clients)
        {
            var tools = await client.ListToolsAsync();
            result.AddRange(tools);
        }

        return result;
    }

    // =====================================================
    // CALL TOOL (ROUTES TO CORRECT SERVER)
    // =====================================================
    public async Task<string> CallToolAsync(string name, Dictionary<string, object?> args)
    {
        foreach (var client in _clients)
        {
            var tools = await client.ListToolsAsync();

            var tool = tools.FirstOrDefault(t => t.Name == name);

            if (tool != null)
            {
                var result = await client.CallToolAsync(name, args);

                var text = result.Content?
                    .OfType<TextContentBlock>()
                    .FirstOrDefault()?
                    .Text;

                return text ?? result.Content.ToString();
            }
        }

        return $"Tool not found: {name}";
    }
}