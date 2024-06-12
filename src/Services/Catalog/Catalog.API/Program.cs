using BuildingBlocks.PipelineBehaviors;
using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.Extentions;

var builder = WebApplication.CreateBuilder(args);

// Add services to container
var assembly = typeof(Program).Assembly;

builder.Services.AddCarterWithAssemblies(assembly);
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehaviors<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(assembly);
builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
}).UseLightweightSessions();

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.MapCarter();

// Manuel global Exception Handler
//app.UseExceptionHandler(() =>
//{

//});
app.UseExceptionHandler(options => { });



app.Run();
