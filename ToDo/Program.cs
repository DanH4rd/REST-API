
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Scalar.AspNetCore;
using ToDoAPI.Data;
using ToDoAPI.Extensions;

namespace ToDoAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // add services to the container.
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
            builder.Services.AddControllers().AddDataAnnotationsLocalization();
            builder.Services.AddOpenApi();
            builder.Services.AddScoped<ToDoRepository>();

            // logging config
            // to add logging to a file we can use Serilog package, not configured here 
            builder.Logging
                .ClearProviders()
                .AddConsole();

            var app = builder.Build();

            // apply db changes (migrations) automatically
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.Migrate();
            }

            // configure localization
            var supportedCultures = new[]
            {
                new CultureInfo("en-US"),
                new CultureInfo("pl-PL")
            };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures,
                RequestCultureProviders = new[] { new QueryStringRequestCultureProvider() }
            });

            // use error handling middleware
            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.ConfigureToDoRoutes();

            // configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapScalarApiReference(); // alternative to swagger, scalar/v1
                app.MapOpenApi();
            }

            app.Run();
        }
    }
}