using ModelContextProtocol.Server;
using System.ComponentModel;

namespace McpServer.Http.Tools;

[McpServerToolType]
public class WeatherTools
{
    [McpServerTool,Description("Returns current weather for a city")]
    public object GetWeather(string city)
        => new { City = city, Temp = 22, Condition = "Sunny" };
}