import { useState } from 'react';
import { isApiError } from '../api/financeApi';
import type { TransactionResponse } from '../api/financeTypes';
import { formatDate, getCategoryLabel, getTransactionTypeLabel, polishApiMessage } from '../labels';

type TransactionHistoryProps = {
  transactions: TransactionResponse[];
  isLoading: boolean;
  onDeleteTransaction: (id: string) => Promise<void>;
};

const moneyFormatter = new Intl.NumberFormat('pl-PL', {
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
      `Czy na pewno usunąć transakcję "${getCategoryLabel(transaction.categoryName)}" za ${moneyFormatter.format(transaction.amount)}?`,
    );

    if (!confirmed) {
      return;
    }

    setMessage('');
    setDeletingId(transaction.id);

    try {
      await onDeleteTransaction(transaction.id);
      setMessage('Transakcja została usunięta.');
    } catch (error) {
      setMessage(
        isApiError(error) ? polishApiMessage(error.message) : 'Nie można usunąć transakcji.',
      );
    } finally {
      setDeletingId(null);
    }
  }

  return (
    <section className="panel history-panel">
      <div className="panel__header">
        <h2>Historia transakcji</h2>
        <span>{transactions.length} pozycji</span>
      </div>

      {isLoading && <p className="empty-state">Ładowanie historii...</p>}

      {message && <p className="history-message">{message}</p>}

      {!isLoading && transactions.length === 0 && (
        <p className="empty-state">Brak transakcji.</p>
      )}

      {!isLoading && transactions.length > 0 && (
        <div className="transaction-list">
          {transactions.map((transaction) => (
            <article className="transaction-row" key={transaction.id}>
              <div>
                <p className="transaction-row__title">{getCategoryLabel(transaction.categoryName)}</p>
                <p className="transaction-row__meta">
                  {formatDate(transaction.transactionDate)}
                  {transaction.description ? ` - ${transaction.description}` : ''}
                </p>
              </div>
              <div className="transaction-row__amount">
                <span className={`type-pill type-pill--${transaction.type.toLowerCase()}`}>
                  {getTransactionTypeLabel(transaction.type)}
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
                  Usuń
                </button>
              </div>
            </article>
          ))}
        </div>
      )}
    </section>
  );
}
