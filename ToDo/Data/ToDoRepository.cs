using Dapper;
using Npgsql;
using System.Data;
using ToDoAPI.Models;

namespace ToDoAPI.Data
{
    public class ToDoRepository
    {
        private readonly string _connectionString;

        public ToDoRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new ArgumentNullException(nameof(configuration), "Connection string is required.");
        }

        private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

        public async Task<IEnumerable<ToDoItem>> GetAllAsync()
        {
            using var connection = CreateConnection();
            return await connection
                .QueryAsync<ToDoItem>("SELECT \"Id\", \"Title\", \"Description\", \"ExpiryDate\", \"PercentComplete\", \"IsDone\" FROM \"ToDoItems\" ORDER BY \"ExpiryDate\"");
        }

        public async Task<ToDoItem?> GetByIdAsync(int id)
        {
            using var connection = CreateConnection();
            return await connection
                .QueryFirstOrDefaultAsync<ToDoItem>("SELECT \"Id\", \"Title\", \"Description\", \"ExpiryDate\", \"PercentComplete\", \"IsDone\" FROM \"ToDoItems\" WHERE \"Id\" = @Id", new { Id = id });
        }

        public async Task<IEnumerable<ToDoItem>> GetRangeAsync(DateTime from, DateTime to)
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<ToDoItem>(
                "SELECT \"Id\", \"Title\", \"Description\", \"ExpiryDate\", \"PercentComplete\", \"IsDone\" FROM \"ToDoItems\" WHERE \"ExpiryDate\" BETWEEN @From AND @To ORDER BY \"ExpiryDate\"",
                new { From = from, To = to });
        }

        public async Task<int> CreateAsync(ToDoItem toDoItem)
        {
            using var connection = CreateConnection();
            var insertedId = await connection.ExecuteScalarAsync<int>(
                "INSERT INTO \"ToDoItems\" (\"Title\", \"Description\", \"ExpiryDate\", \"PercentComplete\", \"IsDone\") " +
                "VALUES (@Title, @Description, @ExpiryDate, @PercentComplete, @IsDone) RETURNING \"Id\"",
                toDoItem);
            return insertedId;
        }

        public async Task<int> UpdateAsync(ToDoItem toDoItem)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteAsync(
                "UPDATE \"ToDoItems\" SET \"Title\" = @Title, \"Description\" = @Description, \"ExpiryDate\" = @ExpiryDate, \"PercentComplete\" = @PercentComplete, \"IsDone\" = @IsDone WHERE \"Id\" = @Id",
                toDoItem);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteAsync("DELETE FROM \"ToDoItems\" WHERE \"Id\" = @Id", new { Id = id });
        }
    }
}
