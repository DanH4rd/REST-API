using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using ToDoAPI.Data;

namespace ToDoAPI.Tests
{
    /// <summary>
    /// Prepare environment for tests.
    /// A new empty test database will be created before a tests set
    /// and removed after tests are completed.
    /// </summary>
    public class DatabaseFixture : IDisposable
    {
        public WebApplicationFactory<Program> Factory { get; private set; } = null!;
        public HttpClient Client { get; private set; } = null!;
        private string _connectionString { get; set; } = null!;

        public DatabaseFixture()
        {
            Factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    // configure test environment
                    builder.UseEnvironment("Test");
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddJsonFile("appsettings.Test.json", optional: false);
                    });

                    builder.ConfigureServices(services =>
                    {
                        // switch the config to test
                        using var scope = services.BuildServiceProvider().CreateScope();
                        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                        var connectionString = config.GetConnectionString("DefaultConnection");
                        _connectionString = connectionString!;

                        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

                        // prepare test db
                        RecreateDatabase(_connectionString);

                        // apply migrations
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        context.Database.Migrate();
                    });
                });

            Client = Factory.CreateClient();
        }

        public void Dispose()
        {
            Factory.Dispose();
            RecreateDatabase(_connectionString, true);
        }

        // recreates a test db
        private static void RecreateDatabase(string connectionString, bool removeOnly = false)
        {
            var masterConnectionString = new NpgsqlConnectionStringBuilder(connectionString)
            {
                Database = "postgres" // use the system postgres db
            }.ToString();

            using var connection = new NpgsqlConnection(masterConnectionString);
            connection.Open();

            var databaseName = new NpgsqlConnectionStringBuilder(connectionString).Database;

            using var command = connection.CreateCommand();

            // check the db exists
            command.CommandText = $@"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'";
            var dbExists = command.ExecuteScalar() != null;

            if (dbExists)
            {
                // close active db connections
                command.CommandText = $@"
                REVOKE CONNECT ON DATABASE ""{databaseName}"" FROM PUBLIC;
                SELECT pg_terminate_backend(pg_stat_activity.pid)
                FROM pg_stat_activity
                WHERE pg_stat_activity.datname = '{databaseName}' AND pid <> pg_backend_pid();";
                command.ExecuteNonQuery();

                // Give some time for connections to close properly
                Thread.Sleep(700);
            }

            // remove old test db
            command.CommandText = $@"DROP DATABASE IF EXISTS ""{databaseName}"";";
            command.ExecuteNonQuery();

            if (!removeOnly)
            {
                // create new test db
                command.CommandText = $@"CREATE DATABASE ""{databaseName}"";";
                command.ExecuteNonQuery();
            }
        }
    }
}
