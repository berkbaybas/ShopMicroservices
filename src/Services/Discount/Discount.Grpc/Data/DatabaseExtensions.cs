using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Discount.Grpc.Data
{
    public static class DatabaseExtensions
    {
        public static IApplicationBuilder UseMigration(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<DiscountContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<DiscountContext>>();


            var retry = Policy.Handle<SqliteException>()
                               .WaitAndRetry(
                                   retryCount: 5,
                                   sleepDurationProvider: retryAttemp => TimeSpan.FromSeconds(Math.Pow(2, retryAttemp)),
                                   onRetry: (exception, retryCount, context) =>
                                   {
                                       logger.LogError($"Retry {retryCount} of {context.PolicyKey} at {context.OperationKey}, due to: {exception}.");
                                   });

            retry.Execute(() => { dbContext.Database.Migrate(); });

            return app;
        }
    }
}
