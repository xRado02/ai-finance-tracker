import { FormEvent, useEffect, useMemo, useState } from 'react';
import { createTransaction, isApiError } from '../api/financeApi';
import type { CategoryResponse, TransactionType } from '../api/financeTypes';
import { getCategoryLabel, getTransactionTypeLabel, polishApiMessage } from '../labels';
import { formatDateForPeriod, getLastDayOfPeriod, type PeriodSelection } from './PeriodPicker';

type TransactionFormProps = {
  categories: CategoryResponse[];
  disabled: boolean;
  period: PeriodSelection;
  onTransactionCreated: () => Promise<void> | void;
};

export function TransactionForm({
  categories,
  disabled,
  period,
  onTransactionCreated,
}: TransactionFormProps) {
  const [type, setType] = useState<TransactionType>('Expense');
  const [amount, setAmount] = useState('');
  const [transactionDate, setTransactionDate] = useState(formatDateForPeriod(period));
  const [categoryId, setCategoryId] = useState('');
  const [description, setDescription] = useState('');
  const [message, setMessage] = useState('');
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    setTransactionDate(formatDateForPeriod(period));
  }, [period]);

  const availableCategories = useMemo(
    () =>
      categories.filter(
        (category) => category.appliesTo === type || category.appliesTo === null,
      ),
    [categories, type],
  );

  const selectedCategoryId =
    availableCategories.some((category) => category.id === categoryId) ? categoryId : '';

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setMessage('');

    const parsedAmount = Number(amount);
    if (!Number.isFinite(parsedAmount) || parsedAmount <= 0) {
      setMessage('Kwota musi być większa od 0.');
      return;
    }

    if (!transactionDate) {
      setMessage('Data transakcji jest wymagana.');
      return;
    }

    if (!selectedCategoryId) {
      setMessage('Wybierz kategorię przed zapisaniem.');
      return;
    }

    setIsSaving(true);
    try {
      await createTransaction({
        amount: parsedAmount,
        type,
        transactionDate,
        description: description.trim() || null,
        categoryId: selectedCategoryId,
      });

      setAmount('');
      setDescription('');
      setMessage('Transakcja została zapisana.');
      await onTransactionCreated();
    } catch (error) {
      setMessage(
        isApiError(error) ? polishApiMessage(error.message) : 'Nie można zapisać transakcji.',
      );
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <section className="panel transaction-form-panel">
      <div className="panel__header">
        <h2>Dodaj transakcję</h2>
      </div>

      <form className="transaction-form" onSubmit={handleSubmit}>
        <div className="segmented-control" aria-label="Typ transakcji">
          <button
            type="button"
            className={type === 'Expense' ? 'segment segment--active' : 'segment'}
            onClick={() => setType('Expense')}
          >
            {getTransactionTypeLabel('Expense')}
          </button>
          <button
            type="button"
            className={type === 'Income' ? 'segment segment--active' : 'segment'}
            onClick={() => setType('Income')}
          >
            {getTransactionTypeLabel('Income')}
          </button>
        </div>

        <label className="field">
          <span>Kwota</span>
          <input
            inputMode="decimal"
            min="0.01"
            name="amount"
            onChange={(event) => setAmount(event.target.value)}
            placeholder="0,00"
            required
            step="0.01"
            type="number"
            value={amount}
          />
        </label>

        <label className="field">
          <span>Data</span>
          <input
            name="transactionDate"
            min={formatDateForPeriod(period)}
            onChange={(event) => setTransactionDate(event.target.value)}
            required
            type="date"
            value={transactionDate}
            max={getLastDayOfPeriod(period)}
          />
        </label>

        <label className="field">
          <span>Kategoria</span>
          <select
            disabled={disabled || availableCategories.length === 0}
            name="categoryId"
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
            name="description"
            onChange={(event) => setDescription(event.target.value)}
            placeholder="Opcjonalna notatka"
            rows={4}
            value={description}
          />
        </label>

        {message && <p className="form-message">{message}</p>}

        <button
          className="primary-action"
          disabled={disabled || isSaving || availableCategories.length === 0}
          type="submit"
        >
          {isSaving ? 'Zapisywanie...' : 'Zapisz transakcję'}
        </button>
      </form>
    </section>
  );
}
