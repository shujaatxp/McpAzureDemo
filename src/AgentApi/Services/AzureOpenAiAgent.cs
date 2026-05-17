using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.Text.Json;

namespace AgentApi.Services;

public class AzureOpenAiAgent
{
    private readonly ChatClient _chatClient;
    private readonly McpAggregatorService _mcp;

    public AzureOpenAiAgent(IConfiguration config, McpAggregatorService mcp)
    {
        _mcp = mcp;

        var azureClient = new AzureOpenAIClient(
            new Uri(config["AzureOpenAI:Endpoint"]!),
            new AzureKeyCredential(config["AzureOpenAI:ApiKey"]!)
        );

        _chatClient = azureClient.GetChatClient(config["AzureOpenAI:Deployment"]!);
    }

    public async Task<object> GetToolsAsync()
    {
        var tools = await _mcp.GetAllToolsAsync();

        return tools.Select(t => new
        {
            t.Name,
            t.Description,
            Schema = t.ProtocolTool.InputSchema
        });
    }

    public async Task<string> AskAsync(string question)
    {
        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = 4096,
            Temperature = 0.7f
        };

        var tools = await _mcp.GetAllToolsAsync();

        foreach (var tool in tools)
        {
            options.Tools.Add(ChatTool.CreateFunctionTool(
                tool.Name,
                tool.Description,
                BinaryData.FromString(tool.ProtocolTool.InputSchema.ToString())
            ));
        }

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are MCP agent"),
            new UserChatMessage(question)
        };

        var response = await _chatClient.CompleteChatAsync(messages, options);

        var result = response.Value;

        if (result.FinishReason == ChatFinishReason.ToolCalls)
        {
            foreach (var call in result.ToolCalls)
            {
                var args = JsonSerializer.Deserialize<Dictionary<string, object?>>(
                    call.FunctionArguments
                ) ?? new();

                return await _mcp.CallToolAsync(call.FunctionName, args);
            }
        }

        return result.Content[0].Text;
    }
}