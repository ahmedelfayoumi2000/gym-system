using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymSystem.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewTablesAndUpdateEquipmentAndAtherTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_SubscriptionPlan_DailyPlanId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_SubscriptionPlan_MonthlyPlanId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_SubscriptionPlan_SubscriptionPlanId",
                table: "Memberships");

            migrationBuilder.DropTable(
                name: "SubscriptionPlan");

            migrationBuilder.DropIndex(
                name: "IX_Memberships_SubscriptionPlanId",
                table: "Memberships");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DailyPlanId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_MonthlyPlanId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlanId",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "DailyPlanId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MonthlyPlanId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Equipments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "dailyAttendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    AttendanceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPresent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dailyAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dailyAttendances_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dailyAttendances_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "monthlyMemberships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monthlyMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_monthlyMemberships_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_monthlyMemberships_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_monthlyMemberships_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_dailyAttendances_ClassId",
                table: "dailyAttendances",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_dailyAttendances_UserId",
                table: "dailyAttendances",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_monthlyMemberships_ClassId",
                table: "monthlyMemberships",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_monthlyMemberships_PlanId",
                table: "monthlyMemberships",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_monthlyMemberships_UserId",
                table: "monthlyMemberships",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dailyAttendances");

            migrationBuilder.DropTable(
                name: "monthlyMemberships");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Equipments");

            migrationBuilder.AddColumn<int>(
                name: "SubscriptionPlanId",
                table: "Memberships",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DailyPlanId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MonthlyPlanId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SubscriptionPlan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    IsStopped = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StopDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationInDays = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlan", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_SubscriptionPlanId",
                table: "Memberships",
                column: "SubscriptionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DailyPlanId",
                table: "AspNetUsers",
                column: "DailyPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_MonthlyPlanId",
                table: "AspNetUsers",
                column: "MonthlyPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_SubscriptionPlan_DailyPlanId",
                table: "AspNetUsers",
                column: "DailyPlanId",
                principalTable: "SubscriptionPlan",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_SubscriptionPlan_MonthlyPlanId",
                table: "AspNetUsers",
                column: "MonthlyPlanId",
                principalTable: "SubscriptionPlan",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_SubscriptionPlan_SubscriptionPlanId",
                table: "Memberships",
                column: "SubscriptionPlanId",
                principalTable: "SubscriptionPlan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
