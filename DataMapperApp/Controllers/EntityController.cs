namespace DataMapperApp.Controllers
{
    using DataMapperApp.DataMappers;
    using DataMapperApp.Models;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("[controller]")]
    public class EntityController : ControllerBase
    {
        private readonly IDataMapper _dataMapper;

        public EntityController(IDataMapper dataMapper)
        {
            _dataMapper = dataMapper;
        }

        [HttpGet(Name = "GetEntities")]
        [ProducesResponseType(typeof(IReadOnlyList<Entity>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<Entity>>> GetAll(CancellationToken cancellationToken)
        {
            var entities = await _dataMapper.GetEntitiesAsync(cancellationToken);
            return Ok(entities);
        }

        [HttpGet("{id}", Name = "GetEntity")]
        [ProducesResponseType(typeof(Entity), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Entity>> Get(int id, CancellationToken cancellationToken)
        {
            var entity = await _dataMapper.GetEntityAsync(id, cancellationToken);
            if (entity is null)
            {
                return NotFound();
            }

            return Ok(entity);
        }

        [HttpPost(Name = "CreateEntity")]
        [ProducesResponseType(typeof(Entity), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Entity>> Create(string? name, CancellationToken cancellationToken)
        {
            if (ValidateName(name) is { } error)
            {
                return error;
            }

            var entity = await _dataMapper.CreateEntityAsync(new Entity { Name = name! }, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
        }

        [HttpPut(Name = "UpdateEntity")]
        [ProducesResponseType(typeof(Entity), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Entity>> Update(int id, string? name, CancellationToken cancellationToken)
        {
            if (ValidateName(name) is { } error)
            {
                return error;
            }

            var entity = await _dataMapper.UpdateEntityAsync(new Entity { Id = id, Name = name! }, cancellationToken);
            if (entity is null)
            {
                return NotFound();
            }

            return Ok(entity);
        }

        [HttpDelete(Name = "DeleteEntity")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var deleted = await _dataMapper.DeleteEntityAsync(id, cancellationToken);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        private ActionResult? ValidateName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Name is required.");
            }

            if (name.Length > Entity.NameMaxLength)
            {
                return BadRequest($"Name must be {Entity.NameMaxLength} characters or fewer.");
            }

            return null;
        }
    }
}
