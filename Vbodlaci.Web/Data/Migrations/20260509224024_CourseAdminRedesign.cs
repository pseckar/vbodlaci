using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Vbodlaci.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class CourseAdminRedesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Courses_Status_StartDateTime",
                table: "Courses");

            migrationBuilder.AddColumn<DateOnly>(
                name: "CourseDate",
                table: "Courses",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Courses",
                type: "character varying(260)",
                maxLength: 260,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsFullDescriptionVisible",
                table: "Courses",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsWhatToExpectVisible",
                table: "Courses",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailPath",
                table: "Courses",
                type: "character varying(260)",
                maxLength: 260,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TimeText",
                table: "Courses",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "Courses"
                SET
                    "CourseDate" = ("StartDateTime" AT TIME ZONE 'Europe/Prague')::date,
                    "TimeText" = to_char(("StartDateTime" AT TIME ZONE 'Europe/Prague'), 'HH24:MI'),
                    "IsFullDescriptionVisible" = TRUE,
                    "IsWhatToExpectVisible" = TRUE
                """);

            migrationBuilder.DropColumn(
                name: "EndDateTime",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "RegistrationDeadline",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "StartDateTime",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "VenueText",
                table: "Courses");

            migrationBuilder.CreateTable(
                name: "CourseTextDefaults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Field = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Text = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseTextDefaults", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "CourseTextDefaults",
                columns: new[] { "Id", "Field", "Text", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("2ef8d70f-f5c0-49a0-8dbf-fc89040d1931"), "ShortDescription", "This is placeholder for default text", "Breathwork", new DateTimeOffset(new DateTime(2026, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("49bdd9a5-b692-4d46-9797-5be87405e914"), "ShortDescription", "This is placeholder for default text", "Horses", new DateTimeOffset(new DateTime(2026, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("5665bfd0-4e84-4699-9174-68eba17f8d41"), "FullDescription", "This is placeholder for default text", "Breathwork", new DateTimeOffset(new DateTime(2026, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("c3addbf4-e5a7-45fd-8e1d-3d079223a967"), "FullDescription", "This is placeholder for default text", "Horses", new DateTimeOffset(new DateTime(2026, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("f5a4d334-301e-4015-9443-425649bc74c8"), "WhatToExpect", "This is placeholder for default text", "Breathwork", new DateTimeOffset(new DateTime(2026, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("fc589548-f8b9-4e50-a7de-4d618d9857d4"), "WhatToExpect", "This is placeholder for default text", "Horses", new DateTimeOffset(new DateTime(2026, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Status_CourseDate",
                table: "Courses",
                columns: new[] { "Status", "CourseDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseTextDefaults_Type_Field",
                table: "CourseTextDefaults",
                columns: new[] { "Type", "Field" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseTextDefaults");

            migrationBuilder.DropIndex(
                name: "IX_Courses_Status_CourseDate",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "CourseDate",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "IsFullDescriptionVisible",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "IsWhatToExpectVisible",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "ThumbnailPath",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "TimeText",
                table: "Courses");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EndDateTime",
                table: "Courses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RegistrationDeadline",
                table: "Courses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartDateTime",
                table: "Courses",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "VenueText",
                table: "Courses",
                type: "character varying(180)",
                maxLength: 180,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Status_StartDateTime",
                table: "Courses",
                columns: new[] { "Status", "StartDateTime" });
        }
    }
}
