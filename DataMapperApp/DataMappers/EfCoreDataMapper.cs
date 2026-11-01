namespace DataMapperApp.DataMappers
{
    using DataMapperApp.Data;
    using DataMapperApp.Models;
    using Microsoft.EntityFrameworkCore;

    public sealed class EfCoreDataMapper : IDataMapper, IDisposable, IAsyncDisposable
    {
        private readonly EntityDbContext _db;
        private readonly bool _ownsContext;
        private bool _disposed;

        public EfCoreDataMapper(EntityDbContext db) : this(db, ownsContext: false)
        {
        }

        public static EfCoreDataMapper Create(string connectionString)
        {
            var options = new DbContextOptionsBuilder<EntityDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new EfCoreDataMapper(new EntityDbContext(options), ownsContext: true);
        }

        private EfCoreDataMapper(EntityDbContext db, bool ownsContext)
        {
            _db = db;
            _ownsContext = ownsContext;
        }

        public async Task<Entity?> GetEntityAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.Entities.AsNoTracking().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Entity>> GetEntitiesAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Entities.AsNoTracking().OrderBy(entity => entity.Id).ToListAsync(cancellationToken);
        }

        public async Task<Entity> CreateEntityAsync(Entity entity, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            entity.Created = now;
            entity.Updated = now;
            _db.Entities.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task<Entity?> UpdateEntityAsync(Entity entity, CancellationToken cancellationToken = default)
        {
            var existing = await _db.Entities.FirstOrDefaultAsync(row => row.Id == entity.Id, cancellationToken);
            if (existing is null)
            {
                return null;
            }

            existing.Name = entity.Name;
            existing.Updated = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return existing;
        }

        public async Task<bool> DeleteEntityAsync(int id, CancellationToken cancellationToken = default)
        {
            var existing = await _db.Entities.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
            if (existing is null)
            {
                return false;
            }

            _db.Entities.Remove(existing);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public void Dispose()
        {
            if (_ownsContext && !_disposed)
            {
                _db.Dispose();
                _disposed = true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_ownsContext && !_disposed)
            {
                await _db.DisposeAsync();
                _disposed = true;
            }
        }
    }
}
