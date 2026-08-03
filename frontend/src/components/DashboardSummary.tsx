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
  const spendingLimit = getMonthlySpendingLimit(monthlySummary.month);
  const remainingSpending = spendingLimit - monthlySummary.totalExpenses;
  const isOverSpendingLimit = remainingSpending < 0;
  const spendingProgress = Math.min((monthlySummary.totalExpenses / spendingLimit) * 100, 100);

  return (
    <section className="dashboard" aria-labelledby="dashboard-title">
      <div className="dashboard__heading">
        <div>
          <p className="workspace__eyebrow">Podsumowanie miesiąca</p>
          <h2 id="dashboard-title">{selectedPeriod}</h2>
        </div>
      </div>

      <section
        className={`spending-budget${isOverSpendingLimit ? ' spending-budget--over' : ''}`}
        aria-labelledby="spending-budget-title"
      >
        <div className="spending-budget__summary">
          <div>
            <span className="spending-budget__eyebrow">Miesięczny budżet wydatków</span>
            <h3 id="spending-budget-title">
              {isOverSpendingLimit ? 'Przekroczono limit' : 'Możesz jeszcze wydać'}
            </h3>
            <p>Limit na {selectedPeriod}: {moneyFormatter.format(spendingLimit)}</p>
          </div>
          <div className="spending-budget__amount">
            <strong>{moneyFormatter.format(Math.abs(remainingSpending))}</strong>
            <span>{isOverSpendingLimit ? 'ponad limit' : 'do końca miesiąca'}</span>
          </div>
        </div>
        <div
          className="spending-budget__track"
          role="progressbar"
          aria-label="Wykorzystanie miesięcznego budżetu wydatków"
          aria-valuemin={0}
          aria-valuemax={100}
          aria-valuenow={Math.round(spendingProgress)}
        >
          <div className="spending-budget__bar" style={{ width: `${spendingProgress}%` }} />
        </div>
        <div className="spending-budget__footer">
          <span>Wydano {moneyFormatter.format(monthlySummary.totalExpenses)}</span>
          <span>{Math.round((monthlySummary.totalExpenses / spendingLimit) * 100)}% limitu</span>
        </div>
      </section>

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
        <article className="metric-card metric-card--total-balance">
          <span>Saldo całkowite</span>
          <strong>{moneyFormatter.format(dashboard.balance)}</strong>
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

function getMonthlySpendingLimit(month: number): number {
  return [8, 9, 12].includes(month) ? 2000 : 1500;
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
