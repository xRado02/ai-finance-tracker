import { useEffect, useState } from 'react';
import './App.css';
import {
  createGoal,
  deleteTransaction,
  getCategories,
  getDashboardSummary,
  getGoals,
  getTransactions,
  getRecurringTransactions,
  isApiError,
} from './api/financeApi';
import type {
  CategoryResponse,
  DashboardSummaryResponse,
  GoalResponse,
  RecurringTransactionResponse,
  TransactionResponse,
} from './api/financeTypes';
import { DashboardSummary } from './components/DashboardSummary';
import { GoalForm } from './components/GoalForm';
import { GoalList } from './components/GoalList';
import { TransactionForm } from './components/TransactionForm';
import { TransactionHistory } from './components/TransactionHistory';
import { RecurringTransactionPanel } from './components/RecurringTransactionPanel';
import { polishApiMessage } from './labels';

type ApiStatus =
  | { state: 'loading' }
  | {
      state: 'ready';
      categories: CategoryResponse[];
      transactions: TransactionResponse[];
      goals: GoalResponse[];
      recurringTransactions: RecurringTransactionResponse[];
      dashboard: DashboardSummaryResponse;
    }
  | { state: 'error'; message: string };

export default function App() {
  const [apiStatus, setApiStatus] = useState<ApiStatus>({ state: 'loading' });
  const [isNavOpen, setIsNavOpen] = useState(false);

  async function loadFinanceData(isActive = true) {
    try {
      const [categories, transactions, goals, dashboard, recurringTransactions] = await Promise.all([
        getCategories(),
        getTransactions(),
        getGoals(),
        getDashboardSummary(),
        getRecurringTransactions(),
      ]);

      if (isActive) {
        setApiStatus({ state: 'ready', categories, transactions, goals, dashboard, recurringTransactions });
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
      <button
        aria-label="Zamknij menu"
        className={`sidebar-backdrop ${isNavOpen ? 'sidebar-backdrop--visible' : ''}`}
        onClick={() => setIsNavOpen(false)}
        type="button"
      />

      <aside className={`sidebar ${isNavOpen ? 'sidebar--open' : ''}`}>
        <div className="sidebar__brand">
          <span className="brand-mark">AF</span>
          <div>
            <strong>Finance Tracker</strong>
            <span>Portfel lokalny</span>
          </div>
        </div>

        <nav className="sidebar__nav" aria-label="Główna nawigacja">
          <span className="sidebar__label">Workspace</span>
          <a className="sidebar__link sidebar__link--active" href="#overview" onClick={() => setIsNavOpen(false)}>
            <span aria-hidden="true">⌂</span>
            Przegląd
          </a>
          <a className="sidebar__link" href="#transactions" onClick={() => setIsNavOpen(false)}>
            <span aria-hidden="true">↗</span>
            Transakcje
          </a>
          <a className="sidebar__link" href="#recurring" onClick={() => setIsNavOpen(false)}>
            <span aria-hidden="true">↻</span>
            Stałe transakcje
          </a>
          <a className="sidebar__link" href="#goals" onClick={() => setIsNavOpen(false)}>
            <span aria-hidden="true">◎</span>
            Cele finansowe
          </a>
        </nav>

        <a className="sidebar__quick-action" href="#add-transaction" onClick={() => setIsNavOpen(false)}>
          <span aria-hidden="true">+</span>
          Dodaj transakcję
        </a>

        <div className="sidebar__footer">
          <div className="local-profile">
            <span className="profile-avatar">DL</span>
            <div>
              <strong>Profil lokalny</strong>
              <span>Dane tylko na tym urządzeniu</span>
            </div>
          </div>
          <span className="sidebar__version">MVP · v1.0</span>
        </div>
      </aside>

      <div className="app-main">
        <header className="topbar">
          <button
            aria-label="Otwórz menu"
            className="menu-toggle"
            onClick={() => setIsNavOpen(true)}
            type="button"
          >
            <span aria-hidden="true">☰</span>
          </button>
          <div className="topbar__heading">
            <p className="workspace__eyebrow">Twój portfel</p>
            <h1>Dobry dzień, Radek</h1>
          </div>
          <div className="topbar__meta">
            <span className="topbar__date">
              {new Intl.DateTimeFormat('pl-PL', { dateStyle: 'long' }).format(new Date())}
            </span>
            <span className="workspace__status"><span className="status-dot" /> Lokalnie</span>
          </div>
        </header>

        <div className="content">
          <section className="welcome-strip" aria-label="Status profilu">
            <div>
              <span className="welcome-strip__kicker">Finanse lokalne</span>
              <strong>Masz pełną kontrolę nad swoim budżetem.</strong>
            </div>
            <span className="welcome-strip__privacy">Dane nie opuszczają urządzenia</span>
          </section>

          <section className={`api-status api-status--${apiStatus.state}`}>
            <span className="status-dot" />
            {apiStatus.state === 'loading' && 'Ładowanie lokalnego API finansowego...'}
            {apiStatus.state === 'error' && apiStatus.message}
            {apiStatus.state === 'ready' &&
              `Synchronizacja zakończona · ${apiStatus.categories.length} kategorii · ${apiStatus.transactions.length} transakcji`}
          </section>

          <section id="overview" className="dashboard-section">
            <DashboardSummary
              dashboard={apiStatus.state === 'ready' ? apiStatus.dashboard : null}
              isLoading={apiStatus.state === 'loading'}
            />
          </section>

          <div className="section-heading" id="transactions">
            <div>
              <span className="section-heading__eyebrow">Aktywność</span>
              <h2>Transakcje</h2>
            </div>
            <span>Dodawaj i kontroluj swoje wpisy</span>
          </div>

          <div className="workspace__grid">
            <div id="add-transaction">
              <TransactionForm
                categories={apiStatus.state === 'ready' ? apiStatus.categories : []}
                disabled={apiStatus.state !== 'ready'}
                onTransactionCreated={() => loadFinanceData()}
              />
            </div>
            <TransactionHistory
              transactions={apiStatus.state === 'ready' ? apiStatus.transactions : []}
              isLoading={apiStatus.state === 'loading'}
              onDeleteTransaction={async (id) => {
                await deleteTransaction(id);
                await loadFinanceData();
              }}
            />
          </div>

          <div className="section-heading section-heading--recurring" id="recurring">
            <div>
              <span className="section-heading__eyebrow">Powtarzalne</span>
              <h2>Stałe transakcje</h2>
            </div>
            <span>Generuj ręcznie raz na miesiąc</span>
          </div>

          <RecurringTransactionPanel
            categories={apiStatus.state === 'ready' ? apiStatus.categories : []}
            recurringTransactions={apiStatus.state === 'ready' ? apiStatus.recurringTransactions : []}
            disabled={apiStatus.state !== 'ready'}
            isLoading={apiStatus.state === 'loading'}
            onChanged={() => loadFinanceData()}
          />

          <div className="section-heading section-heading--goals" id="goals">
            <div>
              <span className="section-heading__eyebrow">Plan na przyszłość</span>
              <h2>Cele finansowe</h2>
            </div>
            <span>Małe kroki, konkretny progress</span>
          </div>

          <div className="goals__grid">
            <GoalForm
              disabled={apiStatus.state !== 'ready'}
              onGoalCreated={async (request) => {
                await createGoal(request);
                await loadFinanceData();
              }}
            />
            <GoalList
              goals={apiStatus.state === 'ready' ? apiStatus.goals : []}
              isLoading={apiStatus.state === 'loading'}
            />
          </div>
        </div>
      </div>
    </main>
  );
}
