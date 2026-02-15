using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Avtoshkola_DZI.Migrations
{
    /// <inheritdoc />
    public partial class second : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentCourseInstances_AspNetUsers_ClientId",
                table: "StudentCourseInstances");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCourseInstances_CourseInstances_CourseInstanceId",
                table: "StudentCourseInstances");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCourseInstances_Vehicles_VehicleId",
                table: "StudentCourseInstances");

            migrationBuilder.DropIndex(
                name: "IX_StudentCourseInstances_ClientId",
                table: "StudentCourseInstances");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "StudentCourseInstances");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "StudentCourseInstances",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "InstructorId",
                table: "StudentCourseInstances",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCourseInstances_InstructorId",
                table: "StudentCourseInstances",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCourseInstances_StudentId",
                table: "StudentCourseInstances",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCourseInstances_AspNetUsers_InstructorId",
                table: "StudentCourseInstances",
                column: "InstructorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCourseInstances_AspNetUsers_StudentId",
                table: "StudentCourseInstances",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCourseInstances_CourseInstances_CourseInstanceId",
                table: "StudentCourseInstances",
                column: "CourseInstanceId",
                principalTable: "CourseInstances",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCourseInstances_Vehicles_VehicleId",
                table: "StudentCourseInstances",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentCourseInstances_AspNetUsers_InstructorId",
                table: "StudentCourseInstances");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCourseInstances_AspNetUsers_StudentId",
                table: "StudentCourseInstances");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCourseInstances_CourseInstances_CourseInstanceId",
                table: "StudentCourseInstances");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentCourseInstances_Vehicles_VehicleId",
                table: "StudentCourseInstances");

            migrationBuilder.DropIndex(
                name: "IX_StudentCourseInstances_InstructorId",
                table: "StudentCourseInstances");

            migrationBuilder.DropIndex(
                name: "IX_StudentCourseInstances_StudentId",
                table: "StudentCourseInstances");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "StudentCourseInstances",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "InstructorId",
                table: "StudentCourseInstances",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "ClientId",
                table: "StudentCourseInstances",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentCourseInstances_ClientId",
                table: "StudentCourseInstances",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCourseInstances_AspNetUsers_ClientId",
                table: "StudentCourseInstances",
                column: "ClientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCourseInstances_CourseInstances_CourseInstanceId",
                table: "StudentCourseInstances",
                column: "CourseInstanceId",
                principalTable: "CourseInstances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentCourseInstances_Vehicles_VehicleId",
                table: "StudentCourseInstances",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
