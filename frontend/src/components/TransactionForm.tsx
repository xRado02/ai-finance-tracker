import { FormEvent, useMemo, useState } from 'react';
import { createTransaction, isApiError } from '../api/financeApi';
import type { CategoryResponse, TransactionType } from '../api/financeTypes';

type TransactionFormProps = {
  categories: CategoryResponse[];
  disabled: boolean;
  onTransactionCreated: () => Promise<void> | void;
};

const today = new Date().toISOString().slice(0, 10);

export function TransactionForm({
  categories,
  disabled,
  onTransactionCreated,
}: TransactionFormProps) {
  const [type, setType] = useState<TransactionType>('Expense');
  const [amount, setAmount] = useState('');
  const [transactionDate, setTransactionDate] = useState(today);
  const [categoryId, setCategoryId] = useState('');
  const [description, setDescription] = useState('');
  const [message, setMessage] = useState('');
  const [isSaving, setIsSaving] = useState(false);

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
      setMessage('Amount must be greater than 0.');
      return;
    }

    if (!transactionDate) {
      setMessage('Transaction date is required.');
      return;
    }

    if (!selectedCategoryId) {
      setMessage('Select a category before saving.');
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
      setMessage('Transaction saved.');
      await onTransactionCreated();
    } catch (error) {
      setMessage(isApiError(error) ? error.message : 'Could not save the transaction.');
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <section className="panel transaction-form-panel">
      <div className="panel__header">
        <h2>Add transaction</h2>
      </div>

      <form className="transaction-form" onSubmit={handleSubmit}>
        <div className="segmented-control" aria-label="Transaction type">
          <button
            type="button"
            className={type === 'Expense' ? 'segment segment--active' : 'segment'}
            onClick={() => setType('Expense')}
          >
            Expense
          </button>
          <button
            type="button"
            className={type === 'Income' ? 'segment segment--active' : 'segment'}
            onClick={() => setType('Income')}
          >
            Income
          </button>
        </div>

        <label className="field">
          <span>Amount</span>
          <input
            inputMode="decimal"
            min="0.01"
            name="amount"
            onChange={(event) => setAmount(event.target.value)}
            placeholder="0.00"
            required
            step="0.01"
            type="number"
            value={amount}
          />
        </label>

        <label className="field">
          <span>Date</span>
          <input
            name="transactionDate"
            onChange={(event) => setTransactionDate(event.target.value)}
            required
            type="date"
            value={transactionDate}
          />
        </label>

        <label className="field">
          <span>Category</span>
          <select
            disabled={disabled || availableCategories.length === 0}
            name="categoryId"
            onChange={(event) => setCategoryId(event.target.value)}
            required
            value={selectedCategoryId}
          >
            <option value="">Select category</option>
            {availableCategories.map((category) => (
              <option key={category.id} value={category.id}>
                {category.name}
              </option>
            ))}
          </select>
        </label>

        <label className="field">
          <span>Description</span>
          <textarea
            maxLength={500}
            name="description"
            onChange={(event) => setDescription(event.target.value)}
            placeholder="Optional note"
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
          {isSaving ? 'Saving...' : 'Save transaction'}
        </button>
      </form>
    </section>
  );
}
