using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPostInteractionsFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PostInteractions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    OriginalPostId = table.Column<int>(type: "int", nullable: false),
                    OriginalAuthorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReplyPostId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostInteractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostInteractions_AspNetUsers_ActorId",
                        column: x => x.ActorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostInteractions_AspNetUsers_OriginalAuthorId",
                        column: x => x.OriginalAuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PostInteractions_PhotoPosts_OriginalPostId",
                        column: x => x.OriginalPostId,
                        principalTable: "PhotoPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostInteractions_PhotoPosts_ReplyPostId",
                        column: x => x.ReplyPostId,
                        principalTable: "PhotoPosts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostInteractions_ActorId",
                table: "PostInteractions",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_PostInteractions_OriginalAuthorId",
                table: "PostInteractions",
                column: "OriginalAuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_PostInteractions_OriginalPostId",
                table: "PostInteractions",
                column: "OriginalPostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostInteractions_ReplyPostId",
                table: "PostInteractions",
                column: "ReplyPostId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostInteractions");
        }
    }
}
