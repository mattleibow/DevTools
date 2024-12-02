using LabeledByAI.Services;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenAI;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Services.AddSingleton<GetBestLabelService>();

builder.AddServiceDefaults();

builder.AddAzureOpenAIClient("ai");
builder.Services.AddSingleton<IChatClient>(static (provider) =>
    provider.GetRequiredService<OpenAIClient>().AsChatClient("ai-model"));

builder.ConfigureFunctionsWebApplication();

builder.Build().Run();
