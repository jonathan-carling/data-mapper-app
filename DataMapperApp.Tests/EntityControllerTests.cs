namespace DataMapperApp.Tests
{
    using DataMapperApp.Controllers;
    using DataMapperApp.DataMappers;
    using DataMapperApp.Models;
    using Microsoft.AspNetCore.Mvc;

    public sealed class EntityControllerTests
    {
        private readonly FakeDataMapper _mapper = new();
        private readonly EntityController _controller;

        public EntityControllerTests()
        {
            _controller = new EntityController(_mapper);
        }

        [Fact]
        public async Task Get_missing_id_returns_404()
        {
            var result = await _controller.Get(4, CancellationToken.None);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Get_returns_the_entity()
        {
            _mapper.Single = Sample(4, "Ada");

            var result = await _controller.Get(4, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(_mapper.Single, ok.Value);
        }

        [Fact]
        public async Task GetAll_returns_the_list()
        {
            _mapper.All.Add(Sample(1, "Ada"));

            var result = await _controller.GetAll(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(_mapper.All, ok.Value);
        }

        [Fact]
        public async Task Create_returns_201_with_the_new_row()
        {
            var result = await _controller.Create("Ada", CancellationToken.None);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(EntityController.Get), created.ActionName);
            Assert.Equal(10, created.RouteValues!["id"]);
            var body = Assert.IsType<Entity>(created.Value);
            Assert.Equal(10, body.Id);
            Assert.Equal("Ada", body.Name);
            Assert.Equal("Ada", _mapper.LastCreatedName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Create_rejects_a_blank_name(string? name)
        {
            var result = await _controller.Create(name, CancellationToken.None);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Name is required.", badRequest.Value);
            Assert.Equal(0, _mapper.CreateCalls);
        }

        [Fact]
        public async Task Create_rejects_a_name_past_the_column_limit()
        {
            var result = await _controller.Create(new string('a', Entity.NameMaxLength + 1), CancellationToken.None);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal($"Name must be {Entity.NameMaxLength} characters or fewer.", badRequest.Value);
            Assert.Equal(0, _mapper.CreateCalls);
        }

        [Fact]
        public async Task Update_missing_id_returns_404()
        {
            var result = await _controller.Update(4, "Ada", CancellationToken.None);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Update_returns_the_saved_row()
        {
            _mapper.UpdateResult = Sample(4, "Ada Lovelace");

            var result = await _controller.Update(4, "Ada Lovelace", CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(_mapper.UpdateResult, ok.Value);
        }

        [Fact]
        public async Task Delete_missing_id_returns_404()
        {
            _mapper.DeleteResult = false;

            var result = await _controller.Delete(4, CancellationToken.None);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_returns_204()
        {
            var result = await _controller.Delete(4, CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
        }

        private static Entity Sample(int id, string name)
        {
            return new Entity
            {
                Id = id,
                Name = name,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
        }

        private sealed class FakeDataMapper : IDataMapper
        {
            public Entity? Single { get; set; }

            public List<Entity> All { get; } = [];

            public Entity? UpdateResult { get; set; }

            public bool DeleteResult { get; set; } = true;

            public int CreateCalls { get; private set; }

            public string? LastCreatedName { get; private set; }

            public Task<Entity?> GetEntityAsync(int id, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(Single);
            }

            public Task<IReadOnlyList<Entity>> GetEntitiesAsync(CancellationToken cancellationToken = default)
            {
                return Task.FromResult<IReadOnlyList<Entity>>(All);
            }

            public Task<Entity> CreateEntityAsync(Entity entity, CancellationToken cancellationToken = default)
            {
                CreateCalls++;
                LastCreatedName = entity.Name;
                return Task.FromResult(Sample(10, entity.Name));
            }

            public Task<Entity?> UpdateEntityAsync(Entity entity, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(UpdateResult);
            }

            public Task<bool> DeleteEntityAsync(int id, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(DeleteResult);
            }
        }
    }
}
