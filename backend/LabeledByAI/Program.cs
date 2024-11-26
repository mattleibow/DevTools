using LabeledByAI.Services;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Services.AddSingleton<AIChatClient>();
builder.Services.AddSingleton<GitHubBestLabelAIChatClient>();

builder.AddServiceDefaults();

builder.AddAzureOpenAIClient("openai");
builder.Services.AddSingleton<IChatClient>(static (provider) =>
    provider.GetRequiredService<OpenAIClient>().AsChatClient("gpt-4o-mini"));

builder.ConfigureFunctionsWebApplication();

builder.Build().Run();
