import { FormEvent, useEffect, useState } from 'react';
import type { ProfileSettingsResponse } from '../api/financeTypes';

type SettingsPanelProps = {
  settings: ProfileSettingsResponse | null;
  disabled: boolean;
  onSaved: (initialBalance: number) => Promise<void>;
};

const moneyFormatter = new Intl.NumberFormat('pl-PL', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

export function SettingsPanel({ settings, disabled, onSaved }: SettingsPanelProps) {
  const [initialBalance, setInitialBalance] = useState('');
  const [message, setMessage] = useState('');
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    if (settings !== null) {
      setInitialBalance(String(settings.initialBalance));
    }
  }, [settings]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setMessage('');

    const parsedBalance = Number(initialBalance);
    if (!Number.isFinite(parsedBalance)) {
      setMessage('Podaj prawidłową kwotę stanu początkowego.');
      return;
    }

    setIsSaving(true);
    try {
      await onSaved(parsedBalance);
      setMessage('Stan początkowy konta został zapisany.');
    } catch {
      setMessage('Nie można zapisać ustawienia.');
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <section className="panel settings-panel" aria-labelledby="settings-title">
      <div className="panel__header">
        <span className="section-heading__eyebrow">Konfiguracja</span>
        <h2 id="settings-title">Ustawienia konta</h2>
        <p>Ustaw kwotę, którą miałeś na koncie przed pierwszą zapisaną transakcją.</p>
      </div>

      <form className="transaction-form" onSubmit={handleSubmit}>
        <label className="field">
          <span>Stan początkowy konta</span>
          <input
            inputMode="decimal"
            name="initialBalance"
            onChange={(event) => setInitialBalance(event.target.value)}
            placeholder="0,00"
            required
            step="0.01"
            type="number"
            value={initialBalance}
          />
        </label>

        <p className="settings-panel__hint">
          Ta kwota nie jest przychodem. Wpływa tylko na saldo całkowite: {moneyFormatter.format(settings?.initialBalance ?? 0)}.
        </p>

        {message && <p className="form-message">{message}</p>}

        <button className="primary-action" disabled={disabled || isSaving} type="submit">
          {isSaving ? 'Zapisywanie...' : 'Zapisz ustawienie'}
        </button>
      </form>
    </section>
  );
}
