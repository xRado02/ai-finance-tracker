import type { GoalForecastResponse, GoalResponse } from '../api/financeTypes';

type GoalListProps = {
  goals: GoalResponse[];
  forecasts: GoalForecastResponse[];
  isLoading: boolean;
};

const moneyFormatter = new Intl.NumberFormat('pl-PL', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

export function GoalList({ goals, forecasts, isLoading }: GoalListProps) {
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
            const forecast = forecasts.find((item) => item.goalId === goal.id);

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
                {forecast && <GoalForecast forecast={forecast} progress={progress} />}
              </article>
            );
          })}
        </div>
      )}
    </section>
  );
}

function GoalForecast({ forecast, progress }: { forecast: GoalForecastResponse; progress: number }) {
  const formatMoney = (amount: number) => moneyFormatter.format(amount);
  const message = getForecastMessage(forecast);
  const estimatedDate = forecast.estimatedDate
    ? new Intl.DateTimeFormat('pl-PL', { month: 'long', year: 'numeric' }).format(
        new Date(`${forecast.estimatedDate}T00:00:00`),
      )
    : null;
  const progressPoint = Math.max(8, Math.min(92, progress));

  return (
    <div className="goal-forecast">
      <div className="goal-forecast__header">
        <span>Prognoza celu</span>
        <strong>{forecast.remainingAmount > 0 ? `Brakuje ${formatMoney(forecast.remainingAmount)}` : 'Gotowe'}</strong>
      </div>
      <svg
        aria-label="Wizualizacja trendu postępu celu"
        className="forecast-chart"
        role="img"
        viewBox="0 0 220 42"
      >
        <path d="M4 35 C 48 34, 76 28, 108 25 S 168 12, 216 6" fill="none" stroke="rgba(255,255,255,0.12)" strokeWidth="2" />
        <line stroke="rgba(185,236,114,0.3)" strokeDasharray="3 4" x1="4" x2="216" y1="35" y2="35" />
        <circle cx={4 + progressPoint * 2.12} cy={35 - progressPoint * 0.29} fill="#b9ec72" r="4" />
      </svg>
      <p className={`goal-forecast__message goal-forecast__message--${forecast.status.toLowerCase()}`}>
        {message}
      </p>
      {forecast.averageMonthlySurplus !== null && forecast.status !== 'Achieved' && (
        <span className="goal-forecast__average">
          Średnia miesięczna nadwyżka: {formatMoney(forecast.averageMonthlySurplus)}
          {estimatedDate ? ` · Termin: ${estimatedDate}` : ''}
        </span>
      )}
    </div>
  );
}

function getForecastMessage(forecast: GoalForecastResponse): string {
  if (forecast.status === 'Achieved') {
    return 'Cel osiągnięty';
  }

  if (forecast.status === 'NoData') {
    return 'Brak danych do oszacowania terminu.';
  }

  if (forecast.status === 'NoPositiveSurplus') {
    return 'Brak dodatniej nadwyżki — nie da się oszacować terminu';
  }

  const months = forecast.estimatedMonths ?? 0;
  const monthsLabel = months === 1 ? 'miesiąc' : months >= 2 && months <= 4 ? 'miesiące' : 'miesięcy';
  return `Przy obecnym tempie osiągniesz cel za około ${months} ${monthsLabel}.`;
}
