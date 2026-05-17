using ModelContextProtocol.Server;
using System.ComponentModel;

namespace McpServer.Http.Tools;

[McpServerToolType]
public class CustomerTools
{
    [McpServerTool, Description("Returns customer list")]
    public IEnumerable<object> GetCustomers()
        => new[]
        {
            new { Id = 1, Name = "Ali", City = "Vilnius" },
            new { Id = 2, Name = "John", City = "Kaunas" }
        };
}