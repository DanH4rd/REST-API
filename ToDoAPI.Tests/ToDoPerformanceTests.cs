using NBomber.CSharp;
using System.Net.Http.Json;
using ToDoAPI.Models;

namespace ToDoAPI.Tests
{
    /// <summary>
    /// Performance tests definition.
    /// </summary>
    public class ToDoPerformanceTests : ToDoTestBase
    {
        public ToDoPerformanceTests(DatabaseFixture fixture) : base(fixture) { }

        [Fact]
        public void RunToDoLoadTest()
        {
            // compose the scenario
            var scenario = Scenario.Create("todo_scenario", async context =>
            {
                // 1. Create ToDo
                var createTodoStep = await Step.Run("create_todo", context, async () =>
                {
                    var newTodo = new ToDoItem
                    {
                        Title = "Test ToDo",
                        Description = "Initial Description",
                        ExpiryDate = DateTime.UtcNow.AddDays(1),
                        IsDone = false,
                        PercentComplete = 0
                    };

                    var response = await _fixture.Client.PostAsJsonAsync("/todo", newTodo, _jsonOptions);
                    if (response.IsSuccessStatusCode)
                    {
                        var createdTodo = await response.Content.ReadFromJsonAsync<ToDoItem>(_jsonOptions);
                        context.Data["TodoId"] = createdTodo!.Id; // save Id for next steps
                        return Response.Ok();
                    }

                    return Response.Fail();
                });

                // 2. Complete ToDo
                var updateTodoStep = Step.Run("complete_todo", context, async () =>
                {
                    if (context.Data.TryGetValue("TodoId", out var todoId))
                    {
                        var response = await _fixture.Client.PatchAsync($"/todo/{todoId}/complete", null);
                        return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
                    }
                    return Response.Fail();
                });

                // 3. Get ToDo by Id
                var getTodoStep = Step.Run("get_todo_by_id", context, async () =>
                {
                    if (context.Data.TryGetValue("TodoId", out var todoId))
                    {
                        var response = await _fixture.Client.GetAsync($"/todo/{todoId}");
                        return response.IsSuccessStatusCode ? Response.Ok() : Response.Fail();
                    }
                    return Response.Fail();
                });

                return Response.Ok();
            })
            .WithLoadSimulations(
                Simulation.Inject(rate: 10, // 10 requests at a time
                                  interval: TimeSpan.FromMilliseconds(500), // interval between injections
                                  during: TimeSpan.FromSeconds(30))); // test duration

            // run load tests
            NBomberRunner
                .RegisterScenarios(scenario)
                .Run();
        }
    }
}
