﻿// <auto-generated />
using System;
using FoodLog.Api.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FoodLog.Api.Migrations
{
    [DbContext(typeof(FoodContext))]
    partial class FoodContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024");

            modelBuilder.Entity("FoodLog.Api.Models.Entry", b =>
                {
                    b.Property<int>("EntryId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Alcohol");

                    b.Property<string>("Breakfast")
                        .HasMaxLength(200);

                    b.Property<bool>("Caffeine");

                    b.Property<bool>("Dairy");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Dinner");

                    b.Property<bool>("Exercise");

                    b.Property<bool>("FattyFood");

                    b.Property<bool>("Gluten");

                    b.Property<string>("Lunch");

                    b.Property<bool>("OnionsPulses");

                    b.Property<int>("Rating");

                    b.Property<string>("SnacksDrinks");

                    b.Property<bool>("Spice");

                    b.HasKey("EntryId");

                    b.ToTable("Entries");
                });
#pragma warning restore 612, 618
        }
    }
}
