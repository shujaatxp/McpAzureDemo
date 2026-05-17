using ModelContextProtocol.Server;

namespace McpServer.Console.Tools;

[McpServerToolType]
public class StringTools
{
    [McpServerTool]
    public string Reverse(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        char[] arr = input.ToCharArray();
        Array.Reverse(arr);
        return new string(arr);
    }
}