
var builder = DistributedApplication.CreateBuilder(args);

var openai = builder.ExecutionContext.IsPublishMode
    ? builder.AddAzureOpenAI("openai")
    : builder.AddConnectionString("openai");

//var openai = builder.AddAzureOpenAI("openai");

var funcStorage = builder.AddAzureStorage("func-storage")
    .RunAsEmulator();

var func = builder
    .AddAzureFunctionsProject<Projects.LabeledByAI>("labeled-by-ai")
    .WithHostStorage(funcStorage)
    .WithReference(openai);

builder.Build().Run();
