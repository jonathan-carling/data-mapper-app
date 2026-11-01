namespace DataMapperApp.Data
{
    using DataMapperApp.Models;
    using Microsoft.EntityFrameworkCore;

    public class EntityDbContext : DbContext
    {
        public EntityDbContext(DbContextOptions<EntityDbContext> options) : base(options)
        {
        }

        public DbSet<Entity> Entities => Set<Entity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<Entity>();
            entity.ToTable("Entity");
            entity.HasKey(row => row.Id);
            entity.Property(row => row.Id).ValueGeneratedOnAdd();
            entity.Property(row => row.Name).HasMaxLength(Entity.NameMaxLength).IsRequired();
            entity.Property(row => row.Created).IsRequired();
            entity.Property(row => row.Updated).IsRequired();
        }
    }
}
