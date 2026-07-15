using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiFinanceTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurringTransactionUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_RecurringTransactionId",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_RecurringTransactionId_TransactionDate",
                table: "Transactions",
                columns: new[] { "RecurringTransactionId", "TransactionDate" },
                unique: true,
                filter: "[RecurringTransactionId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_RecurringTransactionId_TransactionDate",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_RecurringTransactionId",
                table: "Transactions",
                column: "RecurringTransactionId");
        }
    }
}
