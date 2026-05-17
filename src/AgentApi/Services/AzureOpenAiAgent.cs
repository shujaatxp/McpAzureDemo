using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.Text.Json;

namespace AgentApi.Services;

public class AzureOpenAiAgent
{
    private readonly ChatClient _chatClient;
    private readonly McpClientService _mcp;
    private readonly IConfiguration _config;

    public AzureOpenAiAgent(IConfiguration config, McpClientService mcp)
    {
        _config = config;
        _mcp = mcp;

        var endpoint = new Uri(config["AzureOpenAI:Endpoint"]!);
        var apiKey = config["AzureOpenAI:ApiKey"]!;
        var deployment = config["AzureOpenAI:Deployment"]!;

        var azureClient = new AzureOpenAIClient(
            endpoint,
            new AzureKeyCredential(apiKey)
        );

        _chatClient = azureClient.GetChatClient(deployment);
    }

    public async Task<string> AskAsync(string question)
    {
        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = 4096,
            Temperature = 0.7f,
            TopP = 1.0f
        };

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are an MCP AI agent."),
            new UserChatMessage(question)
        };

        // =========================
        // MCP TOOLS (SAFE MAPPING)
        // =========================
        var tools = await _mcp.GetToolsAsync();

        foreach (var tool in tools)
        {
            options.Tools.Add(ChatTool.CreateFunctionTool(
                functionName: tool.Name,
                functionDescription: tool.Description,
                functionParameters: BinaryData.FromObjectAsJson(tool.ProtocolTool.InputSchema)
            ));
        }

        // =========================
        // CALL MODEL
        // =========================
        var response = await _chatClient.CompleteChatAsync(messages, options);

        var result = response.Value;

        // =========================
        // TOOL EXECUTION LOOP (1 STEP)
        // =========================
        if (result.FinishReason == ChatFinishReason.ToolCalls)
        {
            foreach (var toolCall in result.ToolCalls)
            {
                // ✅ FIX: Json → Dictionary (IMPORTANT FIX)
                var args = JsonSerializer.Deserialize<Dictionary<string, object?>>(
                    toolCall.FunctionArguments
                ) ?? new Dictionary<string, object?>();

                var toolResult = await _mcp.CallTool(
                    toolCall.FunctionName,
                    args
                );

                return toolResult;
            }
        }

        return result.Content[0].Text;
    }
}