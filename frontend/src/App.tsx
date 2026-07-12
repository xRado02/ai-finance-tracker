import { useEffect, useState } from 'react';
import './App.css';
import { getCategories, getTransactions, isApiError } from './api/financeApi';
import type { CategoryResponse, TransactionResponse } from './api/financeTypes';
import { TransactionForm } from './components/TransactionForm';
import { TransactionHistory } from './components/TransactionHistory';

type ApiStatus =
  | { state: 'loading' }
  | { state: 'ready'; categories: CategoryResponse[]; transactions: TransactionResponse[] }
  | { state: 'error'; message: string };

export default function App() {
  const [apiStatus, setApiStatus] = useState<ApiStatus>({ state: 'loading' });

  async function loadFinanceData(isActive = true) {
    try {
      const [categories, transactions] = await Promise.all([
        getCategories(),
        getTransactions(),
      ]);

      if (isActive) {
        setApiStatus({ state: 'ready', categories, transactions });
      }
    } catch (error) {
      if (isActive) {
        setApiStatus({
          state: 'error',
          message: isApiError(error)
            ? error.message
            : 'Cannot connect to the local finance API.',
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
            <p className="workspace__eyebrow">Local finance</p>
            <h1>AI Finance Tracker</h1>
          </div>
          <span className="workspace__status">Local API</span>
        </header>

        <section className={`api-status api-status--${apiStatus.state}`}>
          {apiStatus.state === 'loading' && 'Loading local finance API...'}
          {apiStatus.state === 'error' && apiStatus.message}
          {apiStatus.state === 'ready' &&
            `${apiStatus.categories.length} categories loaded, ${apiStatus.transactions.length} transactions found.`}
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
          />
        </div>
      </section>
    </main>
  );
}
