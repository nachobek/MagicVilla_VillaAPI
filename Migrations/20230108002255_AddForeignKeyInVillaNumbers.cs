using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicVillaVillaAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeyInVillaNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VillaId",
                table: "VillaNumbers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "VillaNumbers",
                keyColumn: "VillaNo",
                keyValue: 101,
                columns: new[] { "CreatedDate", "VillaId" },
                values: new object[] { new DateTime(2023, 1, 7, 21, 22, 55, 474, DateTimeKind.Local).AddTicks(2520), 0 });

            migrationBuilder.UpdateData(
                table: "VillaNumbers",
                keyColumn: "VillaNo",
                keyValue: 201,
                columns: new[] { "CreatedDate", "VillaId" },
                values: new object[] { new DateTime(2023, 1, 7, 21, 22, 55, 474, DateTimeKind.Local).AddTicks(2520), 0 });

            migrationBuilder.UpdateData(
                table: "VillaNumbers",
                keyColumn: "VillaNo",
                keyValue: 301,
                columns: new[] { "CreatedDate", "VillaId" },
                values: new object[] { new DateTime(2023, 1, 7, 21, 22, 55, 474, DateTimeKind.Local).AddTicks(2520), 0 });

            migrationBuilder.UpdateData(
                table: "VillaNumbers",
                keyColumn: "VillaNo",
                keyValue: 401,
                columns: new[] { "CreatedDate", "VillaId" },
                values: new object[] { new DateTime(2023, 1, 7, 21, 22, 55, 474, DateTimeKind.Local).AddTicks(2520), 0 });

            migrationBuilder.UpdateData(
                table: "VillaNumbers",
                keyColumn: "VillaNo",
                keyValue: 501,
                columns: new[] { "CreatedDate", "VillaId" },
                values: new object[] { new DateTime(2023, 1, 7, 21, 22, 55, 474, DateTimeKind.Local).AddTicks(2530), 0 });

            migrationBuilder.UpdateData(
                table: "Villas",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2023, 1, 7, 21, 22, 55, 474, DateTimeKind.Local).AddTicks(2420));

            migrationBuilder.UpdateData(
                table: "Villas",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2023, 1, 7, 21, 22, 55, 474, DateTimeKind.Local).AddTicks(2440));

            migrationBuilder.UpdateData(
                table: "Villas",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2023, 1, 7, 21, 22, 55, 474, DateTimeKind.Local).AddTicks(2450));

            migrationBuilder.UpdateData(
                table: "Villas",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2023, 1, 7, 21, 22, 55, 474, DateTimeKind.Local).AddTicks(2470));

            migrationBuilder.UpdateData(
                table: "Villas",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2023, 1, 7, 21, 22, 55, 474, DateTimeKind.Local).AddTicks(2470));

            migrationBuilder.CreateIndex(
                name: "IX_VillaNumbers_VillaId",
                table: "VillaNumbers",
                column: "VillaId");

            migrationBuilder.AddForeignKey(
                name: "FK_VillaNumbers_Villas_VillaId",
                table: "VillaNumbers",
                column: "VillaId",
                principalTable: "Villas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VillaNumbers_Villas_VillaId",
                table: "VillaNumbers");

            migrationBuilder.DropIndex(
                name: "IX_VillaNumbers_VillaId",
                table: "VillaNumbers");

            migrationBuilder.DropColumn(
                name: "VillaId",
                table: "VillaNumbers");

            migrationBuilder.UpdateData(
                table: "VillaNumbers",
                keyColumn: "VillaNo",
                keyValue: 101,
                column: "CreatedDate",
                value: new DateTime(2023, 1, 7, 12, 49, 39, 354, DateTimeKind.Local).AddTicks(4720));

            migrationBuilder.UpdateData(
                table: "VillaNumbers",
                keyColumn: "VillaNo",
                keyValue: 201,
                column: "CreatedDate",
                value: new DateTime(2023, 1, 7, 12, 49, 39, 354, DateTimeKind.Local).AddTicks(4720));

            migrationBuilder.UpdateData(
                table: "VillaNumbers",
                keyColumn: "VillaNo",
                keyValue: 301,
                column: "CreatedDate",
                value: new DateTime(2023, 1, 7, 12, 49, 39, 354, DateTimeKind.Local).AddTicks(4720));

            migrationBuilder.UpdateData(
                table: "VillaNumbers",
                keyColumn: "VillaNo",
                keyValue: 401,
                column: "CreatedDate",
                value: new DateTime(2023, 1, 7, 12, 49, 39, 354, DateTimeKind.Local).AddTicks(4720));

            migrationBuilder.UpdateData(
                table: "VillaNumbers",
                keyColumn: "VillaNo",
                keyValue: 501,
                column: "CreatedDate",
                value: new DateTime(2023, 1, 7, 12, 49, 39, 354, DateTimeKind.Local).AddTicks(4730));

            migrationBuilder.UpdateData(
                table: "Villas",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2023, 1, 7, 12, 49, 39, 354, DateTimeKind.Local).AddTicks(4630));

            migrationBuilder.UpdateData(
                table: "Villas",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2023, 1, 7, 12, 49, 39, 354, DateTimeKind.Local).AddTicks(4650));

            migrationBuilder.UpdateData(
                table: "Villas",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2023, 1, 7, 12, 49, 39, 354, DateTimeKind.Local).AddTicks(4660));

            migrationBuilder.UpdateData(
                table: "Villas",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedDate",
                value: new DateTime(2023, 1, 7, 12, 49, 39, 354, DateTimeKind.Local).AddTicks(4660));

            migrationBuilder.UpdateData(
                table: "Villas",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedDate",
                value: new DateTime(2023, 1, 7, 12, 49, 39, 354, DateTimeKind.Local).AddTicks(4660));
        }
    }
}
