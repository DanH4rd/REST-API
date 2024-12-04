# ToDo Web API
REST API that covers typical operations, .NET 9 is used.
The project doesn’t need additional configuration and should compile and run as is.

# Minimal API
Minimal API is chosen (over traditional Web API) to simplify the development process and reduce boilerplate code. Minimal API offers a lightweight, faster startup, and more concise syntax for small projects like this while maintaining flexibility and performance.

# Database
Database is PostgreSQL hosted on https://neon.tech free tier.
As of now the connection strings added to the repository for simplicity, you can change it to specify your own database, the required tables will be created automatically from migrations.
As this is not a best practice it can be resolved via local secrets or environment variables.

# ORM
Dapper was selected for this project due to its lightweight nature and high performance when executing raw SQL queries. It offers minimal overhead and allows precise control over SQL.
EF Core is used for DB migrations management.
Combining Dapper with EF Core leverages the best of both worlds: 
the performance and flexibility of Dapper for data access,
convenience and structure of EF Core for schema evolution and database management. 

# Code First
Code first approach is applied to this project. This approach offers greater flexibility and control over the database schema by allowing to define models directly in code, which automatically generates the database. It simplifies version control, supports migrations seamlessly, and is ideal for the ToDo project where the database evolves alongside the application.

# Scalar 
Scalar API (https://scalar.com) is used as an alternative API documentation tool for .NET applications (similar to Swagger) to document and explore API endpoints.
To explore ToDo API endpoints follow this relative path in browser:
/scalar/v1 (ex. http://localhost:5063/scalar/v1)

# Localization
Localization is implemented for the model validation messages, supports en-US and pl-PL cultures (default is en-US) currently.

# Possible Further improvements
authorization based on JWT,
GitHub CI/CD pipeline based on GitHub Actions,
Logging using Serilog.

# Integration Tests
xUnit testing tool is used for this task. 
A separate test DB is created before each test set run (using migrations from the main API project) and deleted after all tests are completed.

# Load Tests
NBomber load testing framework (https://nbomber.com) is used here which is ideally integrated into xUnit tests architecture.

Both integration and performance tests don’t require additional configuration and should run from VS 2022. 

# Run in Container
Docker support is added to the ToDoAPI project and the ‘Dockerfile’ is added. To run the API in a container the Docker Desktop application should be installed and running locally.


