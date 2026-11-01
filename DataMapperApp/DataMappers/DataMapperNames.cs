namespace DataMapperApp.DataMappers
{
    public static class DataMapperNames
    {
        public const string AdoNet = "AdoNetDataMapper";

        public const string Dapper = "DapperDataMapper";

        public const string EfCore = "EfCoreDataMapper";

        public static readonly IReadOnlyList<string> All = [AdoNet, Dapper, EfCore];
    }
}
