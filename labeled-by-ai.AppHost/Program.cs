var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureFunctionsProject<Projects.LabeledByAI>("labeledbyai");

builder.Build().Run();
