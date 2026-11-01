namespace DataMapperApp.DataMappers
{
    using Dapper;
    using DataMapperApp.Configuration;
    using DataMapperApp.Helpers;
    using DataMapperApp.Models;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Options;

    public class DapperDataMapper : DataMapper, IDataMapper
    {
        public DapperDataMapper(IOptions<AppSettings> options) : base(options)
        {
        }

        public async Task<Entity?> GetEntityAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var connection = new SqlConnection(DataConnectionString);
            return await connection.QuerySingleOrDefaultAsync<Entity>(
                new CommandDefinition(
                    SqlCommandHelper.SelectById,
                    new { Id = id },
                    cancellationToken: cancellationToken));
        }

        public async Task<IReadOnlyList<Entity>> GetEntitiesAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = new SqlConnection(DataConnectionString);
            var entities = await connection.QueryAsync<Entity>(
                new CommandDefinition(SqlCommandHelper.SelectAll, cancellationToken: cancellationToken));
            return entities.ToList();
        }

        public async Task<Entity> CreateEntityAsync(Entity entity, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            await using var connection = new SqlConnection(DataConnectionString);
            return await connection.QuerySingleAsync<Entity>(
                new CommandDefinition(
                    SqlCommandHelper.Insert,
                    new { entity.Name, Created = now, Updated = now },
                    cancellationToken: cancellationToken));
        }

        public async Task<Entity?> UpdateEntityAsync(Entity entity, CancellationToken cancellationToken = default)
        {
            await using var connection = new SqlConnection(DataConnectionString);
            return await connection.QuerySingleOrDefaultAsync<Entity>(
                new CommandDefinition(
                    SqlCommandHelper.Update,
                    new { entity.Id, entity.Name, Updated = DateTime.UtcNow },
                    cancellationToken: cancellationToken));
        }

        public async Task<bool> DeleteEntityAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var connection = new SqlConnection(DataConnectionString);
            var rowsAffected = await connection.ExecuteAsync(
                new CommandDefinition(
                    SqlCommandHelper.DeleteById,
                    new { Id = id },
                    cancellationToken: cancellationToken));
            return rowsAffected > 0;
        }
    }
}
