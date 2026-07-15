import { FormEvent, useState } from 'react';
import { isApiError } from '../api/financeApi';
import type { CreateGoalRequest } from '../api/financeTypes';
import { polishApiMessage } from '../labels';

type GoalFormProps = {
  disabled: boolean;
  onGoalCreated: (request: CreateGoalRequest) => Promise<void>;
};

export function GoalForm({ disabled, onGoalCreated }: GoalFormProps) {
  const [name, setName] = useState('');
  const [targetAmount, setTargetAmount] = useState('');
  const [message, setMessage] = useState('');
  const [isSaving, setIsSaving] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setMessage('');

    const trimmedName = name.trim();
    const parsedTargetAmount = Number(targetAmount);
    if (!trimmedName) {
      setMessage('Podaj nazwę celu.');
      return;
    }

    if (!Number.isFinite(parsedTargetAmount) || parsedTargetAmount <= 0) {
      setMessage('Kwota celu musi być większa od 0.');
      return;
    }

    setIsSaving(true);
    try {
      await onGoalCreated({ name: trimmedName, targetAmount: parsedTargetAmount });
      setName('');
      setTargetAmount('');
      setMessage('Cel został utworzony.');
    } catch (error) {
      setMessage(
        isApiError(error) ? polishApiMessage(error.message) : 'Nie można utworzyć celu.',
      );
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <section className="panel goal-form-panel">
      <div className="panel__header">
        <h2>Utwórz cel</h2>
      </div>

      <form className="transaction-form" onSubmit={handleSubmit}>
        <label className="field">
          <span>Nazwa celu</span>
          <input
            maxLength={120}
            onChange={(event) => setName(event.target.value)}
            placeholder="Na przykład poduszka finansowa"
            required
            type="text"
            value={name}
          />
        </label>

        <label className="field">
          <span>Kwota docelowa</span>
          <input
            inputMode="decimal"
            min="0.01"
            onChange={(event) => setTargetAmount(event.target.value)}
            placeholder="0,00"
            required
            step="0.01"
            type="number"
            value={targetAmount}
          />
        </label>

        {message && <p className="form-message">{message}</p>}

        <button
          className="primary-action"
          disabled={disabled || isSaving}
          type="submit"
        >
          {isSaving ? 'Zapisywanie...' : 'Utwórz cel'}
        </button>
      </form>
    </section>
  );
}
