using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace CrmSystem.Migrations
{
    public partial class FixClientsIdAutoincrement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Отключаем внешние ключи, чтобы можно было удалить таблицу
            migrationBuilder.Sql("PRAGMA foreign_keys=OFF;");

            // Создаём новую таблицу с автоинкрементом
            migrationBuilder.CreateTable(
                name: "Clients_new",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    Phone = table.Column<string>(nullable: false),
                    Address = table.Column<string>(nullable: false),
                    Company = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients_new", x => x.Id);
                });

            // Копируем данные из старой таблицы в новую
            migrationBuilder.Sql(
                @"INSERT INTO Clients_new (Id, Name, Email, Phone, Address, Company, CreatedAt)
                  SELECT Id, Name, Email, Phone, Address, Company, CreatedAt FROM Clients");

            // Удаляем старую таблицу
            migrationBuilder.DropTable(name: "Clients");

            // Переименовываем новую таблицу в Clients
            migrationBuilder.Sql("ALTER TABLE Clients_new RENAME TO Clients;");

            // Включаем обратно внешние ключи
            migrationBuilder.Sql("PRAGMA foreign_keys=ON;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("PRAGMA foreign_keys=OFF;");

            // Создаём таблицу Clients без автоинкремента (откат миграции)
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    Phone = table.Column<string>(nullable: false),
                    Address = table.Column<string>(nullable: false),
                    Company = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            // Копируем данные обратно из Clients_new
            migrationBuilder.Sql(
                @"INSERT INTO Clients (Id, Name, Email, Phone, Address, Company, CreatedAt)
                  SELECT Id, Name, Email, Phone, Address, Company, CreatedAt FROM Clients_new");

            // Удаляем Clients_new
            migrationBuilder.DropTable(name: "Clients_new");

            migrationBuilder.Sql("PRAGMA foreign_keys=ON;");
        }
    }
}
