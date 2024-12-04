using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesToToDoItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ToDoItem_ExpiryDate",
                table: "ToDoItems",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_ToDoItem_Title",
                table: "ToDoItems",
                column: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ToDoItem_ExpiryDate",
                table: "ToDoItems");

            migrationBuilder.DropIndex(
                name: "IX_ToDoItem_Title",
                table: "ToDoItems");
        }
    }
}
