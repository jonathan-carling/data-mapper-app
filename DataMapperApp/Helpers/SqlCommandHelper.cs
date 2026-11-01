namespace DataMapperApp.Helpers
{
    public static class SqlCommandHelper
    {
        public const string SelectById = """
            SELECT Id, Name, Created, Updated
            FROM dbo.Entity
            WHERE Id = @Id
            """;

        public const string SelectAll = """
            SELECT Id, Name, Created, Updated
            FROM dbo.Entity
            ORDER BY Id
            """;

        public const string Insert = """
            INSERT INTO dbo.Entity (Name, Created, Updated)
            OUTPUT INSERTED.Id, INSERTED.Name, INSERTED.Created, INSERTED.Updated
            VALUES (@Name, @Created, @Updated)
            """;

        public const string Update = """
            UPDATE dbo.Entity
            SET Name = @Name, Updated = @Updated
            OUTPUT INSERTED.Id, INSERTED.Name, INSERTED.Created, INSERTED.Updated
            WHERE Id = @Id
            """;

        public const string DeleteById = """
            DELETE FROM dbo.Entity
            WHERE Id = @Id
            """;
    }
}
