namespace DataMapperApp.DataMappers
{
    using DataMapperApp.Models;

    public interface IDataMapper
    {
        Task<Entity?> GetEntityAsync(int id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Entity>> GetEntitiesAsync(CancellationToken cancellationToken = default);

        Task<Entity> CreateEntityAsync(Entity entity, CancellationToken cancellationToken = default);

        Task<Entity?> UpdateEntityAsync(Entity entity, CancellationToken cancellationToken = default);

        Task<bool> DeleteEntityAsync(int id, CancellationToken cancellationToken = default);
    }
}
