using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymSystem.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class LatestUpdateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_WorkoutPlans_WorkoutPlanId",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_UserFavoriteExercises_Exercises_ExerciseId",
                table: "UserFavoriteExercises");

            migrationBuilder.DropIndex(
                name: "IX_UserFavoriteExercises_ExerciseId",
                table: "UserFavoriteExercises");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_WorkoutPlanId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "WorkoutPlans");

            migrationBuilder.DropColumn(
                name: "ExerciseId",
                table: "UserFavoriteExercises");

            migrationBuilder.DropColumn(
                name: "DayOfWeek",
                table: "gymSchedules");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "WorkoutPlanId",
                table: "Exercises",
                newName: "ExpectedCalories");

            migrationBuilder.AddColumn<int>(
                name: "MembershipId",
                table: "WorkoutPlans",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RestTimeSeconds",
                table: "Exercises",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Goal",
                table: "AspNetUsers",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FitnessLevel",
                table: "AspNetUsers",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ExerciseUserFavoriteExercise",
                columns: table => new
                {
                    ExercisesId = table.Column<int>(type: "int", nullable: false),
                    UserFavoriteExercisesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseUserFavoriteExercise", x => new { x.ExercisesId, x.UserFavoriteExercisesId });
                    table.ForeignKey(
                        name: "FK_ExerciseUserFavoriteExercise_Exercises_ExercisesId",
                        column: x => x.ExercisesId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseUserFavoriteExercise_UserFavoriteExercises_UserFavoriteExercisesId",
                        column: x => x.UserFavoriteExercisesId,
                        principalTable: "UserFavoriteExercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseWorkoutPlan",
                columns: table => new
                {
                    ExercisesId = table.Column<int>(type: "int", nullable: false),
                    WorkoutPlansId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseWorkoutPlan", x => new { x.ExercisesId, x.WorkoutPlansId });
                    table.ForeignKey(
                        name: "FK_ExerciseWorkoutPlan_Exercises_ExercisesId",
                        column: x => x.ExercisesId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseWorkoutPlan_WorkoutPlans_WorkoutPlansId",
                        column: x => x.WorkoutPlansId,
                        principalTable: "WorkoutPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GymScheduleDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GymScheduleId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GymScheduleDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GymScheduleDays_gymSchedules_GymScheduleId",
                        column: x => x.GymScheduleId,
                        principalTable: "gymSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDailyStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CaloriesBurned = table.Column<int>(type: "int", nullable: false),
                    TotalCalories = table.Column<int>(type: "int", nullable: false),
                    Steps = table.Column<int>(type: "int", nullable: false),
                    TotalSteps = table.Column<int>(type: "int", nullable: false),
                    WaterIntake = table.Column<int>(type: "int", nullable: false),
                    TotalWater = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDailyStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDailyStats_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlans_MembershipId",
                table: "WorkoutPlans",
                column: "MembershipId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseUserFavoriteExercise_UserFavoriteExercisesId",
                table: "ExerciseUserFavoriteExercise",
                column: "UserFavoriteExercisesId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseWorkoutPlan_WorkoutPlansId",
                table: "ExerciseWorkoutPlan",
                column: "WorkoutPlansId");

            migrationBuilder.CreateIndex(
                name: "IX_GymScheduleDays_GymScheduleId",
                table: "GymScheduleDays",
                column: "GymScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDailyStats_UserId",
                table: "UserDailyStats",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutPlans_Membershipp_MembershipId",
                table: "WorkoutPlans",
                column: "MembershipId",
                principalTable: "Membershipp",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutPlans_Membershipp_MembershipId",
                table: "WorkoutPlans");

            migrationBuilder.DropTable(
                name: "ExerciseUserFavoriteExercise");

            migrationBuilder.DropTable(
                name: "ExerciseWorkoutPlan");

            migrationBuilder.DropTable(
                name: "GymScheduleDays");

            migrationBuilder.DropTable(
                name: "UserDailyStats");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutPlans_MembershipId",
                table: "WorkoutPlans");

            migrationBuilder.DropColumn(
                name: "MembershipId",
                table: "WorkoutPlans");

            migrationBuilder.DropColumn(
                name: "RestTimeSeconds",
                table: "Exercises");

            migrationBuilder.RenameColumn(
                name: "ExpectedCalories",
                table: "Exercises",
                newName: "WorkoutPlanId");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "WorkoutPlans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExerciseId",
                table: "UserFavoriteExercises",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DayOfWeek",
                table: "gymSchedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Goal",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FitnessLevel",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteExercises_ExerciseId",
                table: "UserFavoriteExercises",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_WorkoutPlanId",
                table: "Exercises",
                column: "WorkoutPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_WorkoutPlans_WorkoutPlanId",
                table: "Exercises",
                column: "WorkoutPlanId",
                principalTable: "WorkoutPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserFavoriteExercises_Exercises_ExerciseId",
                table: "UserFavoriteExercises",
                column: "ExerciseId",
                principalTable: "Exercises",
                principalColumn: "Id");
        }
    }
}
