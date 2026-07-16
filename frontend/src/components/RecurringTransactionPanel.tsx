import { FormEvent, useMemo, useState } from 'react';
import {
  createRecurringTransaction,
  generateCurrentMonthRecurringTransactions,
  isApiError,
  updateRecurringTransactionStatus,
} from '../api/financeApi';
import type {
  CategoryResponse,
  RecurringTransactionResponse,
  TransactionType,
} from '../api/financeTypes';
import { getCategoryLabel, getTransactionTypeLabel, polishApiMessage } from '../labels';
import type { PeriodSelection } from './PeriodPicker';

type RecurringTransactionPanelProps = {
  categories: CategoryResponse[];
  recurringTransactions: RecurringTransactionResponse[];
  disabled: boolean;
  isLoading: boolean;
  period: PeriodSelection;
  onChanged: () => Promise<void>;
};

const moneyFormatter = new Intl.NumberFormat('pl-PL', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

export function RecurringTransactionPanel({
  categories,
  recurringTransactions,
  disabled,
  isLoading,
  period,
  onChanged,
}: RecurringTransactionPanelProps) {
  const [type, setType] = useState<TransactionType>('Expense');
  const [amount, setAmount] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [description, setDescription] = useState('');
  const [isActive, setIsActive] = useState(true);
  const [message, setMessage] = useState('');
  const [isSaving, setIsSaving] = useState(false);
  const [isGenerating, setIsGenerating] = useState(false);
  const [changingId, setChangingId] = useState<string | null>(null);

  const availableCategories = useMemo(
    () => categories.filter((category) => category.appliesTo === type || category.appliesTo === null),
    [categories, type],
  );

  const selectedCategoryId = availableCategories.some((category) => category.id === categoryId)
    ? categoryId
    : '';

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setMessage('');

    const parsedAmount = Number(amount);
    if (!Number.isFinite(parsedAmount) || parsedAmount <= 0) {
      setMessage('Kwota musi być większa od 0.');
      return;
    }

    if (!selectedCategoryId) {
      setMessage('Wybierz kategorię przed zapisaniem.');
      return;
    }

    setIsSaving(true);
    try {
      await createRecurringTransaction({
        amount: parsedAmount,
        type,
        categoryId: selectedCategoryId,
        description: description.trim() || null,
        isActive,
      });
      setAmount('');
      setDescription('');
      setMessage('Stała transakcja została zapisana.');
      await onChanged();
    } catch (error) {
      setMessage(isApiError(error) ? polishApiMessage(error.message) : 'Nie można zapisać stałej transakcji.');
    } finally {
      setIsSaving(false);
    }
  }

  async function handleGenerate() {
    setMessage('');
    setIsGenerating(true);
    try {
      const result = await generateCurrentMonthRecurringTransactions(period.year, period.month);
      setMessage(
        result.generatedCount > 0
          ? `Wygenerowano ${result.generatedCount} transakcji za ${result.month}. Pominięto: ${result.skippedCount}.`
          : `Brak nowych transakcji do wygenerowania. Pominięto: ${result.skippedCount}.`,
      );
      await onChanged();
    } catch (error) {
      setMessage(isApiError(error) ? polishApiMessage(error.message) : 'Nie można wygenerować transakcji.');
    } finally {
      setIsGenerating(false);
    }
  }

  async function handleStatusChange(item: RecurringTransactionResponse) {
    setChangingId(item.id);
    setMessage('');
    try {
      await updateRecurringTransactionStatus(item.id, { isActive: !item.isActive });
      await onChanged();
    } catch (error) {
      setMessage(isApiError(error) ? polishApiMessage(error.message) : 'Nie można zmienić statusu.');
    } finally {
      setChangingId(null);
    }
  }

  return (
    <div className="recurring-section-grid">
      <section className="panel" id="recurring-form">
        <div className="panel__header">
          <div>
            <span className="section-heading__eyebrow">Automatyzacja ręczna</span>
            <h2>Stała transakcja</h2>
          </div>
        </div>

        <form className="transaction-form" onSubmit={(event) => void handleSubmit(event)}>
          <div className="segmented-control" aria-label="Typ stałej transakcji">
            <button
              className={type === 'Expense' ? 'segment segment--active' : 'segment'}
              onClick={() => setType('Expense')}
              type="button"
            >
              {getTransactionTypeLabel('Expense')}
            </button>
            <button
              className={type === 'Income' ? 'segment segment--active' : 'segment'}
              onClick={() => setType('Income')}
              type="button"
            >
              {getTransactionTypeLabel('Income')}
            </button>
          </div>

          <label className="field">
            <span>Kwota</span>
            <input
              inputMode="decimal"
              min="0.01"
              onChange={(event) => setAmount(event.target.value)}
              placeholder="0,00"
              required
              step="0.01"
              type="number"
              value={amount}
            />
          </label>

          <label className="field">
            <span>Kategoria</span>
            <select
              disabled={disabled || availableCategories.length === 0}
              onChange={(event) => setCategoryId(event.target.value)}
              required
              value={selectedCategoryId}
            >
              <option value="">Wybierz kategorię</option>
              {availableCategories.map((category) => (
                <option key={category.id} value={category.id}>
                  {getCategoryLabel(category.name)}
                </option>
              ))}
            </select>
          </label>

          <label className="field">
            <span>Opis</span>
            <textarea
              maxLength={500}
              onChange={(event) => setDescription(event.target.value)}
              placeholder="Na przykład czynsz albo pensja"
              rows={3}
              value={description}
            />
          </label>

          <label className="checkbox-field">
            <input checked={isActive} onChange={(event) => setIsActive(event.target.checked)} type="checkbox" />
            <span>Aktywna od razu</span>
          </label>

          {message && <p className="form-message">{message}</p>}

          <button className="primary-action" disabled={disabled || isSaving} type="submit">
            {isSaving ? 'Zapisywanie...' : 'Zapisz stałą transakcję'}
          </button>
        </form>
      </section>

      <section className="panel recurring-list-panel">
        <div className="panel__header recurring-toolbar">
          <div>
            <span className="section-heading__eyebrow">Definicje</span>
            <h2>Stałe przychody i wydatki</h2>
          </div>
          <button className="secondary-action" disabled={disabled || isGenerating} onClick={() => void handleGenerate()} type="button">
            {isGenerating ? 'Generowanie...' : 'Wygeneruj za ten miesiąc'}
          </button>
        </div>

        {isLoading && <p className="empty-state">Ładowanie stałych transakcji...</p>}
        {!isLoading && recurringTransactions.length === 0 && (
          <p className="empty-state">Nie masz jeszcze żadnej stałej transakcji.</p>
        )}
        {!isLoading && recurringTransactions.length > 0 && (
          <div className="recurring-list">
            {recurringTransactions.map((item) => (
              <article className="recurring-row" key={item.id}>
                <div>
                  <div className="recurring-row__title">
                    <strong>{getCategoryLabel(item.categoryName)}</strong>
                    <span className={`type-pill type-pill--${item.type.toLowerCase()}`}>
                      {getTransactionTypeLabel(item.type)}
                    </span>
                  </div>
                  <p>
                    {item.description || 'Bez opisu'} · {moneyFormatter.format(item.amount)}
                  </p>
                </div>
                <div className="recurring-row__actions">
                  <span className={item.isActive ? 'recurring-status' : 'recurring-status recurring-status--inactive'}>
                    {item.isActive ? 'Aktywna' : 'Nieaktywna'}
                  </span>
                  <button
                    className="delete-action"
                    disabled={changingId !== null}
                    onClick={() => void handleStatusChange(item)}
                    type="button"
                  >
                    {item.isActive ? 'Wyłącz' : 'Włącz'}
                  </button>
                </div>
              </article>
            ))}
          </div>
        )}
      </section>
    </div>
  );
}
