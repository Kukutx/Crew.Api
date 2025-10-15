using System;
using Crew.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crew.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);
            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "postgis");

            modelBuilder.Entity("Crew.Domain.Entities.ChatGroup", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("uuid");

                b.Property<DateTimeOffset>("CreatedAt")
                    .HasColumnType("timestamp with time zone");

                b.Property<Guid?>("EventId")
                    .HasColumnType("uuid");

                b.Property<int>("Scope")
                    .HasColumnType("integer");

                b.HasKey("Id");

                b.ToTable("chat_groups");
            });

            modelBuilder.Entity("Crew.Domain.Entities.ChatMembership", b =>
            {
                b.Property<Guid>("GroupId")
                    .HasColumnType("uuid");

                b.Property<Guid>("UserId")
                    .HasColumnType("uuid");

                b.Property<DateTimeOffset>("JoinedAt")
                    .HasColumnType("timestamp with time zone");

                b.Property<string>("Role")
                    .HasMaxLength(32)
                    .HasColumnType("character varying(32)");

                b.HasKey("GroupId", "UserId");

                b.HasIndex("UserId");

                b.ToTable("chat_memberships");
            });

            modelBuilder.Entity("Crew.Domain.Entities.ChatMessage", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("uuid");

                b.Property<string>("AttachmentsJson")
                    .HasColumnType("text");

                b.Property<string>("Content")
                    .HasColumnType("text");

                b.Property<Guid>("GroupId")
                    .HasColumnType("uuid");

                b.Property<Guid>("SenderId")
                    .HasColumnType("uuid");

                b.Property<DateTimeOffset>("SentAt")
                    .HasColumnType("timestamp with time zone");

                b.HasKey("Id");

                b.HasIndex("GroupId");

                b.ToTable("chat_messages");
            });

            modelBuilder.Entity("Crew.Domain.Entities.EventSegment", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("uuid");

                b.Property<Guid>("EventId")
                    .HasColumnType("uuid");

                b.Property<string>("Note")
                    .HasColumnType("text");

                b.Property<int>("Seq")
                    .HasColumnType("integer");

                b.Property<Point>("Waypoint")
                    .HasColumnType("geometry (Point, 4326)")
                    .HasSrid(4326);

                b.HasKey("Id");

                b.HasIndex("EventId");

                b.ToTable("event_segments");
            });

            modelBuilder.Entity("Crew.Domain.Entities.OutboxMessage", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("uuid");

                b.Property<string>("Error")
                    .HasColumnType("text");

                b.Property<DateTimeOffset>("OccurredAt")
                    .HasColumnType("timestamp with time zone");

                b.Property<string>("Payload")
                    .HasColumnType("text");

                b.Property<DateTimeOffset?>("ProcessedAt")
                    .HasColumnType("timestamp with time zone");

                b.Property<string>("Type")
                    .HasMaxLength(256)
                    .HasColumnType("character varying(256)");

                b.HasKey("Id");

                b.ToTable("outbox_messages");
            });

            modelBuilder.Entity("Crew.Domain.Entities.PrivateDialog", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("uuid");

                b.Property<DateTimeOffset>("CreatedAt")
                    .HasColumnType("timestamp with time zone");

                b.Property<Guid>("UserA")
                    .HasColumnType("uuid");

                b.Property<Guid>("UserB")
                    .HasColumnType("uuid");

                b.HasKey("Id");

                b.HasIndex("UserA", "UserB")
                    .IsUnique();

                b.ToTable("private_dialogs");
            });

            modelBuilder.Entity("Crew.Domain.Entities.PrivateMessage", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("uuid");

                b.Property<string>("Content")
                    .HasColumnType("text");

                b.Property<Guid>("DialogId")
                    .HasColumnType("uuid");

                b.Property<Guid>("SenderId")
                    .HasColumnType("uuid");

                b.Property<DateTimeOffset>("SentAt")
                    .HasColumnType("timestamp with time zone");

                b.HasKey("Id");

                b.HasIndex("DialogId");

                b.ToTable("private_messages");
            });

            modelBuilder.Entity("Crew.Domain.Entities.Registration", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("uuid");

                b.Property<DateTimeOffset>("CreatedAt")
                    .HasColumnType("timestamp with time zone");

                b.Property<Guid>("EventId")
                    .HasColumnType("uuid");

                b.Property<int>("Status")
                    .HasColumnType("integer");

                b.Property<Guid>("UserId")
                    .HasColumnType("uuid");

                b.HasKey("Id");

                b.HasIndex("EventId");

                b.HasIndex("UserId");

                b.HasIndex("EventId", "UserId")
                    .IsUnique();

                b.ToTable("registrations");
            });

            modelBuilder.Entity("Crew.Domain.Entities.RoadTripEvent", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("uuid");

                b.Property<string>("Description")
                    .HasColumnType("text");

                b.Property<Point>("EndPoint")
                    .HasColumnType("geometry (Point, 4326)")
                    .HasSrid(4326);

                b.Property<DateTimeOffset?>("EndTime")
                    .HasColumnType("timestamp with time zone");

                b.Property<int?>("MaxParticipants")
                    .HasColumnType("integer");

                b.Property<Guid>("OwnerId")
                    .HasColumnType("uuid");

                b.Property<string>("RoutePolyline")
                    .HasMaxLength(4096)
                    .HasColumnType("character varying(4096)");

                b.Property<Point>("StartPoint")
                    .HasColumnType("geometry (Point, 4326)")
                    .HasSrid(4326);

                b.Property<DateTimeOffset>("StartTime")
                    .HasColumnType("timestamp with time zone");

                b.Property<string>("Title")
                    .HasMaxLength(256)
                    .HasColumnType("character varying(256)");

                b.Property<int>("Visibility")
                    .HasColumnType("integer");

                b.HasKey("Id");

                b.ToTable("road_trip_events");
            });

            modelBuilder.Entity("Crew.Domain.Entities.User", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("uuid");

                b.Property<DateTimeOffset>("CreatedAt")
                    .HasColumnType("timestamp with time zone");

                b.Property<string>("DisplayName")
                    .HasMaxLength(256)
                    .HasColumnType("character varying(256)");

                b.Property<string>("FirebaseUid")
                    .HasMaxLength(128)
                    .HasColumnType("character varying(128)");

                b.Property<DateTimeOffset?>("UpdatedAt")
                    .HasColumnType("timestamp with time zone");

                b.HasKey("Id");

                b.HasIndex("FirebaseUid")
                    .IsUnique();

                b.ToTable("users");
            });

            modelBuilder.Entity("Crew.Domain.Entities.ChatMembership", b =>
            {
                b.HasOne("Crew.Domain.Entities.ChatGroup", "Group")
                    .WithMany("Members")
                    .HasForeignKey("GroupId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("Crew.Domain.Entities.User", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Group");

                b.Navigation("User");
            });

            modelBuilder.Entity("Crew.Domain.Entities.ChatMessage", b =>
            {
                b.HasOne("Crew.Domain.Entities.ChatGroup", "Group")
                    .WithMany("Messages")
                    .HasForeignKey("GroupId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Group");
            });

            modelBuilder.Entity("Crew.Domain.Entities.EventSegment", b =>
            {
                b.HasOne("Crew.Domain.Entities.RoadTripEvent", "Event")
                    .WithMany("Segments")
                    .HasForeignKey("EventId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Event");
            });

            modelBuilder.Entity("Crew.Domain.Entities.PrivateMessage", b =>
            {
                b.HasOne("Crew.Domain.Entities.PrivateDialog", "Dialog")
                    .WithMany("Messages")
                    .HasForeignKey("DialogId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Dialog");
            });

            modelBuilder.Entity("Crew.Domain.Entities.Registration", b =>
            {
                b.HasOne("Crew.Domain.Entities.RoadTripEvent", "Event")
                    .WithMany("Registrations")
                    .HasForeignKey("EventId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("Crew.Domain.Entities.User", "User")
                    .WithMany()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Event");

                b.Navigation("User");
            });

            modelBuilder.Entity("Crew.Domain.Entities.ChatGroup", b =>
            {
                b.Navigation("Members");

                b.Navigation("Messages");
            });

            modelBuilder.Entity("Crew.Domain.Entities.PrivateDialog", b =>
            {
                b.Navigation("Messages");
            });

            modelBuilder.Entity("Crew.Domain.Entities.RoadTripEvent", b =>
            {
                b.Navigation("Registrations");

                b.Navigation("Segments");
            });
#pragma warning restore 612, 618
        }
    }
}
