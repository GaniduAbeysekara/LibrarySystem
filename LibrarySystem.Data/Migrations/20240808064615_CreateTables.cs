using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibrarySystem.Data.Migrations
{
    public partial class CreateTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Auth",
                columns: table => new
                {
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AuthId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PasswordHash = table.Column<byte[]>(type: "varbinary(500)", maxLength: 500, nullable: false),
                    PasswordSalt = table.Column<byte[]>(type: "varbinary(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auth", x => x.Email);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    ISBN = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BookTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Author = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.ISBN);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", maxLength: 5, nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => new { x.UserId, x.Email });
                });

            migrationBuilder.InsertData(
                table: "Auth",
                columns: new[] { "Email", "PasswordHash", "PasswordSalt" },
                values: new object[] { "admin@email.com", new byte[] { 160, 20, 206, 200, 109, 191, 114, 239, 99, 190, 209, 59, 43, 58, 30, 52, 237, 109, 220, 47, 57, 126, 212, 44, 224, 31, 177, 22, 72, 227, 242, 10 }, new byte[] { 213, 210, 162, 163, 108, 15, 103, 180, 241, 97, 128, 19, 120, 135, 200, 74 } });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Email", "UserId", "FirstName", "Gender", "IsAdmin", "IsDelete", "LastName", "PhoneNumber" },
                values: new object[] { "admin@email.com", 1, "Admin", "Male", true, false, "Admin", "0000000000" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Auth");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
