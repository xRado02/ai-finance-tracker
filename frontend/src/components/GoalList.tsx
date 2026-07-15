import type { GoalResponse } from '../api/financeTypes';

type GoalListProps = {
  goals: GoalResponse[];
  isLoading: boolean;
};

const moneyFormatter = new Intl.NumberFormat('pl-PL', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

export function GoalList({ goals, isLoading }: GoalListProps) {
  return (
    <section className="panel goals-panel">
      <div className="panel__header">
        <h2>Twoje cele</h2>
        <span>{goals.length} celów</span>
      </div>

      {isLoading && <p className="empty-state">Ładowanie celów...</p>}

      {!isLoading && goals.length === 0 && (
        <p className="empty-state">Nie masz jeszcze żadnego celu.</p>
      )}

      {!isLoading && goals.length > 0 && (
        <div className="goal-list">
          {goals.map((goal) => {
            const progress = Math.max(0, Math.min(goal.progressPercentage, 100));

            return (
              <article className="goal-row" key={goal.id}>
                <div className="goal-row__header">
                  <div>
                    <h3>{goal.name}</h3>
                    <p>
                      {moneyFormatter.format(goal.currentAmount)} z {moneyFormatter.format(goal.targetAmount)}
                    </p>
                  </div>
                  <strong>{progress.toFixed(0)}%</strong>
                </div>
                <div
                  aria-label={`Postęp celu: ${progress.toFixed(0)} procent`}
                  className="progress-track"
                  role="progressbar"
                  aria-valuemax={100}
                  aria-valuemin={0}
                  aria-valuenow={progress}
                >
                  <div className="progress-bar" style={{ width: `${progress}%` }} />
                </div>
              </article>
            );
          })}
        </div>
      )}
    </section>
  );
}
