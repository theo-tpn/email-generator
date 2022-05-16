using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Noctus.Persistence.Migrations
{
    public partial class Initial009 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GenBucketConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ref = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdateOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenBucketConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IdentifiersInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MotherBoardSerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserDiscordId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdateOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentifiersInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LicenseKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdateOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountsGenBuckets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ref = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentStock = table.Column<int>(type: "int", nullable: false),
                    LicenseKeyId = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdateOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountsGenBuckets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountsGenBuckets_LicenseKeys_LicenseKeyId",
                        column: x => x.LicenseKeyId,
                        principalTable: "LicenseKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LicenseKeyEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Event = table.Column<int>(type: "int", nullable: false),
                    IdentifiersInfoId = table.Column<int>(type: "int", nullable: true),
                    LicenseKeyId = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdateOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseKeyEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LicenseKeyEvents_IdentifiersInfo_IdentifiersInfoId",
                        column: x => x.IdentifiersInfoId,
                        principalTable: "IdentifiersInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LicenseKeyEvents_LicenseKeys_LicenseKeyId",
                        column: x => x.LicenseKeyId,
                        principalTable: "LicenseKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LicenseKeyFlags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentifiersInfoId = table.Column<int>(type: "int", nullable: true),
                    LicenseKeyId = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdateOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseKeyFlags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LicenseKeyFlags_IdentifiersInfo_IdentifiersInfoId",
                        column: x => x.IdentifiersInfoId,
                        principalTable: "IdentifiersInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LicenseKeyFlags_LicenseKeys_LicenseKeyId",
                        column: x => x.LicenseKeyId,
                        principalTable: "LicenseKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PipelineRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PvaCountryCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountCountryCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobsNumber = table.Column<int>(type: "int", nullable: false),
                    JobsParallelism = table.Column<int>(type: "int", nullable: false),
                    HasForwarding = table.Column<bool>(type: "bit", nullable: false),
                    LicenseKeyId = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdateOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PipelineRuns_LicenseKeys_LicenseKeyId",
                        column: x => x.LicenseKeyId,
                        principalTable: "LicenseKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PipelineEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    IdentifiersInfoId = table.Column<int>(type: "int", nullable: true),
                    PipelineRunId = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdateOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PipelineEvents_IdentifiersInfo_IdentifiersInfoId",
                        column: x => x.IdentifiersInfoId,
                        principalTable: "IdentifiersInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PipelineEvents_PipelineRuns_PipelineRunId",
                        column: x => x.PipelineRunId,
                        principalTable: "PipelineRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountsGenBuckets_LicenseKeyId",
                table: "AccountsGenBuckets",
                column: "LicenseKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseKeyEvents_IdentifiersInfoId",
                table: "LicenseKeyEvents",
                column: "IdentifiersInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseKeyEvents_LicenseKeyId",
                table: "LicenseKeyEvents",
                column: "LicenseKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseKeyFlags_IdentifiersInfoId",
                table: "LicenseKeyFlags",
                column: "IdentifiersInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseKeyFlags_LicenseKeyId",
                table: "LicenseKeyFlags",
                column: "LicenseKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineEvents_IdentifiersInfoId",
                table: "PipelineEvents",
                column: "IdentifiersInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineEvents_PipelineRunId",
                table: "PipelineEvents",
                column: "PipelineRunId");

            migrationBuilder.CreateIndex(
                name: "IX_PipelineRuns_LicenseKeyId",
                table: "PipelineRuns",
                column: "LicenseKeyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountsGenBuckets");

            migrationBuilder.DropTable(
                name: "GenBucketConfigs");

            migrationBuilder.DropTable(
                name: "LicenseKeyEvents");

            migrationBuilder.DropTable(
                name: "LicenseKeyFlags");

            migrationBuilder.DropTable(
                name: "PipelineEvents");

            migrationBuilder.DropTable(
                name: "IdentifiersInfo");

            migrationBuilder.DropTable(
                name: "PipelineRuns");

            migrationBuilder.DropTable(
                name: "LicenseKeys");
        }
    }
}
