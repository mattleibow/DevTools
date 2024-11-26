
var builder = DistributedApplication.CreateBuilder(args);

// Set this const to true in order to run this function and
// AI locally without Azure hosting when doing development.
const bool UseLocalAI = true;

// Only use Application Insights in the published app.
var insights = builder.ExecutionContext.IsPublishMode
    ? builder.AddAzureApplicationInsights("app-insights")
    : null;

// Use the Azure OpenAI in the published app, but a local Ollama
// in development or connect to an existing Azure OpenAI.
IResourceBuilder<IResourceWithConnectionString> ai = builder.ExecutionContext.IsPublishMode
    ? builder.AddAzureOpenAI("openai")
        .AddDeployment(new("ai-model", "gpt-4o-mini", "2024-07-18", "Global Standard"))
    : UseLocalAI
        ? builder.AddOllama("ollama")
            .WithDataVolume()
            .WithContainerRuntimeArgs("--gpus=all")
            .WithOpenWebUI()
            .AddModel("llama3")
        : builder.AddConnectionString("openai");

// Specify the storage for the function.
var funcStorage = builder.AddAzureStorage("func-storage")
    .RunAsEmulator();

// Export the function with public endpoints.
var func = builder
    .AddAzureFunctionsProject<Projects.LabeledByAI>("labeled-by-ai")
    .WithExternalHttpEndpoints()
    .WithHostStorage(funcStorage)
    .WithReference(ai)
    .WithOptionalReference(insights);

builder.Build().Run();
