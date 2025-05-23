using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QueryInfo.Example.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Grades_Schools_SchoolId",
                table: "Grades");

            migrationBuilder.DropIndex(
                name: "IX_Grades_SchoolId",
                table: "Grades");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "Grades");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SchoolId",
                table: "Grades",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Grades_SchoolId",
                table: "Grades",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Grades_Schools_SchoolId",
                table: "Grades",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
