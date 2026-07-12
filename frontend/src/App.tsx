import { useEffect, useState } from 'react';
import './App.css';
import { getCategories, getTransactions, isApiError } from './api/financeApi';
import type { CategoryResponse, TransactionResponse } from './api/financeTypes';

type ApiStatus =
  | { state: 'loading' }
  | { state: 'ready'; categories: CategoryResponse[]; transactions: TransactionResponse[] }
  | { state: 'error'; message: string };

export default function App() {
  const [apiStatus, setApiStatus] = useState<ApiStatus>({ state: 'loading' });

  useEffect(() => {
    let isActive = true;

    async function loadFinanceData() {
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

    void loadFinanceData();

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
          <span className="workspace__status">Frontend scaffold</span>
        </header>

        <section className={`api-status api-status--${apiStatus.state}`}>
          {apiStatus.state === 'loading' && 'Loading local finance API...'}
          {apiStatus.state === 'error' && apiStatus.message}
          {apiStatus.state === 'ready' &&
            `${apiStatus.categories.length} categories loaded, ${apiStatus.transactions.length} transactions found.`}
        </section>

        <div className="workspace__grid">
          <section className="panel">
            <h2>Add transaction</h2>
            <p>Transaction form will connect to the existing finance API in the next phases.</p>
          </section>

          <section className="panel">
            <h2>Transaction history</h2>
            <p>History will load from <code>/api/transactions</code>.</p>
          </section>
        </div>
      </section>
    </main>
  );
}
