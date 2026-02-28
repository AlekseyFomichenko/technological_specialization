using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS files CASCADE;
                DROP TABLE IF EXISTS messages CASCADE;
                DROP TABLE IF EXISTS sessions CASCADE;
                DROP TABLE IF EXISTS users CASCADE;
            ");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Login = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Login);
                });

            migrationBuilder.CreateTable(
                name: "files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderLogin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReceiverLogin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDelivered = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_files_users_ReceiverLogin",
                        column: x => x.ReceiverLogin,
                        principalTable: "users",
                        principalColumn: "Login",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_files_users_SenderLogin",
                        column: x => x.SenderLogin,
                        principalTable: "users",
                        principalColumn: "Login",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderLogin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReceiverLogin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDelivered = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_messages_users_ReceiverLogin",
                        column: x => x.ReceiverLogin,
                        principalTable: "users",
                        principalColumn: "Login",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_messages_users_SenderLogin",
                        column: x => x.SenderLogin,
                        principalTable: "users",
                        principalColumn: "Login",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserLogin = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sessions_users_UserLogin",
                        column: x => x.UserLogin,
                        principalTable: "users",
                        principalColumn: "Login",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_files_ReceiverLogin",
                table: "files",
                column: "ReceiverLogin");

            migrationBuilder.CreateIndex(
                name: "IX_files_SenderLogin",
                table: "files",
                column: "SenderLogin");

            migrationBuilder.CreateIndex(
                name: "IX_messages_CreatedAt",
                table: "messages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_messages_ReceiverLogin",
                table: "messages",
                column: "ReceiverLogin");

            migrationBuilder.CreateIndex(
                name: "IX_messages_SenderLogin",
                table: "messages",
                column: "SenderLogin");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_Token",
                table: "sessions",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_UserLogin",
                table: "sessions",
                column: "UserLogin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "files");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
