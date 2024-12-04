using System.Text.Json;

namespace ToDoAPI.Tests
{
    // Test environment definition.
    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }

    /// <summary>
    /// Base class for tests.
    /// All test classes derived from this base class will share the same test environment defined in <see cref="DatabaseFixture"/>.
    /// </summary>
    [Collection("Database collection")]
    public class ToDoTestBase
    {
        protected readonly DatabaseFixture _fixture;

        // specify custom json converter for todo item model
        protected readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new Models.Validation.DateTimeFormatConverter() }
        };

        public ToDoTestBase(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }
    }
}
