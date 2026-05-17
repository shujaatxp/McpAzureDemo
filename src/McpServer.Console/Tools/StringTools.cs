using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
public class StringTools
{
    [McpServerTool, Description("Reverse a string")]
    public string Reverse(string input)
    {
        var rvStr = new string(input.Reverse().ToArray());
        return "reversed string"+rvStr;

    }
}