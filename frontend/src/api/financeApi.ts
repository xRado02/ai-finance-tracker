import type {
  ApiError,
  CategoryResponse,
  CreateTransactionRequest,
  TransactionResponse,
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

export async function getTransactions(limit?: number): Promise<TransactionResponse[]> {
  const query = typeof limit === 'number' ? `?limit=${encodeURIComponent(limit)}` : '';
  return getJson<TransactionResponse[]>(`/api/transactions${query}`);
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
