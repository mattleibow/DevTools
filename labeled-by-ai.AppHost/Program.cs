
var builder = DistributedApplication.CreateBuilder(args);

var insights = builder.AddAzureApplicationInsights("app-insights");

//var openai = !builder.ExecutionContext.IsPublishMode
//    ? builder.AddConnectionString("openai") // use external
//    : builder.AddAzureOpenAI("openai") // deploy with app
//        .AddDeployment(new("openai-model", "gpt-4o-mini", "2024-07-18"));

var openai = builder.AddConnectionString("openai");

//var openai = builder.AddAzureOpenAI("openai");

var funcStorage = builder.AddAzureStorage("func-storage")
    .RunAsEmulator();

var func = builder
    .AddAzureFunctionsProject<Projects.LabeledByAI>("labeled-by-ai")
    .WithExternalHttpEndpoints()
    .WithHostStorage(funcStorage)
    .WithReference(openai)
    .WithReference(insights);

builder.Build().Run();
