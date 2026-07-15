import { useEffect, useState } from 'react';
import './App.css';
import {
  deleteTransaction,
  getCategories,
  getGoals,
  getTransactions,
  isApiError,
} from './api/financeApi';
import type { CategoryResponse, GoalResponse, TransactionResponse } from './api/financeTypes';
import { TransactionForm } from './components/TransactionForm';
import { TransactionHistory } from './components/TransactionHistory';
import { polishApiMessage } from './labels';

type ApiStatus =
  | { state: 'loading' }
  | {
      state: 'ready';
      categories: CategoryResponse[];
      transactions: TransactionResponse[];
      goals: GoalResponse[];
    }
  | { state: 'error'; message: string };

export default function App() {
  const [apiStatus, setApiStatus] = useState<ApiStatus>({ state: 'loading' });

  async function loadFinanceData(isActive = true) {
    try {
      const [categories, transactions, goals] = await Promise.all([
        getCategories(),
        getTransactions(),
        getGoals(),
      ]);

      if (isActive) {
        setApiStatus({ state: 'ready', categories, transactions, goals });
      }
    } catch (error) {
      if (isActive) {
        setApiStatus({
          state: 'error',
          message: isApiError(error)
            ? polishApiMessage(error.message)
            : 'Nie można połączyć się z lokalnym API finansowym.',
        });
      }
    }
  }

  useEffect(() => {
    let isActive = true;

    void loadFinanceData(isActive);

    return () => {
      isActive = false;
    };
  }, []);

  return (
    <main className="app-shell">
      <section className="workspace">
        <header className="workspace__header">
          <div>
            <p className="workspace__eyebrow">Finanse lokalne</p>
            <h1>AI Finance Tracker</h1>
          </div>
          <span className="workspace__status">Lokalne API</span>
        </header>

        <section className={`api-status api-status--${apiStatus.state}`}>
          {apiStatus.state === 'loading' && 'Ładowanie lokalnego API finansowego...'}
          {apiStatus.state === 'error' && apiStatus.message}
          {apiStatus.state === 'ready' &&
            `Załadowano ${apiStatus.categories.length} kategorii, znaleziono ${apiStatus.transactions.length} transakcji.`}
        </section>

        <div className="workspace__grid">
          <TransactionForm
            categories={apiStatus.state === 'ready' ? apiStatus.categories : []}
            disabled={apiStatus.state !== 'ready'}
            onTransactionCreated={() => loadFinanceData()}
          />

          <TransactionHistory
            transactions={apiStatus.state === 'ready' ? apiStatus.transactions : []}
            isLoading={apiStatus.state === 'loading'}
            onDeleteTransaction={async (id) => {
              await deleteTransaction(id);
              await loadFinanceData();
            }}
          />
        </div>
      </section>
    </main>
  );
}
