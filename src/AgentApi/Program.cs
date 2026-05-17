using AgentApi.Services;
using Azure;
using Azure.AI.OpenAI;
using ModelContextProtocol.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var config = builder.Configuration;

// =========================
// Azure OpenAI
// =========================
builder.Services.AddSingleton(sp =>
{
    var cfg = config.GetSection("AzureOpenAI").Get<AzureOpenAiSettings>();

    return new AzureOpenAIClient(
        new Uri(cfg!.Endpoint),
        new AzureKeyCredential(cfg.ApiKey));
});

// =========================
// MCP CLIENT (CORRECT HTTP TRANSPORT)
// =========================
builder.Services.AddSingleton<McpClient>(sp =>
{
    var endpoint = config["MCP:Endpoint"]!;

    var options = new HttpClientTransportOptions
    {
        Endpoint = new Uri(endpoint)
    };

    var transport = new HttpClientTransport(options);

    return McpClient.CreateAsync(transport)
        .GetAwaiter()
        .GetResult();
});

// =========================
// Services
// =========================
builder.Services.AddScoped<McpClientService>();
builder.Services.AddScoped<AzureOpenAiAgent>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();