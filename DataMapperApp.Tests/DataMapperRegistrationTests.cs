namespace DataMapperApp.Tests
{
    using DataMapperApp.Configuration;
    using DataMapperApp.DataMappers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public sealed class DataMapperRegistrationTests
    {
        [Theory]
        [InlineData(DataMapperNames.AdoNet, typeof(AdoNetDataMapper))]
        [InlineData(DataMapperNames.Dapper, typeof(DapperDataMapper))]
        [InlineData(DataMapperNames.EfCore, typeof(EfCoreDataMapper))]
        public async Task Registers_the_selected_mapper(string mapperName, Type mapperType)
        {
            var services = new ServiceCollection();
            services.AddDataMapper(Configuration(
                "Server=localhost;Database=Development;TrustServerCertificate=True",
                mapperName));

            await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
            await using var scope = provider.CreateAsyncScope();
            var mapper = scope.ServiceProvider.GetRequiredService<IDataMapper>();

            Assert.IsType(mapperType, mapper);
        }

        [Fact]
        public void Empty_connection_string_explains_how_to_set_one()
        {
            var services = new ServiceCollection();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                services.AddDataMapper(Configuration("  ", DataMapperNames.AdoNet)));

            Assert.Contains("AppSettings:DataConnectionString", exception.Message);
            Assert.Contains("AppSettings__DataConnectionString", exception.Message);
        }

        [Fact]
        public void Unknown_mapper_name_lists_the_supported_names()
        {
            var services = new ServiceCollection();

            var exception = Assert.Throws<InvalidOperationException>(() =>
                services.AddDataMapper(Configuration("Server=localhost;", "ReflectionDataMapper")));

            Assert.Contains(DataMapperNames.AdoNet, exception.Message);
            Assert.Contains(DataMapperNames.Dapper, exception.Message);
            Assert.Contains(DataMapperNames.EfCore, exception.Message);
        }

        private static IConfiguration Configuration(string? connectionString, string? mapperName)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [$"{AppSettings.SectionName}:DataConnectionString"] = connectionString,
                    [$"{AppSettings.SectionName}:DataMapperClassName"] = mapperName
                })
                .Build();
        }
    }
}
