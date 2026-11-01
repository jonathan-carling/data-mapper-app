namespace DataMapperApp.Tests
{
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using Microsoft.Data.SqlClient;
    using Testcontainers.MsSql;

    public sealed class SqlServerFixture : IAsyncLifetime
    {
        public const string DatabaseName = "DataMapperAppTests";

        private const string LocalDbConnectionString =
            @"Server=(localdb)\MSSQLLocalDB;Integrated Security=True;TrustServerCertificate=True";

        private MsSqlContainer? _container;

        public string ConnectionString { get; private set; } = "";

        public async Task InitializeAsync()
        {
            if (await IsDockerRunningAsync())
            {
                _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
                    .WithDatabase(DatabaseName)
                    .Build();
                await _container.StartAsync();
                await ApplySchemaAsync(_container.GetConnectionString());
                ConnectionString = WithDatabase(_container.GetConnectionString());
                return;
            }

            try
            {
                await ApplySchemaAsync(LocalDbConnectionString);
                ConnectionString = WithDatabase(LocalDbConnectionString);
            }
            catch (SqlException exception)
            {
                throw new InvalidOperationException(
                    "Data mapper tests need Docker or SQL Server LocalDB. Start Docker Desktop, or install LocalDB.",
                    exception);
            }
        }

        public async Task DisposeAsync()
        {
            if (_container is not null)
            {
                await _container.DisposeAsync();
            }
        }

        public async Task ResetAsync()
        {
            await using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM dbo.Entity";
            await command.ExecuteNonQueryAsync();
        }

        private static string WithDatabase(string connectionString)
        {
            return new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = DatabaseName,
                TrustServerCertificate = true
            }.ConnectionString;
        }

        private static async Task ApplySchemaAsync(string connectionString)
        {
            var scriptPath = Path.Combine(AppContext.BaseDirectory, "database", "create-entity.sql");
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException("Could not find the Entity table script.", scriptPath);
            }

            var script = (await File.ReadAllTextAsync(scriptPath))
                .Replace("$(DatabaseName)", DatabaseName, StringComparison.Ordinal);
            var master = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master",
                TrustServerCertificate = true
            }.ConnectionString;

            await using var connection = new SqlConnection(master);
            await connection.OpenAsync();

            foreach (var batch in Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(batch))
                {
                    continue;
                }

                await using var command = connection.CreateCommand();
                command.CommandText = batch;
                await command.ExecuteNonQueryAsync();
            }
        }

        private static async Task<bool> IsDockerRunningAsync()
        {
            try
            {
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "info",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                if (process is null)
                {
                    return false;
                }

                var stdout = process.StandardOutput.ReadToEndAsync();
                var stderr = process.StandardError.ReadToEndAsync();
                var exited = await Task.Run(() => process.WaitForExit(15000));
                if (!exited)
                {
                    process.Kill(entireProcessTree: true);
                    return false;
                }

                await Task.WhenAll(stdout, stderr);
                return process.ExitCode == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
