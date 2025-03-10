using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymSystem.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewTablesAndUpdateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Classes_ClassId",
                table: "Attendances");

            migrationBuilder.AddColumn<int>(
                name: "HaveDays",
                table: "monthlyMemberships",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "monthlyMemberships",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StopDate",
                table: "monthlyMemberships",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserCode",
                table: "monthlyMemberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                table: "monthlyMemberships",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "monthlyMemberships",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Classes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Classes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "ClassName",
                table: "Classes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "MemberName",
                table: "Classes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PlanId",
                table: "Classes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MembershipId",
                table: "Attendances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserCode",
                table: "Attendances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_PlanId",
                table: "Classes",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_MembershipId",
                table: "Attendances",
                column: "MembershipId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Classes_ClassId",
                table: "Attendances",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_monthlyMemberships_MembershipId",
                table: "Attendances",
                column: "MembershipId",
                principalTable: "monthlyMemberships",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Plans_PlanId",
                table: "Classes",
                column: "PlanId",
                principalTable: "Plans",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Classes_ClassId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_monthlyMemberships_MembershipId",
                table: "Attendances");

            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Plans_PlanId",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Classes_PlanId",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Attendances_MembershipId",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "HaveDays",
                table: "monthlyMemberships");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "monthlyMemberships");

            migrationBuilder.DropColumn(
                name: "StopDate",
                table: "monthlyMemberships");

            migrationBuilder.DropColumn(
                name: "UserCode",
                table: "monthlyMemberships");

            migrationBuilder.DropColumn(
                name: "UserEmail",
                table: "monthlyMemberships");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "monthlyMemberships");

            migrationBuilder.DropColumn(
                name: "MemberName",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "PlanId",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "MembershipId",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "UserCode",
                table: "Attendances");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Classes",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Classes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ClassName",
                table: "Classes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Classes_ClassId",
                table: "Attendances",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
