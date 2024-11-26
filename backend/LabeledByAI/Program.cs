using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI;

var builder = FunctionsApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddAzureOpenAIClient("openai");
builder.Services.AddSingleton<IChatClient>(static (provider) =>
    provider.GetRequiredService<OpenAIClient>().AsChatClient("ai-model"));

builder.ConfigureFunctionsWebApplication();

builder.Build().Run();
