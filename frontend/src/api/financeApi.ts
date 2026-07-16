import type {
  ApiError,
  CategoryResponse,
  CreateGoalRequest,
  CreateRecurringTransactionRequest,
  CreateTransactionRequest,
  DashboardSummaryResponse,
  GenerateRecurringTransactionsResponse,
  GoalForecastResponse,
  GoalResponse,
  MonthlySummaryResponse,
  RecurringTransactionResponse,
  TransactionResponse,
  UpdateRecurringTransactionStatusRequest,
} from './financeTypes';

type ProblemDetails = {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
};

export async function getCategories(): Promise<CategoryResponse[]> {
  return getJson<CategoryResponse[]>('/api/categories');
}

export async function getTransactions(
  year?: number,
  month?: number,
  limit?: number,
): Promise<TransactionResponse[]> {
  const params = new URLSearchParams();
  if (typeof year === 'number') params.set('year', String(year));
  if (typeof month === 'number') params.set('month', String(month));
  if (typeof limit === 'number') params.set('limit', String(limit));
  const query = params.size > 0 ? `?${params.toString()}` : '';
  return getJson<TransactionResponse[]>(`/api/transactions${query}`);
}

export async function getGoals(): Promise<GoalResponse[]> {
  return getJson<GoalResponse[]>('/api/goals');
}

export async function getGoalForecast(): Promise<GoalForecastResponse[]> {
  return getJson<GoalForecastResponse[]>('/api/goals/forecast');
}

export async function createGoal(request: CreateGoalRequest): Promise<GoalResponse> {
  return getJson<GoalResponse>('/api/goals', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  });
}

export async function getDashboardSummary(): Promise<DashboardSummaryResponse> {
  return getJson<DashboardSummaryResponse>('/api/dashboard/summary');
}

export async function getMonthlySummary(year: number, month: number): Promise<MonthlySummaryResponse> {
  return getJson<MonthlySummaryResponse>(
    `/api/dashboard/monthly-summary?year=${encodeURIComponent(year)}&month=${encodeURIComponent(month)}`,
  );
}

export async function createTransaction(
  request: CreateTransactionRequest,
): Promise<TransactionResponse> {
  return getJson<TransactionResponse>('/api/transactions', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  });
}

export async function getRecurringTransactions(): Promise<RecurringTransactionResponse[]> {
  return getJson<RecurringTransactionResponse[]>('/api/recurring-transactions');
}

export async function createRecurringTransaction(
  request: CreateRecurringTransactionRequest,
): Promise<RecurringTransactionResponse> {
  return getJson<RecurringTransactionResponse>('/api/recurring-transactions', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  });
}

export async function updateRecurringTransactionStatus(
  id: string,
  request: UpdateRecurringTransactionStatusRequest,
): Promise<RecurringTransactionResponse> {
  return getJson<RecurringTransactionResponse>(
    `/api/recurring-transactions/${encodeURIComponent(id)}/status`,
    {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(request),
    },
  );
}

export async function generateCurrentMonthRecurringTransactions(
  year?: number,
  month?: number,
): Promise<GenerateRecurringTransactionsResponse> {
  const params = new URLSearchParams();
  if (typeof year === 'number') params.set('year', String(year));
  if (typeof month === 'number') params.set('month', String(month));
  const query = params.size > 0 ? `?${params.toString()}` : '';
  return getJson<GenerateRecurringTransactionsResponse>(
    `/api/recurring-transactions/generate-current-month${query}`,
    { method: 'POST' },
  );
}

export async function deleteTransaction(id: string): Promise<void> {
  const response = await fetch(`/api/transactions/${encodeURIComponent(id)}`, {
    method: 'DELETE',
  });

  if (!response.ok) {
    throw await normalizeApiError(response);
  }
}

export function isApiError(error: unknown): error is ApiError {
  return typeof error === 'object' && error !== null && 'message' in error;
}

async function getJson<T>(input: RequestInfo | URL, init?: RequestInit): Promise<T> {
  const response = await fetch(input, init);

  if (!response.ok) {
    throw await normalizeApiError(response);
  }

  return (await response.json()) as T;
}

async function normalizeApiError(response: Response): Promise<ApiError> {
  const fallback = {
    message: 'The finance API request failed.',
    status: response.status,
  };

  const contentType = response.headers.get('content-type') ?? '';
  if (!contentType.includes('application/problem+json') && !contentType.includes('application/json')) {
    return fallback;
  }

  try {
    const problem = (await response.json()) as ProblemDetails;
    const validationMessages = Object.values(problem.errors ?? {}).flat();
    const message =
      validationMessages.length > 0
        ? validationMessages.join(' ')
        : [problem.title, problem.detail].filter(Boolean).join(' ');

    return {
      message: message || fallback.message,
      status: problem.status ?? response.status,
    };
  } catch {
    return fallback;
  }
}
