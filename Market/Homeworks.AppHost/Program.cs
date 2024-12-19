var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Market>("market");

builder.Build().Run();
