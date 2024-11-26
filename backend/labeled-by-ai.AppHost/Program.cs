
using Microsoft.AspNetCore.Mvc.Rendering;

var builder = DistributedApplication.CreateBuilder(args);

var insights = builder.ExecutionContext.IsPublishMode
    ? builder.AddAzureApplicationInsights("app-insights")
    : null;

// use the Azure OpenAI in the published app,
// but a local Ollama in development
IResourceBuilder<IResourceWithConnectionString> ai = builder.ExecutionContext.IsPublishMode
    ? builder.AddAzureOpenAI("openai")
        .AddDeployment(new("ai-model", "gpt-4o-mini", "2024-07-18", "Global Standard"))
    : builder.AddOllama("ollama")
        .WithDataVolume()
        .WithContainerRuntimeArgs("--gpus=all")
        .WithOpenWebUI()
        .AddModel("llama3");

var funcStorage = builder.AddAzureStorage("func-storage")
    .RunAsEmulator();

var func = builder
    .AddAzureFunctionsProject<Projects.LabeledByAI>("labeled-by-ai")
    .WithExternalHttpEndpoints()
    .WithHostStorage(funcStorage)
    .WithReference(ai)
    .WithOptionalReference(insights);

builder.Build().Run();
