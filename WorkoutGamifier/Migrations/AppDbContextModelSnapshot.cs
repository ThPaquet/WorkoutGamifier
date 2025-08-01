﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WorkoutGamifier.Data;

#nullable disable

namespace WorkoutGamifier.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.7");

            modelBuilder.Entity("WorkoutGamifier.Models.Action", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<int>("PointValue")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Actions");
                });

            modelBuilder.Entity("WorkoutGamifier.Models.ActionCompletion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ActionId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CompletedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("PointsAwarded")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SessionId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ActionId");

                    b.HasIndex("SessionId");

                    b.ToTable("ActionCompletions");
                });

            modelBuilder.Entity("WorkoutGamifier.Models.Session", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("EndTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<int>("PointsEarned")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PointsSpent")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("WorkoutPoolId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("WorkoutPoolId");

                    b.ToTable("Sessions");
                });

            modelBuilder.Entity("WorkoutGamifier.Models.Workout", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<int>("Difficulty")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DurationMinutes")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Instructions")
                        .HasMaxLength(2000)
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsHidden")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsPreloaded")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Workouts");
                });

            modelBuilder.Entity("WorkoutGamifier.Models.WorkoutPool", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("WorkoutPools");
                });

            modelBuilder.Entity("WorkoutGamifier.Models.WorkoutPoolWorkout", b =>
                {
                    b.Property<int>("WorkoutPoolId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WorkoutId")
                        .HasColumnType("INTEGER");

                    b.HasKey("WorkoutPoolId", "WorkoutId");

                    b.HasIndex("WorkoutId");

                    b.ToTable("WorkoutPoolWorkouts");
                });

            modelBuilder.Entity("WorkoutGamifier.Models.WorkoutReceived", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("PointsSpent")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ReceivedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("SessionId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WorkoutId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("SessionId");

                    b.HasIndex("WorkoutId");

                    b.ToTable("WorkoutReceived");
                });

            modelBuilder.Entity("WorkoutGamifier.Models.ActionCompletion", b =>
                {
                    b.HasOne("WorkoutGamifier.Models.Action", "Action")
                        .WithMany("ActionCompletions")
                        .HasForeignKey("ActionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("WorkoutGamifier.Models.Session", "Session")
                        .WithMany("ActionCompletions")
                        .HasForeignKey("SessionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Action");

                    b.Navigation("Session");
                });

            modelBuilder.Entity("WorkoutGamifier.Models.Session", b =>
                {
                    b.HasOne("WorkoutGamifier.Models.WorkoutPool", "WorkoutPool")
                        .WithMany("Sessions")
                        .HasForeignKey("WorkoutPoolId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("WorkoutPool");
                });

            modelBuilder.Entity("WorkoutGamifier.Models.WorkoutPoolWorkout", b =>
                {
                    b.HasOne("WorkoutGamifier.Models.Workout", "Workout")
                        .WithMany("WorkoutPoolWorkouts")
                        .HasForeignKey("WorkoutId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WorkoutGamifier.Models.WorkoutPool", "WorkoutPool")
                        .WithMany("WorkoutPoolWorkouts")
                        .HasForeignKey("WorkoutPoolId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Workout");

                    b.Navigation("WorkoutPool");
                });

            modelBuilder.Entity("WorkoutGamifier.Models.WorkoutReceived", b =>
                {
                    b.HasOne("WorkoutGamifier.Models.Session", "Session")
                        .WithMany("WorkoutReceived")
                        .HasForeignKey("SessionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WorkoutGamifier.Models.Workout", "Workout")
                        .WithMany("WorkoutReceived")
                        .HasForeignKey("WorkoutId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Session");

                    b.Navigation("Workout");
                });

            modelBuilder.Entity("WorkoutGamifier.Models.Action", b =>
                {
                    b.Navigation("ActionCompletions");
                });

            modelBuilder.Entity("WorkoutGamifier.Models.Session", b =>
                {
                    b.Navigation("ActionCompletions");

                    b.Navigation("WorkoutReceived");
                });

            modelBuilder.Entity("WorkoutGamifier.Models.Workout", b =>
                {
                    b.Navigation("WorkoutPoolWorkouts");

                    b.Navigation("WorkoutReceived");
                });

            modelBuilder.Entity("WorkoutGamifier.Models.WorkoutPool", b =>
                {
                    b.Navigation("Sessions");

                    b.Navigation("WorkoutPoolWorkouts");
                });
#pragma warning restore 612, 618
        }
    }
}
