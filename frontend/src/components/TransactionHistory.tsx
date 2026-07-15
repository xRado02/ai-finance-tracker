import { useState } from 'react';
import { isApiError } from '../api/financeApi';
import type { TransactionResponse } from '../api/financeTypes';

type TransactionHistoryProps = {
  transactions: TransactionResponse[];
  isLoading: boolean;
  onDeleteTransaction: (id: string) => Promise<void>;
};

const moneyFormatter = new Intl.NumberFormat('en-US', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

export function TransactionHistory({
  transactions,
  isLoading,
  onDeleteTransaction,
}: TransactionHistoryProps) {
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [message, setMessage] = useState('');

  async function handleDelete(transaction: TransactionResponse) {
    const confirmed = window.confirm(
      `Delete transaction ${transaction.categoryName} for ${moneyFormatter.format(transaction.amount)}?`,
    );

    if (!confirmed) {
      return;
    }

    setMessage('');
    setDeletingId(transaction.id);

    try {
      await onDeleteTransaction(transaction.id);
      setMessage('Transaction deleted.');
    } catch (error) {
      setMessage(isApiError(error) ? error.message : 'Could not delete the transaction.');
    } finally {
      setDeletingId(null);
    }
  }

  return (
    <section className="panel history-panel">
      <div className="panel__header">
        <h2>Transaction history</h2>
        <span>{transactions.length} items</span>
      </div>

      {isLoading && <p className="empty-state">Loading history...</p>}

      {message && <p className="history-message">{message}</p>}

      {!isLoading && transactions.length === 0 && (
        <p className="empty-state">No transactions yet.</p>
      )}

      {!isLoading && transactions.length > 0 && (
        <div className="transaction-list">
          {transactions.map((transaction) => (
            <article className="transaction-row" key={transaction.id}>
              <div>
                <p className="transaction-row__title">{transaction.categoryName}</p>
                <p className="transaction-row__meta">
                  {transaction.transactionDate}
                  {transaction.description ? ` - ${transaction.description}` : ''}
                </p>
              </div>
              <div className="transaction-row__amount">
                <span className={`type-pill type-pill--${transaction.type.toLowerCase()}`}>
                  {transaction.type}
                </span>
                <strong>
                  {transaction.type === 'Expense' ? '-' : '+'}
                  {moneyFormatter.format(transaction.amount)}
                </strong>
                <button
                  className="delete-action"
                  disabled={deletingId !== null}
                  onClick={() => void handleDelete(transaction)}
                  type="button"
                >
                  Delete
                </button>
              </div>
            </article>
          ))}
        </div>
      )}
    </section>
  );
}
