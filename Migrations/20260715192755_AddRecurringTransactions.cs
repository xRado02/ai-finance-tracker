using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiFinanceTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurringTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RecurringTransactionId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RecurringTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LocalProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurringTransactions_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecurringTransactions_LocalProfiles_LocalProfileId",
                        column: x => x.LocalProfileId,
                        principalTable: "LocalProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_RecurringTransactionId",
                table: "Transactions",
                column: "RecurringTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactions_CategoryId",
                table: "RecurringTransactions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTransactions_LocalProfileId",
                table: "RecurringTransactions",
                column: "LocalProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_RecurringTransactions_RecurringTransactionId",
                table: "Transactions",
                column: "RecurringTransactionId",
                principalTable: "RecurringTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_RecurringTransactions_RecurringTransactionId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "RecurringTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_RecurringTransactionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "RecurringTransactionId",
                table: "Transactions");
        }
    }
}
