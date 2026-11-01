namespace DataMapperApp.Configuration
{
    public class AppSettings
    {
        public const string SectionName = "AppSettings";

        public string DataMapperClassName { get; set; } = "";

        public string DataConnectionString { get; set; } = "";
    }
}
