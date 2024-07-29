using IPInfoApp.Business.Contracts;
using IPInfoApp.Business.Services;
using IPInfoApp.Data.IRepositories;
using IPInfoApp.Data.Repositories;


namespace IPInfoApp.API.Extentions
{
    public static  class ServiceCollections
    {
        public static void AddServiceCollections(this IServiceCollection services)
        {
            services.AddSingleton<ICacheService,CacheService>();
            services.AddScoped<IIpInformationService, IpInformationService>();
            services.AddScoped<IReportRepository, ReportRepository>();

        }
    }
}
