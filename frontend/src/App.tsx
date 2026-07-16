import { useEffect, useState } from 'react';
import './App.css';
import {
  createGoal,
  deleteTransaction,
  getCategories,
  getDashboardSummary,
  getGoals,
  getGoalForecast,
  getMonthlySummary,
  getTransactions,
  getRecurringTransactions,
  getProfileSettings,
  updateProfileSettings,
  isApiError,
} from './api/financeApi';
import type {
  CategoryResponse,
  DashboardSummaryResponse,
  GoalResponse,
  GoalForecastResponse,
  MonthlySummaryResponse,
  ProfileSettingsResponse,
  RecurringTransactionResponse,
  TransactionResponse,
} from './api/financeTypes';
import { DashboardSummary } from './components/DashboardSummary';
import { GoalForm } from './components/GoalForm';
import { GoalList } from './components/GoalList';
import { TransactionForm } from './components/TransactionForm';
import { TransactionHistory } from './components/TransactionHistory';
import { RecurringTransactionPanel } from './components/RecurringTransactionPanel';
import { getCurrentPeriod, PeriodPicker, type PeriodSelection } from './components/PeriodPicker';
import { SettingsPanel } from './components/SettingsPanel';
import { polishApiMessage } from './labels';

type ApiStatus =
  | { state: 'loading' }
  | {
      state: 'ready';
      categories: CategoryResponse[];
      transactions: TransactionResponse[];
      goals: GoalResponse[];
      goalForecasts: GoalForecastResponse[];
      recurringTransactions: RecurringTransactionResponse[];
      dashboard: DashboardSummaryResponse;
      monthlySummary: MonthlySummaryResponse;
      profileSettings: ProfileSettingsResponse;
    }
  | { state: 'error'; message: string };

type SectionId = 'dashboard' | 'transactions' | 'recurring' | 'goals' | 'settings';

export default function App() {
  const [apiStatus, setApiStatus] = useState<ApiStatus>({ state: 'loading' });
  const [isNavOpen, setIsNavOpen] = useState(false);
  const [activeSection, setActiveSection] = useState<SectionId>('dashboard');
  const [selectedPeriod, setSelectedPeriod] = useState<PeriodSelection>(getCurrentPeriod);

  function selectSection(section: SectionId) {
    setActiveSection(section);
    setIsNavOpen(false);
  }

  async function loadFinanceData(period: PeriodSelection, isActive = true) {
    try {
      const [categories, transactions, goals, dashboard, recurringTransactions, goalForecasts, monthlySummary, profileSettings] = await Promise.all([
        getCategories(),
        getTransactions(period.year, period.month),
        getGoals(),
        getDashboardSummary(),
        getRecurringTransactions(),
        getGoalForecast(),
        getMonthlySummary(period.year, period.month),
        getProfileSettings(),
      ]);

      if (isActive) {
        setApiStatus({ state: 'ready', categories, transactions, goals, dashboard, recurringTransactions, goalForecasts, monthlySummary, profileSettings });
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
    void loadFinanceData(selectedPeriod, isActive);

    return () => {
      isActive = false;
    };
  }, [selectedPeriod]);

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
          <button className={`sidebar__link ${activeSection === 'dashboard' ? 'sidebar__link--active' : ''}`} onClick={() => selectSection('dashboard')} type="button">
            <span aria-hidden="true">⌂</span>
            Przegląd
          </button>
          <button className={`sidebar__link ${activeSection === 'transactions' ? 'sidebar__link--active' : ''}`} onClick={() => selectSection('transactions')} type="button">
            <span aria-hidden="true">↗</span>
            Transakcje
          </button>
          <button className={`sidebar__link ${activeSection === 'recurring' ? 'sidebar__link--active' : ''}`} onClick={() => selectSection('recurring')} type="button">
            <span aria-hidden="true">↻</span>
            Stałe transakcje
          </button>
          <button className={`sidebar__link ${activeSection === 'goals' ? 'sidebar__link--active' : ''}`} onClick={() => selectSection('goals')} type="button">
            <span aria-hidden="true">◎</span>
            Cele finansowe
          </button>
          <button className={`sidebar__link ${activeSection === 'settings' ? 'sidebar__link--active' : ''}`} onClick={() => selectSection('settings')} type="button">
            <span aria-hidden="true">⚙</span>
            Ustawienia
          </button>
        </nav>

        <button className="sidebar__quick-action" onClick={() => selectSection('transactions')} type="button">
          <span aria-hidden="true">+</span>
          Dodaj transakcję
        </button>

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
            <PeriodPicker value={selectedPeriod} onChange={setSelectedPeriod} />
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

          {activeSection === 'dashboard' && (
            <section id="overview" className="section-view dashboard-section">
              <DashboardSummary
                dashboard={apiStatus.state === 'ready' ? apiStatus.dashboard : null}
                monthlySummary={apiStatus.state === 'ready' ? apiStatus.monthlySummary : null}
                isLoading={apiStatus.state === 'loading'}
              />
            </section>
          )}

          {activeSection === 'transactions' && (
            <section className="section-view" aria-labelledby="transactions-title">
              <div className="section-heading" id="transactions">
                <div>
                  <span className="section-heading__eyebrow">Aktywność</span>
                  <h2 id="transactions-title">Transakcje</h2>
                </div>
                <span>Dodawaj i kontroluj swoje wpisy</span>
              </div>

              <div className="workspace__grid">
                <div id="add-transaction">
                  <TransactionForm
                    categories={apiStatus.state === 'ready' ? apiStatus.categories : []}
                    disabled={apiStatus.state !== 'ready'}
                    period={selectedPeriod}
                    onTransactionCreated={() => loadFinanceData(selectedPeriod)}
                  />
                </div>
                <TransactionHistory
                  transactions={apiStatus.state === 'ready' ? apiStatus.transactions : []}
                  isLoading={apiStatus.state === 'loading'}
                  onDeleteTransaction={async (id) => {
                    await deleteTransaction(id);
                    await loadFinanceData(selectedPeriod);
                  }}
                />
              </div>
            </section>
          )}

          {activeSection === 'recurring' && (
            <section className="section-view" aria-labelledby="recurring-title">
              <div className="section-heading section-heading--recurring" id="recurring">
                <div>
                  <span className="section-heading__eyebrow">Powtarzalne</span>
                  <h2 id="recurring-title">Stałe transakcje</h2>
                </div>
                <span>Generuj ręcznie raz na miesiąc</span>
              </div>

              <RecurringTransactionPanel
                categories={apiStatus.state === 'ready' ? apiStatus.categories : []}
                recurringTransactions={apiStatus.state === 'ready' ? apiStatus.recurringTransactions : []}
                disabled={apiStatus.state !== 'ready'}
                isLoading={apiStatus.state === 'loading'}
                period={selectedPeriod}
                onChanged={() => loadFinanceData(selectedPeriod)}
              />
            </section>
          )}

          {activeSection === 'goals' && (
            <section className="section-view" aria-labelledby="goals-title">
              <div className="section-heading section-heading--goals" id="goals">
                <div>
                  <span className="section-heading__eyebrow">Plan na przyszłość</span>
                  <h2 id="goals-title">Cele finansowe</h2>
                </div>
                <span>Małe kroki, konkretny progress</span>
              </div>

              <div className="goals__grid">
                <GoalForm
                  disabled={apiStatus.state !== 'ready'}
                  onGoalCreated={async (request) => {
                    await createGoal(request);
                    await loadFinanceData(selectedPeriod);
                  }}
                />
                <GoalList
                  goals={apiStatus.state === 'ready' ? apiStatus.goals : []}
                  forecasts={apiStatus.state === 'ready' ? apiStatus.goalForecasts : []}
                  isLoading={apiStatus.state === 'loading'}
                />
              </div>
            </section>
          )}

          {activeSection === 'settings' && (
            <section className="section-view" aria-labelledby="settings-title">
              <div className="section-heading section-heading--settings" id="settings">
                <div>
                  <span className="section-heading__eyebrow">Profil lokalny</span>
                  <h2 id="settings-title">Ustawienia</h2>
                </div>
                <span>Saldo początkowe konta</span>
              </div>

              <SettingsPanel
                settings={apiStatus.state === 'ready' ? apiStatus.profileSettings : null}
                disabled={apiStatus.state !== 'ready'}
                onSaved={async (initialBalance) => {
                  await updateProfileSettings(initialBalance);
                  await loadFinanceData(selectedPeriod);
                }}
              />
            </section>
          )}
        </div>
      </div>
    </main>
  );
}
