namespace DataMapperApp.DataMappers
{
    using System.Data;
    using DataMapperApp.Configuration;
    using DataMapperApp.Helpers;
    using DataMapperApp.Models;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Options;

    public class AdoNetDataMapper : DataMapper, IDataMapper
    {
        public AdoNetDataMapper(IOptions<AppSettings> options) : base(options)
        {
        }

        public async Task<Entity?> GetEntityAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var connection = await OpenConnectionAsync(cancellationToken);
            await using var command = new SqlCommand(SqlCommandHelper.SelectById, connection);
            command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return ReadEntity(reader);
        }

        public async Task<IReadOnlyList<Entity>> GetEntitiesAsync(CancellationToken cancellationToken = default)
        {
            var entities = new List<Entity>();
            await using var connection = await OpenConnectionAsync(cancellationToken);
            await using var command = new SqlCommand(SqlCommandHelper.SelectAll, connection);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                entities.Add(ReadEntity(reader));
            }

            return entities;
        }

        public async Task<Entity> CreateEntityAsync(Entity entity, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            await using var connection = await OpenConnectionAsync(cancellationToken);
            await using var command = new SqlCommand(SqlCommandHelper.Insert, connection);
            command.Parameters.Add("@Name", SqlDbType.NVarChar, Entity.NameMaxLength).Value = entity.Name;
            command.Parameters.Add("@Created", SqlDbType.DateTime2).Value = now;
            command.Parameters.Add("@Updated", SqlDbType.DateTime2).Value = now;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException("Insert did not return the new entity.");
            }

            return ReadEntity(reader);
        }

        public async Task<Entity?> UpdateEntityAsync(Entity entity, CancellationToken cancellationToken = default)
        {
            await using var connection = await OpenConnectionAsync(cancellationToken);
            await using var command = new SqlCommand(SqlCommandHelper.Update, connection);
            command.Parameters.Add("@Id", SqlDbType.Int).Value = entity.Id;
            command.Parameters.Add("@Name", SqlDbType.NVarChar, Entity.NameMaxLength).Value = entity.Name;
            command.Parameters.Add("@Updated", SqlDbType.DateTime2).Value = DateTime.UtcNow;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return ReadEntity(reader);
        }

        public async Task<bool> DeleteEntityAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var connection = await OpenConnectionAsync(cancellationToken);
            await using var command = new SqlCommand(SqlCommandHelper.DeleteById, connection);
            command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }

        private async Task<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
        {
            var connection = new SqlConnection(DataConnectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        }

        private static Entity ReadEntity(SqlDataReader reader)
        {
            return new Entity
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Created = reader.GetDateTime(reader.GetOrdinal("Created")),
                Updated = reader.GetDateTime(reader.GetOrdinal("Updated"))
            };
        }
    }
}
