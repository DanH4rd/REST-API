using Microsoft.EntityFrameworkCore;
using ToDoAPI.Models;

namespace ToDoAPI.Data
{
    /// <summary>
    /// Class describes ToDo data context.
    /// </summary>
    public class AppDbContext : DbContext
    {
        // we use required modifier to avoid compiler warning
        // "Non-nullable property 'ToDoItems' must contain a non-null value when exiting constructor"
        public required DbSet<ToDoItem> ToDoItems { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // set Id as primary key
            modelBuilder.Entity<ToDoItem>().HasKey(t => t.Id);

            // create non-unique index for Title
            modelBuilder.Entity<ToDoItem>()
                .HasIndex(t => t.Title)
                .HasDatabaseName("IX_ToDoItem_Title");

            // create non-unique index for ExpiryDate
            modelBuilder.Entity<ToDoItem>()
                .HasIndex(t => t.ExpiryDate)
                .HasDatabaseName("IX_ToDoItem_ExpiryDate");
        }
    }
}
