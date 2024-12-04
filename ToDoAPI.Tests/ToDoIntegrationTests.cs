using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using ToDoAPI.Models;

namespace ToDoAPI.Tests
{
    /// <summary>
    /// Integration tests definition.
    /// </summary>
    public class ToDoEndpointsTests : ToDoTestBase
    {
        public ToDoEndpointsTests(DatabaseFixture fixture) : base(fixture) { }

        // GET /todo - get all todos
        [Fact]
        public async Task GetAllTodos_ShouldReturn_List()
        {
            var newTodo1 = new ToDoItem
            {
                Title = "Test ToDo Item 1",
                Description = "Test Description 1",
                ExpiryDate = DateTime.UtcNow.AddDays(1),
                IsDone = false,
                PercentComplete = 12
            };

            var newTodo2 = new ToDoItem
            {
                Title = "Test ToDo Item 2",
                Description = "Test Description 2",
                ExpiryDate = DateTime.UtcNow.AddDays(3),
                IsDone = false,
                PercentComplete = 8
            };

            await _fixture.Client.PostAsJsonAsync("/todo", newTodo1, _jsonOptions);
            await _fixture.Client.PostAsJsonAsync("/todo", newTodo2, _jsonOptions);

            var response = await _fixture.Client.GetAsync("/todo");
            response.EnsureSuccessStatusCode();

            var todos = await response.Content.ReadFromJsonAsync<List<ToDoItem>>(_jsonOptions);
            todos.Should().NotBeNull();
            todos!.Count.Should().BeGreaterThanOrEqualTo(2);
        }

        // POST /todo - create todo
        [Fact]
        public async Task CreateTodo_ShouldAddItemToDatabase()
        {
            var newTodo = new ToDoItem
            {
                Title = "Test ToDo Item",
                Description = "Test Description",
                ExpiryDate = DateTime.UtcNow.AddDays(1),
                IsDone = false,
                PercentComplete = 0
            };

            var response = await _fixture.Client.PostAsJsonAsync("/todo", newTodo, _jsonOptions);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var createdTodo = await response.Content.ReadFromJsonAsync<ToDoItem>(_jsonOptions);
            createdTodo.Should().NotBeNull();
            createdTodo!.Id.Should().BeGreaterThan(0);
            createdTodo!.Title.Should().Be(newTodo.Title);
            createdTodo!.Description.Should().Be(newTodo.Description);
            createdTodo!.ExpiryDate.Date.Should().Be(newTodo.ExpiryDate.Date);
            createdTodo!.IsDone.Should().BeFalse();
            createdTodo!.PercentComplete.Should().Be(0);
        }

        // GET /todo/{id:int} - get task by Id
        [Fact]
        public async Task GetTodoById_ShouldReturnCorrectTodo()
        {
            var newTodo = new ToDoItem { Title = "GetTest", Description = "GetTest Desc", ExpiryDate = DateTime.UtcNow.AddHours(1) };
            var createResponse = await _fixture.Client.PostAsJsonAsync("/todo", newTodo, _jsonOptions);
            var createdTodo = await createResponse.Content.ReadFromJsonAsync<ToDoItem>(_jsonOptions);
            var response = await _fixture.Client.GetAsync($"/todo/{createdTodo!.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // GET /todo/period/{period} - get tasks by period
        [Theory]
        [InlineData("today")]
        [InlineData("nextday")]
        [InlineData("currentweek")]
        public async Task GetTodosByPeriod_ShouldReturnExpectedResults(string period)
        {
            // Arrange: create tasks for different periods
            var todayTodo = new ToDoItem
            {
                Title = "Today's Task",
                Description = "Task for today",
                ExpiryDate = DateTime.UtcNow.AddMinutes(5),
                IsDone = false,
                PercentComplete = 0
            };
            await _fixture.Client.PostAsJsonAsync("/todo", todayTodo, _jsonOptions);

            var nextDayTodo = new ToDoItem
            {
                Title = "Tomorrow's Task",
                Description = "Task for tomorrow",
                ExpiryDate = DateTime.Today.AddDays(1),
                IsDone = false,
                PercentComplete = 0
            };
            await _fixture.Client.PostAsJsonAsync("/todo", nextDayTodo, _jsonOptions);

            var weekTask1 = new ToDoItem
            {
                Title = "Weekly Task 1",
                Description = "Task in current week",
                ExpiryDate = DateTime.Today.AddDays(2),
                IsDone = false,
                PercentComplete = 0
            };
            var weekTask2 = new ToDoItem
            {
                Title = "Weekly Task 2",
                Description = "Another task in current week",
                ExpiryDate = DateTime.Today.AddDays(3),
                IsDone = false,
                PercentComplete = 0
            };

            await _fixture.Client.PostAsJsonAsync("/todo", weekTask1, _jsonOptions);
            await _fixture.Client.PostAsJsonAsync("/todo", weekTask2, _jsonOptions);

            // Act: get todos for the period specified
            var response = await _fixture.Client.GetAsync($"/todo/period/{period}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Assert: check todos count
            var todos = await response.Content.ReadFromJsonAsync<List<ToDoItem>>(_jsonOptions);
            todos.Should().NotBeNull();
            todos!.Count.Should().BeGreaterThanOrEqualTo(1);
        }

        // PUT /todo/{id:int} - update task
        [Fact]
        public async Task UpdateTodo_ShouldUpdateExistingTodo()
        {
            var newTodo = new ToDoItem { Title = "UpdateTest", Description = "Update Desc", ExpiryDate = DateTime.UtcNow.AddHours(1) };
            var createResponse = await _fixture.Client.PostAsJsonAsync("/todo", newTodo, _jsonOptions);
            var createdTodo = await createResponse.Content.ReadFromJsonAsync<ToDoItem>(_jsonOptions);
            createdTodo!.Title = "Updated Title";

            var updateResponse = await _fixture.Client.PutAsJsonAsync($"/todo/{createdTodo.Id}", createdTodo, _jsonOptions);
            updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await _fixture.Client.GetAsync($"/todo/{createdTodo.Id}");
            var updatedTodo = await getResponse.Content.ReadFromJsonAsync<ToDoItem>(_jsonOptions);
            updatedTodo!.Title.Should().Be("Updated Title");
        }

        // PATCH /todo/{id:int}/complete - complete todo
        [Fact]
        public async Task MarkTodoAsComplete_ShouldSetTodoAsCompleted()
        {
            var newTodo = new ToDoItem { Title = "CompleteTest", Description = "Complete Desc", ExpiryDate = DateTime.UtcNow.AddHours(1) };
            var createResponse = await _fixture.Client.PostAsJsonAsync("/todo", newTodo, _jsonOptions);
            var createdTodo = await createResponse.Content.ReadFromJsonAsync<ToDoItem>(_jsonOptions);

            var response = await _fixture.Client.PatchAsync($"/todo/{createdTodo!.Id}/complete", null);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await _fixture.Client.GetAsync($"/todo/{createdTodo.Id}");
            var completedTodo = await getResponse.Content.ReadFromJsonAsync<ToDoItem>(_jsonOptions);
            completedTodo!.IsDone.Should().BeTrue();
            completedTodo.PercentComplete.Should().Be(100);
        }

        // DELETE /todo/{id:int} - remove todo
        [Fact]
        public async Task DeleteTodo_ShouldRemoveTodoFromDatabase()
        {
            var newTodo = new ToDoItem { Title = "DeleteTest", Description = "Delete Desc", ExpiryDate = DateTime.UtcNow.AddHours(1) };
            var createResponse = await _fixture.Client.PostAsJsonAsync("/todo", newTodo, _jsonOptions);
            var createdTodo = await createResponse.Content.ReadFromJsonAsync<ToDoItem>(_jsonOptions);

            var deleteResponse = await _fixture.Client.DeleteAsync($"/todo/{createdTodo!.Id}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await _fixture.Client.GetAsync($"/todo/{createdTodo.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
