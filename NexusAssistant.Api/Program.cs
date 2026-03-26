using Microsoft.Extensions.Options;
using NexusAssistant.Api.Agents;
using NexusAssistant.Api.Config;
using NexusAssistant.Api.Memory;
using NexusAssistant.Api.Pipeline;
using NexusAssistant.Api.Providers;

var builder = WebApplication.CreateBuilder(args);

// Config
builder.Services.Configure<NexusConfig>(
    builder.Configuration.GetSection("Nexus"));

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Provider — lu depuis appsettings.json
var providerName = builder.Configuration["Nexus:DefaultProvider"];

builder.Services.AddSingleton<ILLMProvider>(sp =>
{
    var config = sp.GetRequiredService<IOptions<NexusConfig>>();

    return providerName switch
    {
        "OpenAI" => new OpenAIProvider(config),
        "Anthropic" => new AnthropicProvider(config),
        _ => new OllamaProvider(config)  // défaut
    };
});

// Mémoire
builder.Services.AddSingleton<ISessionStore, InMemorySessionStore>();

// Agents
builder.Services.AddSingleton<Clement>();
builder.Services.AddSingleton<Eric>();
builder.Services.AddSingleton<Fabienne>();
builder.Services.AddSingleton<Mauricette>();
builder.Services.AddSingleton<Oscar>();

// Pipeline
builder.Services.AddSingleton<AgentPipeline>();

// API
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.Use(async (context, next) =>
{
    context.Response.ContentType = "text/event-stream; charset=utf-8";
    await next();
});
app.UseCors();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();
app.Run("http://localhost:5100");