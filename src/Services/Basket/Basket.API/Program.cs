using Basket.API.Data;
using BuildingBlocks.Exceptions.Handler;
using BuildingBlocks.Extentions;
using BuildingBlocks.PipelineBehaviors;
using Discount.Grpc;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using BuildingBlocks.Messaging.MassTransit;
using BuildingBlocks.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var assembly = typeof(Program).Assembly;
var connectionStringPostgre = builder.Configuration.GetConnectionString("Database")!;
var connectionStringRedis = builder.Configuration.GetConnectionString("Redis")!;

builder.Host.UseSerilog(SeriLogger.Configure);

//Application Services
builder.Services.AddCarterWithAssemblies(assembly);

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});


builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();

//Data Services
builder.Services.AddMarten(opts =>
{
    opts.Connection(connectionStringPostgre);
}).UseLightweightSessions();

builder.Services.AddStackExchangeRedisCache(opt =>
{
    opt.Configuration = builder.Configuration.GetConnectionString("Redis");
});

//Grpc Services
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]!);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };

    return handler;
});

//Async Communication Services
builder.Services.AddMessageBroker(builder.Configuration);

//Cross-Cutting Services
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddHealthChecks()
                    .AddNpgSql(connectionStringPostgre)
                    .AddRedis(connectionStringRedis);

var app = builder.Build();

app.MapCarter();

app.UseExceptionHandler(options => { });

app.UseHealthChecks("/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.Run();
