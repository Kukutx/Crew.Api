using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Crew.Infrastructure.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    firebase_uid = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    display_name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "chat_groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    scope = table.Column<int>(type: "INTEGER", nullable: false),
                    event_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chat_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    type = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    payload = table.Column<string>(type: "TEXT", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    error = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "private_dialogs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    user_a = table.Column<Guid>(type: "TEXT", nullable: false),
                    user_b = table.Column<Guid>(type: "TEXT", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_private_dialogs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "road_trip_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    owner_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    title = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    start_time = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    end_time = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    start_point = table.Column<Point>(type: "BLOB", nullable: false),
                    end_point = table.Column<Point>(type: "BLOB", nullable: true),
                    route_polyline = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: true),
                    max_participants = table.Column<int>(type: "INTEGER", nullable: true),
                    visibility = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_road_trip_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "chat_memberships",
                columns: table => new
                {
                    group_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    role = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    joined_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chat_memberships", x => new { x.group_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_chat_memberships_chat_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "chat_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_chat_memberships_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chat_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    group_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    sender_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    content = table.Column<string>(type: "TEXT", nullable: false),
                    attachments_json = table.Column<string>(type: "TEXT", nullable: true),
                    sent_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chat_messages", x => x.id);
                    table.ForeignKey(
                        name: "fk_chat_messages_chat_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "chat_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_segments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    event_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    seq = table.Column<int>(type: "INTEGER", nullable: false),
                    waypoint = table.Column<Point>(type: "BLOB", nullable: false),
                    note = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_segments", x => x.id);
                    table.ForeignKey(
                        name: "fk_event_segments_road_trip_events_event_id",
                        column: x => x.event_id,
                        principalTable: "road_trip_events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "private_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    dialog_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    sender_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    content = table.Column<string>(type: "TEXT", nullable: false),
                    sent_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_private_messages", x => x.id);
                    table.ForeignKey(
                        name: "fk_private_messages_private_dialogs_dialog_id",
                        column: x => x.dialog_id,
                        principalTable: "private_dialogs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "registrations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    event_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    status = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_registrations", x => x.id);
                    table.ForeignKey(
                        name: "fk_registrations_road_trip_events_event_id",
                        column: x => x.event_id,
                        principalTable: "road_trip_events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_registrations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_chat_memberships_user_id",
                table: "chat_memberships",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_chat_messages_group_id",
                table: "chat_messages",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_segments_event_id",
                table: "event_segments",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_private_dialogs_user_a_user_b",
                table: "private_dialogs",
                columns: new[] { "user_a", "user_b" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_private_messages_dialog_id",
                table: "private_messages",
                column: "dialog_id");

            migrationBuilder.CreateIndex(
                name: "ix_registrations_event_id",
                table: "registrations",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_registrations_user_id",
                table: "registrations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_registrations_event_id_user_id",
                table: "registrations",
                columns: new[] { "event_id", "user_id" },
                unique: true);



            migrationBuilder.CreateIndex(
                name: "ix_users_firebase_uid",
                table: "users",
                column: "firebase_uid",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chat_memberships");

            migrationBuilder.DropTable(
                name: "chat_messages");

            migrationBuilder.DropTable(
                name: "event_segments");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "private_messages");

            migrationBuilder.DropTable(
                name: "registrations");

            migrationBuilder.DropTable(
                name: "chat_groups");

            migrationBuilder.DropTable(
                name: "private_dialogs");

            migrationBuilder.DropTable(
                name: "road_trip_events");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
