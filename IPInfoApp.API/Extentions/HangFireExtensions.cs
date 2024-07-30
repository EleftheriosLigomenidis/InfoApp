using Hangfire;
using Hangfire.SqlServer;
using IPInfoApp.Business.Contracts;

namespace IPInfoApp.API.Extentions
{
    public static class HangFireExtensions
    {
        public static void AddHangfire(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Hangfire");

            services
                .AddHangfire(
                    config =>
                        config
                            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                            .UseSimpleAssemblyNameTypeSerializer()
                            .UseRecommendedSerializerSettings()
                            .UseSqlServerStorage(
                                connectionString
                            )
                );

            services.AddHangfireServer();
        }

        public static void EnqueueBackgroundJobs(this IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();

            var reccuringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
            var ipInformationCron = configuration.GetValue<string>("IpInformationUpdatorIntervalCron");

            reccuringJobManager.AddOrUpdate(
                "IpInformationUpdator",
                (IIpInformationService iinformationService) => iinformationService.UpdateIpInformation(default),
                ipInformationCron,
                new RecurringJobOptions() { MisfireHandling = MisfireHandlingMode.Ignorable }
            );

        }
    }
}
