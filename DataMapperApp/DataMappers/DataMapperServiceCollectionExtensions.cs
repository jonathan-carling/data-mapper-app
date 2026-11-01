namespace DataMapperApp.DataMappers
{
    using DataMapperApp.Configuration;
    using DataMapperApp.Data;
    using Microsoft.EntityFrameworkCore;

    public static class DataMapperServiceCollectionExtensions
    {
        public static IServiceCollection AddDataMapper(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration[$"{AppSettings.SectionName}:DataConnectionString"];
            var mapperName = configuration[$"{AppSettings.SectionName}:DataMapperClassName"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "AppSettings:DataConnectionString is empty. Set a SQL Server connection string with " +
                    "dotnet user-secrets set \"AppSettings:DataConnectionString\" \"<connection string>\" --project DataMapperApp " +
                    "or the AppSettings__DataConnectionString environment variable.");
            }

            if (string.IsNullOrWhiteSpace(mapperName) || !DataMapperNames.All.Contains(mapperName))
            {
                throw new InvalidOperationException(
                    $"Unknown DataMapperClassName '{mapperName}'. Use {string.Join(", ", DataMapperNames.All)}.");
            }

            services.Configure<AppSettings>(configuration.GetSection(AppSettings.SectionName));

            switch (mapperName)
            {
                case DataMapperNames.AdoNet:
                    services.AddScoped<IDataMapper, AdoNetDataMapper>();
                    break;
                case DataMapperNames.Dapper:
                    services.AddScoped<IDataMapper, DapperDataMapper>();
                    break;
                case DataMapperNames.EfCore:
                    services.AddDbContext<EntityDbContext>(options => options.UseSqlServer(connectionString));
                    services.AddScoped<IDataMapper, EfCoreDataMapper>();
                    break;
            }

            return services;
        }
    }
}
