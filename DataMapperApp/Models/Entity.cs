namespace DataMapperApp.Models
{
    public class Entity
    {
        public const int NameMaxLength = 200;

        public int Id { get; set; }

        public required string Name { get; set; }

        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }
    }
}
