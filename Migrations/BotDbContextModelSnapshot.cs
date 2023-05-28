﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TelegramToTrello;

#nullable disable

namespace TelegramToTrello.Migrations
{
    [DbContext(typeof(BotDbContext))]
    partial class BotDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0-preview.3.23174.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BoardRegisteredUser", b =>
                {
                    b.Property<int>("BoardsId")
                        .HasColumnType("integer");

                    b.Property<int>("UsersTelegramId")
                        .HasColumnType("integer");

                    b.HasKey("BoardsId", "UsersTelegramId");

                    b.HasIndex("UsersTelegramId");

                    b.ToTable("UsersBoards", (string)null);
                });

            modelBuilder.Entity("TelegramToTrello.Board", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("BoardName")
                        .HasColumnType("text");

                    b.Property<string>("TrelloBoardId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Boards");
                });

            modelBuilder.Entity("TelegramToTrello.RegisteredUser", b =>
                {
                    b.Property<int>("TelegramId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("TelegramId"));

                    b.Property<bool>("NotificationsEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("TelegramName")
                        .HasColumnType("text");

                    b.Property<string>("TrelloId")
                        .HasColumnType("text");

                    b.Property<string>("TrelloName")
                        .HasColumnType("text");

                    b.Property<string>("TrelloToken")
                        .HasColumnType("text");

                    b.HasKey("TelegramId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("TelegramToTrello.TTTTask", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Date")
                        .HasColumnType("text");

                    b.Property<bool>("InEditMode")
                        .HasColumnType("boolean");

                    b.Property<string>("ListId")
                        .HasColumnType("text");

                    b.Property<int>("MessageForDeletionId")
                        .HasColumnType("integer");

                    b.Property<string>("Tag")
                        .HasColumnType("text");

                    b.Property<string>("TaskDesc")
                        .HasColumnType("text");

                    b.Property<string>("TaskId")
                        .HasColumnType("text");

                    b.Property<string>("TaskName")
                        .HasColumnType("text");

                    b.Property<string>("TaskPartId")
                        .HasColumnType("text");

                    b.Property<string>("TaskPartName")
                        .HasColumnType("text");

                    b.Property<string>("TrelloBoardId")
                        .HasColumnType("text");

                    b.Property<string>("TrelloBoardName")
                        .HasColumnType("text");

                    b.Property<string>("TrelloId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("CreatingTasks");
                });

            modelBuilder.Entity("TelegramToTrello.Table", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("BoardId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("TableId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("BoardId");

                    b.ToTable("BoardTables");
                });

            modelBuilder.Entity("TelegramToTrello.TaskNotification", b =>
                {
                    b.Property<string>("TaskId")
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Due")
                        .HasColumnType("text");

                    b.Property<bool>("EditMode")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string[]>("Participants")
                        .HasColumnType("text[]");

                    b.Property<string>("TaskBoard")
                        .HasColumnType("text");

                    b.Property<string>("TaskList")
                        .HasColumnType("text");

                    b.Property<string>("Url")
                        .HasColumnType("text");

                    b.Property<int>("User")
                        .HasColumnType("integer");

                    b.HasKey("TaskId");

                    b.ToTable("TaskNotifications");
                });

            modelBuilder.Entity("TelegramToTrello.UsersOnBoard", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<int>("TrelloUserBoardId")
                        .HasColumnType("integer");

                    b.Property<string>("TrelloUserId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("TrelloUserBoardId");

                    b.ToTable("UsersOnBoards");
                });

            modelBuilder.Entity("BoardRegisteredUser", b =>
                {
                    b.HasOne("TelegramToTrello.Board", null)
                        .WithMany()
                        .HasForeignKey("BoardsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TelegramToTrello.RegisteredUser", null)
                        .WithMany()
                        .HasForeignKey("UsersTelegramId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TelegramToTrello.Table", b =>
                {
                    b.HasOne("TelegramToTrello.Board", "TrelloUserBoard")
                        .WithMany("Tables")
                        .HasForeignKey("BoardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TrelloUserBoard");
                });

            modelBuilder.Entity("TelegramToTrello.UsersOnBoard", b =>
                {
                    b.HasOne("TelegramToTrello.Board", "TrelloBoard")
                        .WithMany("UsersOnBoards")
                        .HasForeignKey("TrelloUserBoardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TrelloBoard");
                });

            modelBuilder.Entity("TelegramToTrello.Board", b =>
                {
                    b.Navigation("Tables");

                    b.Navigation("UsersOnBoards");
                });
#pragma warning restore 612, 618
        }
    }
}
