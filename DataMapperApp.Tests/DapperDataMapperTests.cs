namespace DataMapperApp.Tests
{
    using DataMapperApp.DataMappers;
    using Microsoft.Extensions.Options;

    public sealed class DapperDataMapperTests : DataMapperContractTests
    {
        public DapperDataMapperTests(SqlServerFixture fixture) : base(fixture)
        {
        }

        protected override IDataMapper CreateMapper(string connectionString)
        {
            return new DapperDataMapper(Options.Create(AdoNetDataMapperTests.Settings(connectionString, DataMapperNames.Dapper)));
        }
    }
}
