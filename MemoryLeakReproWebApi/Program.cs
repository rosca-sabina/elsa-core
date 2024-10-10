using Elsa.Common.DistributedLocks.Noop;
using Elsa.Extensions;
using Elsa.MongoDb.Extensions;
using Elsa.MongoDb.Modules.Alterations;
using Elsa.MongoDb.Modules.Management;
using Elsa.MongoDb.Modules.Runtime;
using MemoryLeakReproWebApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var mongoDbConnString = "mongodb://localhost:27017/elsa?authSource=admin";

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
});

builder.Services.AddElsa(elsa =>
{
elsa
    .AddActivitiesFrom<WeatherForecast>()
    .AddWorkflowsFrom<WeatherForecast>()
    .UseMongoDb(mongoDbConnString, mongoDbOptions => mongoDbOptions.DatabaseName = "elsa")
    .UseWorkflowRuntime(runtime =>
    {
        runtime.UseMongoDb();

        runtime.DistributedLockProvider = sp => new NoopDistributedSynchronizationProvider();

        runtime.WorkflowInboxCleanupOptions = opts =>
        {
            opts.SweepInterval = TimeSpan.FromMilliseconds(200);
        };

        //runtime.WorkflowInboxStore = sp => new NoopWorkflowInboxMessageStore();
    })
    .UseWorkflowManagement(management => management.UseMongoDb());
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
