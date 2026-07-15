import type { DashboardSummaryResponse } from '../api/financeTypes';
import { getCategoryLabel } from '../labels';

type DashboardSummaryProps = {
  dashboard: DashboardSummaryResponse | null;
  isLoading: boolean;
};

const moneyFormatter = new Intl.NumberFormat('pl-PL', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

export function DashboardSummary({ dashboard, isLoading }: DashboardSummaryProps) {
  if (isLoading || dashboard === null) {
    return (
      <section className="dashboard dashboard--loading">
        <p className="empty-state">Ładowanie podsumowania...</p>
      </section>
    );
  }

  return (
    <section className="dashboard" aria-labelledby="dashboard-title">
      <div className="dashboard__heading">
        <div>
          <p className="workspace__eyebrow">Podsumowanie</p>
          <h2 id="dashboard-title">Twój przegląd finansów</h2>
        </div>
      </div>

      <div className="dashboard__metrics">
        <article className="metric-card metric-card--income">
          <span>Przychody</span>
          <strong>{moneyFormatter.format(dashboard.totalIncome)}</strong>
        </article>
        <article className="metric-card metric-card--expense">
          <span>Wydatki</span>
          <strong>{moneyFormatter.format(dashboard.totalExpenses)}</strong>
        </article>
        <article className="metric-card metric-card--balance">
          <span>Saldo</span>
          <strong>{moneyFormatter.format(dashboard.balance)}</strong>
        </article>
      </div>

      <div className="dashboard__details">
        <section className="dashboard__detail" aria-labelledby="expense-categories-title">
          <div className="dashboard__detail-header">
            <h3 id="expense-categories-title">Największe kategorie wydatków</h3>
            <span>{dashboard.expenseCategories.length}</span>
          </div>
          {dashboard.expenseCategories.length === 0 ? (
            <p className="empty-state">Brak wydatków do podsumowania.</p>
          ) : (
            <div className="dashboard-list">
              {dashboard.expenseCategories.map((category) => (
                <div className="dashboard-list__row" key={category.categoryName}>
                  <span>{getCategoryLabel(category.categoryName)}</span>
                  <strong>{moneyFormatter.format(category.amount)}</strong>
                </div>
              ))}
            </div>
          )}
        </section>

        <section className="dashboard__detail" aria-labelledby="dashboard-goals-title">
          <div className="dashboard__detail-header">
            <h3 id="dashboard-goals-title">Postęp celów</h3>
            <span>{dashboard.goals.length}</span>
          </div>
          {dashboard.goals.length === 0 ? (
            <p className="empty-state">Brak celów finansowych.</p>
          ) : (
            <div className="dashboard-list">
              {dashboard.goals.map((goal) => (
                <div className="dashboard-list__row" key={goal.id}>
                  <span>{goal.name}</span>
                  <strong>{Math.max(0, Math.min(goal.progressPercentage, 100)).toFixed(0)}%</strong>
                </div>
              ))}
            </div>
          )}
        </section>
      </div>
    </section>
  );
}
