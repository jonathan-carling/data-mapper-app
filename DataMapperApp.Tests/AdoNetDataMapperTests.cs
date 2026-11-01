namespace DataMapperApp.Tests
{
    using DataMapperApp.Configuration;
    using DataMapperApp.DataMappers;
    using Microsoft.Extensions.Options;

    public sealed class AdoNetDataMapperTests : DataMapperContractTests
    {
        public AdoNetDataMapperTests(SqlServerFixture fixture) : base(fixture)
        {
        }

        protected override IDataMapper CreateMapper(string connectionString)
        {
            return new AdoNetDataMapper(Options.Create(Settings(connectionString, DataMapperNames.AdoNet)));
        }

        internal static AppSettings Settings(string connectionString, string mapperName)
        {
            return new AppSettings
            {
                DataConnectionString = connectionString,
                DataMapperClassName = mapperName
            };
        }
    }
}
