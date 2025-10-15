using System;
using Crew.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

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
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            modelBuilder.Entity("Crew.Domain.Entities.ChatGroup", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("TEXT");

                b.Property<DateTimeOffset>("CreatedAt")
                    .HasColumnType("TEXT");

                b.Property<Guid?>("EventId")
                    .HasColumnType("TEXT");

                b.Property<int>("Scope")
                    .HasColumnType("INTEGER");

                b.HasKey("Id");

                b.ToTable("chat_groups");
            });

            modelBuilder.Entity("Crew.Domain.Entities.ChatMembership", b =>
            {
                b.Property<Guid>("GroupId")
                    .HasColumnType("TEXT");

                b.Property<Guid>("UserId")
                    .HasColumnType("TEXT");

                b.Property<DateTimeOffset>("JoinedAt")
                    .HasColumnType("TEXT");

                b.Property<string>("Role")
                    .HasMaxLength(32)
                    .HasColumnType("TEXT");

                b.HasKey("GroupId", "UserId");

                b.HasIndex("UserId");

                b.ToTable("chat_memberships");
            });

            modelBuilder.Entity("Crew.Domain.Entities.ChatMessage", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("TEXT");

                b.Property<string>("AttachmentsJson")
                    .HasColumnType("TEXT");

                b.Property<string>("Content")
                    .HasColumnType("TEXT");

                b.Property<Guid>("GroupId")
                    .HasColumnType("TEXT");

                b.Property<Guid>("SenderId")
                    .HasColumnType("TEXT");

                b.Property<DateTimeOffset>("SentAt")
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("GroupId");

                b.ToTable("chat_messages");
            });

            modelBuilder.Entity("Crew.Domain.Entities.EventSegment", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("TEXT");

                b.Property<Guid>("EventId")
                    .HasColumnType("TEXT");

                b.Property<string>("Note")
                    .HasColumnType("TEXT");

                b.Property<int>("Seq")
                    .HasColumnType("INTEGER");

                b.Property<Point>("Waypoint")
                    .HasColumnType("BLOB");

                b.HasKey("Id");

                b.HasIndex("EventId");

                b.ToTable("event_segments");
            });

            modelBuilder.Entity("Crew.Domain.Entities.OutboxMessage", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("TEXT");

                b.Property<string>("Error")
                    .HasColumnType("TEXT");

                b.Property<DateTimeOffset>("OccurredAt")
                    .HasColumnType("TEXT");

                b.Property<string>("Payload")
                    .HasColumnType("TEXT");

                b.Property<DateTimeOffset?>("ProcessedAt")
                    .HasColumnType("TEXT");

                b.Property<string>("Type")
                    .HasMaxLength(256)
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.ToTable("outbox_messages");
            });

            modelBuilder.Entity("Crew.Domain.Entities.PrivateDialog", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("TEXT");

                b.Property<DateTimeOffset>("CreatedAt")
                    .HasColumnType("TEXT");

                b.Property<Guid>("UserA")
                    .HasColumnType("TEXT");

                b.Property<Guid>("UserB")
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("UserA", "UserB")
                    .IsUnique();

                b.ToTable("private_dialogs");
            });

            modelBuilder.Entity("Crew.Domain.Entities.PrivateMessage", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("TEXT");

                b.Property<string>("Content")
                    .HasColumnType("TEXT");

                b.Property<Guid>("DialogId")
                    .HasColumnType("TEXT");

                b.Property<Guid>("SenderId")
                    .HasColumnType("TEXT");

                b.Property<DateTimeOffset>("SentAt")
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("DialogId");

                b.ToTable("private_messages");
            });

            modelBuilder.Entity("Crew.Domain.Entities.Registration", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("TEXT");

                b.Property<DateTimeOffset>("CreatedAt")
                    .HasColumnType("TEXT");

                b.Property<Guid>("EventId")
                    .HasColumnType("TEXT");

                b.Property<int>("Status")
                    .HasColumnType("INTEGER");

                b.Property<Guid>("UserId")
                    .HasColumnType("TEXT");

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
                    .HasColumnType("TEXT");

                b.Property<Guid>("OwnerId")
                    .HasColumnType("TEXT");

                b.Property<string>("Description")
                    .HasColumnType("TEXT");

                b.Property<Point>("EndPoint")
                    .HasColumnType("BLOB");

                b.Property<DateTimeOffset?>("EndTime")
                    .HasColumnType("TEXT");

                b.Property<int?>("MaxParticipants")
                    .HasColumnType("INTEGER");

                b.Property<string>("RoutePolyline")
                    .HasMaxLength(4096)
                    .HasColumnType("TEXT");

                b.Property<Point>("StartPoint")
                    .HasColumnType("BLOB");

                b.Property<DateTimeOffset>("StartTime")
                    .HasColumnType("TEXT");

                b.Property<string>("Title")
                    .HasMaxLength(256)
                    .HasColumnType("TEXT");

                b.Property<int>("Visibility")
                    .HasColumnType("INTEGER");

                b.HasKey("Id");



                b.ToTable("road_trip_events");
            });

            modelBuilder.Entity("Crew.Domain.Entities.User", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("TEXT");

                b.Property<DateTimeOffset>("CreatedAt")
                    .HasColumnType("TEXT");

                b.Property<string>("DisplayName")
                    .HasMaxLength(256)
                    .HasColumnType("TEXT");

                b.Property<string>("FirebaseUid")
                    .HasMaxLength(128)
                    .HasColumnType("TEXT");

                b.Property<DateTimeOffset?>("UpdatedAt")
                    .HasColumnType("TEXT");

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
