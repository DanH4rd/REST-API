using System.ComponentModel.DataAnnotations;
using ToDoAPI.Data;
using ToDoAPI.Models;

namespace ToDoAPI.Extensions
{
    public static class EndpointsConfiguration
    {
        public static IEndpointRouteBuilder ConfigureToDoRoutes(this IEndpointRouteBuilder endpoints)
        {
            // root welcome
            endpoints.MapGet("/", () => "Welcome to ToDo API!").WithName("Welcome");

            // get all ToDos
            endpoints.MapGet("/todo", async (ToDoRepository repo) => await repo.GetAllAsync())
                     .WithName("GetAllTodos")
                     .WithDescription("Gets all ToDos list");


            // get incoming ToDos
            endpoints.MapGet("/todo/period/{period?}", async (string? period, ToDoRepository repo) =>
            {
                DateTime startDate, endDate;

                switch (period?.ToLower())
                {
                    case "today":
                        startDate = DateTime.Today;
                        endDate = DateTime.Today.AddDays(1).AddTicks(-1);
                        break;

                    case "nextday":
                        startDate = DateTime.Today.AddDays(1);
                        endDate = DateTime.Today.AddDays(2).AddTicks(-1);
                        break;

                    case "currentweek":
                        startDate = DateTime.Today;
                        endDate = DateTime.Today.AddDays(7 - (int)DateTime.Today.DayOfWeek).AddTicks(-1);
                        break;

                    default:
                        return Results.BadRequest("Invalid period. Valid options are: today, nextday, currentweek.");
                }

                var items = await repo.GetRangeAsync(startDate, endDate);
                return items.Any() ? Results.Ok(items) : Results.NotFound("No ToDo items found for the specified period.");
            })
            .WithName("GetTodosByPeriod")
            .WithDescription("Valid values for the 'period' parameter are: 'today', 'nextday', 'currentweek'.");


            // get specific ToDo
            endpoints.MapGet("/todo/{id:int}", async (int id, ToDoRepository repo) =>
            {
                var item = await repo.GetByIdAsync(id);
                return item is not null ? Results.Ok(item) : Results.NotFound();
            })
            .WithName("GetTodoById")
            .WithDescription("Gets a single ToDo by Id.");


            // create ToDo
            endpoints.MapPost("/todo", async (ToDoItem item, ToDoRepository repo) =>
            {
                var validationContext = new ValidationContext(item);
                var validationResults = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(item, validationContext, validationResults, true);

                if (!isValid)
                {
                    return Results.BadRequest(validationResults);
                }

                var insertedId = await repo.CreateAsync(item);
                var createdItem = await repo.GetByIdAsync(insertedId);
                return Results.Created($"/todo/{insertedId}", createdItem);
            })
            .WithName("CreateTodo")
            .WithDescription("Creates a ToDo item.");


            // update ToDo
            endpoints.MapPut("/todo/{id:int}", async (int id, ToDoItem item, ToDoRepository repo) =>
            {
                var validationContext = new ValidationContext(item);
                var validationResults = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(item, validationContext, validationResults, true);

                if (!isValid)
                {
                    return Results.BadRequest(validationResults);
                }

                var existingItem = await repo.GetByIdAsync(id);
                if (existingItem is null)
                {
                    return Results.NotFound($"ToDo item with ID {id} not found.");
                }

                item.Id = id;
                var result = await repo.UpdateAsync(item);
                return result > 0 ? Results.NoContent() : Results.BadRequest("Item could not be updated.");

            })
            .WithName("UpdateTodo")
            .WithDescription("Updates a single ToDo by Id");


            // set ToDo as complete
            // in the task setting percent and IsDone are different operations 
            // however completing ToDo item means setting PercentComplete to 100 and IsDone to true
            // to keep it consistent
            endpoints.MapPatch("/todo/{id:int}/complete", async (int id, ToDoRepository repo) =>
            {
                var existingItem = await repo.GetByIdAsync(id);
                if (existingItem is null)
                {
                    return Results.NotFound($"ToDo item with ID {id} not found.");
                }

                existingItem.PercentComplete = 100;
                existingItem.IsDone = true;

                var result = await repo.UpdateAsync(existingItem);
                return result > 0 ? Results.NoContent() : Results.BadRequest("Item could not be updated.");
            })
            .WithName("MarkTodoAsComplete")
            .WithDescription("Sets a ToDo as completed by Id");


            // delete ToDo
            endpoints.MapDelete("/todo/{id:int}", async (int id, ToDoRepository repo) =>
            {
                var result = await repo.DeleteAsync(id);
                return result > 0 ? Results.NoContent() : Results.NotFound($"ToDo item with ID {id} not found.");
            })
            .WithName("DeleteTodo")
            .WithDescription("Deletes a ToDo by Id");

            return endpoints;
        }
    }
}
