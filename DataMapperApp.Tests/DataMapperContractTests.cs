namespace DataMapperApp.Tests
{
    using DataMapperApp.DataMappers;
    using DataMapperApp.Models;

    [Collection(SqlServerCollection.Name)]
    public abstract class DataMapperContractTests : IAsyncLifetime
    {
        private readonly SqlServerFixture _fixture;
        private IDataMapper _mapper = null!;

        protected DataMapperContractTests(SqlServerFixture fixture)
        {
            _fixture = fixture;
        }

        protected abstract IDataMapper CreateMapper(string connectionString);

        public async Task InitializeAsync()
        {
            await _fixture.ResetAsync();
            _mapper = CreateMapper(_fixture.ConnectionString);
        }

        public async Task DisposeAsync()
        {
            if (_mapper is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else if (_mapper is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        [Fact]
        public async Task Create_returns_the_new_row_including_its_identity()
        {
            var created = await _mapper.CreateEntityAsync(new Entity { Name = "Ada" });

            Assert.True(created.Id > 0);
            Assert.Equal("Ada", created.Name);

            var stored = await _mapper.GetEntityAsync(created.Id);
            Assert.NotNull(stored);
            Assert.Equal(created.Id, stored.Id);
            Assert.Equal("Ada", stored.Name);
            Assert.Equal(created.Created, stored.Created);
            Assert.Equal(created.Updated, stored.Updated);
        }

        [Fact]
        public async Task Create_round_trips_a_name_that_would_break_concatenated_sql()
        {
            var name = "O'Brien'); DROP TABLE dbo.Entity;--";

            var created = await _mapper.CreateEntityAsync(new Entity { Name = name });
            var stored = await _mapper.GetEntityAsync(created.Id);

            Assert.NotNull(stored);
            Assert.Equal(name, stored.Name);
            Assert.Contains(await _mapper.GetEntitiesAsync(), entity => entity.Id == created.Id);
        }

        [Fact]
        public async Task Create_accepts_a_name_at_the_column_limit()
        {
            var name = new string('n', Entity.NameMaxLength);

            var created = await _mapper.CreateEntityAsync(new Entity { Name = name });
            var stored = await _mapper.GetEntityAsync(created.Id);

            Assert.NotNull(stored);
            Assert.Equal(name, stored.Name);
        }

        [Fact]
        public async Task Get_missing_id_returns_null()
        {
            var stored = await _mapper.GetEntityAsync(int.MaxValue);

            Assert.Null(stored);
        }

        [Fact]
        public async Task GetAll_returns_rows_in_id_order()
        {
            await _mapper.CreateEntityAsync(new Entity { Name = "first" });
            await _mapper.CreateEntityAsync(new Entity { Name = "second" });

            var names = (await _mapper.GetEntitiesAsync()).Select(entity => entity.Name).ToArray();

            Assert.Equal(["first", "second"], names);
        }

        [Fact]
        public async Task Update_changes_the_name_and_leaves_created_unchanged()
        {
            var created = await _mapper.CreateEntityAsync(new Entity { Name = "Ada" });
            var stored = await _mapper.GetEntityAsync(created.Id);
            Assert.NotNull(stored);

            var updated = await _mapper.UpdateEntityAsync(new Entity { Id = created.Id, Name = "Ada Lovelace" });

            Assert.NotNull(updated);
            Assert.Equal(created.Id, updated.Id);
            Assert.Equal("Ada Lovelace", updated.Name);
            Assert.Equal(stored.Created, updated.Created);
            Assert.True(updated.Updated >= stored.Updated);

            var reread = await _mapper.GetEntityAsync(created.Id);
            Assert.Equal("Ada Lovelace", reread!.Name);
        }

        [Fact]
        public async Task Update_missing_id_returns_null()
        {
            var updated = await _mapper.UpdateEntityAsync(new Entity { Id = int.MaxValue, Name = "missing" });

            Assert.Null(updated);
        }

        [Fact]
        public async Task Delete_removes_an_existing_row_and_reports_a_miss()
        {
            var created = await _mapper.CreateEntityAsync(new Entity { Name = "Ada" });

            Assert.True(await _mapper.DeleteEntityAsync(created.Id));
            Assert.Null(await _mapper.GetEntityAsync(created.Id));
            Assert.False(await _mapper.DeleteEntityAsync(created.Id));
        }
    }
}
