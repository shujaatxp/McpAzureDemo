using AgentApi.Services;
using Azure;
using Azure.AI.OpenAI;
using ModelContextProtocol.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

// =========================
// Azure OpenAI
// =========================
builder.Services.AddSingleton(sp =>
{
    var cfg = builder.Configuration.GetSection("AzureOpenAI");

    return new AzureOpenAIClient(
        new Uri(cfg["Endpoint"]!),
        new AzureKeyCredential(cfg["ApiKey"]!)
    );
});

// =========================
// MCP Aggregator
// =========================
builder.Services.AddSingleton<McpAggregatorService>();

// =========================
// Agent
// =========================
builder.Services.AddScoped<AzureOpenAiAgent>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();