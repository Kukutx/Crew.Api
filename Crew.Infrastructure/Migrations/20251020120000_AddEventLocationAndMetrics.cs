using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Crew.Infrastructure.Migrations
{
    public partial class AddEventLocationAndMetrics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS postgis;", suppressTransaction: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "road_trip_events",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now() at time zone 'utc'");

            migrationBuilder.AddColumn<Point>(
                name: "location",
                table: "road_trip_events",
                type: "geography (Point, 4326)",
                nullable: true);

            migrationBuilder.Sql("ALTER TABLE road_trip_events ALTER COLUMN start_point TYPE geography(Point,4326) USING start_point::geography;");
            migrationBuilder.Sql("ALTER TABLE road_trip_events ALTER COLUMN end_point TYPE geography(Point,4326) USING end_point::geography;");
            migrationBuilder.Sql("UPDATE road_trip_events SET location = start_point WHERE location IS NULL;");
            migrationBuilder.Sql("ALTER TABLE road_trip_events ALTER COLUMN location SET NOT NULL;");

            migrationBuilder.CreateTable(
                name: "event_metrics",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    likes_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    registrations_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_metrics", x => x.event_id);
                    table.ForeignKey(
                        name: "fk_event_metrics_road_trip_events_event_id",
                        column: x => x.event_id,
                        principalTable: "road_trip_events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_road_trip_events_location",
                table: "road_trip_events",
                column: "location")
                .Annotation("Npgsql:IndexMethod", "gist");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_road_trip_events_location",
                table: "road_trip_events");

            migrationBuilder.DropTable(
                name: "event_metrics");

            migrationBuilder.DropColumn(
                name: "location",
                table: "road_trip_events");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "road_trip_events");

            migrationBuilder.Sql("ALTER TABLE road_trip_events ALTER COLUMN start_point TYPE geometry(Point,4326) USING start_point::geometry;");
            migrationBuilder.Sql("ALTER TABLE road_trip_events ALTER COLUMN end_point TYPE geometry(Point,4326) USING end_point::geometry;");
        }
    }
}
