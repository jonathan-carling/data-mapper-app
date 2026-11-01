namespace DataMapperApp.DataMappers
{
    using DataMapperApp.Configuration;
    using Microsoft.Extensions.Options;

    public abstract class DataMapper
    {
        protected string DataConnectionString { get; }

        protected DataMapper(IOptions<AppSettings> options)
        {
            DataConnectionString = options.Value.DataConnectionString;
        }
    }
}
