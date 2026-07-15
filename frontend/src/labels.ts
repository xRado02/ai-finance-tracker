import type { TransactionType } from './api/financeTypes';

const categoryLabels: Record<string, string> = {
  Bills: 'Rachunki',
  Entertainment: 'Rozrywka',
  Food: 'Jedzenie',
  Health: 'Zdrowie',
  Housing: 'Mieszkanie',
  Other: 'Inne',
  'Other Income': 'Inne przychody',
  Salary: 'Wynagrodzenie',
  Transport: 'Transport',
};

export function getCategoryLabel(name: string): string {
  return categoryLabels[name] ?? name;
}

export function getTransactionTypeLabel(type: TransactionType): string {
  return type === 'Income' ? 'Przychód' : 'Wydatek';
}

export function formatDate(value: string): string {
  return new Intl.DateTimeFormat('pl-PL').format(new Date(`${value}T00:00:00`));
}

export function polishApiMessage(message: string): string {
  if (message.includes('Transaction not found')) {
    return 'Nie znaleziono transakcji.';
  }

  if (message.includes('selected category does not apply')) {
    return 'Wybrana kategoria nie pasuje do typu transakcji.';
  }

  if (message.includes('selected category does not exist')) {
    return 'Wybrana kategoria nie istnieje.';
  }

  if (message.includes('Amount must be greater than 0')) {
    return 'Kwota musi być większa od 0.';
  }

  if (message.includes('TransactionDate is required')) {
    return 'Data transakcji jest wymagana.';
  }

  if (message.includes('Description must be 500 characters or fewer')) {
    return 'Opis może mieć maksymalnie 500 znaków.';
  }

  if (message.includes('The selected category')) {
    return 'Nie można użyć wybranej kategorii dla tej transakcji.';
  }

  if (message.includes('The finance API request failed')) {
    return 'Żądanie do lokalnego API nie powiodło się.';
  }

  return message;
}
