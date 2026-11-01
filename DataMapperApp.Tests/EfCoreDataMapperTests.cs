namespace DataMapperApp.Tests
{
    using DataMapperApp.DataMappers;

    public sealed class EfCoreDataMapperTests : DataMapperContractTests
    {
        public EfCoreDataMapperTests(SqlServerFixture fixture) : base(fixture)
        {
        }

        protected override IDataMapper CreateMapper(string connectionString)
        {
            return EfCoreDataMapper.Create(connectionString);
        }
    }
}
