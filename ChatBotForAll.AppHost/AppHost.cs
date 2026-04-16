var builder = DistributedApplication.CreateBuilder(args);

//var cache = builder.AddRedis("cache");

//var postgres = builder.AddPostgres("postgres")
//.WithDataVolume("chatbotforall-pgdata");

//var chatbotDb = postgres.AddDatabase("chatbotforall");

var apiService = builder.AddProject<Projects.ChatBotForAll_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");
    //.WithReference(chatbotDb)
    //.WaitFor(chatbotDb);

builder.AddProject<Projects.ChatBotForAll_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    //.WithReference(cache)
    //.WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
