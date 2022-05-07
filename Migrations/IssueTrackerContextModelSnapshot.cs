﻿// <auto-generated />
using System;
using IssueTracker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace IssueTracker.Migrations
{
    [DbContext(typeof(IssueTrackerContext))]
    partial class IssueTrackerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.2");

            modelBuilder.Entity("IssueTracker.Models.Comment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasMaxLength(2000)
                        .HasColumnType("VARCHAR (2000)");

                    b.Property<DateTime>("EditedOn")
                        .HasColumnType("TEXT");

                    b.Property<int>("IssueId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PersonId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("IssueId");

                    b.HasIndex("PersonId");

                    b.ToTable("Comments");
                });

            modelBuilder.Entity("IssueTracker.Models.Issue", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ActualResolutionDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("AsigneeId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<int>("CreatorId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasMaxLength(2000)
                        .HasColumnType("VARCHAR (2000)");

                    b.Property<int>("Priority")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ProjectId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("State")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Summary")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("VARCHAR (200)");

                    b.Property<DateTime>("TargetResolutionDate")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("AsigneeId");

                    b.HasIndex("CreatorId");

                    b.HasIndex("ProjectId");

                    b.ToTable("Issues");
                });

            modelBuilder.Entity("IssueTracker.Models.Person", b =>
                {
                    b.Property<int>("PersonId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("VARCHAR (255)");

                    b.Property<string>("FirstName")
                        .HasMaxLength(50)
                        .HasColumnType("VARCHAR (50)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("VARCHAR (64)");

                    b.Property<string>("SecondName")
                        .HasMaxLength(50)
                        .HasColumnType("VARCHAR (50)");

                    b.HasKey("PersonId");

                    b.ToTable("Persons");
                });

            modelBuilder.Entity("IssueTracker.Models.PersonProject", b =>
                {
                    b.Property<int>("PersonId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ProjectId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Role")
                        .HasMaxLength(100)
                        .HasColumnType("INTEGER");

                    b.HasKey("PersonId", "ProjectId");

                    b.HasIndex("ProjectId");

                    b.ToTable("PersonProjects");
                });

            modelBuilder.Entity("IssueTracker.Models.Project", b =>
                {
                    b.Property<int>("ProjectId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ActualEndDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("VARCHAR (100)");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("TargetEndDate")
                        .HasColumnType("TEXT");

                    b.HasKey("ProjectId");

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("IssueTracker.Models.Comment", b =>
                {
                    b.HasOne("IssueTracker.Models.Issue", "Issue")
                        .WithMany()
                        .HasForeignKey("IssueId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("IssueTracker.Models.Person", "Person")
                        .WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Issue");

                    b.Navigation("Person");
                });

            modelBuilder.Entity("IssueTracker.Models.Issue", b =>
                {
                    b.HasOne("IssueTracker.Models.Person", "Asignee")
                        .WithMany()
                        .HasForeignKey("AsigneeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("IssueTracker.Models.Person", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("IssueTracker.Models.Project", "Project")
                        .WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Asignee");

                    b.Navigation("Creator");

                    b.Navigation("Project");
                });

            modelBuilder.Entity("IssueTracker.Models.PersonProject", b =>
                {
                    b.HasOne("IssueTracker.Models.Person", "Person")
                        .WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("IssueTracker.Models.Project", "Project")
                        .WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Person");

                    b.Navigation("Project");
                });
#pragma warning restore 612, 618
        }
    }
}
