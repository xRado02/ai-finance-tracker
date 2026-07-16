import type { DashboardSummaryResponse, MonthlySummaryResponse } from '../api/financeTypes';
import { getCategoryLabel } from '../labels';

type DashboardSummaryProps = {
  dashboard: DashboardSummaryResponse | null;
  monthlySummary: MonthlySummaryResponse | null;
  isLoading: boolean;
};

const moneyFormatter = new Intl.NumberFormat('pl-PL', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

const monthFormatter = new Intl.DateTimeFormat('pl-PL', {
  month: 'long',
  year: 'numeric',
});

export function DashboardSummary({ dashboard, monthlySummary, isLoading }: DashboardSummaryProps) {
  if (isLoading || dashboard === null || monthlySummary === null) {
    return (
      <section className="dashboard dashboard--loading">
        <p className="empty-state">Ładowanie podsumowania...</p>
      </section>
    );
  }

  const selectedPeriod = monthFormatter.format(new Date(monthlySummary.year, monthlySummary.month - 1, 1));

  return (
    <section className="dashboard" aria-labelledby="dashboard-title">
      <div className="dashboard__heading">
        <div>
          <p className="workspace__eyebrow">Podsumowanie miesiąca</p>
          <h2 id="dashboard-title">{selectedPeriod}</h2>
        </div>
      </div>

      <div className="dashboard__metrics">
        <article className="metric-card metric-card--income">
          <span>Przychody w miesiącu</span>
          <strong>{moneyFormatter.format(monthlySummary.totalIncome)}</strong>
        </article>
        <article className="metric-card metric-card--expense">
          <span>Wydatki w miesiącu</span>
          <strong>{moneyFormatter.format(monthlySummary.totalExpenses)}</strong>
        </article>
        <article className="metric-card metric-card--balance">
          <span>Saldo miesiąca</span>
          <strong>{moneyFormatter.format(monthlySummary.balance)}</strong>
        </article>
      </div>

      <div className="dashboard__details">
        <CategorySummary
          title="Największe kategorie wydatków"
          emptyMessage="Brak wydatków w tym miesiącu."
          categories={monthlySummary.expenseCategories.map((category) => ({
            categoryName: category.categoryName,
            amount: category.amount,
          }))}
        />
        <CategorySummary
          title="Kategorie przychodów"
          emptyMessage="Brak przychodów w tym miesiącu."
          categories={monthlySummary.incomeCategories.map((category) => ({
            categoryName: category.categoryName,
            amount: category.amount,
          }))}
        />
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

type CategorySummaryProps = {
  title: string;
  emptyMessage: string;
  categories: Array<{ categoryName: string; amount: number }>;
};

function CategorySummary({ title, emptyMessage, categories }: CategorySummaryProps) {
  return (
    <section className="dashboard__detail" aria-label={title}>
      <div className="dashboard__detail-header">
        <h3>{title}</h3>
        <span>{categories.length}</span>
      </div>
      {categories.length === 0 ? (
        <p className="empty-state">{emptyMessage}</p>
      ) : (
        <div className="dashboard-list">
          {categories.map((category) => (
            <div className="dashboard-list__row" key={category.categoryName}>
              <span>{getCategoryLabel(category.categoryName)}</span>
              <strong>{moneyFormatter.format(category.amount)}</strong>
            </div>
          ))}
        </div>
      )}
    </section>
  );
}
