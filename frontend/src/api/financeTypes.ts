export type TransactionType = 'Income' | 'Expense';

export type CategoryResponse = {
  id: string;
  name: string;
  appliesTo: TransactionType | null;
};

export type CreateTransactionRequest = {
  amount: number;
  type: TransactionType;
  transactionDate: string;
  description: string | null;
  categoryId: string;
};

export type TransactionResponse = {
  id: string;
  amount: number;
  type: TransactionType;
  transactionDate: string;
  description: string | null;
  categoryId: string;
  categoryName: string;
};

export type CreateGoalRequest = {
  name: string;
  targetAmount: number;
};

export type GoalResponse = {
  id: string;
  name: string;
  targetAmount: number;
  currentAmount: number;
  progressPercentage: number;
};

export type ApiError = {
  message: string;
  status?: number;
};
